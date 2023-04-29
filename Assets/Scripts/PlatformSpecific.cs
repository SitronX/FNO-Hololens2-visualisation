using System.Collections;
using UnityEngine;

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

    Camera _mainCamera;

    private void Start()
    {
        _mainCamera = FindObjectOfType<Camera>();

        if(_targetPlatform==TargetPlatform.Hololens2)
        {
            _qrCodeManager.SetActive(true);
        }
        else if(_targetPlatform== TargetPlatform.Quest)
        {
            RenderSettings.skybox = _skyboxMaterial;
            StartCoroutine(SpawnDelay());       
        }
    }
    IEnumerator SpawnDelay()        //There is a delay with camera update on xr initialization so we must wait for proper camera position for model spawn
    {
        while(_mainCamera.transform.position==Vector3.zero) //Wait until the camera position changes
            yield return new WaitForSeconds(0.1f);
            
        
        Vector3 fixedRot = _mainCamera.transform.rotation.eulerAngles;
        fixedRot.x -= 12;
        fixedRot.y += 100;
        Instantiate(_prefabObject, _mainCamera.transform.position + (_mainCamera.transform.forward), Quaternion.Euler(fixedRot));
    }
}
