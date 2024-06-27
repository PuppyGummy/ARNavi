using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class SearchControl : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown targetListDropdown;
    [SerializeField] private TMP_Dropdown floorListDropdown;
    [SerializeField] private TMP_InputField userSearch;



    //Probably better to make this a 2d list but just for testing for now
    private List<string> targetListFloor1 = new List<string>() {
        "Project Room 2",
        "Project Room 3",
        "Classroom A",
        "Classroom B"
    };

    private List<string> targetListFloor2 = new List<string>() {
        "Window",
        "Bathroom",
        "Kitchen"
    };

    //when floor is changed update to only include options on that floor
    //initially floor 1 is selected by default
    //check the text input when floor is changed, filter by floor, then by text
    //when text is added, reset all dropdown options, then populate with ones that are similar to the text input
    public void TextInputOnChange()
    {
        //get appropriate list based on value of floor dropdown
        List<string> entriesByFloor = new List<string>();
        //if on floor 1
        if(floorListDropdown.value == 0) {
            entriesByFloor = targetListFloor1;
        } 
        //if on floor 2
        else if(floorListDropdown.value == 1) {
            entriesByFloor = targetListFloor2;
        }   
        //get appropriate sublist based on similarity to text input
        string input = userSearch.text;
        List<string> filteredEntries = entriesByFloor.Where(s => s.ToLower().StartsWith(input.ToLower())).ToList();
        //populate target dropdown with sublist
        targetListDropdown.ClearOptions();
        List<string> finalTargets = new List<string>();
        foreach (string target in filteredEntries)
        {
            finalTargets.Add(target);
        }
        targetListDropdown.AddOptions(finalTargets);
        targetListDropdown.Show();

    }

    public void OnTargetChoice() {
        //get the text of the choice chosen in dropdown and changes the current search to match it

        userSearch.text = targetListDropdown.captionText.text;
    }

}
