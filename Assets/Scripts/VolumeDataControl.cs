using itk.simple;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    [SerializeField] GameObject _volumetricDataMainParentObject;
    [SerializeField] SliderIntervalUpdater _sliderIntervalUpdater1;
    [SerializeField] SliderIntervalUpdater _sliderIntervalUpdater2;
    [SerializeField] GameObject _secondSliderCheckbox;
    [SerializeField] MeshRenderer _volumeMesh;
    [SerializeField] TMP_Text _raymarchStepsLabel;
    [SerializeField] ProgressIndicatorOrbsRotator _dataLoadingIndicator;
    [SerializeField] ProgressIndicatorOrbsRotator _gradientLoadingIndicator;
    [SerializeField] CutoutBox _cutoutBox;
    [SerializeField] GameObject _cutoutPlane;
    [SerializeField] GameObject _slicingPlaneXNormalAxisObject;
    [SerializeField] GameObject _slicingPlaneYNormalAxisObject;
    [SerializeField] GameObject _slicingPlaneZNormalAxisObject;

    ErrorNotifier _errorNotifier;

    bool _showSecondSlider = false;
    Vector3 _startLocalPosition;
    Vector3 _startLocalRotation;
    Vector3 _startLocalScale;

    Vector3 _startLocalPlanePosition;
    Vector3 _startLocalPlaneRotation;
    Vector3 _startLocalPlaneScale;

    bool _isImageSequence = false;

    public static Action<VolumeDataControl> DatasetSpawned { get; set; }
    public static Action<VolumeDataControl> DatasetDespawned { get; set; }

    public static List<string> TF2D { get; set; } = new List<string>();
    public static List<string> TF1D { get; set; } = new List<string>();

    string _filePath;            

    VolumeRenderedObject _volumeRenderedObject;

    private void Start()
    {
        _errorNotifier = FindObjectOfType<ErrorNotifier>();
        _volumeRenderedObject = _volumetricDataMainParentObject.GetComponent<VolumeRenderedObject>();
    }
    public async void LoadDatasetData(string dataFolderName)        //Async addition so all the loading doesnt freeze the app
    {
        await _dataLoadingIndicator.OpenAsync();

        LoadDicomDataPath(dataFolderName+"/Data/");
        LoadTFDataPath(Application.streamingAssetsPath + "/TransferFunctions/");

        _sliderIntervalUpdater1.OnIntervaSliderValueChanged += UpdateIsoRanges;
        _sliderIntervalUpdater2.OnIntervaSliderValueChanged += UpdateIsoRanges;

        

        SimpleITKImageSequenceImporter sequenceImporter = new SimpleITKImageSequenceImporter();
        SimpleITKImageFileImporter fileImporter = new SimpleITKImageFileImporter();
        VolumeDataset dataset = null;

        if (_isImageSequence)
        {
            // Read all files
            IEnumerable<string> fileCandidates = Directory.EnumerateFiles(_filePath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase));

            IEnumerable<IImageSequenceSeries> sequence = await sequenceImporter.LoadSeriesAsync(fileCandidates);

            if (sequence.Count() > 1)
            {
                _errorNotifier.ShowErrorMessageToUser("DICOM folder contains multiple image series, it must contain single image series at runtime!");
                return;
            }
            dataset = await sequenceImporter.ImportSeriesAsync(sequence.First()); 
        }
        else
        {
            dataset = await fileImporter.ImportAsync(_filePath);
        }

        if (dataset != null)
        {
            await VolumeObjectFactory.FillObjectWithDatasetDataAsync(dataset, _volumetricDataMainParentObject, _volumetricDataMainParentObject.transform.GetChild(0).gameObject);   
        }

        if (TF1D.Count > 0)
            SetTransferFunction(TF1D[0]);
        else if (TF2D.Count > 0)
            SetTransferFunction(TF2D[0]);
        else
            _errorNotifier.ShowErrorMessageToUser("No transfer function found. Create and paste atleast one transfer functions in /TransferFunctionsFolder");
       
       
        _volumeRenderedObject.FillSlicingPlaneWithData(_slicingPlaneXNormalAxisObject);
        _volumeRenderedObject.FillSlicingPlaneWithData(_slicingPlaneYNormalAxisObject);
        _volumeRenderedObject.FillSlicingPlaneWithData(_slicingPlaneZNormalAxisObject);

        ResetInitialPosition();
        UpdateIsoRanges();
       
        await _dataLoadingIndicator.CloseAsync();

        await _gradientLoadingIndicator.OpenAsync();

        await dataset.CreateGradientTextureInternalAsync();

        await _gradientLoadingIndicator.CloseAsync();

        DatasetSpawned?.Invoke(this);

    }

    private void OnDestroy()
    {
        DatasetDespawned?.Invoke(this);
    }
    public void SetCrossSectionType(CrossSectionType type)
    {
        if(type ==CrossSectionType.Plane)
        {
            _cutoutPlane.SetActive(true);
            _cutoutBox.gameObject.SetActive(false);
        }
        else if(type== CrossSectionType.BoxInclusive)
        {
            _cutoutPlane.SetActive(false);
            _cutoutBox.gameObject.SetActive(true);
            _cutoutBox.cutoutType = CutoutType.Inclusive;
        }
        else if (type == CrossSectionType.BoxExclusive)
        {
            _cutoutPlane.SetActive(false);
            _cutoutBox.gameObject.SetActive(true);
            _cutoutBox.cutoutType = CutoutType.Exclusive;
        }
    }

    public void SetTransferFunction(string tfName)
    {
        if (TF1D.Contains(tfName))
        {
            TransferFunction transferFunction = TransferFunctionDatabase.LoadTransferFunction(tfName);

            _volumeRenderedObject.SetTransferFunction(transferFunction);
            _volumeRenderedObject.SetTransferFunctionMode(TFRenderMode.TF1D);
        }
        else if (TF2D.Contains(tfName))
        {
            TransferFunction2D transferFunction = TransferFunctionDatabase.LoadTransferFunction2D(tfName);

            _volumeRenderedObject.SetTransferFunction2D(transferFunction);
            _volumeRenderedObject.SetTransferFunctionMode(TFRenderMode.TF2D);
        }
        else
        {
            _errorNotifier.ShowErrorMessageToUser("Wrong TF name, try to use the suggestor");
        }
    }
   
    public void UpdateIsoRanges()
    {
        _sliderIntervalUpdater1.GetSliderValues(out float minVal1,out float maxVal1);
        if (!_showSecondSlider)
        {
            _volumeData.SetVisibilityWindow(minVal1, maxVal1, 0, 0);
        }
        else
        {
            _sliderIntervalUpdater2.GetSliderValues(out float minVal2, out float maxVal2);
            _volumeData.SetVisibilityWindow(minVal1, maxVal1, minVal2, maxVal2);
        } 
    }
    public void UpdateRenderingMode(RenderMode renderMode)
    {
        if(renderMode!=_volumeData.GetRenderMode())
        {
            _volumeData.SetRenderMode(renderMode);
            UpdateIsoRanges();
        }    
    }
    public void UpdateCubicInterpolation(bool value)
    {
        _volumeData.SetCubicInterpolationEnabled(value);
    }
    public void ShowSecondSliderChange()
    {
        _showSecondSlider= !_showSecondSlider;

        if (_showSecondSlider)
        {
            _sliderIntervalUpdater2.gameObject.SetActive(true);
            Vector3 tmp = _secondSliderCheckbox.transform.localPosition;
            tmp.y = 2.7f;

            _secondSliderCheckbox.transform.localPosition = tmp;
        }
        else
        {
            _sliderIntervalUpdater2.gameObject.SetActive(false);

            Vector3 tmp = _secondSliderCheckbox.transform.localPosition;
            tmp.y = 2.55f;

            _secondSliderCheckbox.transform.localPosition = tmp;
        }

        UpdateIsoRanges();
    }
    public void SetRaymarchStepCount(int value)
    {
        _volumeMesh.sharedMaterial.SetInt("_stepNumber", value);
    }
    public void UpdateLighting(bool value)
    {
        _volumeData.SetLightingEnabled(value);
    }
    public void LoadDicomDataPath(string dicomFolderPath)
    {      
        List<string> dicomFilesCandidates = Directory.GetFiles(dicomFolderPath).ToList();

        dicomFilesCandidates.RemoveAll(x => x.EndsWith(".meta"));

        DatasetType datasetType = DatasetImporterUtility.GetDatasetType(dicomFilesCandidates.First());

        if(datasetType==DatasetType.ImageSequence||datasetType==DatasetType.DICOM)
        {
            _isImageSequence = true;
            _filePath= dicomFolderPath;
        }
        else if(datasetType==DatasetType.Unknown)
        {
            _errorNotifier.ShowErrorMessageToUser("Unknown file/data detected in DicomFolder");
            _filePath = dicomFilesCandidates[0];
        }
        else
        {
            _filePath = dicomFilesCandidates[0];
        }
    }
    public void LoadTFDataPath(string transferFunctionFolderPath)
    {
        TF1D = new List<string>();
        TF2D = new List<string>();

        List<string> dicomFilesCandidates = Directory.GetFiles(transferFunctionFolderPath).ToList();

        dicomFilesCandidates.RemoveAll(x => x.EndsWith(".meta"));


        foreach (string i in dicomFilesCandidates)
        {
            if (i.EndsWith("tf"))
                TF1D.Add(i);
            else if (i.EndsWith("tf2d"))
                TF2D.Add(i);
        }
    }
    public void UpdateSlicePlane(bool value)
    {
        _slicingPlaneXNormalAxisObject.SetActive(value);
        _slicingPlaneYNormalAxisObject.SetActive(value);
        _slicingPlaneZNormalAxisObject.SetActive(value);
    }
    public void ResetObjectTransform()
    {
        transform.localPosition = _startLocalPosition;
        transform.localRotation= Quaternion.Euler(_startLocalRotation);
        transform.localScale = _startLocalScale;

        _cutoutPlane.transform.localPosition = _startLocalPlanePosition;
        _cutoutPlane.transform.localRotation= Quaternion.Euler(_startLocalPlaneRotation);
        _cutoutPlane.transform.localScale = _startLocalPlaneScale;
    }

    private void ResetInitialPosition()
    {
        _startLocalPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
        _startLocalRotation = new Vector3(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);
        _startLocalScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);

        _startLocalPlanePosition = new Vector3(_cutoutPlane.transform.localPosition.x, _cutoutPlane.transform.localPosition.y, _cutoutPlane.transform.localPosition.z);
        _startLocalPlaneRotation = new Vector3(_cutoutPlane.transform.localRotation.eulerAngles.x, _cutoutPlane.transform.localRotation.eulerAngles.y, _cutoutPlane.transform.localRotation.eulerAngles.z);
        _startLocalPlaneScale = new Vector3(_cutoutPlane.transform.localScale.x, _cutoutPlane.transform.localScale.y, _cutoutPlane.transform.localScale.z);
    }
    public void SetVolumePosition(Vector3 position)
    {
        transform.position = position;
    }

}
