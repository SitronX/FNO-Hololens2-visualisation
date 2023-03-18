using Microsoft.MixedReality.Toolkit.UI;
using QRTracking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomColorPicker : MonoBehaviour
{
    [SerializeField] PinchSlider _rSlider;
    [SerializeField] PinchSlider _gSlider;
    [SerializeField] PinchSlider _bSlider;
    [SerializeField] GameObject _pickerObject;

    Color _pickerColor;
    private int _callerID;

    public static CustomColorPicker Instance { get; private set; }
    public Action<Color,int> ColorUpdated { get; set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    public void ColorUpdate()
    {
        _pickerColor.r = _rSlider.SliderValue;
        _pickerColor.g = _gSlider.SliderValue;
        _pickerColor.b = _bSlider.SliderValue;
        _pickerColor.a = 1;

        ColorUpdated?.Invoke(_pickerColor, _callerID);
    }
    public void ShowColorPicker(Color defaultColor,Transform targetTransform,int callerID)
    {
        transform.parent = targetTransform;
        _callerID = callerID;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        _pickerObject.SetActive(true);

        _rSlider.SliderValue = defaultColor.r;
        _gSlider.SliderValue = defaultColor.g;
        _bSlider.SliderValue = defaultColor.b;
    }
    


}
