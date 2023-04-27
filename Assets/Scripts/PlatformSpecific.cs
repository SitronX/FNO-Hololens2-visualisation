using UnityEngine;

public class PlatformSpecific : MonoBehaviour
{
    [field: SerializeField] public TargetPlatform CurrentPlatform { get; set; }

    [SerializeField] GameObject _remotingObject;
    [SerializeField] GameObject _volumeObjectPrefab;
    [SerializeField] GameObject _qrCodeManager;
    [SerializeField] Material _skyboxMaterial;
    [SerializeField] GameObject _hololensHandMenu;
    [SerializeField] GameObject _vrControllerMenu;
    [SerializeField] GameObject _vrNecessary;
    [SerializeField] GameObject _mainCamera;
    [SerializeField] SkyboxChanger _skyboxChanger;

    public static PlatformSpecific Instance { get; private set; }       //Singleton

    public enum TargetPlatform
    {
        Hololens2, PCVR
    }
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
            _skyboxChanger.ChangeSkybox(SkyboxChanger.SkyboxType.Classic);
        }
    }
}
