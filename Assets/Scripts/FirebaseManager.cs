using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.Storage;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine.Networking;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseFirestore Firestore { get; private set; }
    public static FirebaseStorage Storage { get; private set; }
    public static StorageReference StorageReference { get; private set; }

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            Firestore = FirebaseFirestore.DefaultInstance;
            Storage = FirebaseStorage.DefaultInstance;
            StorageReference = Storage.GetReferenceFromUrl("gs://arnavi-89df3.appspot.com");
            Debug.Log("Firebase initialized successfully");
        });
    }
    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // randomize a number and upload to firebase for testing
            int random = UnityEngine.Random.Range(0, 100);
            Firestore.Collection("test").Document("random").SetAsync(new Dictionary<string, object>
            {
                { "randomNumber", random }
            }).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Random number uploaded to Firestore successfully.");
                }
                else
                {
                    Debug.LogError("Failed to upload random number to Firestore: " + task.Exception);
                }
            });
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // upload map_data.json to firebase storage for testing
            string fileName = Path.GetFileName(SaveLoadManager.MapSavePath);
            var storageRef = StorageReference.Child(fileName);
            storageRef.PutFileAsync(SaveLoadManager.MapSavePath).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Mapdata uploaded to Firebase successfully.");
                }
                else
                {
                    Debug.LogError("Failed to upload Mapdata to Firebase: " + task.Exception);
                }
            });
        }
#endif
    }
    public static void UploadAnchorDataList(AnchorDataList anchorDataList)
    {
        // Convert the AnchorDataList object to JSON
        string json = JsonUtility.ToJson(anchorDataList);

        // Convert the JSON string to a Dictionary
        Dictionary<string, object> anchorDataDict = new Dictionary<string, object>();
        anchorDataDict["anchorDataList"] = json;

        // Save the data to Firestore
        Firestore.Collection("anchorData").Document("anchorDataList").SetAsync(anchorDataDict).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("AnchorDataList saved successfully.");
            }
            else
            {
                Debug.LogError("Error saving AnchorDataList: " + task.Exception);
            }
        });
    }

    public static void DownloadAnchorDataList(Action<AnchorDataList> onComplete)
    {
        // Load the data from Firestore
        Firestore.Collection("anchorData").Document("anchorDataList").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    // Get the JSON string from the document
                    string json = snapshot.GetValue<string>("anchorDataList");

                    // Convert the JSON string back to an AnchorDataList object
                    AnchorDataList anchorDataList = JsonUtility.FromJson<AnchorDataList>(json);

                    Debug.Log("AnchorDataList loaded successfully.");
                    onComplete?.Invoke(anchorDataList); // Invoke the callback with the loaded data
                }
                else
                {
                    Debug.LogError("No AnchorDataList found in Firestore.");
                    onComplete?.Invoke(null); // Invoke the callback with null if no data found
                }
            }
            else
            {
                Debug.LogError("Error loading AnchorDataList: " + task.Exception);
                onComplete?.Invoke(null); // Invoke the callback with null if there was an error
            }
        });
    }

    public static void UploadMedia(byte[] bytes, string fileName, AnchorData anchorData)
    {
        StorageReference storageRef = StorageReference.Child("media/" + fileName);
        Debug.Log("Uploading media to Firebase Storage: " + fileName);

        storageRef.PutBytesAsync(bytes).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Media uploaded to Firebase Storage successfully.");
                storageRef.GetDownloadUrlAsync().ContinueWithOnMainThread(urlTask =>
                {
                    if (urlTask.IsCompleted)
                    {
                        string downloadUrl = urlTask.Result.ToString();
                        anchorData.anchorData = downloadUrl;
                        Debug.Log("Download URL: " + downloadUrl);
                    }
                    else
                    {
                        Debug.LogError("Failed to get download URL: " + urlTask.Exception);
                    }
                });
            }
            else
            {
                Debug.LogError("Failed to upload media: " + task.Exception);
            }
        });
    }

    // public static void UploadARWorldMap(byte[] bytes, string filePath)
    // {
    //     string fileName = Path.GetFileName(filePath);
    //     Debug.Log("Uploading ARWorldMap to Firestore...");

    //     // Check if file exists
    //     if (!File.Exists(filePath))
    //     {
    //         Debug.LogError("File not found at path: " + filePath);
    //         return;
    //     }

    //     try
    //     {
    //         byte[] fileBytes = File.ReadAllBytes(filePath);
    //         string base64String = Convert.ToBase64String(fileBytes);

    //         Firestore.Collection("worldmaps").Document(fileName).SetAsync(new Dictionary<string, object>
    //     {
    //         { fileName, base64String }
    //     }).ContinueWithOnMainThread(task =>
    //     {
    //         if (task.IsCompleted)
    //         {
    //             Debug.Log("ARWorldMap uploaded to Firestore successfully.");
    //         }
    //         else
    //         {
    //             Debug.LogError("Failed to upload ARWorldMap to Firestore: " + task.Exception);
    //         }
    //     });
    //     }
    //     catch (Exception e)
    //     {
    //         Debug.LogError("Failed to read or upload file: " + e.Message);
    //     }
    // }
    public static void UploadARWorldMap(byte[] bytes, string fileName)
    {
        Debug.Log("Uploading ARWorldMap to Firebase Storage...");

        StorageReference storageRef = FirebaseStorage.DefaultInstance.RootReference.Child("worldmaps/" + fileName);
        storageRef.PutBytesAsync(bytes).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("ARWorldMap uploaded to Firebase Storage successfully.");
            }
            else
            {
                Debug.LogError("Failed to upload ARWorldMap: " + task.Exception);
            }
        });
    }

    // public static void DownloadWorldMap(string fileName, string savePath, Action<bool> callback)
    // {
    //     Debug.Log("Downloading ARWorldMap from Firestore...");

    //     Firestore.Collection("worldmaps").Document(fileName).GetSnapshotAsync().ContinueWithOnMainThread(task =>
    //     {
    //         bool success = false;

    //         if (task.IsCompleted)
    //         {
    //             var snapshot = task.Result;
    //             if (snapshot.Exists)
    //             {
    //                 Debug.Log("Document found, downloading ARWorldMap...");
    //                 Dictionary<string, object> document = snapshot.ToDictionary();
    //                 if (document.TryGetValue(fileName, out object base64Object))
    //                 {
    //                     string base64String = base64Object as string;
    //                     if (!string.IsNullOrEmpty(base64String))
    //                     {
    //                         try
    //                         {
    //                             byte[] fileBytes = Convert.FromBase64String(base64String);
    //                             File.WriteAllBytes(savePath, fileBytes);
    //                             Debug.Log("ARWorldMap downloaded and saved successfully to: " + savePath);
    //                             success = true;
    //                         }
    //                         catch (Exception e)
    //                         {
    //                             Debug.LogError("Failed to write file: " + e.Message);
    //                         }
    //                     }
    //                     else
    //                     {
    //                         Debug.LogError("Base64 string is null or empty.");
    //                     }
    //                 }
    //                 else
    //                 {
    //                     Debug.LogError("Base64 string not found in document.");
    //                 }
    //             }
    //             else
    //             {
    //                 Debug.LogError("Document does not exist.");
    //             }
    //         }
    //         else
    //         {
    //             Debug.LogError("Failed to download ARWorldMap from Firestore: " + task.Exception);
    //         }

    //         // Invoke the callback with the success status
    //         callback?.Invoke(success);
    //     });
    // }
    public static IEnumerator DownloadWorldMap(string fileName, string savePath, System.Action<bool> callback)
    {
        StorageReference storageRef = FirebaseStorage.DefaultInstance.RootReference.Child("worldmaps/" + fileName);
        storageRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                string downloadUrl = task.Result.ToString();
                Debug.Log("Download URL: " + downloadUrl);
                // Call StartCoroutine on an instance of MonoBehaviour
                MonoBehaviour instance = new MonoBehaviour();
                instance.StartCoroutine(DownloadFile(downloadUrl, savePath, callback));
            }
            else
            {
                Debug.LogError("Failed to get download URL: " + task.Exception);
                callback?.Invoke(false);
            }
        });
        yield return null;
    }
    private static IEnumerator DownloadFile(string downloadUrl, string savePath, System.Action<bool> callback)
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
                    Debug.Log("World map downloaded and saved successfully to: " + savePath);
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
                Debug.LogError("Failed to download world map: " + request.error);
                callback?.Invoke(false); // Invoke callback with failure
            }
        }
    }
    public static void DownloadARWorldMap(string fileName, Action<byte[]> onDownloadComplete)
    {
        Debug.Log("Downloading ARWorldMap from Firebase Storage...");

        StorageReference storageRef = FirebaseStorage.DefaultInstance.RootReference.Child("worldmaps/" + fileName);

        storageRef.GetBytesAsync(1024 * 1024 * 10).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                byte[] downloadedBytes = task.Result;
                Debug.Log("ARWorldMap downloaded successfully.");
                onDownloadComplete(downloadedBytes); // Call the callback with the downloaded data
            }
            else
            {
                Debug.LogError("Failed to download ARWorldMap: " + task.Exception);
            }
        });
    }
}