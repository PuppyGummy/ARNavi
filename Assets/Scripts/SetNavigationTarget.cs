using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Niantic.Experimental.Lightship.AR.WorldPositioning;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;
using UnityEngine.UI;

public class SetNavigationTarget : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown targetListDropdown;
    [SerializeField] private List<Target> targetList = new List<Target>();
    [SerializeField] private Camera topDownCamera;
    [SerializeField] private Vector3 targetPosition = Vector3.zero;
    [SerializeField] private TMP_Text statusText;
    // [SerializeField] private TMP_Text lineToggleText;
    [SerializeField] private XROrigin sessionOrigin;

    private NavMeshPath path;
    private LineRenderer line;
    private ARWorldPositioningCameraHelper cameraHelper;
    private GameObject currentTarget;
    [SerializeField] private ARCameraManager arCameraManager;
    [SerializeField] private ARWorldPositioningManager wpsManager;
    [SerializeField] private Button wallToggleButton;
    [SerializeField] private GameObject wallParent;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material occlusionMaterial;
    private bool wallToggle = false;

    // private bool lineToggle = false;
    // Start is called before the first frame update
    private void Start()
    {
        path = new NavMeshPath();
        line = GetComponent<LineRenderer>();
        // line.enabled = lineToggle;
        line.enabled = false;
        cameraHelper = arCameraManager.GetComponent<ARWorldPositioningCameraHelper>();
        SearchForMeshRenderer(wallParent.transform, occlusionMaterial);
    }

    // Update is called once per frame
    private void Update()
    {
        // if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
        // {
        //     lineToggle = !lineToggle;
        // }
        statusText.text = "WPS: " + wpsManager.Status.ToString();
        if (targetPosition != Vector3.zero)
        {
            WorldPositionUpdate();
            NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);
            line.positionCount = path.corners.Length;
            line.SetPositions(path.corners);
            line.enabled = true;
        }
        else
        {
            line.enabled = false;
        }
    }
    public void SetCurrentNavigationTarget()
    {
        // Disable the previous target
        if (currentTarget != null)
        {
            currentTarget.GetComponent<MeshRenderer>().enabled = false;
        }

        currentTarget = targetList[targetListDropdown.value].positionObj;
        currentTarget.GetComponent<MeshRenderer>().enabled = true;
        targetPosition = currentTarget.transform.position;
    }
    private void WorldPositionUpdate()
    {
        float heading = cameraHelper.TrueHeading;
        transform.rotation = Quaternion.Euler(0, heading, 0);
    }
    public void ToggleWall()
    {
        wallToggle = !wallToggle;
        if (wallToggle)
        {
            wallToggleButton.GetComponentInChildren<TMP_Text>().text = "Hide Wall";
            SearchForMeshRenderer(wallParent.transform, defaultMaterial);
        }
        else
        {
            wallToggleButton.GetComponentInChildren<TMP_Text>().text = "Show Wall";
            SearchForMeshRenderer(wallParent.transform, occlusionMaterial);
        }
    }
    private void SearchForMeshRenderer(Transform parent, Material material)
    {
        // Search for all the children of the wall parent with a mesh renderer component
        // if the child has a mesh renderer component, change the material to the default material
        // else search for the children of the child and repeat the process
        foreach (Transform child in parent)
        {
            child.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer);
            if (meshRenderer != null)
            {
                meshRenderer.material = material;
            }
            else
            {
                SearchForMeshRenderer(child, material);
            }
        }
    }
}