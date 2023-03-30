using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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


    ScrollableButton _latestDatasetData;


    public enum ButtonState
    {
        Processing,Ready,DatasetStillLoading
    }
    private void Start()
    {
        ScrollableButton.DatasetGrabbed += OnLatestDatasetGrabbed;
        VolumeDataControl.DatasetSpawned += OnVolumeDatasetSpawned;

    }
    private void OnDisable()
    {
        ScrollableButton.DatasetGrabbed -= OnLatestDatasetGrabbed;
        VolumeDataControl.DatasetSpawned -= OnVolumeDatasetSpawned;
    }
    public void OnLatestDatasetGrabbed(ScrollableButton button)
    {
        if (button.VolumeControlObject.HasBeenLoaded)
            ChangeButtonLoadingState(_downsamplingButton,_downsamplingButtonCollider,ButtonState.Ready, "IconProfiler","Downscale Dataset");
        else
            ChangeButtonLoadingState(_downsamplingButton,_downsamplingButtonCollider, ButtonState.DatasetStillLoading, "IconClose","Dataset is still loading");

        if (button.VolumeControlObject.HasBeenLoaded)
            ChangeButtonLoadingState(_mirrorFlipButton,_mirrorButtonCollider, ButtonState.Ready, "IconSettings", "Mirror Flip correction");
        else
            ChangeButtonLoadingState(_mirrorFlipButton,_mirrorButtonCollider, ButtonState.DatasetStillLoading, "IconClose", "Dataset is still loading");

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
        ScrollableButton but = DatasetLister.Instance.AllButtons.Find(x => x.VolumeControlObject == control);
        OnLatestDatasetGrabbed(but);
    }
    public async void DownscaleDataset()
    {
        ChangeButtonLoadingState(_downsamplingButton,_downsamplingButtonCollider, ButtonState.Processing, "IconShow", "Downscaling");
        ChangeButtonLoadingState(_mirrorFlipButton, _mirrorButtonCollider, ButtonState.Processing, "IconShow", "Downscaling");

        await _latestDatasetData.VolumeControlObject.DownScaleDataset();

        ChangeButtonLoadingState(_downsamplingButton,_downsamplingButtonCollider, ButtonState.Ready, "IconProfiler", "Downscale Dataset");
        ChangeButtonLoadingState(_mirrorFlipButton, _mirrorButtonCollider, ButtonState.Ready, "IconSettings", "Mirror Flip correction");

        OnLatestDatasetGrabbed(_latestDatasetData);
    }
    public async void MirrorFlipDataset()
    {
        ChangeButtonLoadingState(_mirrorFlipButton,_mirrorButtonCollider, ButtonState.Processing, "IconShow", "Flipping");
        ChangeButtonLoadingState(_downsamplingButton, _downsamplingButtonCollider, ButtonState.Processing, "IconShow", "Flipping");

        await _latestDatasetData.VolumeControlObject.MirrorFlipTextures();

        ChangeButtonLoadingState(_mirrorFlipButton,_mirrorButtonCollider, ButtonState.Ready, "IconSettings", "Mirror Flip correction");
        ChangeButtonLoadingState(_downsamplingButton, _downsamplingButtonCollider, ButtonState.Ready, "IconProfiler", "Downscale Dataset");

        OnLatestDatasetGrabbed(_latestDatasetData);
    }
    public void ChangeButtonLoadingState(ButtonConfigHelper button,Collider collider, ButtonState state, string iconName, string message)
    {
        if(state== ButtonState.Processing)
        {
            button.MainLabelText = message;
            button.SetQuadIconByName(iconName);
            collider.enabled = false;
        }    
        else if(state== ButtonState.Ready)
        {
            button.MainLabelText = message;
            button.SetQuadIconByName(iconName);
            collider.enabled = true;
        }
        else
        {
            button.MainLabelText = message;
            button.SetQuadIconByName(iconName);
            collider.enabled = false;
        }
    }
}
