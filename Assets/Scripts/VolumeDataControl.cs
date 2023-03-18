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
    [SerializeField] TMP_Text _raymarchStepsLabel;
    [SerializeField] ProgressIndicatorOrbsRotator _dataLoadingIndicator;
    [SerializeField] CrossSectionSphere _cutoutSphere;
    [SerializeField] GameObject _cutoutPlane;
    [SerializeField] GameObject _slicingPlaneXNormalAxisObject;
    [SerializeField] GameObject _slicingPlaneYNormalAxisObject;
    [SerializeField] GameObject _slicingPlaneZNormalAxisObject;
    [SerializeField] GameObject _controlHandle;
    [SerializeField] GameObject _segmentationSliderPrefab;
    [SerializeField] GameObject _segmentationParentContainer;
    [SerializeField] GameObject _segmentationParent;

    [field: SerializeField] public MeshRenderer VolumeMesh { get; set; }

    public VolumeDataset Dataset { get; set; }
    List<SegmentationRowHelper> _segments = new List<SegmentationRowHelper>();

    bool _showSecondSlider = false;
    Vector3 _startLocalPosition;
    Vector3 _startLocalRotation;
    Vector3 _startLocalScale;

    Vector3 _startLocalPlanePosition;
    Vector3 _startLocalPlaneRotation;
    Vector3 _startLocalPlaneScale;

    Vector3 _startLocalSpherePosition;
    Vector3 _startLocalSphereRotation;
    Vector3 _startLocalSphereScale;

    Vector3 _startLocalHandlePosition;
    Vector3 _startLocalHandleRotation;
    Vector3 _startLocalHandleScale;

    public static Action<VolumeDataControl> DatasetSpawned { get; set; }
    public static Action<VolumeDataControl> DatasetDespawned { get; set; }

    public static List<string> TF2D { get; set; } = new List<string>();
    public static List<string> TF1D { get; set; } = new List<string>();

               
    VolumeRenderedObject _volumeRenderedObject;

    bool _segmentationPanelVisible = false;

    private void Start()
    {
        _volumeRenderedObject = _volumetricDataMainParentObject.GetComponent<VolumeRenderedObject>();
        SetInitialTransforms();

    }
    public async void LoadDataset(string datasetFolderName)        //Async addition so all the loading doesnt freeze the app
    {
        await _dataLoadingIndicator.OpenAsync();
        _dataLoadingIndicator.Message = "Loading data...";


        LoadTFDataPath(Application.streamingAssetsPath + "/TransferFunctions/");

        _sliderIntervalUpdater1.OnIntervaSliderValueChanged += UpdateIsoRanges;
        _sliderIntervalUpdater2.OnIntervaSliderValueChanged += UpdateIsoRanges;

        Dataset = await CreateVolumeDatasetAsync(datasetFolderName);

        if (Dataset == null)
            return;

        await VolumeObjectFactory.FillObjectWithDatasetDataAsync(Dataset, _volumetricDataMainParentObject, _volumetricDataMainParentObject.transform.GetChild(0).gameObject);
        

        if (await TryLoadSegmentationToVolumeAsync(datasetFolderName, Dataset))
        {
            _dataLoadingIndicator.Message = "Loading segmentation...";
            await InitSegmentation();
            _segmentationParent.SetActive(true);
        }
        VolumeMesh.gameObject.SetActive(true);                          //It is disabled to this point, otherwise default mat is blocking loading indicator

        if (TF1D.Count > 0)
            SetTransferFunction(TF1D[0]);
        else if (TF2D.Count > 0)
            SetTransferFunction(TF2D[0]);
        else
            ErrorNotifier.Instance.AddErrorMessageToUser("No transfer function found. Create and paste atleast one transfer functions in /TransferFunctionsFolder");


        _volumeRenderedObject.FillSlicingPlaneWithData(_slicingPlaneXNormalAxisObject);
        _volumeRenderedObject.FillSlicingPlaneWithData(_slicingPlaneYNormalAxisObject);
        _volumeRenderedObject.FillSlicingPlaneWithData(_slicingPlaneZNormalAxisObject);


        _dataLoadingIndicator.Message = "Creating gradient";

        await Dataset.CreateGradientTextureInternalAsync();

        await _dataLoadingIndicator.CloseAsync();

        DatasetSpawned?.Invoke(this);
    }
    public async Task<VolumeDataset> CreateVolumeDatasetAsync(string datasetFolderName)
    {
        string datasetName = datasetFolderName.Split('/').Last();
        string dataFolderName = datasetFolderName + "/Data/";
        if (!Directory.Exists(dataFolderName))
        {
            ErrorNotifier.Instance.AddErrorMessageToUser($"Data folder for dataset named: {datasetName} doesnt exist!!!");
        }

        LoadDicomDataPath(dataFolderName, out string filePath, out bool isDicomImageSequence,out int errorFlag);

        if (errorFlag == 1)
        {
            ErrorNotifier.Instance.AddErrorMessageToUser($"No data detected in dataset named: {datasetName} in folder Data");
            return null;
        }
        else if (errorFlag == 2)
        {
            ErrorNotifier.Instance.AddErrorMessageToUser($"Unknown data detected in dataset named: {datasetName} in folder Data");
            return null;
        }


        SimpleITKImageSequenceImporter sequenceImporter = new SimpleITKImageSequenceImporter();
        SimpleITKImageFileImporter fileImporter = new SimpleITKImageFileImporter();
        VolumeDataset dataset = null;

        if (isDicomImageSequence)
        {
            // Read all files
            IEnumerable<string> fileCandidates = Directory.EnumerateFiles(filePath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase));

            IEnumerable<IImageSequenceSeries> sequence = await sequenceImporter.LoadSeriesAsync(fileCandidates);

            try
            {
                dataset = await sequenceImporter.ImportSeriesAsync(sequence.First(), datasetName);
            }
            catch
            {
                ErrorNotifier.Instance.AddErrorMessageToUser($"Corrupted image series in dataset named: {datasetName} in folder Data");
            }
        }
        else
        {
            try
            {
                dataset = await fileImporter.ImportAsync(filePath,datasetName);
            }
            catch
            {
                ErrorNotifier.Instance.AddErrorMessageToUser($"Corrupted data in dataset named: {datasetName} in folder Data");
            }
        }
        return dataset;
    }

    public async Task<bool> TryLoadSegmentationToVolumeAsync(string datasetFolderName, VolumeDataset volumeDataset)
    {
        string segmentationFolderName = datasetFolderName + "/Labels/";
        if (!Directory.Exists(segmentationFolderName))
        {
            return false;
        }
        LoadDicomDataPath(segmentationFolderName, out string filePath, out bool isDicomImageSequence,out int errorFlag);

        if (errorFlag == 1)
        {
            ErrorNotifier.Instance.AddErrorMessageToUser($"No data detected in dataset named: {datasetFolderName.Split('/').Last()} in folder Labels");
            return false;
        }
        else if (errorFlag == 2)
        {
            ErrorNotifier.Instance.AddErrorMessageToUser($"Unknown data detected in dataset named: {datasetFolderName.Split('/').Last()} in folder Labels");
            return false;
        }

        SimpleITKImageSequenceImporter sequenceImporter = new SimpleITKImageSequenceImporter();
        SimpleITKImageFileImporter fileImporter = new SimpleITKImageFileImporter();

        if (isDicomImageSequence)
        {
            // Read all files
            IEnumerable<string> fileCandidates = Directory.EnumerateFiles(filePath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase));

            IEnumerable<IImageSequenceSeries> sequence = await sequenceImporter.LoadSeriesAsync(fileCandidates);

            try
            {
                await sequenceImporter.ImportSeriesSegmentationAsync(sequence.First(), volumeDataset);
            }
            catch
            {
                ErrorNotifier.Instance.AddErrorMessageToUser($"Corrupted image series in dataset named: {datasetFolderName.Split('/').Last()} in folder Labels");
            }
        }
        else
        {
            try
            {
                if(volumeDataset!=null)
                    await fileImporter.ImportSegmentationAsync(filePath, volumeDataset);
            }
            catch
            {
                ErrorNotifier.Instance.AddErrorMessageToUser($"Corrupted data in dataset named: {datasetFolderName.Split('/').Last()} in folder Labels");
            }
        }
        return true;
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
            _cutoutSphere.gameObject.SetActive(false);
        }
        else if(type== CrossSectionType.SphereInclusive)
        {
            _cutoutPlane.SetActive(false);
            _cutoutSphere.gameObject.SetActive(true);
            _cutoutSphere.CutoutType = CutoutType.Inclusive;
        }
        else if (type == CrossSectionType.SphereExclusive)
        {
            _cutoutPlane.SetActive(false);
            _cutoutSphere.gameObject.SetActive(true);
            _cutoutSphere.CutoutType = CutoutType.Exclusive;
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
            ErrorNotifier.Instance.AddErrorMessageToUser("Wrong TF name, try to use the suggestor");
        }
    }
   
    public void UpdateIsoRanges()
    {
        try                                                                                 //ON app start sliders are defaultly updated, but volume object is not present yet
        {
            _sliderIntervalUpdater1.GetSliderValues(out float minVal1, out float maxVal1);
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
        catch { }                   
    }
    public void UpdateRenderingMode(RenderMode renderMode)
    {
        if(renderMode!=_volumeData.GetRenderMode())
        {
            _volumeData.SetRenderMode(renderMode);
            UpdateIsoRanges();

            if(renderMode==RenderMode.DirectVolumeRendering)
                _segmentationParent.SetActive(true);
            else
                _segmentationParent.SetActive(false);
        }
    }
    public void SwitchSegmentationPanel()
    {
        _segmentationPanelVisible=!_segmentationPanelVisible;
        _segmentationParentContainer.SetActive(_segmentationPanelVisible);
        TurnMaterialLabelingKeyword(_segmentationPanelVisible);
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
            tmp.x = -0.17f;

            _secondSliderCheckbox.transform.localPosition = tmp;
        }
        else
        {
            _sliderIntervalUpdater2.gameObject.SetActive(false);

            Vector3 tmp = _secondSliderCheckbox.transform.localPosition;
            tmp.x = -0.06f;

            _secondSliderCheckbox.transform.localPosition = tmp;
        }

        UpdateIsoRanges();
    }
    public void SetRaymarchStepCount(int value)
    {
        VolumeMesh.sharedMaterial.SetInt("_stepNumber", value);
    }
    public void UpdateLighting(bool value)
    {
        _volumeData.SetLightingEnabled(value);
    }
    public void LoadDicomDataPath(string dicomFolderPath,out string dicomPath,out bool isImageSequence,out int errorFlag)
    {
        List<string> dicomFilesCandidates = Directory.GetFiles(dicomFolderPath).ToList();

        dicomFilesCandidates.RemoveAll(x => x.EndsWith(".meta"));

        if(dicomFilesCandidates.Count==0)
        {
            errorFlag = 1;
            dicomPath = null;
            isImageSequence = false;
            return;
        }

        DatasetType datasetType = DatasetImporterUtility.GetDatasetType(dicomFilesCandidates.First());

        if(datasetType==DatasetType.ImageSequence||datasetType==DatasetType.DICOM)
        {
            isImageSequence = true;
            dicomPath = dicomFolderPath;
            errorFlag = 0;
        }
        else if(datasetType==DatasetType.Unknown)
        {
            isImageSequence = false;
            dicomPath = dicomFilesCandidates[0];
            errorFlag = 2;
        }
        else
        {
            isImageSequence = false;
            dicomPath = dicomFilesCandidates[0];
            errorFlag = 0;
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
    public async Task InitSegmentation()
    {
        VolumeMesh.sharedMaterial.SetTexture("_LabelTex", await Dataset.GetLabelTextureAsync());           //Very long

        Color[] uniqueColors = Utils.CreateColors(Dataset.LabelValues.Keys.Count);
        for (int i = 1; i < Dataset.LabelValues.Keys.Count; i++)
        {
            Color col = uniqueColors[i - 1];
            GameObject tmp = Instantiate(_segmentationSliderPrefab, _segmentationParentContainer.transform);
            tmp.transform.localPosition = new Vector3(0,0.3f -(0.06f * i), 0.15f);
            tmp.transform.localRotation = Quaternion.Euler(new Vector3(0,-90,0));
            SegmentationRowHelper helper = tmp.GetComponent<SegmentationRowHelper>();
            helper.SegmentID= i-1;
            helper.ColorUpdated += UpdateShaderLabelArray;
            helper.InitColor(col);

            _segments.Add(helper);
        }
        UpdateShaderLabelArray();

    }
    public void TurnAllSegmentAlphas(bool value)
    {
        _segments.ForEach(x => x.AlphaUpdate(value?1:0));
    }
    private void TurnMaterialLabelingKeyword(bool value)
    {
        if(value)
            VolumeMesh.material.EnableKeyword("LABELING_SUPPORT_ON");
        else
            VolumeMesh.material.DisableKeyword("LABELING_SUPPORT_ON");
    }

    public void UpdateShaderLabelArray()
    {
        VolumeMesh.material.SetColorArray("_SegmentsColors", _segments.Select(x => x.SegmentColor).ToArray());
    }

    public void UpdateSlicePlane(bool value)
    {
        _slicingPlaneXNormalAxisObject.SetActive(value);
        _slicingPlaneYNormalAxisObject.SetActive(value);
        _slicingPlaneZNormalAxisObject.SetActive(value);
    }
    public void ResetAllTransforms()
    {
        ResetMainObjectTransform();
        ResetCrossSectionToolsTransform();
        ResetHandleTransform();
        ResetSlicesTransform();
    }
    public void ResetMainObjectTransform()
    {
        transform.localPosition = _startLocalPosition;
        transform.localRotation = Quaternion.Euler(_startLocalRotation);
        transform.localScale = _startLocalScale;
    }
    public void ResetCrossSectionToolsTransform()
    {
        _cutoutPlane.transform.localPosition = _startLocalPlanePosition;
        _cutoutPlane.transform.localRotation = Quaternion.Euler(_startLocalPlaneRotation);
        _cutoutPlane.transform.localScale = _startLocalPlaneScale;

        _cutoutSphere.transform.localPosition = _startLocalSpherePosition;
        _cutoutSphere.transform.localRotation = Quaternion.Euler(_startLocalSphereRotation);
        _cutoutSphere.transform.localScale = _startLocalSphereScale;
    }
    public void ResetHandleTransform()
    {
        _controlHandle.transform.localPosition = _startLocalHandlePosition;
        _controlHandle.transform.localRotation = Quaternion.Euler(_startLocalHandleRotation);
        _controlHandle.transform.localScale = _startLocalHandleScale;
    }
    public void ResetSlicesTransform()
    {
        _slicingPlaneXNormalAxisObject.transform.localPosition = Vector3.zero;
        _slicingPlaneXNormalAxisObject.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));

        _slicingPlaneYNormalAxisObject.transform.localPosition = Vector3.zero;
        _slicingPlaneYNormalAxisObject.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

        _slicingPlaneZNormalAxisObject.transform.localPosition = Vector3.zero;
        _slicingPlaneZNormalAxisObject.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, -90));
    }

    private void SetInitialTransforms()
    {
        _startLocalPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
        _startLocalRotation = new Vector3(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);
        _startLocalScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);

        _startLocalPlanePosition = new Vector3(_cutoutPlane.transform.localPosition.x, _cutoutPlane.transform.localPosition.y, _cutoutPlane.transform.localPosition.z);
        _startLocalPlaneRotation = new Vector3(_cutoutPlane.transform.localRotation.eulerAngles.x, _cutoutPlane.transform.localRotation.eulerAngles.y, _cutoutPlane.transform.localRotation.eulerAngles.z);
        _startLocalPlaneScale = new Vector3(_cutoutPlane.transform.localScale.x, _cutoutPlane.transform.localScale.y, _cutoutPlane.transform.localScale.z);

        _startLocalSpherePosition = new Vector3(_cutoutSphere.transform.localPosition.x, _cutoutSphere.transform.localPosition.y, _cutoutSphere.transform.localPosition.z);
        _startLocalSphereRotation = new Vector3(_cutoutSphere.transform.localRotation.eulerAngles.x, _cutoutSphere.transform.localRotation.eulerAngles.y, _cutoutSphere.transform.localRotation.eulerAngles.z);
        _startLocalSphereScale = new Vector3(_cutoutSphere.transform.localScale.x, _cutoutSphere.transform.localScale.y, _cutoutSphere.transform.localScale.z);

        _startLocalHandlePosition = new Vector3(_controlHandle.transform.localPosition.x, _controlHandle.transform.localPosition.y, _controlHandle.transform.localPosition.z);
        _startLocalHandleRotation = new Vector3(_controlHandle.transform.localRotation.eulerAngles.x, _controlHandle.transform.localRotation.eulerAngles.y, _controlHandle.transform.localRotation.eulerAngles.z);
        _startLocalHandleScale = new Vector3(_controlHandle.transform.localScale.x, _controlHandle.transform.localScale.y, _controlHandle.transform.localScale.z);
    }
    public void SetVolumePosition(Vector3 position)
    {
        transform.position = position;
    }
}
