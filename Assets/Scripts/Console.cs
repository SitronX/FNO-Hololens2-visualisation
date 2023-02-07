using Microsoft.MixedReality.Toolkit.Experimental.UI;
using QFSW.QC;
using QRTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Console : MonoBehaviour
{
    [SerializeField] QRCodesManager _qrCodeManager;
    [SerializeField] QuantumConsole _quantumConsole;

    [Command]
    public void StartQR()
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
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            OpenConsole();
        }
    }
}
