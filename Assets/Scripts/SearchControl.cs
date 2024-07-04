using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//use a serialized field for the container (content holder) of the scroll view and get its children (the results we instantiate at runtime)
//TODO:
/*
 talk to Lan about iOS building
    also about mascot on/off toggle
 */
public class SearchControl : MonoBehaviour
{
    [SerializeField] private GameObject searchResultsHolder;
    [SerializeField] private TMP_Dropdown targetListDropdown;
    [SerializeField] private TMP_Dropdown floorListDropdown;
    [SerializeField] private TMP_InputField userSearch;




    public GameObject searchResultPrefab; 



    private static MapData targetList;
    private List<Target> targets;
    private List<Floor> floors;

    private static Target currentTarget;
    private static Floor currentFloor;

   
    public void Awake()
    {
        

      
     
    }

    public void Start()
    {
        
        targetList = SaveLoadManager.LoadMap();
        floors = targetList.floors;
        //the first floor is chosen by default
        targets = targetList.floors[0].targetsOnFloor;
        currentFloor = floors[0];
        currentTarget = targets[0];
        userSearch.text = currentTarget.targetName;
        FillFloorDropdown();
        FillTargetDropdown();

        InstantiateSearchResults();
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
            newButton.name = target.targetName;
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = target.targetName;

            
        }
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
        foreach(Floor floor in floors)
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
        foreach(Target target in targetsByFloor)
        {
            entriesByFloor.Add(target.targetName);
        }
           
        //get appropriate sublist based on similarity to text input
        string input = userSearch.text;
        //List<string> filteredEntries = entriesByFloor.Where(s => s.ToLower().StartsWith(input.ToLower())).ToList();
       

        List<Target> newTargets =  targetsByFloor.Where(s => s.targetName.ToLower().StartsWith(input.ToLower())).ToList();
        targets = newTargets;
        FillTargetDropdown();

        
        targetListDropdown.Show();
        InstantiateSearchResults();

    }

    public void OnTargetChoice()
    {
        //get the text of the choice chosen in dropdown and changes the current search to match 
        userSearch.text = targetListDropdown.captionText.text;
        //change currentTarget to match
        foreach(Target target in targets)
        {
            if (target.targetName == targetListDropdown.captionText.text)
            {
                currentTarget = target;
            }
        }
    }

   
    public void OnFloorSelect() {
        //set the currentFloor to the selected floor
        currentFloor = floors[floorListDropdown.value];
        //set the targets to the current floor's targets
        targets = currentFloor.targetsOnFloor;
        FillTargetDropdown();
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
    

    //test function to get which button was clicked
    public void SearchResultOnClick()
    {
        string buttonName = EventSystem.current.currentSelectedGameObject.name;
        //loop through the floors and set currentFloor and target according to the button that was pressed :)
        foreach(Floor floor in floors)
        {
            foreach(Target target in floor.targetsOnFloor)
            {
                if(target.targetName == buttonName)
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

    //Create search result buttons
    /*
    public void CreateSearchResults()
    {
        create buttons
            can make a button prefab with the sprite and spawn them in in the viewport
        should be children of the viewport (under the scroll view)
        sprite?
    }
     */
   
}
