using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System;
using System.Collections;

public class SaveLoadManager : MonoBehaviour
{
    // Set save path to resources folder
    public static string MapSavePath => Path.Combine(Application.dataPath, "Resources", "map_data.json");
    public static string AnchorSavePath => Path.Combine(Application.persistentDataPath, "anchors.json");
    public static string MediaSavePath => Path.Combine(Application.persistentDataPath, "media");

    public static void SaveMap(MapData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(MapSavePath, json);
        Debug.Log($"Map saved to {MapSavePath}");
    }

    public static MapData LoadMap()
    {
        TextAsset jsonTextAsset = Resources.Load<TextAsset>("map_data");
        if (jsonTextAsset != null)
        {
            string json = jsonTextAsset.text;
            return JsonUtility.FromJson<MapData>(json);
        }
        Debug.LogError("Map file not found in Resources folder");
        return null;
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
    public static IEnumerator DownloadMedia(string downloadUrl, string savePath, System.Action<bool> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(downloadUrl))
        {
            // Send request and wait for a response
            yield return request.SendWebRequest();

            // Check for errors
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // Ensure the directory exists
                    string directoryPath = Path.GetDirectoryName(savePath);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    // Write the downloaded data to a file
                    File.WriteAllBytes(savePath, request.downloadHandler.data);
                    Debug.Log("Media downloaded and saved successfully to: " + savePath);
                    callback?.Invoke(true); // Invoke callback with success
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to write file: " + e.Message);
                    callback?.Invoke(false); // Invoke callback with failure
                }
            }
            else
            {
                Debug.LogError("Failed to download media: " + request.error);
                callback?.Invoke(false); // Invoke callback with failure
            }
        }
    }
    
}
