using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class HandMenu : MonoBehaviour
{
    [SerializeField] InteractableToggleCollection _renderModes;
    [SerializeField] TMP_Text _raymarchStepLabel;

    List<VolumeDataControl> _volumeObjects = new List<VolumeDataControl>();
    

    private void Start()
    {
        FindObjectsOfType<VolumeDataControl>().ToList().ForEach(x=>_volumeObjects.Add(x));

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
        _volumeObjects.ForEach(_x => _x.UpdateLighting());
    }
    public void SlicePlaneUpdated()
    {
        _volumeObjects.ForEach(_x => _x.UpdateSlicePlane());
    }
    public void CubicInterpolationUpdated()
    {
        _volumeObjects.ForEach(_x => _x.UpdateCubicInterpolation());
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
