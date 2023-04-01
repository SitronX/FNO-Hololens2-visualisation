using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Segment : MonoBehaviour
{
    [SerializeField] PinchSlider _slider;
    [SerializeField] Transform _colorPickerAnchorTransform;
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] TMP_Text _segmentName;
    [SerializeField] GameObject _mainObject;


    public int SegmentID { get; set; }

    public Action ColorUpdated { get; set; }

    private Color _segmentColor;
    public Color SegmentColor
    {
        get { return _segmentColor; }
        set { _segmentColor = value; }
    }

    private void Start()
    {
        CustomColorPicker.Instance.ColorUpdated += OnColorPickerColorUpdated;
    }

    public void AlphaUpdate(float value)
    {
        _segmentColor.a = value;
        _slider.SliderValue = value;
        ColorUpdated?.Invoke();
    }
    public void SliderUpdate(SliderEventData data)
    {
        _segmentColor.a = data.NewValue;
        ColorUpdated?.Invoke();
    }
    public void InitColor(Color color)
    {
        _segmentColor.r = color.r;                  //Do not want to update alpha
        _segmentColor.g = color.g;
        _segmentColor.b = color.b;

        color.a = 1;
        _spriteRenderer.color = color;
    }
    public void ChangeSegmentName(string name)
    {
        _segmentName.text = name;
    }

    public void OpenColorPicker()
    {
        CustomColorPicker.Instance.ShowColorPicker(SegmentColor, _colorPickerAnchorTransform,SegmentID);
    }
    public void OnColorPickerColorUpdated(Color color,int callerID)
    {
        if (callerID == SegmentID)
        {
            InitColor(color);
            ColorUpdated?.Invoke();
        }

    }
}
