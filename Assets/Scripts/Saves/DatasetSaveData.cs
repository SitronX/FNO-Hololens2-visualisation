
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityVolumeRendering;

[Serializable]
public class DatasetSaveData
{
    public List<DensityIntervalSave> DensityIntervalSliders { get; set; }=new List<DensityIntervalSave>();
    public List<ColorSave> SegmentColors { get; set; }
    public TFSave TransferFunction { get; set; }

    public TransformSave MainDatasetTransform { get; set; }
    public TransformSave GrabHandleTransform { get; set; }
    public TransformSave CrossPlaneTransform { get; set; }
    public TransformSave CrossSphereTransform { get; set; }
    public Vector3Save SliceWindowRange { get; set; } = new Vector3Save { X = 0, Y = 1 };  //x is min and y is max
}

public struct TransformSave
{
    public Vector3Save LocalPosition;
    public Vector3Save LocalRotationEuler;
    public Vector3Save LocalScale;
}
public struct TFSave
{
    public List<TFColorSave> ColourControlPoints;
    public List<TFAlphaSave> AlphaControlPoints;
}
public struct TFColorSave
{
    public float SliderValue;
    public ColorSave Color;
}
public struct TFAlphaSave
{
    public float SliderValue;
    public float AlphaValue;
}
public struct DensityIntervalSave
{
    public float MinValue;
    public float MaxValue;
}
public struct ColorSave
{
    public float R;
    public float G;
    public float B;
    public float A;
}
public struct Vector3Save
{
    public float X;
    public float Y;
    public float Z;
}


public class Converters
{
    public static TransformSave ConvertTransform(Transform transform)
    {
        TransformSave save;
        save.LocalPosition = ConvertVector(transform.localPosition);
        save.LocalRotationEuler = ConvertVector(transform.localRotation.eulerAngles);
        save.LocalScale = ConvertVector(transform.localScale);

        return save;
    }
    public static void UpdateTransform(Transform transform,TransformSave save)
    {
        transform.localPosition = new Vector3(save.LocalPosition.X, save.LocalPosition.Y, save.LocalPosition.Z);
        transform.localRotation = Quaternion.Euler(save.LocalRotationEuler.X, save.LocalRotationEuler.Y, save.LocalRotationEuler.Z);
        transform.localScale = new Vector3(save.LocalScale.X, save.LocalScale.Y, save.LocalScale.Z);
    }
    public static TFSave ConvertTransferFunction(TransferFunction tf)
    {
        TFSave transfer;
        transfer.ColourControlPoints = tf.colourControlPoints.Select(x => new TFColorSave { Color = ConvertColor(x.colourValue), SliderValue = x.dataValue }).ToList();
        transfer.AlphaControlPoints = tf.alphaControlPoints.Select(x => new TFAlphaSave { AlphaValue = x.alphaValue, SliderValue = x.dataValue }).ToList();

        return transfer;
    }
    public static List<DensityIntervalSave> ConvertDensitySliders(List<SliderIntervalUpdater> valueList)
    {
        List<DensityIntervalSave> intervals = new List<DensityIntervalSave>();
        foreach (SliderIntervalUpdater slider in valueList)
        {
            slider.GetSliderValues(out float min, out float max);
            DensityIntervalSave save;
            save.MinValue = min;
            save.MaxValue = max;
            intervals.Add(save);
        }
        return intervals;
    }
    public static void UpdateDensitySliders(List<SliderIntervalUpdater> valueList, List<DensityIntervalSave> intervals)
    {
        for (int i = 0; i < intervals.Count; i++)
        {
            valueList[i].SetInitvalue(intervals[i].MinValue, intervals[i].MaxValue);
        }
    }
    public static List<ColorSave> ConvertColors(List<Color> valueList)
    {
        List<ColorSave> colorSaves = new List<ColorSave>();
        foreach (Color color in valueList)
        {
            colorSaves.Add(ConvertColor(color));
        }
        return colorSaves;
    }
    public static List<Color> ConvertColors(List<ColorSave> valueList)
    {
        List<Color> colors = new List<Color>();
        foreach (ColorSave save in valueList)
        {
            colors.Add(ConvertColor(save));
        }
        return colors;
    }

    public static ColorSave ConvertColor(Color col)
    {
        ColorSave save;
        save.R = col.r;
        save.G = col.g;
        save.B = col.b;
        save.A = col.a;
        return save;
    }
    public static Color ConvertColor(ColorSave col)
    {
        Color res;
        res.r = col.R;
        res.g = col.G;
        res.b = col.B;
        res.a = col.A;
        return res;
    }
    public static Vector3Save ConvertVector(Vector3 value)
    {
        Vector3Save save;
        save.X = value.x;
        save.Y = value.y;
        save.Z = value.z;

        return save;
    }
    public static Vector3 ConvertVector(Vector3Save save)
    {
        Vector3 res;
        res.x = save.X;
        res.y = save.Y;
        res.z = save.Z;

        return res;
    }
}