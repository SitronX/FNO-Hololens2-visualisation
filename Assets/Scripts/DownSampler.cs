using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class DownSampler : MonoBehaviour
{
    [SerializeField] MeshRenderer _activeDatasetTextureMesh;
    [SerializeField] TMP_Text _datasetText;
    [SerializeField] TMP_Text _dimensions;
    [SerializeField] ButtonConfigHelper _downsamplingButton;
    [SerializeField] Collider _downsamplingButtonCollider;

    DatasetButton _latestDatasetData;
    public enum DownsampleButtonState
    {
        Downsampling,Ready,Loading
    }
    private void Start()
    {
        DatasetButton.DatasetGrabbed += OnLatestDatasetGrabbed;
        VolumeDataControl.DatasetSpawned += OnVolumeDatasetSpawned;

    }
    private void OnDisable()
    {
        DatasetButton.DatasetGrabbed -= OnLatestDatasetGrabbed;
        VolumeDataControl.DatasetSpawned -= OnVolumeDatasetSpawned;
    }
    public void OnLatestDatasetGrabbed(DatasetButton button)
    {
        if (button.VolumeControlObject.HasBeenLoaded)
            ChangeButtonLoadingState(DownsampleButtonState.Ready);
        else
            ChangeButtonLoadingState(DownsampleButtonState.Loading);
        
            _latestDatasetData = button;
            _activeDatasetTextureMesh.material.mainTexture = button.ThumbnailTexture;
            _datasetText.text = button.DatasetName.text;

        try
        {
            _dimensions.text = $"{button.VolumeControlObject.Dataset.dimX}x{button.VolumeControlObject.Dataset.dimY}x{button.VolumeControlObject.Dataset.dimZ}";
        }
        catch 
        {
            _dimensions.text = "";
        }
    }
    public void OnVolumeDatasetSpawned(VolumeDataControl control)
    {
        DatasetButton but = HandMenu.Instance.AllDatasetButtons.Find(x => x.VolumeControlObject == control);
        OnLatestDatasetGrabbed(but);
    }
    public async void DownscaleDataset()
    {
        ChangeButtonLoadingState(DownsampleButtonState.Downsampling);
        await _latestDatasetData.VolumeControlObject.DownScaleDataset();

        ChangeButtonLoadingState(DownsampleButtonState.Ready);
        OnLatestDatasetGrabbed(_latestDatasetData);
    }
    public void ChangeButtonLoadingState(DownsampleButtonState state)
    {
        if(state==DownsampleButtonState.Downsampling)
        {
            _downsamplingButton.MainLabelText = "Downscaling";
            _downsamplingButton.SetQuadIconByName("IconShow");
            _downsamplingButtonCollider.enabled = false;
        }    
        else if(state==DownsampleButtonState.Ready)
        {
            _downsamplingButton.MainLabelText = "Start downscaling";
            _downsamplingButton.SetQuadIconByName("IconProfiler");
            _downsamplingButtonCollider.enabled = true;
        }
        else
        {
            _downsamplingButton.MainLabelText = "Dataset is still loading";
            _downsamplingButton.SetQuadIconByName("IconClose");
            _downsamplingButtonCollider.enabled = false;
        }
    }
}
