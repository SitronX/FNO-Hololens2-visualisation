using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class VRMenu : MonoBehaviour
{
    [SerializeField] InputData _inputData;
    [SerializeField] GameObject _vrControllerMenu;
    [SerializeField] GameObject _buttonClose;

    bool _previousButtonPressedState = false;
    bool _isMenuParented = false;

    private void Update()
    {
        _inputData._leftController.TryGetFeatureValue(CommonUsages.primaryButton, out bool isButtonPressed);
        if (isButtonPressed)
        {
            if (!_previousButtonPressedState)
                ActivateVrMenu();

            _previousButtonPressedState = true;
        }
        else if(!isButtonPressed)
        {
            if(_previousButtonPressedState&&_isMenuParented)
                DeactivateVrMenu();

            _previousButtonPressedState = false;
        }
    }
    public void ActivateVrMenu()
    {
        GameObject controllerObject = FindObjectsOfType<WindowsMixedRealityControllerVisualizer>().First(x => x.Handedness == Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left).gameObject;

        _vrControllerMenu.transform.parent = controllerObject.transform;
        _vrControllerMenu.transform.localPosition = new Vector3(-0.16f, 0, -0.05f);
        _vrControllerMenu.transform.localRotation = Quaternion.Euler(new Vector3(90, -30, 150));
        _vrControllerMenu.transform.localScale = Vector3.one;

        _vrControllerMenu.transform.GetChild(0).gameObject.SetActive(true);
        _isMenuParented = true;
        _buttonClose.SetActive(false);
    }
    public void DeactivateVrMenu()
    {
        _vrControllerMenu.transform.parent = null;
        _vrControllerMenu.transform.GetChild(0).gameObject.SetActive(false);
        _isMenuParented = false;
    }
    public void UnparrentVrMenu()
    {
        _vrControllerMenu.transform.parent = null;
        _isMenuParented = false;
        _buttonClose.SetActive(true);
    }
}
