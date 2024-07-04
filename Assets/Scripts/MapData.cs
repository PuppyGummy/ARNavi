using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapData
{
    //public List<Target> targets;
    public List<Floor> floors;
    public List<Target> recenterTargets;
}
[Serializable]
public class Target
{
    //public int targetId;
    public string targetName;
    public Vector3 targetPosition;
    public string tag;
   
    //string addressInfo;?
    //string imagepath;?
    //idk about imagepath it might be easier to just do
}
[Serializable]
public class Floor
{
    public string floorName;
    public List<Target> targetsOnFloor;
}



// [Serializable]
// public class RecenterTarget
// {
//     public string recenterTargetName;
//     public Vector3 recenterTargetPosition;
// }