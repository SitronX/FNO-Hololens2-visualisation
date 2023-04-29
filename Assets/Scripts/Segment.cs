using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class Segment : MonoBehaviour,IColorPickerListener
{
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] GameObject _colorPickerAnchor;
    [SerializeField] PinchSlider _alphaSlider;

    public MeshRenderer MeshRenderer { get; set; }

    public void OpenColorPicker()
    {
        CustomColorPicker.Instance.OpenColorPicker(this, MeshRenderer.sharedMaterial.color, _colorPickerAnchor.transform);
    }
    public void InitSegment(MeshRenderer mesh)
    {
        MeshRenderer = mesh;
        Color col = mesh.sharedMaterial.color;
        col.a = 1;
        _spriteRenderer.color = col;
    }

    public void OnColorUpdated(Color color)
    {
        _spriteRenderer.color = color;

        color.a = _alphaSlider.SliderValue;
        MeshRenderer.sharedMaterial.color = color;
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
        MeshRenderer.enabled = (sliderValue != 0);        //If alpha is 0, disable meshrenderer for performance reasons

        if (sliderValue == 1)
        {
            MeshRenderer.sharedMaterial.ToOpaqueMode();
        }
        else
        {
            MeshRenderer.sharedMaterial.ToFadeMode();

            Color color = MeshRenderer.sharedMaterial.color;
            color.a = sliderValue;
            MeshRenderer.sharedMaterial.color = color;
        }
    }

}
