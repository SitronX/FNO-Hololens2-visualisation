using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

public class DatasetSettings : MonoBehaviour
{
    [SerializeField] MeshRenderer _activeDatasetTextureMesh;
    [SerializeField] TMP_Text _datasetText;
    [SerializeField] TMP_Text _dimensions;
    [SerializeField] ButtonConfigHelper _downsamplingButton;
    [SerializeField] ButtonConfigHelper _mirrorFlipButton;
    [SerializeField] Collider _downsamplingButtonCollider;
    [SerializeField] Collider _mirrorButtonCollider;

    DatasetButton _latestDatasetData;
    public enum ButtonType
    {
        DownscaleReady,DownscaleActive,MirrorFlipReady,MirrorFlipActive,DatasetLoading
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
    private void OnLatestDatasetGrabbed(DatasetButton button)
    {
        SetCorrectButtonsState(button);

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
    private void SetCorrectButtonsState(DatasetButton button)
    {
        if (button.VolumeControlObject.HasBeenLoaded)
        {
            if (button.VolumeControlObject.ProcessingType == VolumeDataControl.DatasetProcessingType.Normal)
            {
                ChangeButtonLoadingState(_downsamplingButton, _downsamplingButtonCollider, ButtonType.DownscaleReady);
                ChangeButtonLoadingState(_mirrorFlipButton, _mirrorButtonCollider, ButtonType.MirrorFlipReady);
            }
            else if (button.VolumeControlObject.ProcessingType == VolumeDataControl.DatasetProcessingType.Downsampling)
            {
                ChangeButtonLoadingState(_downsamplingButton, _downsamplingButtonCollider, ButtonType.DownscaleActive);
                ChangeButtonLoadingState(_mirrorFlipButton, _mirrorButtonCollider, ButtonType.DownscaleActive);
            }
            else if(button.VolumeControlObject.ProcessingType == VolumeDataControl.DatasetProcessingType.Mirrorflipping)
            {
                ChangeButtonLoadingState(_downsamplingButton, _downsamplingButtonCollider, ButtonType.MirrorFlipActive);
                ChangeButtonLoadingState(_mirrorFlipButton, _mirrorButtonCollider, ButtonType.MirrorFlipActive);
            }    
        }
        else
        {
            ChangeButtonLoadingState(_downsamplingButton, _downsamplingButtonCollider, ButtonType.DatasetLoading);
            ChangeButtonLoadingState(_mirrorFlipButton, _mirrorButtonCollider, ButtonType.DatasetLoading);
        }
    }
    private void OnVolumeDatasetSpawned(VolumeDataControl control)
    {
        DatasetButton but = HandMenu.Instance.AllDatasetButtons.Find(x => x.VolumeControlObject == control);
        OnLatestDatasetGrabbed(but);
    }
    public async void DownscaleDataset()
    {
        ChangeButtonLoadingState(_downsamplingButton,_downsamplingButtonCollider, ButtonType.DownscaleActive);
        ChangeButtonLoadingState(_mirrorFlipButton, _mirrorButtonCollider, ButtonType.DownscaleActive);

        await _latestDatasetData.VolumeControlObject.DownScaleDatasetAsync();

        OnLatestDatasetGrabbed(_latestDatasetData);
    }
    public async void MirrorFlipDataset()
    {
        ChangeButtonLoadingState(_mirrorFlipButton,_mirrorButtonCollider, ButtonType.MirrorFlipActive);
        ChangeButtonLoadingState(_downsamplingButton, _downsamplingButtonCollider, ButtonType.MirrorFlipActive);

        await _latestDatasetData.VolumeControlObject.MirrorFlipTexturesAsync();

        OnLatestDatasetGrabbed(_latestDatasetData);
    }
    private (string,string) GetButtonInfo(ButtonType type)
    {
        if (type == ButtonType.DownscaleReady)
            return ("IconProfiler", "Downscale Dataset");
        else if (type == ButtonType.DownscaleActive)
            return ("IconShow", "Downscaling");
        else if (type == ButtonType.MirrorFlipReady)
            return ("IconSettings", "Mirror Flip correction");
        else if (type == ButtonType.MirrorFlipActive)
            return ("IconShow", "Flipping");
        else
            return ("IconClose", "Dataset is still loading");
    }
    private void ChangeButtonLoadingState(ButtonConfigHelper button,Collider collider, ButtonType type)
    {
        var but = GetButtonInfo(type);

        button.MainLabelText = but.Item2;
        button.SetQuadIconByName(but.Item1);
    
        if ((type == ButtonType.DownscaleReady)|| (type == ButtonType.MirrorFlipReady))    
            collider.enabled = true;      
        else
            collider.enabled = false;      
    }
}
