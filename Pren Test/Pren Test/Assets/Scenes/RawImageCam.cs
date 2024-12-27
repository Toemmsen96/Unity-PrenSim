using UnityEngine;
using UnityEngine.UI;

public class RawImageCam : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private RawImage display;
    private RenderTexture renderTexture;

    void Start()
    {
        // Use main camera if none assigned
        if (targetCamera == null)
            targetCamera = Camera.main;

        // Create render texture
        renderTexture = new RenderTexture(1280, 720, 24);
        targetCamera.targetTexture = renderTexture;
        
        // Assign to display
        display.texture = renderTexture;
    }

    void Update()
    {
        // Empty as we don't need per-frame updates
    }
}