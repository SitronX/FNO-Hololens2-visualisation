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

    private void Awake()
    {
        if(_targetPlatform==TargetPlatform.Hololens2)
        {
            _remotingObject.SetActive(true);
            _qrCodeManager.SetActive(true);
        }
        else if(_targetPlatform== TargetPlatform.PCVR)
        {
            RenderSettings.skybox = _skyboxMaterial;
            Instantiate(_volumeObjectPrefab, new Vector3(6,1.2f,1.5f), Quaternion.Euler(new Vector3(0,-180,0)));
        }
    }
}
