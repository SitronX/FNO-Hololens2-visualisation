using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityVolumeRendering;

public class ScrollableButton : MonoBehaviour
{
    [SerializeField] SpriteRenderer _loadButtonSprite;
    [SerializeField] SpriteRenderer _removeButtonSprite;
    [SerializeField] SpriteRenderer _enableButtonSprite;
    [SerializeField] GameObject _placeableVolumePrefab;
    [SerializeField] GameObject _loadButton;
    [SerializeField] GameObject _qrActiveLabel;
    [SerializeField] GameObject _disableButton;
    [SerializeField] GameObject _enableButton;
    [SerializeField] GameObject _qrButton;

    ErrorNotifier _errorNotifier;
    float _buttonHoldTime = 1.0f;  //second
    bool _loadButtonPressed = false;
    Camera _mainCamera;
    bool _hasDatasetLoaded = false;

    

    public GameObject VolumeGameObject { get; set; }
    public string DatasetPath { get; set; } 
    public int ButtonIndex { get; set; }
    public DatasetLister ParentDatasetLister { get; set; }
    public Action<int> QrCodeDatasetActivated { get; set; }

    private void Start()
    {
        _errorNotifier=FindObjectOfType<ErrorNotifier>();
        _mainCamera = FindObjectOfType<Camera>();

    }

    public void ChangeBackButtonSprite(string path)
    {
        try
        {
            string[] imgFiles = Directory.GetFiles(path);
            _loadButtonSprite.sprite = IMG2Sprite.LoadNewSprite(imgFiles[0]);
            _removeButtonSprite.sprite = _loadButtonSprite.sprite;
            _enableButtonSprite.sprite = _loadButtonSprite.sprite;
        }
        catch
        {
            _errorNotifier.ShowErrorMessageToUser($"Dataset thumbnail is missing. Searched path: {path}");
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
            obj.LoadDatasetData(DatasetPath);

            _disableButton.gameObject.SetActive(true);
            _loadButton.SetActive(false);
            _hasDatasetLoaded= true;
            _qrButton.SetActive(true);
        }
    }
    public void TryUpdateQRVolume()
    {
        if(DatasetLister.Instance.ActiveQR==this)
        {
            QRDataSpawner qrPlaced = FindObjectOfType<QRDataSpawner>();
            if (qrPlaced != null)
            {
                if (VolumeGameObject == null)
                    LoadDataset();

                EnableVolume();
                qrPlaced.ChangeVolumeData(VolumeGameObject);
            }
        }
    }
    public void QrClicked()
    {
        QrCodeDatasetActivated?.Invoke(ButtonIndex);
    }
    public void SetQrActiveState(bool value)
    {
        if(_hasDatasetLoaded)
            _qrActiveLabel.SetActive(value);
       
    }
    public void DisableVolume()
    {
        VolumeGameObject.SetActive(false);
        _disableButton.SetActive(false);
        _enableButton.SetActive(true);
    }
    public void EnableVolume()
    {
        VolumeGameObject.SetActive(true);
        _disableButton.SetActive(true);
        _enableButton.SetActive(false);
    }
    public void LoadButtonPressed()
    {
        StartCoroutine(LoadButtonPressedCoroutine(0.1f));
    }
    private IEnumerator LoadButtonPressedCoroutine(float checkInterval)     //So user doesnt accidently click load dataset, here is coroutine so player must hold the button for specified time 
    {
        _loadButtonPressed = true;
        float time = 0;
        while(_loadButtonPressed)
        {
            time += checkInterval;

            if(time> _buttonHoldTime)
            {
                LoadDataset();
                TryUpdateQRVolume();
                _loadButtonPressed = false;
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }
    public void LoadButtonReleased()
    {
        _loadButtonPressed = false;
    }
}
