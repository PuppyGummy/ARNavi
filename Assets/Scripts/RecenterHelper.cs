using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing;
using Unity.Collections;


public class RecenterHelper : MonoBehaviour
{
    [SerializeField] private ARSession session;
    [SerializeField] private XROrigin sessionOrigin;
    [SerializeField] private ARCameraManager cameraManager;
    [SerializeField] private List<Target> targetList = new List<Target>();

    private Texture2D cameraImageTexture;
    private IBarcodeReader reader = new BarcodeReader();
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
            SetQRCodeRecenterTarget(result.Text);
        }
    }
    private void SetQRCodeRecenterTarget(string targetName)
    {
        foreach (var target in targetList)
        {
            if (target.targetName == targetName)
            {
                session.Reset();
                sessionOrigin.transform.position = target.positionObj.transform.position;
                sessionOrigin.transform.rotation = target.positionObj.transform.rotation;
                break;
            }
        }
    }
}
