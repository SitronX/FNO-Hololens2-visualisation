using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityVolumeRendering;

public class ScrollableButton : MonoBehaviour
{
    [SerializeField] MeshRenderer _loadButtonBackMesh;
    [SerializeField] GameObject _placeableVolumePrefab;
    [SerializeField] GameObject _loadButton;
    [SerializeField] GameObject _qrActiveLabel;
    [SerializeField] GameObject _qrButton;
    [SerializeField] ButtonConfigHelper _configHelper;
    [SerializeField] TMP_Text _datasetName;
    Camera _mainCamera;
    bool _hasDatasetLoaded = false;

    public enum LoadButtonState
    {
        Selectable,ReadyToLoad,Active
    }
    public LoadButtonState ButtonState { private set; get; }

    public GameObject VolumeGameObject { get; set; }
    public string DatasetPath { get; set; } 
    public int ButtonIndex { get; set; }
    public DatasetLister ParentDatasetLister { get; set; }
    public Action<int> QrCodeDatasetActivated { get; set; }
    public Action<int> LoadButtonPressed { get; set; }

    private void Start()
    {
        _mainCamera = FindObjectOfType<Camera>();

    }

    public void SetNameSprite(string spriteFolder,string name)
    {
        try
        {
            if (!Directory.Exists(spriteFolder))
            {
                ErrorNotifier.Instance.AddErrorMessageToUser($"Thumbnail folder missing for dataset named: {name}!");
                return;
            }

            string[] imgFiles = Directory.GetFiles(spriteFolder);
            _loadButtonBackMesh.material.mainTexture = IMG2Sprite.LoadTexture(imgFiles[0]);
            _datasetName.text = name;
        }
        catch
        {
            ErrorNotifier.Instance.AddErrorMessageToUser($"Thumbnail is missing for dataset named: {name} (.jpg or .png expected)");
        }
    }
    public void LoadDataset()
    {      
        if (VolumeGameObject == null)
        {
            Vector3 rot = _mainCamera.transform.rotation.eulerAngles;
            rot.y += 90;
            rot.x= 0;
            rot.z = 0;

            VolumeGameObject = Instantiate(_placeableVolumePrefab, _mainCamera.transform.position+(_mainCamera.transform.forward), Quaternion.Euler(rot));
            VolumeDataControl obj= VolumeGameObject.GetComponent<VolumeDataControl>();
            obj.LoadDataset(DatasetPath);

            _hasDatasetLoaded= true;
            _qrButton.SetActive(true);
        }
    }
    public void ButtonPressed()
    {
        LoadButtonPressed?.Invoke(ButtonIndex);
    }
    public void SetButtonState(LoadButtonState state)
    {
        if (state != ButtonState)
        {
            ButtonState= state;

            if (state == LoadButtonState.Selectable)
            {
                _configHelper.MainLabelText = "Select";
                _configHelper.SetQuadIconByName("IconHandJoint");
            }
            else if (state == LoadButtonState.ReadyToLoad)
            {
                _configHelper.MainLabelText = "Press again to load";
                _configHelper.SetQuadIconByName("IconAdd");
            }
            else if (state == LoadButtonState.Active)
            {
                _configHelper.MainLabelText = "Reset";
                _configHelper.SetQuadIconByName("IconRefresh");
            }
        }
    }
    public void TryUpdateQRVolume()
    {
        if(DatasetLister.Instance.ActiveQR==this)
        {
            QRDataSpawner qrPlaced = FindObjectOfType<QRDataSpawner>();
            if (qrPlaced != null)
            {
                VolumeGameObject.SetActive(true);
                SetButtonState(LoadButtonState.Active);
                qrPlaced.ChangeVolumeData(VolumeGameObject);
            }
        }
    }
    public void QrClicked()
    {
        QrCodeDatasetActivated?.Invoke(ButtonIndex);
    }
    public void ResetClicked()
    {
        if (VolumeGameObject != null)
            VolumeGameObject.GetComponent<VolumeDataControl>().ResetAllTransforms();      
    }
    public void SetQrActiveState(bool value)
    {
        if(_hasDatasetLoaded)
            _qrActiveLabel.SetActive(value);     
    }
    
}
