using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public string targetName;
    public GameObject positionObj;
    public Target(string targetName, GameObject positionObj)
    {
        this.targetName = targetName;
        this.positionObj = positionObj;
    }
    private void Start()
    {
        if (targetName == null || targetName == "")
        {
            targetName = gameObject.name;
        }
        if (positionObj == null)
        {
            positionObj = gameObject;
        }
    }
}
