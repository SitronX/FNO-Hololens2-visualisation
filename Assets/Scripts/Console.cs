using Microsoft.MixedReality.OpenXR.BasicSample;
using Microsoft.MixedReality.SampleQRCodes;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using QFSW.QC;
using QFSW.QC.Suggestors.Tags;
using QRTracking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CommandPrefix(".")]
public class Console : MonoBehaviour, IQcSuggestor
{
    [SerializeField] QRCodesManager _qrCodeManager;
    [SerializeField] QuantumConsole _quantumConsole;
    [SerializeField] GameObject _prefab;
    [SerializeField] QRCodesVisualizer _qrVisualizer;
    [SerializeField] MixedRealityToolkitConfigurationProfile _wireFrame;
    [SerializeField] MixedRealityToolkitConfigurationProfile _default;
    [SerializeField] MixedRealityToolkitConfigurationProfile _diagnostics;
    [SerializeField] MixedRealityToolkitConfigurationProfile _noSpatial;
    [SerializeField] MixedRealityToolkit _toolkit;
    [SerializeField] ErrorNotifier _errorNotifier;

    public enum MRTKModule
    {
        WireFrame,Default,Diagnostics,NoSpatial
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
    public void SpawnModel(int index)
    {
        try
        {
            DatasetLister.Instance.AllButtons[index].LoadDataset();
        }
        catch
        {
            _errorNotifier.AddErrorMessageToUser("Hand menu must be initialized before spawning anything");
        }
    }
    [Command]
    public void SetQr(int index)
    {
        DatasetLister.Instance.OnAnyQrActivated(index);
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
    [Command]
    public void SetModule(MRTKModule module)
    {
        if (module == MRTKModule.WireFrame)
            _toolkit.ActiveProfile = _wireFrame;
        else if (module == MRTKModule.Default)
            _toolkit.ActiveProfile = _default;
        if (module == MRTKModule.Diagnostics)
            _toolkit.ActiveProfile = _diagnostics;
        if (module == MRTKModule.NoSpatial)
            _toolkit.ActiveProfile = _noSpatial;
    }
    [Command("QRUpdateState")]
    public void EnableQRUpdate(bool value)
    {
        FindObjectsOfType<MonoBehaviour>().OfType<IQRUpdate>().ToList().ForEach(x => x.EnableQRUpdate(value));
    }
    [Command]
    public void Quality(int numberOfSteps)
    {
        FindObjectOfType<VolumeDataControl>().SetRaymarchStepCount(numberOfSteps);
    }
    [Command]
    public void Disconnect()
    {
        FindObjectOfType<AppRemotingSample>().OnDisconnectButtonPressed();
    }
    [Command]
    public void SetTransferFunction(string tf)
    {
        FindObjectsOfType<VolumeDataControl>().ToList().ForEach(x=>x.SetTransferFunction(tf));
    }
    [Command]
    public void ResetObjectsTransform()
    {
        FindObjectsOfType<VolumeDataControl>().ToList().ForEach(x=>x.ResetAllTransforms());
    }
    [Command]
    public void SetVolumePosition(Vector3 position,int volumeIndex)
    {
        FindObjectsOfType<VolumeDataControl>().ToList()[volumeIndex].SetVolumePosition(position);
    }

    public IEnumerable<IQcSuggestion> GetSuggestions(SuggestionContext context, SuggestorOptions options)
    {
        if (context.TargetType == typeof(string))
        {
            foreach (string i in VolumeDataControl.TF1D)
                yield return new RawSuggestion(i);
            foreach (string i in VolumeDataControl.TF2D)
                yield return new RawSuggestion(i);
        }
    }
}
