using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

public class RawImageCam : MonoBehaviour
{
    public Camera targetCamera;
    public UnityEngine.UI.RawImage display;
    private RenderTexture renderTexture;
    private Texture2D texture2D;
    private HttpListener httpListener;
    private Thread serverThread;
    private ConcurrentQueue<byte[]> imageQueue = new ConcurrentQueue<byte[]>();
    private byte[] latestImageBytes;

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

        // Create Texture2D for encoding
        texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);

        // Start HTTP server
        httpListener = new HttpListener();
        httpListener.Prefixes.Add("http://*:8080/");
        httpListener.Start();
        serverThread = new Thread(ServerLoop);
        serverThread.Start();

        // Start image processing thread
        Thread imageProcessingThread = new Thread(ProcessImages);
        imageProcessingThread.Start();
    }

    void Update()
    {
        // Capture the render texture
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();
        RenderTexture.active = null;

        // Encode texture to JPEG on the main thread
        byte[] imageBytes = texture2D.EncodeToJPG();
        imageQueue.Enqueue(imageBytes);
    }

    void OnDestroy()
    {
        httpListener.Stop();
        serverThread.Abort();
    }

    private void ProcessImages()
    {
        while (true)
        {
            if (imageQueue.TryDequeue(out byte[] imageBytes))
            {
                latestImageBytes = imageBytes;
            }

            // Sleep to maintain 30 fps
            //Thread.Sleep(33);
        }
    }

    private void ServerLoop()
    {
        while (httpListener.IsListening)
        {
            var context = httpListener.GetContext();
            ThreadPool.QueueUserWorkItem(o => HandleClient(context));
        }
    }

    private void HandleClient(HttpListenerContext context)
    {
        var response = context.Response;
        response.ContentType = "multipart/x-mixed-replace; boundary=--frame";
        response.StatusCode = (int)HttpStatusCode.OK;

        try
        {
            while (httpListener.IsListening)
            {
                if (latestImageBytes != null)
                {
                    // Write JPEG to response
                    string header = "--frame\r\nContent-Type: image/jpeg\r\nContent-Length: " + latestImageBytes.Length + "\r\n\r\n";
                    byte[] headerBytes = Encoding.ASCII.GetBytes(header);

                    response.OutputStream.Write(headerBytes, 0, headerBytes.Length);
                    response.OutputStream.Write(latestImageBytes, 0, latestImageBytes.Length);
                    response.OutputStream.Write(Encoding.ASCII.GetBytes("\r\n"), 0, 2);
                    response.OutputStream.Flush();
                }

                // Sleep to maintain 30 fps
                //Thread.Sleep(33);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("HandleClient exception: " + ex.Message);
        }
        finally
        {
            response.OutputStream.Close();
        }
    }
}