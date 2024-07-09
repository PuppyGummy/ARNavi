using UnityEngine;
using System.IO;

public class SaveLoadManager : MonoBehaviour
{
    // Set save path to resources folder
    public static string MapSavePath => Path.Combine(Application.dataPath, "Resources", "map_data.json");
    public static string AnchorSavePath => Path.Combine(Application.persistentDataPath, "anchors.json");

    public static void SaveMap(MapData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(MapSavePath, json);
        Debug.Log($"Map saved to {MapSavePath}");
    }

    public static MapData LoadMap()
    {
        //if (File.Exists(MapSavePath))
        //{
        //    string json = File.ReadAllText(MapSavePath);
        //    return JsonUtility.FromJson<MapData>(json);
        //}
        var json = Resources.Load<TextAsset>("/map_data");
        //return JsonUtility.FromJson<MapData>(json.text);
        Debug.Log(json.text);
        return null;
        //Debug.LogError("Map file not found");
        
    }
    public static void SaveAnchors(AnchorDataList anchorDataList)
    {
        string json = JsonUtility.ToJson(anchorDataList);
        File.WriteAllText(AnchorSavePath, json);
    }
    public static AnchorDataList LoadAnchors()
    {
        if (File.Exists(AnchorSavePath))
        {
            string json = File.ReadAllText(AnchorSavePath);
            return JsonUtility.FromJson<AnchorDataList>(json);
        }
        Debug.LogError("Anchors file not found");
        return null;
    }
}
