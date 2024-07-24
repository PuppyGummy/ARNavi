using System;
using System.Collections.Generic;
using UnityEngine;
public enum AnchorType
{
    Text,
    Media,
    Preset
}

[Serializable]
public class MapData
{
    // public List<Target> targets;
    public List<Floor> floors;
    public List<Target> recenterTargets;
}
[Serializable]
public class Target
{
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

[Serializable]
public class AnchorData
{
    public string anchorID;
    public AnchorType anchorType;
    public string anchorData;
}

[Serializable]
public class AnchorDataList
{
    public List<AnchorData> anchors = new List<AnchorData>();
}