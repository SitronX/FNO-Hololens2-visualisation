using JetBrains.Annotations;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Profiling;
using UnityEngine;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

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

    float _maxHounsfieldValue;
    float _minHounsfieldValue;
    float _hounsfieldRange;

    public Action IntervalSliderValueChanged { get; set; }

    bool _isHovering = false;

    public void MiddleSliderUpdated(SliderEventData data)
    {      
        float sliderRange=_firstSlider.SliderValue-_secondSlider.SliderValue;

        bool isSecondSliderGreater = sliderRange < 0;

        float firstSliderNewValue = isSecondSliderGreater ? data.NewValue - (sliderRange * 0.5f) : data.NewValue + (sliderRange * 0.5f);
        float secondSliderNewValue = isSecondSliderGreater ? data.NewValue + (sliderRange * 0.5f) : data.NewValue - (sliderRange * 0.5f);

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
        _hounsfieldRange = maxValue - minValue;
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
        

        _sliderFirstHULabel.text= $"{(int)(_minHounsfieldValue + (_firstSlider.SliderValue * _hounsfieldRange))}<br>HU";
        _sliderSecondHULabel.text = $"{(int)(_minHounsfieldValue + (_secondSlider.SliderValue * _hounsfieldRange))}<br>HU";
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

    public void UpdateHover(bool value)             //This must be called before SetHoverState if the slider is released, however it is not the case inside mrtk PinchSlider so SetHoverState method has one frame delay in coroutine
    {
        _isHovering = value;
        if (value)
            _middleSliderAnimator.SetTrigger("Hover");
        else
            _middleSliderAnimator.SetTrigger("Default");
    }
}
