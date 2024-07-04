using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapData
{
    public List<Target> targets;
    public List<Floor> floors;
    public List<Target> recenterTargets;
}
[Serializable]
public class Target
{
    public string targetName;
    public Vector3 targetPosition;
}
[Serializable]
public class Floor
{
    public string floorName;
}

[Serializable]
public class AnchorData
{
    public float[] position;
    public float[] rotation;
}

[Serializable]
public class AnchorDataList
{
    public List<AnchorData> anchors = new List<AnchorData>();
}