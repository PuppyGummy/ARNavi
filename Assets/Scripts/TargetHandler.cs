using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TargetHandler : MonoBehaviour
{
    [SerializeField] private List<Target> targetList = new List<Target>();
    [SerializeField] private TMP_Dropdown targetListDropdown;
    // Start is called before the first frame update
    void Start()
    {
        FillTargetDropdown();
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void FillTargetDropdown()
    {
        targetListDropdown.ClearOptions();
        List<string> targetNames = new List<string>();
        foreach (var target in targetList)
        {
            targetNames.Add(target.targetName);
        }
        targetListDropdown.AddOptions(targetNames);
    }
}
