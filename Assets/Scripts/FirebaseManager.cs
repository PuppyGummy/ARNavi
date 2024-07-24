using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.Storage;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;

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
    public static void SaveAnchorDataList(AnchorDataList anchorDataList)
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

    public static AnchorDataList LoadAnchorDataList()
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
                    return anchorDataList;
                }
                else
                {
                    Debug.LogError("No AnchorDataList found in Firestore.");
                    return null;
                }
            }
            else
            {
                Debug.LogError("Error loading AnchorDataList: " + task.Exception);
                return null;
            }
        });
        return null;
    }
    public static void UploadAndSaveMedia(string localPath, AnchorData anchorData)
    {
        string fileName = System.Guid.NewGuid().ToString() + System.IO.Path.GetExtension(localPath);
        StorageReference storageRef = StorageReference.Child("media/" + fileName);
        storageRef.PutFileAsync(localPath).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                storageRef.GetDownloadUrlAsync().ContinueWithOnMainThread(urlTask =>
                {
                    if (urlTask.IsCompleted)
                    {
                        string downloadUrl = urlTask.Result.ToString();
                        anchorData.anchorData = downloadUrl;
                        Debug.Log("Media uploaded to Firebase Storage successfully.");
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
    public static void SaveWorldMap(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        Debug.Log("Uploading ARWorldMap to Firebase...");
        Debug.Log("fileName: " + fileName);
        var storageRef = StorageReference.Child("worldmaps/" + fileName);
        storageRef.PutFileAsync(filePath).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("ARWorldMap uploaded to Firebase successfully.");
            }
            else
            {
                Debug.LogError("Failed to upload ARWorldMap to Firebase: " + task.Exception);
            }
        });
    }
    public static IEnumerator DownloadWorldMapFromFirebase(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        var storageRef = StorageReference.Child("worldmaps/" + fileName);
        var task = storageRef.GetFileAsync(filePath);

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("Failed to download ARWorldMap from Firebase: " + task.Exception);
            yield break;
        }

        Debug.Log("ARWorldMap downloaded from Firebase successfully.");
    }
}