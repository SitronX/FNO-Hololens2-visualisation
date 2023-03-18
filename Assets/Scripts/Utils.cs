using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static Color[] CreateColors(int colorsNumber)       //Creating distinct colors based on hue
    {
        Color[] colors = new Color[colorsNumber];
        float incrementStep = 1f / colorsNumber;

        for (int i = 0; i < colorsNumber; i++)
        {
            colors[i] = Color.HSVToRGB(i * incrementStep, 1f, 1f);
        }

        return colors;
    }
}
