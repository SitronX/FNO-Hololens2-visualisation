using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityVolumeRendering;

public class DatasetButton : MonoBehaviour
{
    [SerializeField] MeshRenderer _loadButtonBackMesh;
    [SerializeField] GameObject _placeableVolumePrefab;
    [SerializeField] GameObject _loadButton;
    [SerializeField] GameObject _qrActiveLabel;
    [SerializeField] GameObject _qrButton;
    [SerializeField] ButtonConfigHelper _configHelper;
    [SerializeField] Texture _defaultTexture;
    [SerializeField] GameObject _enablerObject;


    [field: SerializeField] public TMP_Text DatasetName { get; set; }
    Camera _mainCamera;
    bool _hasDatasetLoaded = false;

    public enum LoadButtonState
    {
        Selectable,ReadyToLoad,Active
    }
    public LoadButtonState ButtonState { private set; get; }

    public VolumeDataControl VolumeControlObject { get; set; }

    public Texture ThumbnailTexture { get; set; }
    public string DatasetPath { get; set; }
    public int ButtonIndex { get; set; }
    public Action<int> QrCodeDatasetActivated { get; set; }
    public Action<int> LoadButtonPressed { get; set; }

    public static Action<DatasetButton> DatasetGrabbed { get; set; }

    private void Start()
    {
        _mainCamera = FindObjectOfType<Camera>();
    }

    public void SetNameSprite(string spriteFolder,string name)
    {
     
        if (Directory.Exists(spriteFolder))
        {
            string[] imgFiles = Directory.GetFiles(spriteFolder);
            if(imgFiles.Length > 0)
            {
                ThumbnailTexture = IMG2Sprite.LoadTexture(imgFiles[0]);
                _loadButtonBackMesh.material.mainTexture = ThumbnailTexture;
                DatasetName.text = name;
                return;
            }         
        }
        ThumbnailTexture = _defaultTexture;
        _loadButtonBackMesh.material.mainTexture = ThumbnailTexture;
        DatasetName.text = name;
    }
    public async Task LoadDatasetAsync()
    {      
        if (VolumeControlObject == null)
        {
            Vector3 rot = _mainCamera.transform.rotation.eulerAngles;
            rot.y += 90;
            rot.x= 0;
            rot.z = 0;

            GameObject tmp = Instantiate(_placeableVolumePrefab, _mainCamera.transform.position+(_mainCamera.transform.forward), Quaternion.Euler(rot));

            VolumeControlObject = tmp.GetComponent<VolumeDataControl>();
            ObjectManipulator manip= tmp.GetComponent<ObjectManipulator>();
            manip.OnManipulationStarted.AddListener(OnManipulationDatasetStarted);

            await VolumeControlObject.LoadDatasetAsync(DatasetPath,ThumbnailTexture,DatasetName.text,_mainCamera);
          
            _hasDatasetLoaded= true;
            _enablerObject.SetActive(true);

            if(PlatformSpecific.Instance.CurrentPlatform==PlatformSpecific.TargetPlatform.Hololens2)
                _qrButton.SetActive(true);
        }
    }

    public void OnManipulationDatasetStarted(ManipulationEventData data)
    {
        DatasetGrabbed?.Invoke(this);
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
                _configHelper.SetQuadIconByName("IconHandMesh");
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
        if(HandMenu.Instance.ActiveQRDataset==this)
        {
            QRDataSpawner qrPlaced = FindObjectOfType<QRDataSpawner>();
            if (qrPlaced != null)
            {
                SetButtonState(LoadButtonState.Active);
                qrPlaced.ChangeVolumeData(VolumeControlObject);
            }
        }
    }
    public void QrClicked()
    {
        QrCodeDatasetActivated?.Invoke(ButtonIndex);
    }
    public void ResetClicked()
    {
        if (VolumeControlObject != null)
            VolumeControlObject.ResetAllTransforms();      
    }
    public void SetQrActiveState(bool value)
    {
        if(_hasDatasetLoaded)
            _qrActiveLabel.SetActive(value);     
    }
    public void SetDatasetActive(bool value)
    {
        VolumeControlObject.gameObject.SetActive(value);
    }
    
}
