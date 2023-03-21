using Microsoft.MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Changing skybox in runtime must be done via MixedRealityToolkit profile, it is not possible to update skybox material classically

public class SkyboxChanger : MonoBehaviour
{
    [SerializeField] MixedRealityToolkit _toolkit;
    [SerializeField] MixedRealityToolkitConfigurationProfile _skybox;
    [SerializeField] MixedRealityToolkitConfigurationProfile _darkColor;
    [SerializeField] MixedRealityToolkitConfigurationProfile _lightColor;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
            _toolkit.ActiveProfile = _skybox;
        if (Input.GetKeyDown(KeyCode.F3))
            _toolkit.ActiveProfile = _darkColor;
        if (Input.GetKeyDown(KeyCode.F4))
            _toolkit.ActiveProfile = _lightColor;
    }


}
