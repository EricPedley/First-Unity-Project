using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoCapture : MonoBehaviour
{
    private Camera cam;
    private Texture2D tex;
    private RenderTexture rt;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        cam.targetTexture = rt;
        RenderTexture.active=rt;
    }

    // Update is called once per frame
    void Update()
    {
        cam.Render();
        tex.ReadPixels(new Rect(0,0,100,100),0,0);
        Color[] framebuffer = tex.GetPixels();
    }
}
