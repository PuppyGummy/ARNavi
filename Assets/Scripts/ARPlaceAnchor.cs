using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Video;

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
    public GameObject[] contents;
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
    [SerializeField] private TMP_Dropdown presetsDropdown;
    private ARAnchor currentAnchor;
    private GameObject currentAnchorObject;
    private bool isPlacingContent = false;
    private float distanceFromCamera = 1.0f;
    private AnchorData currentAnchorData;


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

        LoadPresets();
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
#if UNITY_EDITOR
                // Debugging in editor
                if (canEditAnchors && DetectAnchor(touch.position))
                {
                    Debug.Log("Anchor detected");
                }
                else if (canPlaceAnchors)
                {
                    GameObject prefab = Instantiate(contents[1], Camera.main.transform.position + Camera.main.transform.forward * distanceFromCamera, Quaternion.identity);
                    CreateAnchor(prefab);
                }
#endif
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                if (raycastManager.Raycast(touch.position, hits))
                {
                    ARRaycastHit hit = hits[0];
                    if (canEditAnchors && DetectAnchor(touch.position))
                    {
                        Debug.Log("Anchor detected");
                    }
                    // else if (canPlaceAnchors)
                    // {
                    //     CreateAnchor(hit);
                    //     Debug.Log("Anchor created");
                    // }
                    else
                    {
                        if (currentAnchor != null)
                        {
                            currentAnchor = null;
                            transformUI.SetActive(false);
                        }
                    }
                }
            }
        }
        if (isPlacingContent)
        {
            UpdateContentTransform();
        }
    }
    private bool IsPointerOverUIObject(Touch touch)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(touch.position.x, touch.position.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        foreach (var result in results)
        {
            if (result.gameObject.tag == "POI")
            {
                return false;
            }
        }
        return results.Count > 0;
    }
    private bool DetectAnchor(Vector2 touchPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null && hit.collider.GetComponent<ARAnchor>() != null)
            {
                currentAnchor = hit.collider.GetComponent<ARAnchor>();
                SetCurrentAnchor(currentAnchor);
                return true;
            }
            return false;
        }
        return false;
    }

    void FinalizePlacedAnchor(ARAnchor anchor)
    {
        anchor.transform.SetParent(contentParent.transform);
        BoxCollider collider = anchor.AddComponent<BoxCollider>();
        AdjustColliderSize(anchor.gameObject, collider);
        anchor.tag = "POI";
        m_Anchors.Add(anchor);
        currentAnchor = anchor;
    }
    public void SaveAnchors()
    {
        // find all the image and video anchors and upload the files to Firebase Storage
        foreach (var anchorData in anchorDataList.anchors)
        {
            if (anchorData.anchorType == AnchorType.Media)
            {
                Debug.Log("Uploading media to Firebase Storage...");
                
                string path = anchorData.anchorData;
                FirebaseManager.UploadAndSaveMedia(path, anchorData);
            }
        }
        FirebaseManager.SaveAnchorDataList(anchorDataList);
    }
    public void LoadAnchors()
    {
        AnchorDataList anchorDataList = FirebaseManager.LoadAnchorDataList();
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
                            content = Instantiate(Resources.Load<GameObject>("Presets/" + anchorData.anchorData), anchor.transform.position, anchor.transform.rotation);
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
            foreach (var anchorData in anchorDataList.anchors)
            {
                if (anchorData.anchorID == currentAnchor.trackableId.ToString())
                {
                    anchorDataList.anchors.Remove(anchorData);
                    break;
                }
            }
            m_Anchors.Remove(currentAnchor);
            // destroy the anchor component
            DestroyImmediate(currentContent.GetComponent<ARAnchor>());
            DestroyImmediate(currentContent.GetComponent<BoxCollider>());

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
    }
    private void RemoveAnchor(ARAnchor anchor)
    {
        // find the anchor data with the same ID and remove it
        foreach (var anchorData in anchorDataList.anchors)
        {
            if (anchorData.anchorID == anchor.trackableId.ToString())
            {
                anchorDataList.anchors.Remove(anchorData);
                break;
            }
        }
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
        // ARAnchor anchor = ComponentUtils.GetOrAddIf<ARAnchor>(obj, true);
        ARAnchor anchor = obj.AddComponent<ARAnchor>();

        if (currentAnchorData != null)
        {
            currentAnchorData.anchorID = anchor.trackableId.ToString();
            // switch (currentAnchorData.anchorType)
            // {
            //     case AnchorType.Text:
            //         currentAnchorData.anchorData = obj.GetComponent<TextMeshPro>().text;
            //         break;
            //     case AnchorType.Image:
            //         // currentAnchorData.anchorData = obj.GetComponent<SpriteRenderer>().sprite.name;
            //         break;
            //     case AnchorType.Video:
            //         // currentAnchorData.anchorData = obj.GetComponent<VideoPlayer>().url;
            //         break;
            //     case AnchorType.Preset:
            //         // currentAnchorData.anchorData = obj.name;
            //         break;
            // }
            if (currentAnchorData.anchorType == AnchorType.Text)
            {
                currentAnchorData.anchorData = obj.GetComponent<TextMeshPro>().text;
            }

            anchorDataList.anchors.Add(currentAnchorData);
        }

        FinalizePlacedAnchor(anchor);
    }
    public void ConfirmCreateAnchor()
    {
        if (currentAnchorObject != null)
        {
            CreateAnchor(currentAnchorObject);
            SetCurrentAnchor(currentAnchor);
            currentAnchorObject = null;
            isPlacingContent = false;
            inputText.text = "";
        }
    }
    public void OnCreateTextButtonClicked()
    {
        isPlacingContent = true;

        GameObject textContent = new GameObject("Text");
        textContent.transform.SetParent(contentParent.transform);
        textContent.transform.localScale = new Vector3(1f, 1f, 1f);
        var text = textContent.AddComponent<TextMeshPro>();
        text.text = "";
        text.fontSize = 36;
        text.alignment = TextAlignmentOptions.Center;
        textContent.transform.position = Camera.main.transform.position + Camera.main.transform.forward * distanceFromCamera;
        // make the text face the camera
        textContent.transform.rotation = Quaternion.LookRotation(textContent.transform.position - Camera.main.transform.position);
        currentAnchorData = new AnchorData
        {
            anchorID = "",
            anchorType = AnchorType.Text,
            anchorData = ""
        };

        currentAnchorObject = textContent;
    }
    public void OnTextInputFieldChanged()
    {
        if (currentAnchorObject != null)
        {
            string text = inputText.text;
            currentAnchorObject.GetComponent<TextMeshPro>().text = text;
        }
    }
    public void UpdateContentTransform()
    {
        if (currentAnchorObject != null)
        {
            currentAnchorObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * distanceFromCamera;
            currentAnchorObject.transform.rotation = Quaternion.LookRotation(currentAnchorObject.transform.position - Camera.main.transform.position);
        }
    }
    public void ToggleSideUI()
    {
        sideUI.SetActive(!sideUI.activeSelf);
    }
    private void AdjustColliderSize(GameObject obj, BoxCollider boxCollider)
    {
        if (obj.GetComponent<TMP_Text>() != null) // obj is text
            return;
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (renderer is SpriteRenderer)
            {
                return;
            }
            else
            {
                // obj should be a 3D object
                boxCollider.center = renderer.bounds.center - obj.transform.position;
                boxCollider.size = renderer.bounds.size;
            }
        }
        else
        {
            // obj is a UI element
            // make the collider size equal to the recttransform's width and height
            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                boxCollider.size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, 0.1f);
            }
        }
    }
    private void LoadPresets()
    {
        // load presets from Resources/Presets folder
        contents = Resources.LoadAll<GameObject>("Presets");
        // populate the dropdown with the names of the presets
        List<string> presetNames = new List<string>();
        foreach (var content in contents)
        {
            presetNames.Add(content.name);
        }
        presetsDropdown.ClearOptions();
        presetsDropdown.AddOptions(presetNames);
    }
    public void OnPresetDropdownSelected()
    {
        isPlacingContent = true;

        GameObject preset = Instantiate(contents[presetsDropdown.value]);
        preset.transform.SetParent(contentParent.transform);
        preset.transform.localScale = new Vector3(1f, 1f, 1f);

        preset.transform.position = Camera.main.transform.position + Camera.main.transform.forward * distanceFromCamera;
        preset.transform.rotation = Quaternion.LookRotation(preset.transform.position - Camera.main.transform.position);

        currentAnchorData = new AnchorData
        {
            anchorID = "",
            anchorType = AnchorType.Preset,
            anchorData = preset.name
        };

        currentAnchorObject = preset;
    }
    public void OnCreatePhotoButtonClicked()
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                // Create a new texture from the selected image
                Texture2D texture = NativeGallery.LoadImageAtPath(path, 1024, false, false);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                if (texture == null)
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }

                isPlacingContent = true;
                // Create a new GameObject with a renderer component
                GameObject imageContent = new GameObject("Image");
                imageContent.transform.SetParent(contentParent.transform);
                imageContent.transform.localScale = new Vector3(3f, 3f, 3f);
                var renderer = imageContent.AddComponent<SpriteRenderer>();
                // renderer.material = new Material(Shader.Find("Standard"));
                renderer.sprite = sprite;

                imageContent.transform.position = Camera.main.transform.position + Camera.main.transform.forward * distanceFromCamera;
                imageContent.transform.rotation = Quaternion.LookRotation(imageContent.transform.position - Camera.main.transform.position);

                currentAnchorData = new AnchorData
                {
                    anchorID = "",
                    anchorType = AnchorType.Media,
                    anchorData = path
                };

                currentAnchorObject = imageContent;
            }
        }, "Select a PNG image", "image/png");
    }
    public void OnCreateVideoButtonClicked()
    {
        NativeGallery.Permission permission = NativeGallery.GetVideoFromGallery((path) =>
        {
            if (path != null)
            {
                isPlacingContent = true;
                // Create a new GameObject with a video player component
                GameObject videoContent = new GameObject("Video");
                videoContent.transform.SetParent(contentParent.transform);
                videoContent.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);

                // Add VideoPlayer component
                var videoPlayer = videoContent.AddComponent<VideoPlayer>();
                videoPlayer.url = path;
                videoPlayer.isLooping = true;

                // Create a RawImage for displaying the video
                GameObject videoDisplay = new GameObject("VideoDisplay");
                videoDisplay.transform.SetParent(videoContent.transform);
                videoDisplay.transform.localPosition = Vector3.zero;
                videoDisplay.transform.localScale = new Vector3(1f, 1f, 1f);

                RawImage rawImage = videoDisplay.AddComponent<RawImage>();

                // Set the VideoPlayer's target texture to a RenderTexture
                RenderTexture renderTexture = new RenderTexture(1920, 1080, 0); // Initial size, will be adjusted
                videoPlayer.targetTexture = renderTexture;
                rawImage.texture = renderTexture;

                // Adjust the size of RawImage based on the video resolution
                videoPlayer.prepareCompleted += (VideoPlayer vp) =>
                {
                    // Get the video resolution
                    int videoWidth = (int)vp.width;
                    int videoHeight = (int)vp.height;

                    // Adjust the RenderTexture and RawImage size
                    renderTexture.Release();
                    renderTexture.width = videoWidth;
                    renderTexture.height = videoHeight;
                    renderTexture.Create();

                    rawImage.rectTransform.sizeDelta = new Vector2(videoWidth, videoHeight);
                };

                // Prepare the video player to trigger the prepareCompleted event
                videoPlayer.Prepare();

                // Set the position and rotation of the videoContent
                videoContent.transform.position = Camera.main.transform.position + Camera.main.transform.forward * distanceFromCamera;
                videoContent.transform.rotation = Quaternion.LookRotation(videoContent.transform.position - Camera.main.transform.position);

                currentAnchorData = new AnchorData
                {
                    anchorID = "",
                    anchorType = AnchorType.Media,
                    anchorData = path
                };

                currentAnchorObject = videoDisplay;
            }
        }, "Select a video", "mp4");
    }
}