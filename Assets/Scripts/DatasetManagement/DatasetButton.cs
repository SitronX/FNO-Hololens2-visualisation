using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.IO;
using TMPro;
using UnityEngine;


public class DatasetButton : MonoBehaviour
{
    [SerializeField] MeshRenderer _loadButtonBackMesh;
    [SerializeField] GameObject _placeableVolumePrefab;
    [SerializeField] ButtonConfigHelper _configHelper;
    [SerializeField] Texture _defaultTexture;
    [SerializeField] GameObject _enablerObject;
    [SerializeField] TMP_Text _qrButtonText;
    [SerializeField] MeshRenderer _qrButtonMesh;

    Camera _mainCamera;

    [field: SerializeField] public Interactable LoadButton { get; set; }
    [field: SerializeField] public Interactable QrButton { get; set; }
    [field: SerializeField] public TMP_Text DatasetName { get; set; }
    public LoadButtonState ButtonState { private set; get; }
    public VolumeDataControl VolumeControlObject { get; set; }
    public Texture ThumbnailTexture { get; set; }
    public string DatasetPath { get; set; }
    public int ButtonIndex { get; set; }

    public static Action<DatasetButton> DatasetGrabbed { get; set; }

    public enum LoadButtonState
    {
        Selectable, ReadyToLoad, Active
    }

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
    public void LoadDataset()
    {      
        if (VolumeControlObject == null)
        {
            Vector3 rot = _mainCamera.transform.rotation.eulerAngles;
            rot.y += 90;
            rot.x= 0;
            rot.z = 0;

            GameObject spawned = Instantiate(_placeableVolumePrefab, _mainCamera.transform.position+(_mainCamera.transform.forward), Quaternion.Euler(rot));

            VolumeControlObject = spawned.GetComponent<VolumeDataControl>();
            ObjectManipulator manip= spawned.GetComponent<ObjectManipulator>();

            manip.OnManipulationStarted.AddListener((data)=> DatasetGrabbed?.Invoke(this));

            _enablerObject.SetActive(true);

            if (PlatformSpecific.Instance.CurrentPlatform == PlatformSpecific.TargetPlatform.Hololens2)
                QrButton.gameObject.SetActive(true);

            VolumeControlObject.LoadDatasetAsync(DatasetPath,ThumbnailTexture,DatasetName.text,_mainCamera);        
        }
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
    public void ResetClicked()
    {
        if (VolumeControlObject != null)
            VolumeControlObject.ResetAllTransforms();      
    }
    public void SetQrActiveState(bool value)
    {
        _qrButtonText.color=value ? Color.green : Color.white;
        _qrButtonMesh.sharedMaterial.color = value ? Color.green : Color.white;
    }
    public void SetDatasetActive(bool value)
    {
        VolumeControlObject.gameObject.SetActive(value);
    }
    
}
