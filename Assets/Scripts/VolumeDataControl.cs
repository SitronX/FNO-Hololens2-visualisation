using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.UI;
using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityVolumeRendering;
using RenderMode = UnityVolumeRendering.RenderMode;


public class VolumeDataControl : MonoBehaviour
{
    [SerializeField] VolumeRenderedObject _volumeData;
    [SerializeField] InteractableToggleCollection _renderModes;
    [SerializeField] MeshRenderer _blackPlaneRenderer;
    [SerializeField] PinchSlider _isoValueSlider;
    [SerializeField] PinchSlider _isoRangeSlider;
    [SerializeField] QuantumConsole _quantumConsole;
    [SerializeField] TMP_InputField _consoleInputField;
    [SerializeField] GameObject _volumetricDataMainParentObject;
    [SerializeField] GameObject _slicingPlaneObject;

    bool _showCutPlane = false;
    bool _useCubicInterpolation = false;
    bool _consoleOpened = false;
    Vector3 _startLocalPosition;
    Vector3 _startLocalRotation;
    Vector3 _startLocalScale;

    Vector3 _startLocalPlanePosition;
    Vector3 _startLocalPlaneRotation;
    Vector3 _startLocalPlaneScale;

    private void Start()
    {
        string filePath = Application.dataPath + "/TempDicom/";
        string transferFunctionPath = Application.dataPath + "/TempTransferFunction/default.tf";

        //#if ENABLE_WINMD_SUPPORT
        //            filePath = Windows.Storage.KnownFolders.DocumentsLibrary.Path+"\\DICOM\\";
        //            transferFunctionPath=Windows.Storage.KnownFolders.DocumentsLibrary.Path+"\\TRANSFERFC\\default.tf";
        //#endif
        //        
        //
        //TransferFunction transferFunction = TransferFunctionDatabase.LoadTransferFunctionFromResources(transferFunctionPath);

        TransferFunction transferFunction = TransferFunctionDatabase.LoadTransferFunction(transferFunctionPath);
     
        VolumeRenderedObject volumeRenderedObject = _volumetricDataMainParentObject.GetComponent<VolumeRenderedObject>();
     
        NonNativeKeyboard.Instance.OnTextUpdated += ConsoleTextUpdate;
     
        ImageSequenceFormat imgSeqFormat = ImageSequenceFormat.DICOM;
     
        IEnumerable<string> fileCandidates = Directory.EnumerateFiles(filePath, "*.*", SearchOption.TopDirectoryOnly)
                            .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase));
     
     
        if (fileCandidates.Any())
        {
            IImageSequenceImporter importer = ImporterFactory.CreateImageSequenceImporter(imgSeqFormat);
            IEnumerable<IImageSequenceSeries> seriesList = importer.LoadSeries(fileCandidates);                 //Long load
            float numVolumesCreated = 0;
     
            foreach (IImageSequenceSeries series in seriesList)
            {
                VolumeDataset dataset = importer.ImportSeries(series);                                          //Long load
                if (dataset != null)
                {
                    VolumeObjectFactory.FillObjectWithDatasetData(dataset, _volumetricDataMainParentObject,_volumetricDataMainParentObject.transform.GetChild(0).gameObject,transferFunction);      //Long load
                    numVolumesCreated++;
                }
            }
     
            volumeRenderedObject.SetTransferFunction(transferFunction);
            volumeRenderedObject.FillSlicingPlaneWithData(_slicingPlaneObject);
        }
        else
            Debug.LogError("Could not find any DICOM files to import.");
     
        _isoValueSlider.SliderValue= 0;
        _isoRangeSlider.SliderValue= 1;
    
        ResetInitialPosition();
        UpdateIsoRanges();
    }
    public void UpdateIsoRanges()
    {
        _volumeData.SetVisibilityWindow(Mathf.Min(_isoValueSlider.SliderValue, 0.8f), Mathf.Min(_isoValueSlider.SliderValue + _isoRangeSlider.SliderValue, 1f));
    }
    public void RenderingModeUpdated()
    {
        if (_renderModes.CurrentIndex == 0)
        {
            _volumeData.SetRenderMode(RenderMode.DirectVolumeRendering);
            UpdateIsoRanges();
        }
        else if (_renderModes.CurrentIndex == 1)
        {
            _volumeData.SetRenderMode(RenderMode.MaximumIntensityProjectipon);
            UpdateIsoRanges();
        }
        else if (_renderModes.CurrentIndex == 2)
        {
            _volumeData.SetRenderMode(RenderMode.IsosurfaceRendering);
            UpdateIsoRanges();
        }
    }
    public void ChangeQuality()
    {
        _useCubicInterpolation = !_useCubicInterpolation;
        _volumeData.SetCubicInterpolationEnabled(_useCubicInterpolation);
    }
    public void ShowCutPlane()
    {
        _showCutPlane = !_showCutPlane;

        _blackPlaneRenderer.enabled = _showCutPlane;
    }
    public void ResetObjectTransform()
    {
        transform.localPosition = _startLocalPosition;
        transform.localRotation= Quaternion.Euler(_startLocalRotation);
        transform.localScale = _startLocalScale;

        _slicingPlaneObject.transform.localPosition = _startLocalPlanePosition;
        _slicingPlaneObject.transform.localRotation= Quaternion.Euler(_startLocalPlaneRotation);
        _slicingPlaneObject.transform.localScale = _startLocalPlaneScale;
    }
    public void OpenConsole()
    {
        _consoleOpened = !_consoleOpened;
        

        if (_consoleOpened)
        {
            _quantumConsole.Activate(true);
            NonNativeKeyboard.Instance.PresentKeyboard("");
        }
        else
        {
            _quantumConsole.Deactivate();
            NonNativeKeyboard.Instance.Close();

        }
    }
    public void ConsoleTextUpdate(string text)
    {
        _consoleInputField.text =text;
    }
    [Command("resetinitpos",MonoTargetType.All)]
    public void ResetInitialPosition()
    {
        _startLocalPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
        _startLocalRotation = new Vector3(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);
        _startLocalScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);

        _startLocalPlanePosition = new Vector3(_slicingPlaneObject.transform.localPosition.x, _slicingPlaneObject.transform.localPosition.y, _slicingPlaneObject.transform.localPosition.z);
        _startLocalPlaneRotation = new Vector3(_slicingPlaneObject.transform.localRotation.eulerAngles.x, _slicingPlaneObject.transform.localRotation.eulerAngles.y, _slicingPlaneObject.transform.localRotation.eulerAngles.z);
        _startLocalPlaneScale = new Vector3(_slicingPlaneObject.transform.localScale.x, _slicingPlaneObject.transform.localScale.y, _slicingPlaneObject.transform.localScale.z);
    }
}
