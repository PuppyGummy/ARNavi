using UnityEngine;
using System.IO;

public class SaveLoadManager : MonoBehaviour
{
    // Set save path to resources folder
    public static string SavePath => Path.Combine(Application.dataPath, "Resources", "map_data.json");

    public static void SaveMap(MapData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Map saved to {SavePath}");
    }

    public static MapData LoadMap()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<MapData>(json);
        }
        Debug.LogError("Map file not found");
        return null;
    }
}
