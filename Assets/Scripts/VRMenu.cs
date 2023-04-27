using Microsoft.MixedReality.Toolkit;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class VRMenu : MonoBehaviour
{
    [SerializeField] InputData _inputData;
    [SerializeField] GameObject _vrControllerMenu;
    [SerializeField] GameObject _buttonClose;
    [SerializeField] GameObject _menuEnablerObject;

    bool _previousButtonPressedState = false;
    bool _isMenuAnchored = false;

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
            if(_previousButtonPressedState&&!_isMenuAnchored)
                DeactivateVrMenu();

            _previousButtonPressedState = false;
        }
    }
    public void ActivateVrMenu()
    {
        GameObject leftController = CoreServices.InputSystem.DetectedControllers.FirstOrDefault(x => x.ControllerHandedness == Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left).Visualizer.GameObjectProxy;

        if(leftController != null)
        {
            _vrControllerMenu.transform.parent = leftController.transform;
            _vrControllerMenu.transform.localPosition = new Vector3(-0.16f, 0, -0.05f);
            _vrControllerMenu.transform.localRotation = Quaternion.Euler(new Vector3(90, -30, 150));
            _vrControllerMenu.transform.localScale = Vector3.one;

            _menuEnablerObject.SetActive(true);
            _buttonClose.SetActive(false);
        }    
    }
    public void DeactivateVrMenu()
    {
        _vrControllerMenu.transform.parent = null;
        _menuEnablerObject.SetActive(false);
        _isMenuAnchored = false;
    }
    public void UnparrentVrMenu()
    {
        _vrControllerMenu.transform.parent = null;
        _isMenuAnchored = true;
        _buttonClose.SetActive(true);
    }  
}
