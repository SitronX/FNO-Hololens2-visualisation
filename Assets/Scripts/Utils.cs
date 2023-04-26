using itk.simple;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public static class Utils
{
    public static Color[] CreateDistinctColors(int numberOfColors)       //Creating distinct colors based on hue
    {
        Color[] colors = new Color[numberOfColors];
        float incrementStep = 1f / numberOfColors;

        for (int i = 0; i < numberOfColors; i++)
        {
            colors[i] = Color.HSVToRGB(i * incrementStep, 1f, 1f);
        }

        return colors;
    }
    public static int GetHUFromFloat(float value, float minHu, float maxHu)       //Getting HU value from float value
    {
        float huRange = maxHu - minHu;
        int huValue = (int)(minHu + (value * huRange));
        return huValue;
    }

    public static bool IsVectorEmpty(this Vector3Save vector)
    {
        return vector.X == 0 && vector.Y == 0 && vector.Z == 0;
    }

    public static bool TryGetPositionInPatient(Image sliceImage, out Vector3 position)
    {
        try
        {
            List<string> metadataKeys = sliceImage.GetMetaDataKeys().ToList();

            if (metadataKeys.Contains("0020|0032"))
            {
                string imagePositionPatient = sliceImage.GetMetaData("0020|0032");          //0020|0032 tag for getting location
                string[] arr = imagePositionPatient.Split('\\');

                float x = float.Parse(arr[0], CultureInfo.InvariantCulture);
                float y = float.Parse(arr[1], CultureInfo.InvariantCulture);
                float z = float.Parse(arr[2], CultureInfo.InvariantCulture);

                position = new Vector3(x, y, z);
                return true;
            }
            position = Vector3.zero;
            return false;
        }
        catch
        {
            position = Vector3.zero;
            return false;
        }
    }
    public static bool IsHeadFeetDataset(Image firstImage, Image lastImage)
    {
        if (TryGetPositionInPatient(firstImage, out Vector3 firstPosition))
        {
            if (TryGetPositionInPatient(lastImage, out Vector3 secondPosition))
            {
                if (firstPosition.z > secondPosition.z)
                    return true;
            }
        }
        return false;
    }
    public static Image ExtractSlice(Image imageSeries, int sliceIndex)
    {
        if (sliceIndex < 0 || sliceIndex >= imageSeries.GetDepth())
        {
            throw new ArgumentOutOfRangeException(nameof(sliceIndex), $"Slice index {sliceIndex} is out of range.");
        }

        VectorUInt32 extractionSize = new VectorUInt32(new uint[] { imageSeries.GetWidth(), imageSeries.GetHeight(), 1 });
        VectorInt32 extractionIndex = new VectorInt32(new int[] { 0, 0, sliceIndex });

        ExtractImageFilter extractor = new ExtractImageFilter();
        extractor.SetSize(extractionSize);
        extractor.SetIndex(extractionIndex);

        return extractor.Execute(imageSeries);
    }
}
