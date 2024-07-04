using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using distriqt.plugins.nfc;

public class NFCReader_backup : MonoBehaviour
{
    public TMPro.TextMeshProUGUI text;
    private bool readerModeEnabled = false;
    // Start is called before the first frame update
    void Start()
    {
        if (NFC.isSupported)
        {
            text.text = "NFC is supported on this device";
            NFC.Instance.OnNdefDiscovered += OnNdefDiscovered;

            ScanOptions options = new ScanOptions();
            options.message = "Hold your device near the NFC tag to calibrate";

            NFC.Instance.RegisterForegroundDispatch(options);
        }
        else
        {
            text.text = "NFC is not supported on this device";
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnNdefDiscovered(NFCEvent e)
    {
        foreach (NdefMessage message in e.tag.messages)
        {
            foreach (NdefRecord record in message.records)
            {
                text.text = "Payload: " + StringToASCII(record.payload);
            }
        }
    }
    public static string StringToASCII(string hexString)
    {
        string ascii = string.Empty;
        for (int i = 0; i < hexString.Length; i += 2)
        {
            byte val = System.Convert.ToByte(hexString.Substring(i, 2), 16);
            char character = System.Convert.ToChar(val);
            ascii += character;
        }
        return ascii;
    }
    public void ToggleNFC()
    {
        if (readerModeEnabled)
        {
            NFC.Instance.EnableReaderMode();
            readerModeEnabled = false;
        }
        else
        {
            NFC.Instance.DisableReaderMode();
            readerModeEnabled = true;
        }
    }
}
