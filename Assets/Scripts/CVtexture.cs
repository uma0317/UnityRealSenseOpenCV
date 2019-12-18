using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;

public class CVtexture : MonoBehaviour
{
    // Start is called before the first frame update
    public Texture texture;
    void Start()
    {
        //Texture texture = GetComponent<RawImage>().mainTexture;
        //Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        //RenderTexture currentRT = RenderTexture.active;
        //RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        //Graphics.Blit(texture, renderTexture);

        //RenderTexture.active = renderTexture;
        //texture2D.ReadPixels(new UnityEngine.Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        //texture2D.Apply();

        //RenderTexture.active = currentRT;
        //RenderTexture.ReleaseTemporary(renderTexture);
        //Mat mat = OpenCvSharp.Unity.TextureToMat(texture2D);
        //for (int yi = 0; yi < mat.Height; yi++)
        //{
        //    for (int xi = 0; xi < mat.Width; xi++)
        //    {
        //        Vec3b v = mat.At<Vec3b>(yi, xi);
        //        float gr = 0.2126f * v[2] + 0.7152f * v[1] + 0.0722f * v[0];
        //        v[0] = (byte)gr;
        //        v[1] = (byte)gr;
        //        v[2] = (byte)gr;
        //        mat.Set<Vec3b>(yi, xi, v);
        //    }
        //}
        //Texture2D changedTex = OpenCvSharp.Unity.MatToTexture(mat);
        //GetComponent<RawImage>().texture = changedTex;

    }


    // Update is called once per frame
    void Update()
    {
        //Texture texture = GetComponent<RawImage>().texture;
        //if (texture == null) return;
        //Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        //RenderTexture currentRT = RenderTexture.active;
        //RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        //Graphics.Blit(texture, renderTexture);

        //RenderTexture.active = renderTexture;
        //texture2D.ReadPixels(new UnityEngine.Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        //texture2D.Apply();

        //RenderTexture.active = currentRT;
        //RenderTexture.ReleaseTemporary(renderTexture);
        //Mat mat = OpenCvSharp.Unity.TextureToMat(texture2D);
        //for (int yi = 0; yi < mat.Height; yi++)
        //{
        //    for (int xi = 0; xi < mat.Width; xi++)
        //    {
        //        Vec3b v = mat.At<Vec3b>(yi, xi);
        //        float gr = 0.2126f * v[2] + 0.7152f * v[1] + 0.0722f * v[0];
        //        v[0] = (byte)gr;
        //        v[1] = (byte)gr;
        //        v[2] = (byte)gr;
        //        mat.Set<Vec3b>(yi, xi, v);
        //    }
        //}
        //Texture2D changedTex = OpenCvSharp.Unity.MatToTexture(mat);
        //GetComponent<RawImage>().texture = changedTex;
    }

}
