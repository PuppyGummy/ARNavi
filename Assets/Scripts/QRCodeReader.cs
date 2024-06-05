using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using ZXing;

public class QRCodeReader : MonoBehaviour
{
    [SerializeField] private ARCameraBackground arCameraBackground;
    [SerializeField] private RenderTexture targetRenderTexture;
    [SerializeField] private TextMeshProUGUI qrCodeText;

    private Texture2D cameraImageTexture;
    private IBarcodeReader reader = new BarcodeReader();
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        Graphics.Blit(null, targetRenderTexture, arCameraBackground.material);
        cameraImageTexture = new Texture2D(targetRenderTexture.width, targetRenderTexture.height, TextureFormat.RGBA32, false);
        Graphics.CopyTexture(targetRenderTexture, cameraImageTexture);

        // Detect and decode the QR code inside the bitmap
        var result = reader.Decode(cameraImageTexture.GetPixels32(), cameraImageTexture.width, cameraImageTexture.height);

        if (result != null)
        {
            qrCodeText.text = result.Text;
        }
    }
}
