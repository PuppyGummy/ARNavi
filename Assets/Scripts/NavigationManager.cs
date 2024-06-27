using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Niantic.Experimental.Lightship.AR.WorldPositioning;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;
using UnityEngine.UI;

public class NavigationManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown targetListDropdown;
    [SerializeField] private TMP_Dropdown floorListDropdown;
    [SerializeField] private GameObject floorsParent;
    private List<GameObject> targetList = new List<GameObject>();
    private List<GameObject> floorList = new List<GameObject>();
    [SerializeField] private Camera topDownCamera;
    public Vector3 targetPosition = Vector3.zero;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private XROrigin sessionOrigin;
    private ARWorldPositioningCameraHelper cameraHelper;
    [SerializeField] private ARCameraManager arCameraManager;
    [SerializeField] private ARWorldPositioningManager wpsManager;
    [SerializeField] private Button wallToggleButton;
    [SerializeField] private GameObject wallParent;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material occlusionMaterial;
    public Slider lineYOffsetSlider;
    public GameObject userIndicator;
    private bool wallToggle = false;
    public NavMeshPath path { get; private set; }
    private GameObject currentTarget;
    public GameObject currentFloor { get; private set; }
    [SerializeField] private GameObject lineVisualization;
    [SerializeField] private GameObject arrowVisualization;

    public static NavigationManager Instance;
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        GameObject.DontDestroyOnLoad(this.gameObject);

        // Fill floor list with the children of the floors parent
        foreach (Transform floor in floorsParent.transform)
        {
            floorList.Add(floor.gameObject);
        }

        // Set the current floor to the first floor
        currentFloor = floorList[0];

        // Fill target list with the children of the targets parent
        foreach (Transform target in currentFloor.transform)
        {
            targetList.Add(target.gameObject);
        }
    }
    private void Start()
    {
        path = new NavMeshPath();
        cameraHelper = arCameraManager.GetComponent<ARWorldPositioningCameraHelper>();
        SetMaterial(wallParent.transform, occlusionMaterial);
        lineVisualization.SetActive(true);
        arrowVisualization.SetActive(false);

        FillFloorDropdown();
        FillTargetDropdown();
    }
    private void Update()
    {
        // statusText.text = "WPS: " + wpsManager.Status.ToString();
        // UpdateUserRotation();
        // WorldPositionUpdate();
        if (targetPosition != Vector3.zero)
        {
            NavMesh.CalculatePath(userIndicator.transform.position, targetPosition, NavMesh.AllAreas, path);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveCurrentMap();
        }
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
    private void WorldPositionUpdate()
    {
        float heading = cameraHelper.TrueHeading;
        userIndicator.transform.rotation = Quaternion.Euler(0, heading, 0);
    }
    public void SetCurrentFloor()
    {
        targetList.Clear();
        currentTarget = null;
        targetPosition = Vector3.zero;
        currentFloor = floorList[floorListDropdown.value];
        foreach (Transform target in currentFloor.transform)
        {
            targetList.Add(target.gameObject);
        }
        FillTargetDropdown();
    }
    private void UpdateUserRotation()
    {
        // Set the rotation according to the AR camera's rotation
        Quaternion originalRotation = userIndicator.transform.rotation;
        userIndicator.transform.rotation = new Quaternion(originalRotation.x, -arCameraManager.transform.rotation.y, originalRotation.z, originalRotation.w);
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
    private void FillFloorDropdown()
    {
        floorListDropdown.ClearOptions();
        List<string> floorNames = new List<string>();
        foreach (Transform floor in floorsParent.transform)
        {
            floorNames.Add(floor.name);
        }
        floorListDropdown.AddOptions(floorNames);
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
    public void SwitchVisual()
    {
        lineVisualization.SetActive(!lineVisualization.activeSelf);
        arrowVisualization.SetActive(!arrowVisualization.activeSelf);
    }
    private void SaveCurrentMap()
    {
        MapData data = new MapData
        {
            targets = new List<Target>(),
            floors = new List<Floor>(),
            recenterTargets = new List<Target>()
        };
        foreach (var target in targetList)
        {
            Target newTarget = new Target
            {
                targetName = target.name,
                targetPosition = target.transform.position
            };
            data.targets.Add(newTarget);
        }
        foreach (var floor in floorList)
        {
            Floor newFloor = new Floor
            {
                floorName = floor.name
            };
            data.floors.Add(newFloor);
        }
        foreach (var recenterTarget in RecenterHelper.Instance.recenterTargetList)
        {
            Target newRecenterTarget = new Target
            {
                targetName = recenterTarget.name,
                targetPosition = recenterTarget.transform.position
            };
            data.recenterTargets.Add(newRecenterTarget);
        }
        SaveLoadManager.SaveMap(data);
    }
}