using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityVolumeRendering;

public class TFColorUpdater : MonoBehaviour
{
    public TransferFunction TransferFunction { get; set; }
    [SerializeField] List<SliderData> _sliders;
    [SerializeField] GameObject _mainObject;

    List<float> _initialSliderValues=new List<float>();

    public Action<TransferFunction> TfColorUpdated { get; set; }
    public Action TfColorReset { get; set;}

    [Serializable]
    public struct SliderData
    {
        public PinchSlider _slider;
        public float _initialValue;
    }

    public void InitUpdater(TransferFunction function)
    {
        TransferFunction = function;
        ShowTfUpdater(true);
    }
    public void ShowTfUpdater(bool value)
    {
        _mainObject.SetActive(value);
    }
    private void Start()
    {
        foreach(SliderData i in _sliders)
        {
            i._slider.SliderValue = i._initialValue;
            _initialSliderValues.Add(i._initialValue);
        }
    }
    public void UpdateSliderColorPosition(int sliderIndex,float position)
    {
        _sliders[sliderIndex]._slider.SliderValue = position;
    }
    public void SliderUpdate()
    {
        if (TransferFunction != null)
        {
            for (int i = 0; i < TransferFunction.colourControlPoints.Count; i++)
            {
                TFColourControlPoint point = TransferFunction.colourControlPoints[i];
                point.dataValue = _sliders[i]._slider.SliderValue;

                TransferFunction.colourControlPoints[i] = point;
            }

            TransferFunction.GenerateTexture();
            TfColorUpdated?.Invoke(TransferFunction);
        }
    }
    public void ResetTF()
    {
        for(int i=0; i< _sliders.Count; i++)
        {
            _sliders[i]._slider.SliderValue = _initialSliderValues[i];
        }
        TfColorReset?.Invoke();
    }

}
