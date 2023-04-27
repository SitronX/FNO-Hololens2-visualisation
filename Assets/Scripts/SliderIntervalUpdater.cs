using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class SliderIntervalUpdater : MonoBehaviour
{
    [SerializeField] PinchSlider _firstSlider;
    [SerializeField] PinchSlider _secondSlider;
    [SerializeField] PinchSlider _middleSlider;
    [SerializeField] GameObject _middleSliderCollider;
    [SerializeField] GameObject _firstSliderThumb;
    [SerializeField] GameObject _secondSliderThumb;
    [SerializeField] Animator _middleSliderAnimator;
    [SerializeField] TMP_Text _sliderFirstHULabel;
    [SerializeField] TMP_Text _sliderSecondHULabel;

    float _minHounsfieldValue;
    float _maxHounsfieldValue;
    bool _isHovering = false;

    public Action IntervalSliderValueChanged { get; set; }

    public void MiddleSliderUpdated(SliderEventData data)
    {      
        float sliderRange=_firstSlider.SliderValue-_secondSlider.SliderValue;

        bool isSecondSliderGreater = sliderRange < 0;

        float shift = sliderRange * 0.5f;
        float lower = data.NewValue - shift;
        float upper= data.NewValue + shift;
        (float firstSliderNewValue,float secondSliderNewValue) = isSecondSliderGreater ? (lower, upper):(upper, lower);

        if((firstSliderNewValue <= 1)&& (secondSliderNewValue <= 1))
        {
            if((firstSliderNewValue >= 0)&& (secondSliderNewValue >= 0))
            {
                _firstSlider.SliderValue = firstSliderNewValue;
                _secondSlider.SliderValue = secondSliderNewValue;
            }
            else                                //In case some slider has reached value below 0, move both sliders by same range to the start
            {
                float toFill = isSecondSliderGreater? _firstSlider.SliderValue: _secondSlider.SliderValue; 
              
                _firstSlider.SliderValue -= toFill;
                _secondSlider.SliderValue -= toFill;
            }
        }
        else                                    //In case some slider has reached value above 1, move both sliders by same range to the end
        {
            float toFill = 1 - (isSecondSliderGreater ? _secondSlider.SliderValue : _firstSlider.SliderValue);

            _firstSlider.SliderValue += toFill;
            _secondSlider.SliderValue += toFill;
        } 
    }
    public void SetHounsfieldValues(float minValue,float maxValue)
    {
        _minHounsfieldValue = minValue;
        _maxHounsfieldValue = maxValue;
    }
    public void SetInitvalue(float min,float max)
    {
        _firstSlider.SliderValue = min;
        _secondSlider.SliderValue = max;
        _middleSlider.SliderValue = (max + min) * 0.5f;
    }
    public void OnChangeSliderValue()
    {
        IntervalSliderValueChanged?.Invoke();

        float middleSliderSize=Mathf.Abs(_firstSlider.SliderValue-_secondSlider.SliderValue);

        Vector3 updatedScale = _middleSliderCollider.transform.localScale;
        updatedScale.x = middleSliderSize;

        _middleSliderCollider.transform.localScale = updatedScale;
        _middleSliderCollider.transform.position = (_firstSliderThumb.transform.position + _secondSliderThumb.transform.position) * 0.5f;     

        _sliderFirstHULabel.text= $"{Utils.GetHUFromFloat(_firstSlider.SliderValue, _minHounsfieldValue,_maxHounsfieldValue)}<br>HU";
        _sliderSecondHULabel.text = $"{Utils.GetHUFromFloat(_secondSlider.SliderValue, _minHounsfieldValue, _maxHounsfieldValue)}<br>HU";
    }
    public void GetSliderValues(out float value1,out float value2)
    {
        value1= _firstSlider.SliderValue<_secondSlider.SliderValue?_firstSlider.SliderValue:_secondSlider.SliderValue;
        value2= _firstSlider.SliderValue> _secondSlider.SliderValue ? _firstSlider.SliderValue : _secondSlider.SliderValue;
    }
    public void SetHoverState()
    {
        StartCoroutine(SetHoverStateDelay());     
    }

    IEnumerator SetHoverStateDelay()           
    {
        yield return null;

        if (_isHovering)
            _middleSliderAnimator.SetTrigger("Hover");
    }
    public void UpdateHover(bool value)             //This must be called before SetHoverState when the slider is released, however it was not the case inside mrtk PinchSlider so SetHoverState method has one frame delay in coroutine
    {
        _isHovering = value;
        if (value)
            _middleSliderAnimator.SetTrigger("Hover");
        else
            _middleSliderAnimator.SetTrigger("Default");
    }
}
