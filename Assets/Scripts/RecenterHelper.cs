using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing;
using Unity.Collections;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class RecenterHelper : MonoBehaviour
{
    [SerializeField] private ARSession session;
    [SerializeField] private XROrigin sessionOrigin;
    [SerializeField] private ARCameraManager cameraManager;
    [SerializeField] private GameObject recenterTargetsParent;
    public List<GameObject> recenterTargetList = new List<GameObject>();
    [SerializeField] private TMP_Text calibrationText;
    [SerializeField] private Image scanPanel;
    [SerializeField] private GameObject calibrationSuccessPanel;
    [SerializeField] private GameObject visualMarkerIndicationPanel;
    [SerializeField] private float visualMarkerDistance = 5f;

    private Texture2D cameraImageTexture;
    private IBarcodeReader reader = new BarcodeReader();
    private string qrCodeResult;
    private bool scanningEnabled = false;
    // private bool isNearVisualMarker = false;
    private Dictionary<GameObject, bool> targetFlags = new Dictionary<GameObject, bool>();
    public static RecenterHelper Instance;
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        // GameObject.DontDestroyOnLoad(this.gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform target in recenterTargetsParent.transform)
        {
            recenterTargetList.Add(target.gameObject);
        }
        Recenter("FirstFloorStartPoint");
        foreach (var target in recenterTargetList)
        {
            targetFlags[target] = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.C))
        // {
        //     ShowPanelAnim(calibrationSuccessPanel);
        // }
        IsNearVisualMarker();
    }

    private void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrameReceived;
    }
    private void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }
    private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (!scanningEnabled)
        {
            return;
        }
        if (!cameraManager.TryAcquireLatestCpuImage(out var cameraImage))
        {
            return;
        }

        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, cameraImage.width, cameraImage.height),
            outputDimensions = new Vector2Int(cameraImage.width / 2, cameraImage.height / 2),
            outputFormat = TextureFormat.RGBA32,
            transformation = XRCpuImage.Transformation.MirrorY
        };
        int size = cameraImage.GetConvertedDataSize(conversionParams);
        var buffer = new NativeArray<byte>(size, Allocator.Temp);
        cameraImage.Convert(conversionParams, buffer);
        cameraImage.Dispose();
        cameraImageTexture = new Texture2D(conversionParams.outputDimensions.x, conversionParams.outputDimensions.y, conversionParams.outputFormat, false);
        cameraImageTexture.LoadRawTextureData(buffer);
        cameraImageTexture.Apply();
        buffer.Dispose();
        var result = reader.Decode(cameraImageTexture.GetPixels32(), cameraImageTexture.width, cameraImageTexture.height);
        if (result != null)
        {
            qrCodeResult = result.Text;
            calibrationText.text = "QR Code Detected: " + qrCodeResult;
            SetQRCodeRecenterTarget();
        }
        else
        {
            calibrationText.text = "No QR Code Detected";
        }
    }

    public void SetQRCodeRecenterTarget()
    {
        Recenter(qrCodeResult);

        scanningEnabled = false;
        calibrationText.gameObject.SetActive(false);
        scanPanel.gameObject.SetActive(false);
        ShowPanelAnim(calibrationSuccessPanel);
    }
    public void ToggleScanning()
    {
        scanningEnabled = !scanningEnabled;
        if (scanningEnabled)
        {
            calibrationText.gameObject.SetActive(true);
            scanPanel.gameObject.SetActive(true);
        }
        else
        {
            calibrationText.gameObject.SetActive(false);
            scanPanel.gameObject.SetActive(false);
        }
    }
    public void RecenterCurrentFloor()
    {
        // remove the spaces from the floor name
        string currentFloorName = NavigationManager.Instance.currentFloor.name.Replace(" ", "");
        string startPointName = currentFloorName + "StartPoint";
        Recenter(startPointName);
    }
    public void Recenter(string targetName)
    {
        foreach (var target in recenterTargetList)
        {
            if (target.name == targetName)
            {
                session.Reset();
                sessionOrigin.transform.SetPositionAndRotation(target.transform.position, target.transform.rotation);
                break;
            }
        }
    }
    private void ShowPanelAnim(GameObject panel)
    {
        panel.SetActive(true);
        panel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            panel.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).SetDelay(0.5f).OnComplete(() =>
            {
                panel.SetActive(false);
            });
        });
    }
    private void IsNearVisualMarker()
    {
        // if the user is near any of the recenter targets, show the visual marker indication panel
        foreach (var target in recenterTargetList)
        {
            // show the panel only once when the user is near the visual marker, and reset the flag
            if (Vector3.Distance(sessionOrigin.transform.position, target.transform.position) < visualMarkerDistance)
            {
                if (!targetFlags[target])
                {
                    ShowPanelAnim(visualMarkerIndicationPanel);
                    targetFlags[target] = true;
                }
            }
            else
            {
                targetFlags[target] = false;
            }
        }
    }
}