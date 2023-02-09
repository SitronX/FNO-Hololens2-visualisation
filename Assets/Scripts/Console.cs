using Microsoft.MixedReality.SampleQRCodes;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using QFSW.QC;
using QFSW.QC.Suggestors.Tags;
using QRTracking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Console : MonoBehaviour
{
    [SerializeField] QRCodesManager _qrCodeManager;
    [SerializeField] QuantumConsole _quantumConsole;
    [SerializeField] GameObject _prefab;
    [SerializeField] QRCodesVisualizer _qrVisualizer;

    [Command]
    public void InitQR()
    {
        _qrCodeManager.SetupQRTracking();
    }
    public void OpenConsole()
    {
        if (!_quantumConsole.IsActive)
        {
            _quantumConsole.Activate(true);
        }
        else
        {
            _quantumConsole.Deactivate();
        }
    }
    [Command]
    public void SpawnModel()
    {
        Instantiate(_prefab, Vector3.zero, Quaternion.identity);
    }
    [Command]
    public void Quit()
    {
        Application.Quit();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            OpenConsole();
        }
    }
    [Command("QRUpdateState")]
    public void EnableQRUpdate(bool value)
    {
        FindObjectsOfType<MonoBehaviour>().OfType<IQRUpdateDisable>().ToList().ForEach(x => x.EnableQRUpdate(value));
    }
    [Command]
    public void Quality(int numberOfSteps)
    {
        GameObject.FindGameObjectWithTag("VolumeObject").GetComponent<MeshRenderer>().sharedMaterial.SetInt("_stepNumber", numberOfSteps);
    }
}
