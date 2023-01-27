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
    Vector3 _startPosition;
    Vector3 _startRotation;
    Vector3 _startScale;

    private void Start()
    {
        ResetInitialPosition();

        string filePath = "H:\\jatra\\Saved\\Nrrdf\\Dicom\\";
        string transferFunctionPath = "H:\\GithubSync\\FnO-Hololens2-visualisation\\Assets\\TransferFunction\\default.tf";


        TransferFunction transferFunction=TransferFunctionDatabase.LoadTransferFunction(transferFunctionPath);

        VolumeRenderedObject volumeRenderedObject = _volumetricDataMainParentObject.GetComponent<VolumeRenderedObject>();

        NonNativeKeyboard.Instance.OnTextUpdated += ConsoleTextUpdate;

        ImageSequenceFormat imgSeqFormat = ImageSequenceFormat.DICOM;

        IEnumerable<string> fileCandidates = Directory.EnumerateFiles(filePath, "*.*", SearchOption.TopDirectoryOnly)
                            .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase));


        if (fileCandidates.Any())
        {
            IImageSequenceImporter importer = ImporterFactory.CreateImageSequenceImporter(imgSeqFormat);
            IEnumerable<IImageSequenceSeries> seriesList = importer.LoadSeries(fileCandidates);
            float numVolumesCreated = 0;

            foreach (IImageSequenceSeries series in seriesList)
            {
                VolumeDataset dataset = importer.ImportSeries(series);
                if (dataset != null)
                {
                    if (EditorPrefs.GetBool("DownscaleDatasetPrompt"))
                    {
                        if (EditorUtility.DisplayDialog("Optional DownScaling",
                            $"Do you want to downscale the dataset? The dataset's dimension is: {dataset.dimX} x {dataset.dimY} x {dataset.dimZ}", "Yes", "No"))
                        {
                            dataset.DownScaleData();
                        }
                    }

                    VolumeObjectFactory.FillObjectWithDatasetData(dataset, _volumetricDataMainParentObject,_volumetricDataMainParentObject.transform.GetChild(0).gameObject,transferFunction);
                    numVolumesCreated++;
                }
            }

            volumeRenderedObject.SetTransferFunction(transferFunction);
            volumeRenderedObject.FillSlicingPlaneWithData(_slicingPlaneObject);
        }
        else
            Debug.LogError("Could not find any DICOM files to import.");


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
        transform.SetPositionAndRotation(_startPosition, Quaternion.Euler(_startRotation));
        transform.localScale = _startScale;
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
        _startPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        _startRotation = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        _startScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }
}
