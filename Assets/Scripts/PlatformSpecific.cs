using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Management;

public class PlatformSpecific : MonoBehaviour
{
    public enum TargetPlatform
    {
        Hololens2, Quest
    }

    [SerializeField] TargetPlatform _targetPlatform;
    [SerializeField] GameObject _prefabObject;
    [SerializeField] GameObject _qrCodeManager;
    [SerializeField] Material _skyboxMaterial;

    private void Awake()
    {
        if(_targetPlatform==TargetPlatform.Hololens2)
        {
            _qrCodeManager.SetActive(true);
        }
        else if(_targetPlatform== TargetPlatform.Quest)
        {
            RenderSettings.skybox = _skyboxMaterial;
            Instantiate(_prefabObject, new Vector3(6,1.2f,1.5f), Quaternion.Euler(new Vector3(0,-180,0)));
        }
    }
}
