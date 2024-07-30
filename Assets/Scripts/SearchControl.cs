using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class SearchControl : MonoBehaviour
{
    [SerializeField] private GameObject searchResultsHolder;
    [SerializeField] private TMP_Dropdown targetListDropdown;
    [SerializeField] private TMP_Dropdown floorListDropdown;
    [SerializeField] private TMP_InputField userSearch;
    [SerializeField] private GameObject searchAutofillHolder;
    [SerializeField] private GameObject searchAutofillScrollview;

    [SerializeField] private Toggle funToggle;
    [SerializeField] private Toggle amenitiesToggle;
    [SerializeField] private Toggle medToggle;
    [SerializeField] private Toggle searchAutofillToggle;

    public GameObject searchResultPrefab;
    public GameObject searchAutofillPrefab;

    private static MapData targetList;
    private List<Target> targets;
    private List<Floor> floors;

    private static Target currentTarget;
    private static Floor currentFloor;

    private static bool fromExplore;

    public void Awake()
    {
        targetList = SaveLoadManager.LoadMap();
    }

    public void Start()
    {
        floors = targetList.floors;
        //the first floor is chosen by default
        targets = targetList.floors[0].targetsOnFloor;
        currentFloor = floors[0];
        currentTarget = targets[0];
        userSearch.text = "";
        FillFloorDropdown();
        fromExplore = false;
        FillTargetDropdown();
        InstantiateSearchResults();
        InstantiateSearchAutofill();
    }
    private void InstantiateSearchResults()
    {
        foreach (Transform child in searchResultsHolder.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        foreach (Target target in targets)
        {
            GameObject newButton = Instantiate(searchResultPrefab, searchResultsHolder.transform);
            newButton.GetComponent<Button>().onClick.AddListener(() => SearchResultOnClick());
            newButton.name = target.targetName + "-" + target.targetId;
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = target.targetName;
            newButton.transform.Find("Description").gameObject.GetComponent<TextMeshProUGUI>().text = target.addressInfo;
            newButton.transform.Find("SearchImage").gameObject.GetComponent<Image>().sprite = SaveLoadManager.LoadSearchImage(target.imgPath);

        }
    }

    public void OnToggleChange()
    {
        List<string> entriesByFloor = new List<string>();
        //get targets of current floor
        List<Target> targetsByFloor = targetList.floors[floorListDropdown.value].targetsOnFloor;
        foreach (Target target in targetsByFloor)
        {
            entriesByFloor.Add(target.targetName);
        }

        //get appropriate sublist based on similarity to text input
        string input = userSearch.text;

        List<Target> newTargets = targetsByFloor.Where(s => s.targetName.ToLower().StartsWith(input.ToLower())).ToList();

        List<Target> filteredTargets = new List<Target>();

        foreach (Target target in newTargets)
        {
            if (funToggle.isOn && target.tag == "Fun")
            {
                filteredTargets.Add(target);
            }
            if (medToggle.isOn && target.tag == "Medical")
            {
                filteredTargets.Add(target);
            }
            if (amenitiesToggle.isOn && target.tag == "Amenities")
            {
                filteredTargets.Add(target);
            }
        }

        //if no filters are on, just display all targets on the current floor
        if (!funToggle.isOn && !medToggle.isOn && !amenitiesToggle.isOn)
        {
            foreach (Target target in newTargets)
            {
                filteredTargets.Add(target);
            }
        }

        targets = filteredTargets;
        foreach (Target target in targets)
        {
            Debug.Log(target.targetName);
        }

        FillTargetDropdown();

        InstantiateSearchResults();
        InstantiateSearchAutofill();
    }


    //when floor is changed update to only include options on that floor
    //initially floor 1 is selected by default
    //check the text input when floor is changed, filter by floor, then by text
    //when text is added, reset all dropdown options, then populate with ones that are similar to the text input

    //clear all options in the target dropdownlist, then populate
    private void FillTargetDropdown()
    {
        targetListDropdown.ClearOptions();
        List<string> targetNames = new List<string>();
        targetNames.Add(userSearch.text + "...");

        foreach (Target target in targets)
        {
            targetNames.Add(target.targetName);
        }
        targetListDropdown.AddOptions(targetNames);
    }

    //clear all the options in the floor list dropdown, then populate
    private void FillFloorDropdown()
    {
        floorListDropdown.ClearOptions();
        List<string> floorNames = new List<string>();
        foreach (Floor floor in floors)
        {
            floorNames.Add(floor.floorName);
        }
        floorListDropdown.AddOptions(floorNames);
    }

    public void TextInputOnChange()
    {
        //get appropriate list based on value of floor dropdown
        List<string> entriesByFloor = new List<string>();
        //get targets of current floor
        List<Target> targetsByFloor = targetList.floors[floorListDropdown.value].targetsOnFloor;
        foreach (Target target in targetsByFloor)
        {
            entriesByFloor.Add(target.targetName);
        }

        //get appropriate sublist based on similarity to text input
        string input = userSearch.text;

        List<Target> newTargets = targetsByFloor.Where(s => s.targetName.ToLower().StartsWith(input.ToLower())).ToList();

        targets = newTargets;
        searchAutofillToggle.isOn = true;
        ToggleAutofill();
        OnToggleChange();
        InstantiateSearchAutofill();
        InstantiateSearchResults();
    }

    public void OnTargetChoice()
    {
        //get the text of the choice chosen in dropdown and changes the current search to match 
        userSearch.text = targetListDropdown.captionText.text;
        //change currentTarget to match
        foreach (Target target in targets)
        {
            if (target.targetName == targetListDropdown.captionText.text)
            {
                currentTarget = target;
            }
        }
    }

    public void OnFloorSelect()
    {
        //set the currentFloor to the selected floor
        currentFloor = floors[floorListDropdown.value];
        //set the targets to the current floor's targets
        targets = currentFloor.targetsOnFloor;
        //check the filters
        OnToggleChange();
        TextInputOnChange();
        InstantiateSearchAutofill();
        InstantiateSearchResults();
    }

    public static Target GetCurrentTarget()
    {
        return currentTarget;
    }

    public static Floor GetCurrentFloor()
    {
        return currentFloor;
    }

    public static bool GetFromExplore()
    {
        return fromExplore;
    }

    //test function to get which button was clicked
    public void SearchResultOnClick()
    {
        string buttonName = EventSystem.current.currentSelectedGameObject.name;
        Scene scene = SceneManager.GetActiveScene();
        if (scene.buildIndex == 6)
        {
            fromExplore = true;
        }
        else
        {
            fromExplore = false;
        }
        //loop through the floors and set currentFloor and target according to the button that was pressed
        foreach (Floor floor in floors)
        {
            foreach (Target target in floor.targetsOnFloor)
            {
                if (target.targetName + "-" + target.targetId == buttonName)
                {
                    currentTarget = target;
                    currentFloor = floor;
                    break;
                }
            }
        }
        //go to the navigation page after setting the floor and target
        NavigationButtons.ToConfirmDestination();
    }
    //Toggle the search autofill options on/off
    public void ToggleAutofill()
    {
        searchAutofillScrollview.SetActive(searchAutofillToggle.isOn);
        //if the dropdown was turned off delete all children from it
        if (!searchAutofillToggle.isOn)
        {
            foreach (Transform child in searchAutofillHolder.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        //otherwise, insantiate all the children again (done so that the scroll view resets after it is closed and opened again)
        else
        {
            InstantiateSearchAutofill();
        }
    }
    //Create search result buttons
    public void InstantiateSearchAutofill()
    {
        //clear all previous autofilled results
        foreach (Transform child in searchAutofillHolder.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        //instantiate new results
        foreach (Target target in targets)
        {
            GameObject newButton = Instantiate(searchAutofillPrefab, searchAutofillHolder.transform);
            newButton.GetComponent<Button>().onClick.AddListener(() => SearchResultAutofillOnClick());
            newButton.name = target.targetName;
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = target.targetName;
        }
    }
    //Change text input to match autofill result, close dropdown 
    public void SearchResultAutofillOnClick()
    {
        string selection = EventSystem.current.currentSelectedGameObject.name;
        userSearch.text = selection;
    }
}
