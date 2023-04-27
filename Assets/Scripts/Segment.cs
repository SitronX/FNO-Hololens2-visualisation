using Microsoft.MixedReality.Toolkit.UI;
using System;
using TMPro;
using UnityEngine;

public class Segment : MonoBehaviour, IColorPickerListener
{
    [SerializeField] PinchSlider _slider;
    [SerializeField] Transform _colorPickerAnchorTransform;
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] TMP_Text _segmentName;
    [SerializeField] GameObject _mainObject;

    private Color _segmentColor;

    public Action ColorUpdated { get; set; }

    public Color SegmentColor
    {
        get { return _segmentColor; }
        set { _segmentColor = value; }
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
        CustomColorPicker.Instance.OpenColorPicker(this, SegmentColor, _colorPickerAnchorTransform);
    }

    public void OnColorUpdated(Color color)
    {
        InitColor(color);
        ColorUpdated?.Invoke();
    }
}
