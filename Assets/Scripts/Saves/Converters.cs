using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityVolumeRendering;

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
    public static void UpdateTransform(Transform transform, TransformSave save,bool checkEmptiness)
    {
        bool update = checkEmptiness ? (!save.LocalPosition.IsVectorEmpty() || !save.LocalRotationEuler.IsVectorEmpty() || !save.LocalScale.IsVectorEmpty()):true;

        if (update)
        {
            transform.localPosition = ConvertVector(save.LocalPosition);
            transform.localRotation = Quaternion.Euler(ConvertVector(save.LocalRotationEuler));
            transform.localScale = ConvertVector(save.LocalScale);
        }        
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
    public static List<ColorSave> ConvertColors(List<Color> valueList)
    {
        List<ColorSave> colorSaves = new List<ColorSave>();
        foreach (Color color in valueList)
        {
            colorSaves.Add(ConvertColor(color));
        }
        return colorSaves;
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