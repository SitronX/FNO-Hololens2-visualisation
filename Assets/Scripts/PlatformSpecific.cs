using QRTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Management;

public class PlatformSpecific : MonoBehaviour
{
    public enum TargetPlatform
    {
        Hololens2, PCVR
    }

    [field:SerializeField] public TargetPlatform CurrentPlatform { get; set; }
    [SerializeField] GameObject _remotingObject;
    [SerializeField] GameObject _volumeObjectPrefab;
    [SerializeField] GameObject _qrCodeManager;
    [SerializeField] Material _skyboxMaterial;
    [SerializeField] GameObject _hololensHandMenu;
    [SerializeField] GameObject _vrControllerMenu;
    [SerializeField] GameObject _vrNecessary;
    [SerializeField] GameObject _mainCamera;

    public static PlatformSpecific Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        if (CurrentPlatform == TargetPlatform.Hololens2)
        {
            _hololensHandMenu.SetActive(true);
            _remotingObject.SetActive(true);
            _qrCodeManager.SetActive(true);           
        }
        else if(CurrentPlatform == TargetPlatform.PCVR)
        {
            _vrNecessary.SetActive(true);
            _vrControllerMenu.SetActive(true);
            _vrControllerMenu.GetComponent<HandMenu>().EnableQRButton(false);
            RenderSettings.skybox = _skyboxMaterial;
        }
    }
}
