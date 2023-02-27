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

    [SerializeField] TargetPlatform _targetPlatform;
    [SerializeField] GameObject _remotingObject;
    [SerializeField] GameObject _volumeObjectPrefab;
    [SerializeField] GameObject _qrCodeManager;
    [SerializeField] Material _skyboxMaterial;
    [SerializeField] GameObject _hololensHandMenu;
    [SerializeField] GameObject _vrControllerMenu;
    [SerializeField] GameObject _vrNecessary;
    [SerializeField] GameObject _mainCamera;

    private void Awake()
    {
        if(_targetPlatform==TargetPlatform.Hololens2)
        {
            _hololensHandMenu.SetActive(true);
            _remotingObject.SetActive(true);
            _qrCodeManager.SetActive(true);
        }
        else if(_targetPlatform== TargetPlatform.PCVR)
        {
            _vrNecessary.SetActive(true);
            _vrControllerMenu.SetActive(true);

            RenderSettings.skybox = _skyboxMaterial;
        }
    }
}
