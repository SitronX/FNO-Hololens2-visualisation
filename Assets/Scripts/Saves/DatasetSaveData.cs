using Mono.CSharp;
using System;
using System.Collections.Generic;

[Serializable]
public class DatasetSaveData
{
    public List<DensityIntervalSave> DensityIntervalSliders { get; set; }=new List<DensityIntervalSave>();
    public List<ColorSave> SegmentColors { get; set; }=new List<ColorSave> ();
    public TFSave TransferFunction { get; set; }
    public TransformSave MainDatasetTransform { get; set; }
    public TransformSave GrabHandleTransform { get; set; }
    public TransformSave CrossPlaneTransform { get; set; }
    public TransformSave CrossSphereTransform { get; set; }

    public TransformSave SliceXTransform { get; set; }
    public TransformSave SliceYTransform { get; set; }
    public TransformSave SliceZTransform { get; set; }

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


