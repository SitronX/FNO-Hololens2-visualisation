using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityVolumeRendering;

public class TFColorUpdater : MonoBehaviour
{
    TransferFunction _transferFunction;
    [SerializeField] List<SliderData> _sliders;
    [SerializeField] GameObject _mainObject;

    List<float> _initialSliderValues=new List<float>();

    public Action<TransferFunction> TfColorUpdated { get; set; }

    [Serializable]
    public struct SliderData
    {
        public PinchSlider _slider;
        public float _initialValue;
    }

    public void InitUpdater(TransferFunction function)
    {
        _transferFunction = function;
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
    public void SliderUpdate()
    {
        if (_transferFunction != null)
        {
            for (int i = 0; i < _transferFunction.colourControlPoints.Count; i++)
            {
                TFColourControlPoint point = _transferFunction.colourControlPoints[i];
                point.dataValue = _sliders[i]._slider.SliderValue;

                _transferFunction.colourControlPoints[i] = point;
            }

            _transferFunction.GenerateTexture();
            TfColorUpdated?.Invoke(_transferFunction);
        }
    }
    public void ResetTF()
    {
        for(int i=0; i< _sliders.Count; i++)
        {
            _sliders[i]._slider.SliderValue = _initialSliderValues[i];
        }
    }

}
