using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class CustomColorPicker : MonoBehaviour
{
    [SerializeField] PinchSlider _rSlider;
    [SerializeField] PinchSlider _gSlider;
    [SerializeField] PinchSlider _bSlider;
    [SerializeField] GameObject _pickerObject;

    Color _pickerColor;
    IColorPickerListener _listener;

    public static CustomColorPicker Instance { get; private set; }

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
        try
        {
            _pickerColor.r = _rSlider.SliderValue;
            _pickerColor.g = _gSlider.SliderValue;
            _pickerColor.b = _bSlider.SliderValue;
            _pickerColor.a = 1;

            _listener.OnColorUpdated(_pickerColor);
        }catch{ }
    }
    public void OpenColorPicker(IColorPickerListener listener, Color defaultColor,Transform targetTransform)
    {
        _listener = listener;
        transform.parent = targetTransform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        _pickerObject.SetActive(true);

        _rSlider.SliderValue = defaultColor.r;
        _gSlider.SliderValue = defaultColor.g;
        _bSlider.SliderValue = defaultColor.b;
    }
}
