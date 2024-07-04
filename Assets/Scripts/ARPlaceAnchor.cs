using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARPlaceAnchor : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The enabled Anchor Manager in the scene.")]
    ARAnchorManager m_AnchorManager;

    [SerializeField]
    [Tooltip("The prefab to be instantiated for each anchor.")]
    GameObject m_Prefab;

    ARRaycastManager raycastManager;

    List<ARAnchor> m_Anchors = new();

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
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                if (raycastManager.Raycast(touch.position, hits))
                {
                    CreateAnchor(hits[0]);
                }
            }
        }
    }

    void CreateAnchor(ARRaycastHit arRaycastHit)
    {
        ARAnchor anchor;

        // If we hit a plane, try to "attach" the anchor to the plane
        if (m_AnchorManager.descriptor.supportsTrackableAttachments && arRaycastHit.trackable is ARPlane plane)
        {
            if (m_Prefab != null)
            {
                var oldPrefab = m_AnchorManager.anchorPrefab;
                m_AnchorManager.anchorPrefab = m_Prefab;
                anchor = m_AnchorManager.AttachAnchor(plane, arRaycastHit.pose);
                m_AnchorManager.anchorPrefab = oldPrefab;
            }
            else
            {
                anchor = m_AnchorManager.AttachAnchor(plane, arRaycastHit.pose);
            }

            // FinalizePlacedAnchor(anchor, $"Attached to plane {plane.trackableId}");
            m_Anchors.Add(anchor);
            return;
        }

        // Otherwise, just create a regular anchor at the hit pose
        if (m_Prefab != null)
        {
            // Note: the anchor can be anywhere in the scene hierarchy
            var anchorPrefab = Instantiate(m_Prefab, arRaycastHit.pose.position, arRaycastHit.pose.rotation);
            anchor = ComponentUtils.GetOrAddIf<ARAnchor>(anchorPrefab, true);
        }
        else
        {
            var anchorPrefab = new GameObject("Anchor");
            anchorPrefab.transform.SetPositionAndRotation(arRaycastHit.pose.position, arRaycastHit.pose.rotation);
            anchor = anchorPrefab.AddComponent<ARAnchor>();
        }

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
        AnchorDataList anchorDataList = new AnchorDataList();
        foreach (var anchor in m_Anchors)
        {
            AnchorData anchorData = new AnchorData
            {
                position = new float[] { anchor.transform.position.x, anchor.transform.position.y, anchor.transform.position.z },
                rotation = new float[] { anchor.transform.rotation.x, anchor.transform.rotation.y, anchor.transform.rotation.z, anchor.transform.rotation.w }
            };
            anchorDataList.anchors.Add(anchorData);
        }
        SaveLoadManager.SaveAnchors(anchorDataList);
    }
    public void LoadAnchors()
    {
        AnchorDataList anchorDataList = SaveLoadManager.LoadAnchors();
        if (anchorDataList != null)
        {
            foreach (var anchorData in anchorDataList.anchors)
            {
                ARAnchor anchor;

                if (m_Prefab != null)
                {
                    var anchorPrefab = Instantiate(m_Prefab, new Vector3(anchorData.position[0], anchorData.position[1], anchorData.position[2]), new Quaternion(anchorData.rotation[0], anchorData.rotation[1], anchorData.rotation[2], anchorData.rotation[3]));
                    anchor = ComponentUtils.GetOrAddIf<ARAnchor>(anchorPrefab, true);
                }
                else
                {
                    var anchorPrefab = new GameObject("Anchor");
                    anchorPrefab.transform.SetPositionAndRotation(new Vector3(anchorData.position[0], anchorData.position[1], anchorData.position[2]), new Quaternion(anchorData.rotation[0], anchorData.rotation[1], anchorData.rotation[2], anchorData.rotation[3]));
                    anchor = anchorPrefab.AddComponent<ARAnchor>();
                }

                m_Anchors.Add(anchor);
            }
        }
    }
    public void HandleLoadedAnchors()
    {
        foreach (var anchor in anchorManager.trackables)
        {
            Debug.Log($"Anchor loaded: {anchor.trackableId}");
            Instantiate(m_Prefab, anchor.transform.position, anchor.transform.rotation);
        }
    }
}