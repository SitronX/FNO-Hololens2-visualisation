using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class HandMenu : MonoBehaviour
{
    [SerializeField] InteractableToggleCollection _renderModes;
    [SerializeField] TMP_Text _raymarchStepLabel;
    [SerializeField] Animator _additionalSettingAnimator;
    [SerializeField] InteractableToggleCollection _crossSectionModes;
    [SerializeField] PinchSlider _raymarchSlider;
    [SerializeField] GameObject _qrUpdateButton;
    [SerializeField] TMP_Text _dayUpdate;
    [SerializeField] GameObject _datasetButtonPrefab;
    [SerializeField] GridObjectCollection _gridObjectCollection;
    [SerializeField] ScrollingObjectCollection _scrollingObjectCollection;
    [SerializeField] TMP_Text _version;
    [SerializeField] string _lastUpdateText;

    bool _showCutPlane = false;
    bool _useCubicInterpolation = false;
    bool _useLighting = false;
    bool _additionalSettingShown = false;
    bool _qrUpdatesEnabled = true;

    public List<DatasetButton> AllDatasetButtons { get; set; } = new List<DatasetButton>();
    public DatasetButton ActiveQRDataset { get; set; }
    public static HandMenu Instance { get; private set; }              //Singleton

    IEnumerator Start()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        _version.text =$"Version: {Application.version}";
        _dayUpdate.text =$"Last Update {_lastUpdateText}";

        List<string> _datasetDirectories = Directory.EnumerateDirectories(Application.streamingAssetsPath + "/Datasets/").ToList();

        for (int i = 0; i < _datasetDirectories.Count; i++)
        {
            GameObject current = Instantiate(_datasetButtonPrefab, _gridObjectCollection.transform);
            DatasetButton currentScroll = current.GetComponent<DatasetButton>();

            currentScroll.SetNameSprite(_datasetDirectories[i] + "/Thumbnail/", _datasetDirectories[i].Split('/').Last());
            currentScroll.DatasetPath = _datasetDirectories[i];
            currentScroll.ButtonIndex = i;

            currentScroll.QrButton.OnClick.AddListener(()=> OnAnyQrActivated(currentScroll.ButtonIndex));
            currentScroll.LoadButton.OnClick.AddListener(()=> OnDatasetLoadButtonClicked(currentScroll.ButtonIndex)); 

            AllDatasetButtons.Add(currentScroll);
        }

        LightingUpdated();
        UpdateQrStatus();

        VolumeDataControl.DatasetSpawned += OnNewDatasetSpawned;
        QRDataSpawner.QrCodeSpawned += UpdateQrStatus;

        yield return new WaitForEndOfFrame();
        _gridObjectCollection.UpdateCollection();
        yield return new WaitForEndOfFrame();
        _scrollingObjectCollection.UpdateContent();     //This needs to be here in coroutine due this to bug : https://github.com/microsoft/MixedRealityToolkit-Unity/issues/10350
    } 
    private void OnDestroy()
    {
        VolumeDataControl.DatasetSpawned -= OnNewDatasetSpawned;
        QRDataSpawner.QrCodeSpawned -= UpdateQrStatus;
    }
    private void OnNewDatasetSpawned(VolumeDataControl volumeDataControl)
    {
        volumeDataControl.UpdateSlicePlane(_showCutPlane);                              //Check if all volume objects are set same as handmenu settings
        volumeDataControl.UpdateCubicInterpolation(_useCubicInterpolation);
        volumeDataControl.UpdateLighting(_useLighting);
        volumeDataControl.UpdateIsoRanges();

        UpdateRenderingMode();
        ChangeCrossSectionType();
        UpdateRaymarchSteps();
    }
    public void UpdateRenderingMode()
    {
        UnityVolumeRendering.RenderMode mode = _renderModes.CurrentIndex == 0 ? UnityVolumeRendering.RenderMode.DirectVolumeRendering : _renderModes.CurrentIndex == 1 ? UnityVolumeRendering.RenderMode.MaximumIntensityProjectipon : UnityVolumeRendering.RenderMode.IsosurfaceRendering;
         AllDatasetButtons.ForEach(_x => { if ((_x.VolumeControlObject != null) && _x.VolumeControlObject.HasBeenLoaded) _x.VolumeControlObject.UpdateRenderingMode(mode); });
    }
    public void LightingUpdated()
    {
        _useLighting = !_useLighting;
        AllDatasetButtons.ForEach(_x => { if ((_x.VolumeControlObject != null) && _x.VolumeControlObject.HasBeenLoaded) _x.VolumeControlObject.UpdateLighting(_useLighting); });
    }
    public void SlicePlaneUpdated()
    {
        _showCutPlane=!_showCutPlane;
        AllDatasetButtons.ForEach(_x => { if ((_x.VolumeControlObject != null) && _x.VolumeControlObject.HasBeenLoaded) _x.VolumeControlObject.UpdateSlicePlane(_showCutPlane); });
    }
    public void CubicInterpolationUpdated()
    {
        _useCubicInterpolation=!_useCubicInterpolation;
        AllDatasetButtons.ForEach(_x => { if ((_x.VolumeControlObject != null) && _x.VolumeControlObject.HasBeenLoaded) _x.VolumeControlObject.UpdateCubicInterpolation(_useCubicInterpolation); });
    }
    public void UpdateRaymarchSteps()
    {
        int steps = (int)(_raymarchSlider.SliderValue * 1000);
        _raymarchStepLabel.text = $"{steps} Steps";

        AllDatasetButtons.ForEach(_x => { if ((_x.VolumeControlObject != null) && _x.VolumeControlObject.HasBeenLoaded) _x.VolumeControlObject.SetRaymarchStepCount(steps); });
    }

    public void UpdateAdditionalSettings()
    {
        _additionalSettingShown = !_additionalSettingShown;
        _additionalSettingAnimator.SetBool("AdditionalSettings", _additionalSettingShown);
    }
    public void ChangeCrossSectionType()
    {
        UnityVolumeRendering.CrossSectionType type = _crossSectionModes.CurrentIndex == 0 ? UnityVolumeRendering.CrossSectionType.Plane : _crossSectionModes.CurrentIndex == 1 ? UnityVolumeRendering.CrossSectionType.SphereInclusive : UnityVolumeRendering.CrossSectionType.SphereExclusive;
        AllDatasetButtons.ForEach(_x => { if ((_x.VolumeControlObject != null) && _x.VolumeControlObject.HasBeenLoaded) _x.VolumeControlObject.SetCrossSectionType(type); });
    }
    public void ChangeQRUpdates()
    {
        _qrUpdatesEnabled= !_qrUpdatesEnabled;
        UpdateQrStatus();
    }
    public void UpdateQrStatus()
    {
        foreach (IQRUpdate i in FindObjectsOfType<MonoBehaviour>().OfType<IQRUpdate>())
            i.EnableQRUpdate(_qrUpdatesEnabled);
    }
    public void EnableQRButton(bool value)
    {
        _qrUpdateButton.SetActive(value);
    }

    public void OnAnyQrActivated(int index)
    {
        for (int i = 0; i < AllDatasetButtons.Count; i++)
        {
            if (i == index)
            {
                ActiveQRDataset = AllDatasetButtons[i];
                AllDatasetButtons[i].SetQrActiveState(true);
                AllDatasetButtons[i].TryUpdateQRVolume();
            }
            else
            {
                AllDatasetButtons[i].SetQrActiveState(false);
            }
        }
    }
    private void OnDatasetLoadButtonClicked(int index)
    {
        for (int i = 0; i < AllDatasetButtons.Count; i++)
        {
            if (i == index)
            {
                DatasetButton button = AllDatasetButtons[i];

                if (button.ButtonState == DatasetButton.LoadButtonState.Selectable)
                {
                    button.SetButtonState(DatasetButton.LoadButtonState.ReadyToLoad);
                }
                else if (button.ButtonState == DatasetButton.LoadButtonState.ReadyToLoad)
                {
                    button.SetButtonState(DatasetButton.LoadButtonState.Active);
                    button.LoadDataset();
                }
                else if (button.ButtonState == DatasetButton.LoadButtonState.Active)
                {
                    button.ResetClicked();
                }
            }
            else
            {
                if (AllDatasetButtons[i].ButtonState == DatasetButton.LoadButtonState.ReadyToLoad)
                {
                    AllDatasetButtons[i].SetButtonState(DatasetButton.LoadButtonState.Selectable);
                }
            }
        }
    }
}
