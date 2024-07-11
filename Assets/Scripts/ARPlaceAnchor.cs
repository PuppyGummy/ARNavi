using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;
using UnityEngine.EventSystems;

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
        GameObject.DontDestroyOnLoad(this.gameObject);
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
        if (Input.touchCount > 0 && canPlaceAnchors)
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
                    CreateAnchor(hits[0]);
                }
            }
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

    void CreateAnchor(ARRaycastHit arRaycastHit)
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

            // FinalizePlacedAnchor(anchor, $"Attached to plane {plane.trackableId}");
            anchor.transform.SetParent(contentParent.transform);
            m_Anchors.Add(anchor);
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
        anchor.transform.SetParent(contentParent.transform);
        // FinalizePlacedAnchor(anchor, $"Anchor (from {arRaycastHit.hitType})");
        m_Anchors.Add(anchor);
    }

    // void FinalizePlacedAnchor(ARAnchor anchor, string text)
    // {
    //     // var canvasTextManager = anchor.GetComponent<CanvasTextManager>();
    //     // if (canvasTextManager != null)
    //     // {
    //     //     canvasTextManager.text = text;
    //     // }
    //     m_Anchors.Add(anchor);
    // }
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
        ARWorldMapController.Instance.TogglePlaceAnchorsUI(canPlaceAnchors);
    }
}