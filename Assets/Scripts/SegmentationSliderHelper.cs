using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentationSliderHelper : MonoBehaviour
{
    public int SliderID { get; set; }

    public Action<int,float> SliderUpdated { get; set; }
    

    public void SliderUpdate(SliderEventData data)
    {
        SliderUpdated?.Invoke(SliderID, data.NewValue);
    }
}
