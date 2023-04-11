using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Segment : MonoBehaviour,IColorPickerListener
{
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] GameObject _colorPickerAnchor;
    [SerializeField] PinchSlider _alphaSlider;

    MeshRenderer _meshRenderer;

    public void OpenColorPicker()
    {
        CustomColorPicker.Instance.OpenColorPicker(this, _meshRenderer.sharedMaterial.color, _colorPickerAnchor.transform);
    }
    public void InitSegment(MeshRenderer mesh)
    {
        _meshRenderer = mesh;
        _spriteRenderer.color= mesh.sharedMaterial.color;
    }

    public void OnColorUpdated(Color color)
    {
        _spriteRenderer.color = color;

        color.a = _alphaSlider.SliderValue;
        _meshRenderer.sharedMaterial.color = color;
    }

    public void SliderUpdate(SliderEventData data)
    {
        UpdateAlpha(data.NewValue);    
    }
    public void UpdateSlider(float value)
    {
        _alphaSlider.SliderValue = value;
    }
    private void UpdateAlpha(float sliderValue)
    {
        if (sliderValue == 1)
            _meshRenderer.sharedMaterial.ToOpaqueMode();
        else
        {
            _meshRenderer.sharedMaterial.ToFadeMode();

            Color color = _meshRenderer.sharedMaterial.color;
            color.a = sliderValue;
            _meshRenderer.sharedMaterial.color = color;
        }
    }

}
