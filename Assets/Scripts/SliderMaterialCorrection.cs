using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

public class SliderMaterialCorrection : MonoBehaviour
{
    [SerializeField] TMP_Text _firstSliderText;
    [SerializeField] MeshRenderer _firstSliderRenderer;
    [SerializeField] TMP_Text _secondSliderText;
    [SerializeField] MeshRenderer _secondSliderRenderer;
    [SerializeField] PinchSlider _firstSlider;
    [SerializeField] PinchSlider _secondSlider;

    public void OnSliderUpdate()        //Knob color fix in case user drags them over each other
    {
        bool isFirstSliderGreater = _firstSlider.SliderValue > _secondSlider.SliderValue;
        (Color col1,Color col2) = isFirstSliderGreater ? (Color.white, Color.black):(Color.black, Color.white);

        _firstSliderText.color = col2;
        _firstSliderRenderer.sharedMaterial.color = col1;

        _secondSliderText.color = col1;
        _secondSliderRenderer.sharedMaterial.color = col2;
    }
}
