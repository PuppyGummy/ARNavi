using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
// using Niantic.Experimental.Lightship.AR.WorldPositioning;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;
using UnityEngine.UI;

public class NavigationManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown targetListDropdown;
    [SerializeField] private GameObject targetsParent;
    [SerializeField] private List<GameObject> targetList = new List<GameObject>();
    [SerializeField] private Camera topDownCamera;
    [SerializeField] private Vector3 targetPosition = Vector3.zero;
    [SerializeField] private TMP_Text statusText;
    // [SerializeField] private TMP_Text lineToggleText;
    [SerializeField] private XROrigin sessionOrigin;
    [SerializeField] private LineRenderer line;
    // private ARWorldPositioningCameraHelper cameraHelper;
    [SerializeField] private ARCameraManager arCameraManager;
    // [SerializeField] private ARWorldPositioningManager wpsManager;
    [SerializeField] private Button wallToggleButton;
    [SerializeField] private GameObject wallParent;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material occlusionMaterial;
    [SerializeField] private Slider lineYOffsetSlider;
    [SerializeField] private GameObject userIndicator;
    private bool wallToggle = false;
    private NavMeshPath path;
    private GameObject currentTarget;

    // private bool lineToggle = false;
    // Start is called before the first frame update
    private void Start()
    {
        path = new NavMeshPath();
        // line = GetComponent<LineRenderer>();
        // line.enabled = lineToggle;
        line.enabled = false;
        // cameraHelper = arCameraManager.GetComponent<ARWorldPositioningCameraHelper>();
        SetMaterial(wallParent.transform, occlusionMaterial);
        // Fill target list with the children of the targets parent
        foreach (Transform target in targetsParent.transform)
        {
            targetList.Add(target.gameObject);
        }
    }
    private void OnGUI()
    {
        FillTargetDropdown();
    }

    // Update is called once per frame
    private void Update()
    {
        // if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
        // {
        //     lineToggle = !lineToggle;
        // }
        // statusText.text = "WPS: " + wpsManager.Status.ToString();
        UpdateUserRotation();
        if (targetPosition != Vector3.zero)
        {
            // WorldPositionUpdate();
            NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);
            line.positionCount = path.corners.Length;
            Vector3[] calculatedPosition = CalculateLineOffset();
            line.SetPositions(calculatedPosition);
            line.enabled = true;
        }
        else
        {
            line.enabled = false;
        }
    }
    private Vector3[] CalculateLineOffset()
    {
        if (lineYOffsetSlider.value == 0)
        {
            return path.corners;
        }
        Vector3[] calculatedPosition = new Vector3[path.corners.Length];
        for (int i = 0; i < path.corners.Length; i++)
        {
            calculatedPosition[i] = path.corners[i] + new Vector3(0, lineYOffsetSlider.value, 0);
        }
        return calculatedPosition;
    }
    public void SetLineYOffsetText()
    {
        lineYOffsetSlider.GetComponentInChildren<TMP_Text>().text = "Line Y Offset: " + lineYOffsetSlider.value.ToString();
    }
    public void SetCurrentNavigationTarget()
    {
        // Disable the previous target
        if (currentTarget != null)
        {
            currentTarget.GetComponent<MeshRenderer>().enabled = false;
        }

        currentTarget = targetList[targetListDropdown.value].gameObject;
        currentTarget.GetComponent<MeshRenderer>().enabled = true;
        targetPosition = currentTarget.transform.position;
    }
    // private void WorldPositionUpdate()
    // {
    //     float heading = cameraHelper.TrueHeading;
    //     transform.rotation = Quaternion.Euler(0, heading, 0);
    // }
    private void UpdateUserRotation()
    {
        // Set the rotation according to the AR camera's rotation
        userIndicator.transform.rotation = arCameraManager.transform.rotation;
    }
    public void ToggleWall()
    {
        wallToggle = !wallToggle;
        if (wallToggle)
        {
            wallToggleButton.GetComponentInChildren<TMP_Text>().text = "Hide Wall";
            SetMaterial(wallParent.transform, defaultMaterial);
        }
        else
        {
            wallToggleButton.GetComponentInChildren<TMP_Text>().text = "Show Wall";
            SetMaterial(wallParent.transform, occlusionMaterial);
        }
    }
    private void SetMaterial(Transform parent, Material material)
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
                SetMaterial(child, material);
            }
        }
    }
    private void FillTargetDropdown()
    {
        targetListDropdown.ClearOptions();
        List<string> targetNames = new List<string>();
        foreach (GameObject target in targetList)
        {
            targetNames.Add(target.name);
        }
        targetListDropdown.AddOptions(targetNames);
    }
}