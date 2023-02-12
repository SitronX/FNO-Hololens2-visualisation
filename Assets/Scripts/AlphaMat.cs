using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlphaMat : MonoBehaviour
{
    [SerializeField] Image _colorImage;

    Material mainMaterial;
    public Material MainMaterial
    {
        get
        {
            return mainMaterial;
        }
        set
        {
            mainMaterial = value;
            Color fixedColor = mainMaterial.color;
            fixedColor.a = 1;
            _colorImage.color = fixedColor;
        }
    }
    public void UpdateAlphaSlider(SliderEventData data)
    {
        if(data.NewValue==1)
        {
            MainMaterial.ToOpaqueMode();

        }
        else if(data.OldValue==1) 
        {
            MainMaterial.ToFadeMode();
            UpdateAlpha(data.NewValue);
        }
        else
        {
            UpdateAlpha(data.NewValue);
        }
    }

    private void UpdateAlpha(float sliderValue)
    {
        Color color = MainMaterial.color;
        color.a = sliderValue;
        MainMaterial.SetColor("_Color", color);
    }

}
