using JetBrains.Annotations;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
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

    public Action OnIntervaSliderValueChanged { get; set; }

    bool _isHovering = false;

    public void MiddleSliderUpdated(SliderEventData data)
    {      
        float changedValue = data.NewValue - data.OldValue;

        float firstSliderNewValue = _firstSlider.SliderValue + changedValue;
        float secondSliderNewValue = _secondSlider.SliderValue + changedValue;

        if (((firstSliderNewValue < 1) && (firstSliderNewValue > 0)) && ((secondSliderNewValue < 1) && (secondSliderNewValue > 0)))
        {
            _firstSlider.SliderValue = firstSliderNewValue;
            _secondSlider.SliderValue = secondSliderNewValue;
        }           
    }
    public void SetInitvalue(float min,float max)
    {
        _firstSlider.SliderValue = min;
        _secondSlider.SliderValue = max;
    }
    public void OnChangeSliderValue()
    {
        OnIntervaSliderValueChanged?.Invoke();

        float middleSliderSize=Mathf.Abs(_firstSlider.SliderValue-_secondSlider.SliderValue);

        Vector3 updatedScale = _middleSliderCollider.transform.localScale;
        updatedScale.x = middleSliderSize;
        _middleSliderCollider.transform.localScale= updatedScale;
        _middleSliderCollider.transform.position=(_firstSliderThumb.transform.position+_secondSliderThumb.transform.position)*0.5f;
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
