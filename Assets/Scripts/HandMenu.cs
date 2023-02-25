using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.XR;

using UnityEngine.XR;

public class HandMenu : MonoBehaviour
{
    [SerializeField] InteractableToggleCollection _renderModes;
    [SerializeField] TMP_Text _raymarchStepLabel;

    List<VolumeDataControl> _volumeObjects = new List<VolumeDataControl>();

    bool _showCutPlane = false;
    bool _useCubicInterpolation = false;
    bool _useLighting = false;

    private void Start()
    {
        FindObjectsOfType<VolumeDataControl>().ToList().ForEach(x=>_volumeObjects.Add(x));

        LightingUpdated();

        VolumeDataControl.DatasetSpawned += OnNewDatasetSpawned;
        VolumeDataControl.DatasetDespawned+= OnNewDatasetDespawned;
    } 
    private void OnDestroy()
    {
        VolumeDataControl.DatasetSpawned -= OnNewDatasetSpawned;
        VolumeDataControl.DatasetDespawned -= OnNewDatasetDespawned;
    }
    private void OnNewDatasetSpawned(VolumeDataControl volumeDataControl)
    {
        _volumeObjects.Add(volumeDataControl);

        volumeDataControl.UpdateSlicePlane(_showCutPlane);                              //Check if all volume objects are set same as handmenu
        volumeDataControl.UpdateCubicInterpolation(_useCubicInterpolation);
        volumeDataControl.UpdateLighting(_useLighting);

        if (_renderModes.CurrentIndex == 0)
            volumeDataControl.UpdateRenderingMode(UnityVolumeRendering.RenderMode.DirectVolumeRendering);
        else if(_renderModes.CurrentIndex == 1)
            volumeDataControl.UpdateRenderingMode(UnityVolumeRendering.RenderMode.MaximumIntensityProjectipon);
        else if (_renderModes.CurrentIndex == 2)
            volumeDataControl.UpdateRenderingMode(UnityVolumeRendering.RenderMode.IsosurfaceRendering);

    }
    private void OnNewDatasetDespawned(VolumeDataControl volumeDataControl)
    {
        _volumeObjects.Remove(volumeDataControl);
    }
    public void RenderingModeUpdated()
    {
        if (_renderModes.CurrentIndex == 0)
            _volumeObjects.ForEach(x => x.UpdateRenderingMode(UnityVolumeRendering.RenderMode.DirectVolumeRendering)); 
        else if (_renderModes.CurrentIndex == 1)
            _volumeObjects.ForEach(x => x.UpdateRenderingMode(UnityVolumeRendering.RenderMode.MaximumIntensityProjectipon));
        else if (_renderModes.CurrentIndex == 2)
            _volumeObjects.ForEach(x => x.UpdateRenderingMode(UnityVolumeRendering.RenderMode.IsosurfaceRendering));
    }
    public void LightingUpdated()
    {
        _useLighting = !_useLighting;
        _volumeObjects.ForEach(_x => _x.UpdateLighting(_useLighting));
    }
    public void SlicePlaneUpdated()
    {
        _showCutPlane=!_showCutPlane;
        _volumeObjects.ForEach(_x => _x.UpdateSlicePlane(_showCutPlane));
    }
    public void CubicInterpolationUpdated()
    {
        _useCubicInterpolation=!_useCubicInterpolation;
        _volumeObjects.ForEach(_x => _x.UpdateCubicInterpolation(_useCubicInterpolation));
    }
    public void RaymarchSliderUpdated(SliderEventData data)
    {
        int steps = (int)(data.NewValue * 1000);
        _raymarchStepLabel.text = $"{steps} Steps";
        _volumeObjects.ForEach(_x => _x.SetRaymarchStepCount(steps));
    }  
    public void ResetPositionClicked()
    {
        _volumeObjects.ForEach(_x => _x.ResetObjectTransform());
    }
}