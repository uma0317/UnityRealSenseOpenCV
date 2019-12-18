using Intel.RealSense;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using OpenCvSharp;
using UnityEngine.UI;

public class RsTextureOpenCV : MonoBehaviour
{
    private static TextureFormat Convert(Format lrsFormat)
    {
        switch (lrsFormat)
        {
            case Format.Z16: return TextureFormat.R16;
            case Format.Disparity16: return TextureFormat.R16;
            case Format.Rgb8: return TextureFormat.RGB24;
            case Format.Rgba8: return TextureFormat.RGBA32;
            case Format.Bgra8: return TextureFormat.BGRA32;
            case Format.Y8: return TextureFormat.Alpha8;
            case Format.Y16: return TextureFormat.R16;
            case Format.Raw16: return TextureFormat.R16;
            case Format.Raw8: return TextureFormat.Alpha8;
            case Format.Disparity32: return TextureFormat.RFloat;
            case Format.Yuyv:
            case Format.Bgr8:
            case Format.Raw10:
            case Format.Xyz32f:
            case Format.Uyvy:
            case Format.MotionRaw:
            case Format.MotionXyz32f:
            case Format.GpioRaw:
            case Format.Any:
            default:
                throw new ArgumentException(string.Format("librealsense format: {0}, is not supported by Unity", lrsFormat));
        }
    }

    private static int BPP(TextureFormat format)
    {
        switch (format)
        {
            case TextureFormat.ARGB32:
            case TextureFormat.BGRA32:
            case TextureFormat.RGBA32:
                return 32;
            case TextureFormat.RGB24:
                return 24;
            case TextureFormat.R16:
                return 16;
            case TextureFormat.R8:
            case TextureFormat.Alpha8:
                return 8;
            default:
                throw new ArgumentException("unsupported format {0}", format.ToString());

        }
    }

    public RsFrameProvider Source;

    [System.Serializable]
    public class TextureEvent : UnityEvent<Texture> { }

    public Stream _stream;
    public Format _format;
    public int _streamIndex;

    public FilterMode filterMode = FilterMode.Point;

    protected Texture2D texture;


    [Space]
    public TextureEvent textureBinding;

    FrameQueue q;
    Predicate<Frame> matcher;

    void Start()
    {
        Source.OnStart += OnStartStreaming;
        Source.OnStop += OnStopStreaming;
    }

    void OnDestroy()
    {
        if (texture != null)
        {
            Destroy(texture);
            texture = null;
        }

        if (q != null)
        {
            q.Dispose();
        }
    }

    protected void OnStopStreaming()
    {
        Source.OnNewSample -= OnNewSample;
        if (q != null)
        {
            q.Dispose();
            q = null;
        }
    }

    public void OnStartStreaming(PipelineProfile activeProfile)
    {
        q = new FrameQueue(1);
        matcher = new Predicate<Frame>(Matches);
        Source.OnNewSample += OnNewSample;
    }

    private bool Matches(Frame f)
    {
        using (var p = f.Profile)
            return p.Stream == _stream && p.Format == _format && p.Index == _streamIndex;
    }

    void OnNewSample(Frame frame)
    {
        try
        {
            if (frame.IsComposite)
            {
                using (var fs = frame.As<FrameSet>())
                using (var f = fs.FirstOrDefault(matcher))
                {
                    if (f != null)
                        q.Enqueue(f);
                    return;
                }
            }

            if (!matcher(frame))
                return;

            using (frame)
            {
                q.Enqueue(frame);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            // throw;
        }

    }

    bool HasTextureConflict(VideoFrame vf)
    {
        return !texture ||
            texture.width != vf.Width ||
            texture.height != vf.Height ||
            BPP(texture.format) != vf.BitsPerPixel;
    }

    protected void LateUpdate()
    {
        if (q != null)
        {
            VideoFrame frame;
            if (q.PollForFrame<VideoFrame>(out frame))
                using (frame)
                    ProcessFrame(frame);
        }
    }

    private void ProcessFrame(VideoFrame frame)
    {
        if (HasTextureConflict(frame))
        {

            if (texture != null)
            {
                Destroy(texture);
            }

            using (var p = frame.Profile)
            {
                bool linear = (QualitySettings.activeColorSpace != ColorSpace.Linear)
                    || (p.Stream != Stream.Color && p.Stream != Stream.Infrared);
                texture = new Texture2D(frame.Width, frame.Height, Convert(p.Format), false, linear)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = filterMode
                };
            }

            textureBinding.Invoke(texture);
        }


        texture.LoadRawTextureData(frame.Data, frame.Stride * frame.Height);

        //Mat mat = OpenCvSharp.Unity.TextureToMat(texture);

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
        //texture = OpenCvSharp.Unity.MatToTexture(mat);
        //Debug.Log(texture.GetPixels32()[0]);

        //
        //Gray Scalse
        //
        //Mat mat = OpenCvSharp.Unity.TextureToMat(texture);
        //Mat changedMat = new Mat();
        //Cv2.CvtColor(mat, changedMat, ColorConversionCodes.BGR2GRAY);

        //texture = OpenCvSharp.Unity.MatToTexture(changedMat);

        //Gray Scale
        Mat mat = OpenCvSharp.Unity.TextureToMat(texture);
        Mat gray = new Mat();
        Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);

        //Threshold
        Mat thresh = new Mat();
        Mat bin_img = new Mat();
        Cv2.Threshold(gray, thresh, 0, 255, ThresholdTypes.Otsu);
        bin_img = thresh;

        //morphologyEx
        Mat morphology = new Mat();
        Cv2.MorphologyEx(bin_img, morphology, MorphTypes.Open, new InputArray(1), null, 2);

        //Dilate
        Mat dilate = new Mat();
        Cv2.Dilate(morphology, dilate, new InputArray(1), null, 2, BorderTypes.Constant);
        //thresh.Dilate(dilate, null, 1, BorderTypes.Constant, null);

        //Distance Transform
        Mat distance_transform = new Mat();
        Cv2.DistanceTransform(morphology, distance_transform, DistanceTypes.L2, DistanceMaskSize.Mask5);

        //sure foreground
        Mat sure_fg = new Mat();
        Cv2.Threshold(distance_transform, sure_fg, 11, 255, ThresholdTypes.Binary);


        //SubStract
        //Mat subtract = new Mat();
        //Cv2.Subtract(dilate, sure_fg, subtract);

        //Contours
        Point[][] contours;
        HierarchyIndex[] h_index;
        Cv2.FindContours(dilate, out contours, out h_index, RetrievalModes.List, ContourApproximationModes.ApproxTC89KCOS, null);


        //
        // draw max area
        //
        //double max_area = 0;
        //int max_area_contour = -1;
        //for (int j = 0; j < contours.Length; j++)
        //{
        //    double area = Cv2.ContourArea(contours[j]);
        //    if (max_area < area)
        //    {
        //        max_area = area;
        //        max_area_contour = j;
        //    }
        //}
        //int count = contours[max_area_contour].Length;
        //double x = 0;
        //double y = 0;
        //for (int k = 0; k < count; k++)
        //{
        //    x += contours[max_area_contour][k].X;
        //    y += contours[max_area_contour][k].Y;
        //}
        //x /= count;
        //y /= count;
        //Cv2.Circle(mat, (int)x, (int)y, 50, new Scalar(0, 0, 255), 3, LineTypes.Link4);


        /// draw segment
        Cv2.Polylines(mat, contours, true, new Scalar(0, 0, 255), 5);

        texture = OpenCvSharp.Unity.MatToTexture(mat);

        textureBinding.Invoke(texture);
        texture.Apply();

        //texture.Apply();
    }
}
