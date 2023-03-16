using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentationRowHelper : MonoBehaviour
{
    [SerializeField] PinchSlider _slider;
    public int SliderID { get; set; }

    public Action<int,float> SliderUpdated { get; set; }
    

    public void SliderUpdate(SliderEventData data)
    {
        SliderUpdated?.Invoke(SliderID, data.NewValue);
    }
    public void SetSliderValue(float val)
    {
        _slider.SliderValue= val;
    }
    public void OpenColorPicker()
    {

    }
}
