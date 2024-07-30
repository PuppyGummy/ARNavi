using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;
using UnityEngine.UI;
using Unity.VisualScripting;

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
    [SerializeField] private ARCameraManager arCameraManager;
    [SerializeField] private Button wallToggleButton;
    [SerializeField] private GameObject wallParent;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material occlusionMaterial;
    [SerializeField] private GameObject minimap;
    public Slider lineYOffsetSlider;
    public GameObject userIndicator;
    private bool wallToggle = false;
    public NavMeshPath path { get; private set; }
    public GameObject currentTarget;
    public GameObject currentFloor { get; private set; }
    [SerializeField] private GameObject lineVisualization;
    [SerializeField] private GameObject arrowVisualization;
    [SerializeField] private GameObject arrivalIndicationPanel;
    [SerializeField] private float arrivalDistance = 1.5f;
    private bool arrivedFlag = false;

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
        // GameObject.DontDestroyOnLoad(this.gameObject);

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
        SetMaterial(wallParent.transform, occlusionMaterial);
        lineVisualization.SetActive(false);
        arrowVisualization.SetActive(true);

        FillFloorDropdown();
        FillTargetDropdown();

        //set the values chosen in the main menu screen
        SetFloorInitial();
        SetTargetInitial();
    }
    private void Update()
    {
        // UpdateUserRotation();
        if (targetPosition != Vector3.zero)
        {
            NavMesh.CalculatePath(userIndicator.transform.position, targetPosition, NavMesh.AllAreas, path);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveCurrentMap();
        }
        HasArrived();
    }
    //after getting to navigation screen from the selection page, change the target to reflect the choice
    public void SetTargetInitial()
    {
        //get current target set in main menu page
        Target tarSelect = SearchControl.GetCurrentTarget();
        if (tarSelect == null)
        {
            targetListDropdown.value = 0;
        }
        else
        {
            //iterate through all options in the targetlist
            for (int i = 0; i < targetListDropdown.options.Count; i++)
            {
                //if the name matches, set the dropdown value to the corresponding value
                if (targetListDropdown.options[i].text == tarSelect.targetName)
                {
                    targetListDropdown.value = i;
                    break;
                }
            }
        }
        //set current navigation target accrording to the newly set dropdown
        SetCurrentNavigationTarget();
    }

    //after getting to the navigation screen from the selection page, change the floor to reflect the choice
    public void SetFloorInitial()
    {
        //clear the targetlist and targetposition
        targetList.Clear();
        targetPosition = Vector3.zero;
        //get the floor that was selected in the main menu screen
        Floor floorSelection = SearchControl.GetCurrentFloor();
        if (floorSelection == null)
        {
            floorListDropdown.value = 0;
        }
        else
        {
            //iterate through the list of floors
            for (int i = 0; i < floorListDropdown.options.Count; i++)
            {
                //check if name of option matching the selected floor and set accordingly
                if (floorListDropdown.options[i].text == floorSelection.floorName)
                {
                    floorListDropdown.value = i;
                    break;
                }
            }
        }

        //set current floor according to the newly set dropdown
        SetCurrentFloor();
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
            floors = new List<Floor>(),
            recenterTargets = new List<Target>()
        };

        //iterate through all floors
        int id = 0;
        foreach (var floor in floorList)
        {
            //create a newFloor with the name of the current floor iteration
            Floor newFloor = new Floor
            {
                floorName = floor.name,
                targetsOnFloor = new List<Target>()
            };
            //for each child (target) of the floor, create a corresponding target and add it to the floor
            for (int i = 0; i < floor.transform.childCount; i++)
            {
                var myTarget = floor.transform.GetChild(i);
                Target targetToAdd = new Target
                {

                    targetName = myTarget.name,
                    targetPosition = myTarget.transform.position,
                    addressInfo = Variables.Object(myTarget).Get<string>("description"),
                    tag = myTarget.tag,
                    targetId = id,
                    imgPath = myTarget.name + "-" + id
                };
                id++;
                newFloor.targetsOnFloor.Add(targetToAdd);

            }
            //add the new floor to the MapDa
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
    public void ToggleMinimap()
    {
        minimap.SetActive(!minimap.activeSelf);
    }
    private void HasArrived()
    {
        if (targetPosition == Vector3.zero)
        {
            return;
        }
        if (Vector3.Distance(userIndicator.transform.position, targetPosition) < arrivalDistance)
        {
            if (!arrivedFlag)
            {
                arrivedFlag = true;
                arrivalIndicationPanel.SetActive(true);
            }
        }
        else
        {
            arrivedFlag = false;
        }
    }
    public void HideNavigationTarget(bool canEditAnchors)
    {
        if (canEditAnchors)
        {
            if (currentTarget != null)
            {
                currentTarget.GetComponent<MeshRenderer>().enabled = false;
                arrowVisualization.SetActive(false);
            }
        }
        else
        {
            if (currentTarget != null)
            {
                currentTarget.GetComponent<MeshRenderer>().enabled = true;
                arrowVisualization.SetActive(true);
            }
        }
    }
}