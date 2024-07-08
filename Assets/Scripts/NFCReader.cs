using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitsNFCToolkit;

public class NFCReader : MonoBehaviour
{
    public TMPro.TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
#if (!UNITY_EDITOR)
        NativeNFCManager.AddNDEFReadFinishedListener(OnNDEFReadFinished);
        Debug.Log("NDEF Read Supported: " + NativeNFCManager.IsNDEFReadSupported());
#endif
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void StartNFCRead()
    {
#if (!UNITY_EDITOR)
			NativeNFCManager.ResetOnTimeout = true;
			NativeNFCManager.Enable();
#endif
    }
    public void OnNDEFReadFinished(NDEFReadResult result)
    {
        if (result.Success)
        {
            List<NDEFRecord> records = result.Message.Records;
            NDEFRecord record = records[0];
            TextRecord textRecord = (TextRecord)record;
            RecenterHelper.Instance.Recenter(textRecord.text);
            text.text = "NFC Tag Detected: " + textRecord.text;
            NativeNFCManager.Disable();
        }
    }
}
