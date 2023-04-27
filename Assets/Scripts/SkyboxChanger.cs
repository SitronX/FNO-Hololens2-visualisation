using Microsoft.MixedReality.Toolkit;
using UnityEngine;

//Changing skybox in runtime must be done via MixedRealityToolkit profile, it is not possible to update skybox material classically

public class SkyboxChanger : MonoBehaviour
{
    [SerializeField] MixedRealityToolkit _toolkit;
    [SerializeField] MixedRealityToolkitConfigurationProfile _skybox;
    [SerializeField] MixedRealityToolkitConfigurationProfile _darkColor;
    [SerializeField] MixedRealityToolkitConfigurationProfile _lightColor;
    [SerializeField] MixedRealityToolkitConfigurationProfile _noSkybox;


    public enum SkyboxType
    {
        Classic, Dark, Light, NoSkybox
    }
    private void Update()
    {
        if (PlatformSpecific.Instance.CurrentPlatform == PlatformSpecific.TargetPlatform.PCVR)  //On hololens changing background is blocked
        {
            if (Input.GetKeyDown(KeyCode.F2))
                ChangeSkybox(SkyboxType.Classic);
            if (Input.GetKeyDown(KeyCode.F3))
                ChangeSkybox(SkyboxType.Dark);
            if (Input.GetKeyDown(KeyCode.F4))
                ChangeSkybox(SkyboxType.Light);
        }
    }
    public void ChangeSkybox(SkyboxType type)
    {
        if (type==SkyboxType.Classic)
            _toolkit.ActiveProfile = _skybox;
        else if (type == SkyboxType.Dark)
            _toolkit.ActiveProfile = _darkColor;
        else if (type == SkyboxType.Light)
            _toolkit.ActiveProfile = _lightColor;
        else if (type == SkyboxType.NoSkybox)
            _toolkit.ActiveProfile = _noSkybox;
    }
}
