using itk.simple;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using QFSW.QC.Actions;
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
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;
using UnityVolumeRendering;
using RenderMode = UnityVolumeRendering.RenderMode;

public class VolumeDataControl : MonoBehaviour, IMixedRealityInputHandler
{
    [SerializeField] VolumeRenderedObject _volumeData;
    [SerializeField] InteractableToggleCollection _renderModes;
    [SerializeField] GameObject _volumetricDataMainParentObject;
    [SerializeField] TMP_Text _raymarchStepsLabel;
    [SerializeField] CrossSectionSphere _cutoutSphere;
    [SerializeField] GameObject _cutoutPlane;
    [SerializeField] GameObject _slicingPlaneXNormalAxisObject;
    [SerializeField] GameObject _slicingPlaneYNormalAxisObject;
    [SerializeField] GameObject _slicingPlaneZNormalAxisObject;
    [SerializeField] GameObject _controlHandle;
    [SerializeField] GameObject _segmentationSliderPrefab;
    [SerializeField] GameObject _segmentationParentContainer;
    [SerializeField] GameObject _segmentationParent;
    [SerializeField] TFColorUpdater _tfColorUpdater;
    [SerializeField] MeshRenderer _volumeDatasetIcon;
    [SerializeField] TMP_Text _volumeDatasetDescription;
    [SerializeField] GameObject _densitySliderPrefab;
    [SerializeField] GameObject _densitySlidersContainer;
    [SerializeField] GameObject _sliderControlButtons;
    [SerializeField] GameObject _removeSliderButton;
    [SerializeField] DatasetSaveSystem _saveSystem;
    [SerializeField] OrbProgressView _orbProgressView;

    [field: SerializeField] public MeshRenderer VolumeMesh { get; set; }

    public VolumeDataset Dataset { get; set; }
    public bool HasBeenLoaded { get; set; }
    public List<Segment> Segments { get; set; } = new List<Segment>();

    public TransferFunction TransferFunction { get; set; }

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

    Vector3 _mirrorFlippedRotation = new Vector3(29.78f,95.6f,77.744f);
    Vector3 _normalRotation = new Vector3(-150.22f,95.6f,282.256f);

    public static Action<VolumeDataControl> DatasetSpawned { get; set; }
    public static Action<VolumeDataControl> DatasetDespawned { get; set; }

    public Action AllAlphaButtonsPressed { get; set; }
    public Action DensityIntervalsChanged { get; set; }

    //public static List<string> TF2D { get; set; } = new List<string>();
    //public static List<string> TF1D { get; set; } = new List<string>();

    public List<SliderIntervalUpdater> DensityIntervalSliders { get; private set; } = new List<SliderIntervalUpdater>();
    VolumeRenderedObject _volumeRenderedObject;

    bool _segmentationPanelVisible = false;
    bool _isDatasetReversed;

    private void Start()
    {
        _volumeRenderedObject = _volumetricDataMainParentObject.GetComponent<VolumeRenderedObject>();
        SetInitialTransforms();
        _tfColorUpdater.TfColorUpdated += SetTransferFunction;
        _tfColorUpdater.TfColorReset += OnTFReset;
    }
    public async Task LoadDatasetAsync(string datasetFolderName,Texture volumeIcon,string description)        //Async addition so all the loading doesnt freeze the app
    {
        using (ProgressHandler progressHandler = new ProgressHandler(_orbProgressView))
        {
            _saveSystem.TryLoadSaveTransformData();

            progressHandler.Start("Loading started...", "MainLoad");

            _volumeRenderedObject.InitVisiblityWindow();
            _volumeDatasetIcon.material.mainTexture = volumeIcon;
            _volumeDatasetDescription.text = description;

            var result = await CreateVolumeDatasetAsync(datasetFolderName,progressHandler);

            Dataset = result.Item1;
            _isDatasetReversed = result.Item2;

            if (Dataset == null)
                return;

            if (_isDatasetReversed)
                _volumeData.gameObject.transform.transform.localRotation = Quaternion.Euler(_mirrorFlippedRotation);
            else
                _volumeData.gameObject.transform.localRotation = Quaternion.Euler(_normalRotation);

            await VolumeObjectFactory.FillObjectWithDatasetDataAsync(Dataset, _volumetricDataMainParentObject, _volumetricDataMainParentObject.transform.GetChild(0).gameObject,progressHandler);


            if (await TryLoadSegmentationToVolumeAsync(datasetFolderName, Dataset, _isDatasetReversed,progressHandler))
            {
                await InitSegmentationAsync(progressHandler);
                _segmentationParent.SetActive(true);
            }

            TransferFunction = TransferFunctionDatabase.LoadTransferFunctionFromResources("default");      //TF in resources must be in .txt format, the .tf that is default for transfer function cannot be loaded from resources
            SetTransferFunction(TransferFunction);


            _tfColorUpdater.InitUpdater(TransferFunction);

            _saveSystem.TryLoadTFData(_tfColorUpdater);
            _saveSystem.TryLoadSaveSegmentData(this);

            if (!_saveSystem.TryLoadSaveDensitySliders(this))
                AddValueDensitySlider(0, 1);                                     //Add default density slider if there are no save data

            _densitySlidersContainer.SetActive(true);


            VolumeMesh.gameObject.SetActive(true);                          //It is disabled to this point, otherwise default mat is blocking loading indicator

            _volumeRenderedObject.FillSlicingPlaneWithData(_slicingPlaneXNormalAxisObject);
            _volumeRenderedObject.FillSlicingPlaneWithData(_slicingPlaneYNormalAxisObject);
            _volumeRenderedObject.FillSlicingPlaneWithData(_slicingPlaneZNormalAxisObject);

            progressHandler.ReportProgress(0, "Creating gradient...");

            await Dataset.GetGradientTextureAsync(true,progressHandler);

            progressHandler.Finish(ProgressStatus.Succeeded);

            HasBeenLoaded = true;
            DatasetSpawned?.Invoke(this);
            await _saveSystem.SaveDataAsync(this);
        }
    }
    public async Task<(VolumeDataset,bool)> CreateVolumeDatasetAsync(string datasetFolderName,ProgressHandler progressHandler)
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
            return (null,false);
        }
        else if (errorFlag == 2)
        {
            ErrorNotifier.Instance.AddErrorMessageToUser($"Unknown data detected in dataset named: {datasetName} in folder Data");
            return (null,false);
        }


        SimpleITKImageSequenceImporter sequenceImporter = new SimpleITKImageSequenceImporter();
        SimpleITKImageFileImporter fileImporter = new SimpleITKImageFileImporter();
        VolumeDataset dataset = null;
        bool isDatasetReversed = true;

        if (isDicomImageSequence)
        {
            // Read all files
            IEnumerable<string> fileCandidates = Directory.EnumerateFiles(filePath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase));

            IEnumerable<IImageSequenceSeries> sequence = await sequenceImporter.LoadSeriesAsync(fileCandidates,progressHandler,false);

            try
            {
                var result = await sequenceImporter.ImportSeriesAsync(sequence.First(), datasetName,progressHandler);
                dataset = result.Item1;
                isDatasetReversed = result.Item2;
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
                var result = await fileImporter.ImportAsync(filePath,datasetName,progressHandler);
                dataset=result.Item1;
                isDatasetReversed = result.Item2;
            }
            catch
            {
                ErrorNotifier.Instance.AddErrorMessageToUser($"Corrupted data in dataset named: {datasetName} in folder Data");
            }
        }
        return (dataset,isDatasetReversed);
    }

    public async Task<bool> TryLoadSegmentationToVolumeAsync(string datasetFolderName, VolumeDataset volumeDataset, bool isDatasetReversed,ProgressHandler progressHandler)
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

            IEnumerable<IImageSequenceSeries> sequence = await sequenceImporter.LoadSeriesAsync(fileCandidates,progressHandler,true);

            try
            {
                await sequenceImporter.ImportSeriesSegmentationAsync(sequence.First(), volumeDataset,isDatasetReversed,progressHandler);
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
                    await fileImporter.ImportSegmentationAsync(filePath, volumeDataset, isDatasetReversed,progressHandler);
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

    //public void SetTransferFunction(string tfName)
    //{
    //    if (TF1D.Contains(tfName))
    //    {
    //        TransferFunction = TransferFunctionDatabase.LoadTransferFunction(tfName);
    //
    //        _volumeRenderedObject.SetTransferFunction(TransferFunction);
    //        _volumeRenderedObject.SetTransferFunctionMode(TFRenderMode.TF1D);
    //    }
    //    else if (TF2D.Contains(tfName))
    //    {
    //        TransferFunction2D transferFunction = TransferFunctionDatabase.LoadTransferFunction2D(tfName);
    //
    //        _volumeRenderedObject.SetTransferFunction2D(transferFunction);
    //        _volumeRenderedObject.SetTransferFunctionMode(TFRenderMode.TF2D);
    //    }
    //    else
    //    {
    //        ErrorNotifier.Instance.AddErrorMessageToUser("Wrong TF name, try to use the suggestor");
    //    }
    //}
    public void SetTransferFunction(TransferFunction tf)
    {
        _volumeRenderedObject.SetTransferFunction(tf);      
    }
    public void AddDensitySlider()
    {
        AddValueDensitySlider(0, 0.2f);
    }
    public void AddValueDensitySlider(float minVal,float maxVal)
    {
        GameObject newSlider = Instantiate(_densitySliderPrefab, _densitySlidersContainer.transform);

        newSlider.transform.localPosition = new Vector3(0.09f - (DensityIntervalSliders.Count * 0.09f), 0.011f, 0.3f);
        newSlider.transform.localRotation = Quaternion.Euler(new Vector3(90, -90, 0));
        SliderIntervalUpdater sliderUpdater = newSlider.GetComponent<SliderIntervalUpdater>();
        sliderUpdater.OnIntervaSliderValueChanged += UpdateIsoRanges;
        sliderUpdater.SetInitvalue(minVal, maxVal);
        _sliderControlButtons.transform.localPosition = new Vector3(-0.03f - (DensityIntervalSliders.Count * 0.09f), 0, DensityIntervalSliders.Count > 0 ? 0.3f : 0.22f);

        DensityIntervalSliders.Add(sliderUpdater);

        if (DensityIntervalSliders.Count > 1)
            _removeSliderButton.SetActive(true);

        UpdateIsoRanges();
        _saveSystem.SaveDataAsync(this);
    }
    public void RemoveDensitySlider()
    {
        SliderIntervalUpdater sliderUpdater = DensityIntervalSliders.Last();
        sliderUpdater.OnIntervaSliderValueChanged -= UpdateIsoRanges;

        _sliderControlButtons.transform.localPosition = new Vector3(-0.03f - ((DensityIntervalSliders.Count - 2) * 0.09f), 0, DensityIntervalSliders.Count - 2>=1? 0.3f:0.22f);

        DensityIntervalSliders.Remove(sliderUpdater);
        Destroy(sliderUpdater.gameObject);

        if (DensityIntervalSliders.Count <= 1)
            _removeSliderButton.SetActive(false);

        UpdateIsoRanges();
        DensityIntervalsChanged?.Invoke();
        _saveSystem.SaveDataAsync(this);
    }

    public void UpdateIsoRanges()
    {
        try                                                                                 //ON app start sliders are defaultly updated, but volume object is not present yet
        {
            List<float> minVals = new List<float>();
            List<float> maxVals = new List<float>();


            foreach(SliderIntervalUpdater i in DensityIntervalSliders)
            {
                i.GetSliderValues(out float minVal, out float maxVal);
                minVals.Add(minVal);
                maxVals.Add(maxVal);
            }

           
            _volumeData.SetVisibilityWindow(minVals.ToArray(),maxVals.ToArray(), DensityIntervalSliders.Count);
            
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
            {
                if(Segments.Count>0)
                    _segmentationParent.SetActive(true);
            }
            else
                _segmentationParent.SetActive(false);

            if (renderMode == RenderMode.MaximumIntensityProjectipon)
                _tfColorUpdater.ShowTfUpdater(false);
            else
                _tfColorUpdater.ShowTfUpdater(true);
        }
    }
    public void SwitchSegmentationPanel()
    {
        _segmentationPanelVisible=!_segmentationPanelVisible;

        _segmentationParentContainer.SetActive(_segmentationPanelVisible);

        _tfColorUpdater.ShowTfUpdater(!_segmentationPanelVisible);
        TurnMaterialLabelingKeyword(_segmentationPanelVisible);
    }
    public void UpdateCubicInterpolation(bool value)
    {
        _volumeData.SetCubicInterpolationEnabled(value);
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
    //public void LoadTFDataPath(string transferFunctionFolderPath)
    //{
    //    TF1D = new List<string>();
    //    TF2D = new List<string>();
    //
    //    List<string> dicomFilesCandidates = Directory.GetFiles(transferFunctionFolderPath).ToList();
    //
    //    dicomFilesCandidates.RemoveAll(x => x.EndsWith(".meta"));
    //
    //
    //    foreach (string i in dicomFilesCandidates)
    //    {
    //        if (i.EndsWith("tf"))
    //            TF1D.Add(i);
    //        else if (i.EndsWith("tf2d"))
    //            TF2D.Add(i);
    //    }
    //}
    public async Task InitSegmentationAsync(ProgressHandler progressHandler)
    {
        VolumeMesh.sharedMaterial.SetTexture("_LabelTex", await Dataset.GetLabelTextureAsync(true, progressHandler));           //Very long

        Color[] uniqueColors = Utils.CreateColors(Dataset.LabelValues.Keys.Count);
        for (int i = 1; i < Dataset.LabelValues.Keys.Count; i++)
        {
            Color col = uniqueColors[i - 1];
            GameObject tmp = Instantiate(_segmentationSliderPrefab, _segmentationParentContainer.transform);
            tmp.transform.localPosition = new Vector3(0,0.3f -(0.06f * i), 0.33f);
            tmp.transform.localRotation = Quaternion.Euler(new Vector3(0,-90,0));
            Segment segment = tmp.GetComponent<Segment>();
            segment.SegmentID= i-1;
            segment.ColorUpdated += UpdateShaderLabelArray;
            segment.InitColor(col);

            if(Dataset.LabelNames.Count>=i)
                segment.ChangeSegmentName(Dataset.LabelNames[i-1]);

            Segments.Add(segment);
        }
        UpdateShaderLabelArray();

    }
    public void TurnAllSegmentAlphas(bool value)
    {
        Segments.ForEach(x => x.AlphaUpdate(value?1:0));
        AllAlphaButtonsPressed?.Invoke();
        _saveSystem.SaveDataAsync(this);
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
        VolumeMesh.material.SetColorArray("_SegmentsColors", Segments.Select(x => x.SegmentColor).ToArray());
    }
    public async Task DownScaleDatasetAsync()
    {
        using (ProgressHandler progressHandler = new ProgressHandler(_orbProgressView))
        {
            progressHandler.Start("Downscaling", "Downscaling dataset...");
           
            await Dataset.DownScaleDataAsync();

            await RegenerateTexturesDataAsync(progressHandler);

            progressHandler.Finish();
        }
    }
    private async Task RegenerateTexturesDataAsync(ProgressHandler progressHandler)
    {
        progressHandler.ReportProgress(0, "Refreshing data...");

        VolumeMesh.sharedMaterial.SetTexture("_DataTex", await Dataset.GetDataTextureAsync(true, progressHandler));           //Very long

        progressHandler.ReportProgress(0, "Refreshing gradient...");

        VolumeMesh.sharedMaterial.SetTexture("_GradientTex", await Dataset.GetGradientTextureAsync(true,progressHandler));           //Very long        
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
    public async Task MirrorFlipTexturesAsync()
    {
        using (ProgressHandler progressHandler = new ProgressHandler(_orbProgressView))
        {
            _isDatasetReversed = !_isDatasetReversed;

            progressHandler.Start("Flipping", "Flipping Data...");

            await Task.Run(() => Dataset.FlipTextureArrays());

            if (Segments.Count > 0)
            {
                progressHandler.ReportProgress(0, "Refreshing Segmentation...");

                VolumeMesh.sharedMaterial.SetTexture("_LabelTex", await Dataset.GetLabelTextureAsync(true,progressHandler));           //Very long
            }
            if (_isDatasetReversed)
                _volumeData.gameObject.transform.localRotation = Quaternion.Euler(_mirrorFlippedRotation);
            else
                _volumeData.gameObject.transform.localRotation = Quaternion.Euler(_normalRotation);

            await RegenerateTexturesDataAsync(progressHandler);
        }
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

    private void OnTFReset()
    {
        _saveSystem.SaveDataAsync(this);
    }
    public void OnInputUp(InputEventData eventData)
    {
        _saveSystem.SaveDataAsync(this);
    }

    public void OnInputDown(InputEventData eventData)
    {
        //
    }
}
