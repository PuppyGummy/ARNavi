using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class ARPlaceAnchor : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The enabled Anchor Manager in the scene.")]
    ARAnchorManager m_AnchorManager;

    [SerializeField]
    [Tooltip("The prefab to be instantiated for each anchor.")]
    GameObject m_Prefab;

    private ARRaycastManager raycastManager;

    private List<ARAnchor> m_Anchors = new();
    public List<GameObject> contents = new List<GameObject>();
    public GameObject contentParent;
    private int contentIndex = 0;
    private float contentHeight = 0.6f;
    private AnchorDataList anchorDataList = new AnchorDataList();
    [SerializeField] private bool canPlaceAnchors = false;
    [SerializeField] private bool canEditAnchors = false;
    [SerializeField] private TMP_InputField inputX;
    [SerializeField] private TMP_InputField inputY;
    [SerializeField] private TMP_InputField inputZ;
    [SerializeField] private TMP_Dropdown transformDropdown;
    [SerializeField] private GameObject transformUI;
    [SerializeField] private TMP_InputField inputText;
    [SerializeField] private GameObject sideUI;
    private ARAnchor currentAnchor;
    private GameObject currentTextObject;
    private bool isPlacingText = false;
    private float distanceFromCamera = 1.0f;


    public ARAnchorManager anchorManager
    {
        get => m_AnchorManager;
        set => m_AnchorManager = value;
    }

    public GameObject prefab
    {
        get => m_Prefab;
        set => m_Prefab = value;
    }

    public static ARPlaceAnchor Instance { get; private set; }

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();

        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        // GameObject.DontDestroyOnLoad(this.gameObject);
    }

    public void RemoveAllAnchors()
    {
        foreach (var anchor in m_Anchors)
        {
            Destroy(anchor.gameObject);
        }
        m_Anchors.Clear();
    }

    // Runs when the reset option is called in the context menu in-editor, or when first created.
    void Reset()
    {
        if (m_AnchorManager == null)
#if UNITY_2023_1_OR_NEWER
                m_AnchorManager = FindAnyObjectByType<ARAnchorManager>();
#else
            m_AnchorManager = FindObjectOfType<ARAnchorManager>();
#endif
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (IsPointerOverUIObject(touch))
                {
                    return;
                }
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                if (raycastManager.Raycast(touch.position, hits))
                {
                    ARRaycastHit hit = hits[0];
                    if (canEditAnchors && DetectAnchor(hit.pose.position))
                    {

                    }
                    else if (canPlaceAnchors)
                    {
                        CreateAnchor(hit);
                    }
                    else
                    {
                        currentAnchor.GetComponent<Outline>().enabled = false;
                        currentAnchor = null;
                        transformUI.SetActive(false);
                    }
                }
            }
        }
        if (isPlacingText)
        {
            UpdateTextTransform();
        }
    }
    private bool IsPointerOverUIObject(Touch touch)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(touch.position.x, touch.position.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
    private bool DetectAnchor(Vector3 position)
    {
        foreach (var anchor in m_Anchors)
        {
            if (Vector3.Distance(anchor.transform.position, position) < 0.5f)
            {
                SetCurrentAnchor(anchor);
                return true;
            }
        }
        return false;
    }

    private void CreateAnchor(ARRaycastHit arRaycastHit)
    {
        ARAnchor anchor;

        // If we hit a plane, try to "attach" the anchor to the plane
        if (m_AnchorManager.descriptor.supportsTrackableAttachments && arRaycastHit.trackable is ARPlane plane)
        {
            if (contents != null && contentIndex < contents.Count)
            {
                var oldPrefab = m_AnchorManager.anchorPrefab;
                m_AnchorManager.anchorPrefab = contents[contentIndex];
                anchor = m_AnchorManager.AttachAnchor(plane, new Pose(new Vector3(arRaycastHit.pose.position.x, contentHeight, arRaycastHit.pose.position.z), Quaternion.Euler(0, 90, 0)));
                m_AnchorManager.anchorPrefab = oldPrefab;
                AnchorData anchorData = new AnchorData
                {
                    anchorID = anchor.trackableId.ToString(),
                    contentIndex = contentIndex,
                };
                anchorDataList.anchors.Add(anchorData);
                contentIndex++;
            }
            else
            {
                anchor = m_AnchorManager.AttachAnchor(plane, new Pose(new Vector3(arRaycastHit.pose.position.x, contentHeight, arRaycastHit.pose.position.z), Quaternion.Euler(0, 90, 0)));
            }

            FinalizePlacedAnchor(anchor);
            return;
        }

        // Otherwise, just create a regular anchor at the hit pose
        if (contents != null)
        {
            var anchorPrefab = Instantiate(contents[contentIndex], new Vector3(arRaycastHit.pose.position.x, contentHeight, arRaycastHit.pose.position.z), Quaternion.Euler(0, 90, 0));
            anchor = ComponentUtils.GetOrAddIf<ARAnchor>(anchorPrefab, true);
            AnchorData anchorData = new AnchorData
            {
                anchorID = anchor.trackableId.ToString(),
                contentIndex = contentIndex,
            };
            anchorDataList.anchors.Add(anchorData);
            contentIndex++;
        }
        else
        {
            var anchorPrefab = new GameObject("Anchor");
            anchorPrefab.transform.SetPositionAndRotation(new Vector3(arRaycastHit.pose.position.x, contentHeight, arRaycastHit.pose.position.z), Quaternion.Euler(0, 90, 0));
            anchor = anchorPrefab.AddComponent<ARAnchor>();
        }
        FinalizePlacedAnchor(anchor);
    }

    void FinalizePlacedAnchor(ARAnchor anchor)
    {
        anchor.transform.SetParent(contentParent.transform);
        m_Anchors.Add(anchor);

        Outline outline = anchor.AddComponent<Outline>();
        outline.effectColor = Color.blue;
        outline.effectDistance = new Vector2(0.01f, 0.01f);
        SetCurrentAnchor(anchor);
    }
    public void SaveAnchors()
    {
        SaveLoadManager.SaveAnchors(anchorDataList);
    }
    public void LoadAnchors()
    {
        AnchorDataList anchorDataList = SaveLoadManager.LoadAnchors();
        if (anchorDataList != null)
        {
            foreach (var anchorData in anchorDataList.anchors)
            {
                foreach (var anchor in anchorManager.trackables)
                {
                    if (anchor.trackableId.ToString() == anchorData.anchorID)
                    {
                        GameObject content;
                        if (contents != null)
                        {
                            content = Instantiate(contents[anchorData.contentIndex], anchor.transform.position, anchor.transform.rotation);
                        }
                        else
                        {
                            content = Instantiate(m_Prefab, anchor.transform.position, anchor.transform.rotation);
                        }

                        // Ensure content has ARAnchor component
                        var contentAnchor = content.GetComponent<ARAnchor>();
                        if (contentAnchor == null)
                        {
                            contentAnchor = content.AddComponent<ARAnchor>();
                        }

                        content.transform.SetParent(contentParent.transform);
                        m_Anchors.Add(anchor);
                        break;
                    }
                }
            }
        }
    }
    public void TogglePlaceAnchors()
    {
        canPlaceAnchors = !canPlaceAnchors;
        ARWorldMapController.Instance.TogglePlaceAnchorsUI(canPlaceAnchors || canEditAnchors);
    }
    public void ToggleEditAnchors()
    {
        canEditAnchors = !canEditAnchors;
        ARWorldMapController.Instance.TogglePlaceAnchorsUI(canPlaceAnchors || canEditAnchors);
    }
    public void SetCurrentAnchor(ARAnchor anchor)
    {
        if (currentAnchor != null)
            currentAnchor.GetComponent<Outline>().enabled = false;
        currentAnchor = anchor;
        anchor.GetComponent<Outline>().enabled = true;
        UpdateInputFields();
        transformUI.SetActive(true);
    }
    public void UpdateInputFields()
    {
        if (currentAnchor != null)
        {
            int currentTransform = transformDropdown.value;
            switch (currentTransform)
            {
                case 0:
                    Vector3 position = currentAnchor.transform.position;
                    inputX.text = position.x.ToString();
                    inputY.text = position.y.ToString();
                    inputZ.text = position.z.ToString();
                    break;
                case 1:
                    Vector3 rotation = currentAnchor.transform.rotation.eulerAngles;
                    inputX.text = rotation.x.ToString();
                    inputY.text = rotation.y.ToString();
                    inputZ.text = rotation.z.ToString();
                    break;
                case 2:
                    Vector3 scale = currentAnchor.transform.localScale;
                    inputX.text = scale.x.ToString();
                    inputY.text = scale.y.ToString();
                    inputZ.text = scale.z.ToString();
                    break;
            }
        }
    }
    public void OnApplyButtonClicked()
    {
        if (currentAnchor != null)
        {
            // delete the old anchor and create a new one
            GameObject currentContent = currentAnchor.gameObject;
            m_Anchors.Remove(currentAnchor);
            // destroy the anchor component
            Destroy(currentContent.GetComponent<ARAnchor>());

            float valueX = float.Parse(inputX.text);
            float valueY = float.Parse(inputY.text);
            float valueZ = float.Parse(inputZ.text);

            int currentTransform = transformDropdown.value;
            switch (currentTransform)
            {
                case 0:
                    currentContent.transform.position = new Vector3(valueX, valueY, valueZ);
                    break;
                case 1:
                    currentContent.transform.rotation = Quaternion.Euler(valueX, valueY, valueZ);
                    break;
                case 2:
                    currentContent.transform.localScale = new Vector3(valueX, valueY, valueZ);
                    break;
            }
            CreateAnchor(currentContent);
        }
        transformUI.SetActive(false);
        currentAnchor.GetComponent<Outline>().enabled = false;
    }
    private void RemoveAnchor(ARAnchor anchor)
    {
        m_Anchors.Remove(anchor);
        Destroy(anchor.gameObject);
    }
    public void RemoveCurrentAnchor()
    {
        if (currentAnchor != null)
        {
            RemoveAnchor(currentAnchor);
            currentAnchor = null;
            transformUI.SetActive(false);
        }
    }
    public void CreateAnchor(GameObject obj)
    {
        ARAnchor anchor = ComponentUtils.GetOrAddIf<ARAnchor>(obj, true);
        // AnchorData anchorData = new AnchorData
        // {
        //     anchorID = anchor.trackableId.ToString(),
        //     contentIndex = contentIndex,
        // };
        // anchorDataList.anchors.Add(anchorData);
        FinalizePlacedAnchor(anchor);
    }
    public void CreateTextAnchor()
    {
        if (currentTextObject != null)
        {
            CreateAnchor(currentTextObject);
            currentTextObject = null;
            isPlacingText = false;
        }
    }
    public void OnCreateTextButtonClicked()
    {
        isPlacingText = true;

        GameObject textContent = new GameObject("Text");
        textContent.transform.SetParent(contentParent.transform);
        textContent.transform.localScale = new Vector3(1f, 1f, 1f);
        var text = textContent.AddComponent<TextMeshPro>();
        text.text = "";
        text.fontSize = 36;
        text.alignment = TextAlignmentOptions.Center;
        textContent.transform.position = Camera.main.transform.position + Camera.main.transform.forward * distanceFromCamera;
        //make the text face the camera
        textContent.transform.rotation = Quaternion.LookRotation(textContent.transform.position - Camera.main.transform.position);

        currentTextObject = textContent;
    }
    public void OnTextInputFieldChanged()
    {
        if (currentTextObject != null)
        {
            string text = inputText.text;
            Debug.Log(text);
            currentTextObject.GetComponent<TextMeshPro>().text = text;
        }
    }
    public void UpdateTextTransform()
    {
        if (currentTextObject != null)
        {
            currentTextObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * distanceFromCamera;
            currentTextObject.transform.rotation = Quaternion.LookRotation(currentTextObject.transform.position - Camera.main.transform.position);
        }
    }
    public void ToggleSideUI()
    {
        sideUI.SetActive(!sideUI.activeSelf);
    }
}