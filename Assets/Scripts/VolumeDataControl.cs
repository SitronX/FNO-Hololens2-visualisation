using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityVolumeRendering;
using RenderMode = UnityVolumeRendering.RenderMode;

public class VolumeDataControl : MonoBehaviour, IMixedRealityInputHandler
{
    [SerializeField] InteractableToggleCollection _renderModes;
    [SerializeField] TMP_Text _raymarchStepsLabel;
    [SerializeField] CrossSectionSphere _cutoutSphere;
    [SerializeField] GameObject _cutoutPlane;
    [SerializeField] SlicingPlane _slicingPlaneXNormalAxisObject;
    [SerializeField] SlicingPlane _slicingPlaneYNormalAxisObject;
    [SerializeField] SlicingPlane _slicingPlaneZNormalAxisObject;
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
    [SerializeField] VolumeRenderedObject _volumeRenderedObject;


    TransformSave _cutoutPlaneTransformSave;
    TransformSave _cutoutSphereTransformSave;
    TransformSave _grabHandleTransformSave;

    TransformSave _slicingPlaneXTransformSave;
    TransformSave _slicingPlaneYTransformSave;
    TransformSave _slicingPlaneZTransformSave;

    Vector3 _mirrorFlippedRotation = new Vector3(34.418f,74.889f, 64.967f);
    Vector3 _normalRotation = new Vector3(-150.22f, 95.6f, 282.256f);
    Camera _mainCamera;
    bool _isDatasetReversed = true;
    bool _segmentationPanelVisible = false;

    [field: SerializeField] public MeshRenderer VolumeMesh { get; set; }
    [field: SerializeField] public SliderIntervalUpdater SliceRendererWindow { get; set; }

    public VolumeDataset Dataset { get; set; }
    public bool HasBeenLoaded { get; set; }
    public DatasetProcessingType ProcessingType { get; set; } = DatasetProcessingType.Normal;
    public TransferFunction TransferFunction { get; set; }
    public float SliceWindowMin { get; set; }
    public float SliceWindowMax { get; set; }
    public List<Segment> Segments { get; set; } = new List<Segment>();
    public List<SliderIntervalUpdater> DensityIntervalSliders { get; private set; } = new List<SliderIntervalUpdater>();

    public Action AllAlphaButtonsPressed { get; set; }
    public Action DensityIntervalsChanged { get; set; }
    public static Action<VolumeDataControl> DatasetSpawned { get; set; }

    public enum DatasetProcessingType
    {
        Mirrorflipping,Downsampling,Normal
    }

    private void Start()
    {
        SaveInitialTransforms();
        _tfColorUpdater.TfColorUpdated += SetTransferFunction;
        _tfColorUpdater.TfColorReset += OnTFReset;
        SliceRendererWindow.IntervalSliderValueChanged += UpdateSlicePlaneWindow;
    }
    public async void LoadDatasetAsync(string datasetFolderName,Texture volumeIcon,string description,Camera mainCamera)        //Async addition so all the loading doesnt freeze the app
    {
        _mainCamera = mainCamera;

        using (ProgressHandler progressHandler = new ProgressHandler(_orbProgressView))
        {
            await _saveSystem.TryLoadSaveFileAsync(datasetFolderName);

            _saveSystem.TryLoadSaveTransformData();

            progressHandler.Start("Loading started...",numberOfParts:8);

            _volumeRenderedObject.InitVisiblityWindow();
            _volumeDatasetIcon.material.mainTexture = volumeIcon;
            _volumeDatasetDescription.text = description;

            Dataset = await CreateVolumeDatasetAsync(datasetFolderName,progressHandler);

            if (Dataset == null)
                return;

            await VolumeObjectFactory.FillObjectWithDatasetDataAsync(Dataset, _volumeRenderedObject.gameObject,VolumeMesh.gameObject,progressHandler);

            TransferFunction = TransferFunctionDatabase.LoadTransferFunctionFromResources("defaultTF");      //TF in resources must be in .txt format, the .tf that is default for transfer function cannot be loaded from resources
            SetTransferFunction(TransferFunction);

            _tfColorUpdater.InitUpdater(TransferFunction,Dataset.MinDataValue,Dataset.MaxDataValue);
            _saveSystem.TryLoadTFData(_tfColorUpdater);

            if (!_saveSystem.TryLoadSaveDensitySliders(this))
                AddValueDensitySlider(0, 1,saveAfter:false);                                     //Add default density slider if there are no save data


            if (!_saveSystem.TryLoadSliceWindow(this))
                SliceRendererWindow.SetInitValues(0, 1,Dataset.MinDataValue,Dataset.MaxDataValue);
            

            _densitySlidersContainer.SetActive(true);

            if (await TryLoadSegmentationToVolumeAsync(datasetFolderName, Dataset,progressHandler))
            {
                await InitSegmentationAsync(progressHandler);
                _segmentationParent.SetActive(true);
            }
            else
            {
                progressHandler.UpdateTotalNumberOfParts(5);
            }

            _saveSystem.TryLoadSaveSegmentData(this);

            _volumeRenderedObject.FillSlicingPlaneWithData(_slicingPlaneXNormalAxisObject.gameObject);
            _volumeRenderedObject.FillSlicingPlaneWithData(_slicingPlaneYNormalAxisObject.gameObject);
            _volumeRenderedObject.FillSlicingPlaneWithData(_slicingPlaneZNormalAxisObject.gameObject);

            await Dataset.GetGradientTextureAsync(true,progressHandler);

            HasBeenLoaded = true;
            DatasetSpawned?.Invoke(this);
            _saveSystem.SaveDataAsync(this);
        }
    }
    private async Task<VolumeDataset> CreateVolumeDatasetAsync(string datasetFolderName,ProgressHandler progressHandler)
    {
        return await DataProcessing(datasetFolderName,"Data",progressHandler);
    }
    private async Task<bool> TryLoadSegmentationToVolumeAsync(string datasetFolderName, VolumeDataset volumeDataset,ProgressHandler progressHandler)
    {
        return (await DataProcessing(datasetFolderName,"Labels",progressHandler,volumeDataset)!=null);
    }
    private async Task<VolumeDataset> DataProcessing(string datasetFolderName,string folderName, ProgressHandler progressHandler, VolumeDataset volumeDataset=null)
    {
        string datasetName = datasetFolderName.Split('/').Last();

        if (!LoadDataInternal(folderName, datasetFolderName, out string filePath, out bool isDicomImageSequence))
            return null;

        return await ImportDataInternal(isDicomImageSequence, filePath, datasetName, folderName, progressHandler, volumeDataset);
    }
    private bool LoadDataInternal(string folderName, string datasetFolderName, out string filePath, out bool isDicomImageSequence)
    {
        string FolderName = $"{datasetFolderName}/{folderName}/";
        if (!Directory.Exists(FolderName))
        {
            filePath = "";
            isDicomImageSequence = false;
            return false;
        }

        LoadDicomDataPath(FolderName, out filePath, out isDicomImageSequence, out int errorFlag);

        if (errorFlag == 1)
        {
            ErrorNotifier.Instance.AddErrorMessageToUser($"No data detected in dataset named: {datasetFolderName.Split('/').Last()} in folder {folderName}");
            return false;
        }
        else if (errorFlag == 2)
        {
            ErrorNotifier.Instance.AddErrorMessageToUser($"Unknown data detected in dataset named: {datasetFolderName.Split('/').Last()} in folder {folderName}");
            return false;
        }
        return true;
    }
    private async Task<VolumeDataset> ImportDataInternal(bool isDicomImageSequence,string filePath,string datasetName,string folderName, ProgressHandler progressHandler, VolumeDataset volumeDataset=null)
    {
        SimpleITKImageSequenceImporter sequenceImporter = new SimpleITKImageSequenceImporter();
        SimpleITKImageFileImporter fileImporter = new SimpleITKImageFileImporter();

        if (isDicomImageSequence)
        {
            // Read all files
            IEnumerable<string> fileCandidates = Directory.EnumerateFiles(filePath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase));

            IEnumerable<IImageSequenceSeries> sequence = await sequenceImporter.LoadSeriesAsync(fileCandidates, progressHandler, isSegmentation:volumeDataset!=null);

            try
            {
                if (volumeDataset==null)
                    volumeDataset = await sequenceImporter.ImportSeriesAsync(sequence.First(), datasetName);
                else
                
                    await sequenceImporter.ImportSeriesSegmentationAsync(sequence.First(), volumeDataset);  
            }
            catch
            {
                ErrorNotifier.Instance.AddErrorMessageToUser($"Corrupted image series in dataset named: {datasetName} in folder {folderName}");
                return null;
            }
        }
        else
        {
            try
            {
                progressHandler.ReportProgress(0.2f, $"Loading {folderName}...");

                if(volumeDataset == null)
                    volumeDataset = await fileImporter.ImportAsync(filePath, datasetName);
                else
                    await fileImporter.ImportSegmentationAsync(filePath, volumeDataset);
            }
            catch
            {
                ErrorNotifier.Instance.AddErrorMessageToUser($"Corrupted data in dataset named: {datasetName} in folder {folderName}");
                return null;
            }
        }
        return volumeDataset;
    }
    private void LoadDicomDataPath(string dicomFolderPath, out string dicomPath, out bool isImageSequence, out int errorFlag)
    {
        List<string> dicomFilesCandidates = Directory.GetFiles(dicomFolderPath).ToList();

        dicomFilesCandidates.RemoveAll(x => x.EndsWith(".meta"));

        if (dicomFilesCandidates.Count == 0)
        {
            errorFlag = 1;
            dicomPath = null;
            isImageSequence = false;
            return;
        }

        DatasetType datasetType = DatasetImporterUtility.GetDatasetType(dicomFilesCandidates.First());

        if (datasetType == DatasetType.Unknown)
        {
            isImageSequence = false;
            dicomPath = dicomFilesCandidates[0];
            errorFlag = 2;
        }
        else if (datasetType == DatasetType.ImageSequence || datasetType == DatasetType.DICOM)
        {
            isImageSequence = true;
            dicomPath = dicomFolderPath;
            errorFlag = 0;
        }
        else     //NRRD or Niftis
        {
            isImageSequence = false;
            dicomPath = dicomFilesCandidates[0];
            errorFlag = 0;
        }
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
    public void SetTransferFunction(TransferFunction tf)
    {
        _volumeRenderedObject.SetTransferFunction(tf);      
    }
    public void AddDensitySlider()
    {
        AddValueDensitySlider(0, 0.2f,saveAfter:true);
    }
    public void AddValueDensitySlider(float minVal,float maxVal,bool saveAfter)
    {
        GameObject newSlider = Instantiate(_densitySliderPrefab, _densitySlidersContainer.transform);

        newSlider.transform.localPosition = new Vector3(0.09f - (DensityIntervalSliders.Count * 0.09f), 0.011f, 0.3f);
        newSlider.transform.localRotation = Quaternion.Euler(new Vector3(90, -90, 0));
        SliderIntervalUpdater sliderUpdater = newSlider.GetComponent<SliderIntervalUpdater>();
        sliderUpdater.IntervalSliderValueChanged += UpdateIsoRanges;
        sliderUpdater.SetInitValues(minVal, maxVal, Dataset.MinDataValue, Dataset.MaxDataValue);
        _sliderControlButtons.transform.localPosition = new Vector3(-0.03f - (DensityIntervalSliders.Count * 0.09f), 0, DensityIntervalSliders.Count > 0 ? 0.3f : 0.22f);

        DensityIntervalSliders.Add(sliderUpdater);

        if (DensityIntervalSliders.Count > 1)
            _removeSliderButton.SetActive(true);

        UpdateIsoRanges();

        if(saveAfter)
            _saveSystem.SaveDataAsync(this);
    }
    public void RemoveDensitySlider()
    {
        SliderIntervalUpdater sliderUpdater = DensityIntervalSliders.Last();
        sliderUpdater.IntervalSliderValueChanged -= UpdateIsoRanges;

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
            float[] minVals= new float[DensityIntervalSliders.Count];
            float[] maxVals = new float[DensityIntervalSliders.Count];

            for(int i=0;i<DensityIntervalSliders.Count;i++)
            {
                DensityIntervalSliders[i].GetSliderValues(out float minVal, out float maxVal);
                minVals[i] = minVal;
                maxVals[i] = maxVal;
            }
            _volumeRenderedObject.SetVisibilityWindow(minVals,maxVals, DensityIntervalSliders.Count);
        }
        catch { }
    }
    public void UpdateRenderingMode(RenderMode renderMode)
    {
        if(renderMode!= _volumeRenderedObject.GetRenderMode())
        {
            _volumeRenderedObject.SetRenderMode(renderMode);
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
        TurnLabelingKeyword(_segmentationPanelVisible);
    }
    public void UpdateCubicInterpolation(bool value)
    {
        _volumeRenderedObject.SetCubicInterpolationEnabled(value);
    }
    public void SetRaymarchStepCount(int value)
    {
        VolumeMesh.sharedMaterial.SetInt("_stepNumber", value);
    }
    public void UpdateLighting(bool value)
    {
        _volumeRenderedObject.SetLightingEnabled(value);
    }
    private async Task InitSegmentationAsync(ProgressHandler progressHandler)
    {
        VolumeMesh.sharedMaterial.SetTexture("_LabelTex", await Dataset.GetLabelTextureAsync(true, progressHandler));           //Very long

        Color[] uniqueColors = Utils.CreateDistinctColors(Dataset.LabelValues.Keys.Count);

        int iter = 0;

        foreach (float key in Dataset.LabelValues.Keys.OrderBy(x=>x))
        {
            if (key == 0) continue; 

            Color col = uniqueColors[iter];
            GameObject tmp = Instantiate(_segmentationSliderPrefab, _segmentationParentContainer.transform);
            tmp.transform.localPosition = new Vector3(0,0.24f -(0.06f * iter), 0.33f);
            tmp.transform.localRotation = Quaternion.Euler(new Vector3(0,-90,0));
            Segment segment = tmp.GetComponent<Segment>();
            segment.ColorUpdated += UpdateShaderLabelArray;
            segment.InitColor(col);

            if(Dataset.LabelNames.ContainsKey(key))
                segment.ChangeSegmentName(Dataset.LabelNames[key]);

            Segments.Add(segment);
            iter++;
        }
        UpdateShaderLabelArray();
    }
    public void TurnAllSegmentAlphas(bool value)
    {
        Segments.ForEach(x => x.AlphaUpdate(value?1:0));
        AllAlphaButtonsPressed?.Invoke();
        _saveSystem.SaveDataAsync(this);
    }
    private void TurnLabelingKeyword(bool value)
    {
        if(value)
            VolumeMesh.material.EnableKeyword("LABELING_SUPPORT_ON");
        else
            VolumeMesh.material.DisableKeyword("LABELING_SUPPORT_ON");
    }
    private void UpdateShaderLabelArray()
    {
        VolumeMesh.material.SetColorArray("_SegmentsColors", Segments.Select(x => x.SegmentColor).ToArray());
    }
    public async Task DownScaleDatasetAsync()
    {
        using (ProgressHandler progressHandler = new ProgressHandler(_orbProgressView))
        {
            progressHandler.Start("Downscaling dataset...",numberOfParts:3);

            ProcessingType = DatasetProcessingType.Downsampling;
            await Dataset.DownScaleDataAsync(progressHandler);
            await RegenerateTexturesDataAsync(progressHandler);
            ProcessingType = DatasetProcessingType.Normal;
        }
    }
    private async Task RegenerateTexturesDataAsync(ProgressHandler progressHandler)
    {
        VolumeMesh.sharedMaterial.SetTexture("_DataTex", await Dataset.GetDataTextureAsync(true, progressHandler));           //Very long
        VolumeMesh.sharedMaterial.SetTexture("_GradientTex", await Dataset.GetGradientTextureAsync(true,progressHandler));    //Very long        
    }
    public void UpdateSlicePlane(bool value)
    {
        _tfColorUpdater.gameObject.transform.localPosition = value ? new Vector3(0.275f, 0.02f, 0.3f) : new Vector3(0.1705f, 0.02f, 0.3f);  //Move the tf color slider if enabled

        SliceRendererWindow.gameObject.SetActive(value);

        _slicingPlaneXNormalAxisObject.gameObject.SetActive(value);
        _slicingPlaneYNormalAxisObject.gameObject.SetActive(value);
        _slicingPlaneZNormalAxisObject.gameObject.SetActive(value);
    }
    private void UpdateSlicePlaneWindow()
    {
        SliceRendererWindow.GetSliderValues(out float minVal, out float maxVal);
        SliceWindowMin = minVal;
        SliceWindowMax = maxVal;

        _slicingPlaneXNormalAxisObject.UpdateHounsfieldWindow(minVal, maxVal, Dataset.MinDataValue,Dataset.MaxDataValue);
        _slicingPlaneYNormalAxisObject.UpdateHounsfieldWindow(minVal, maxVal, Dataset.MinDataValue, Dataset.MaxDataValue);
        _slicingPlaneZNormalAxisObject.UpdateHounsfieldWindow(minVal, maxVal, Dataset.MinDataValue, Dataset.MaxDataValue);
    } 
    public async Task MirrorFlipTexturesAsync()
    {
        using (ProgressHandler progressHandler = new ProgressHandler(_orbProgressView))
        {
            ProcessingType = DatasetProcessingType.Mirrorflipping;
            _isDatasetReversed = !_isDatasetReversed;

            progressHandler.Start("Flipping Data...",numberOfParts:5);

            await Task.Run(() => Dataset.FlipTextureArrays());

            if (Segments.Count > 0)
            {
                VolumeMesh.sharedMaterial.SetTexture("_LabelTex", await Dataset.GetLabelTextureAsync(true,progressHandler));           //Very long
            }
            else
            {
                progressHandler.UpdateTotalNumberOfParts(3);
            }
            if (_isDatasetReversed)
                _volumeRenderedObject.gameObject.transform.localRotation = Quaternion.Euler(_mirrorFlippedRotation);
            else
                _volumeRenderedObject.gameObject.transform.localRotation = Quaternion.Euler(_normalRotation);

            await RegenerateTexturesDataAsync(progressHandler);
            ProcessingType = DatasetProcessingType.Normal;
        }
    }
    public void SetQRRotation()
    {
        Vector3 rot = _volumeRenderedObject.gameObject.transform.localRotation.eulerAngles;
        rot.z = 244.906f;
        _volumeRenderedObject.gameObject.transform.localRotation = Quaternion.Euler(rot);
    }
    public void ResetAllTransforms()
    {
        ResetMainObjectTransform();
        ResetCrossSectionToolsTransform();
        ResetHandleTransform();
        ResetSlicesTransform();

        _saveSystem.SaveDataAsync(this);
    }
    public void ResetMainObjectTransform()
    {
        Vector3 rot = _mainCamera.transform.rotation.eulerAngles;
        rot.y += 90;
        rot.x = 0;
        rot.z = 0;

        transform.position = _mainCamera.transform.position + (_mainCamera.transform.forward);
        transform.rotation = Quaternion.Euler(rot);
        transform.localScale = new Vector3(0.33f, 0.33f, 0.33f);
    }
    public void ResetCrossSectionToolsTransform()
    {
        Converters.UpdateTransform(_cutoutPlane.transform, _cutoutPlaneTransformSave, false);
        Converters.UpdateTransform(_cutoutSphere.transform, _cutoutSphereTransformSave, false);
    }
    public void ResetHandleTransform()
    {
        Converters.UpdateTransform(_controlHandle.transform, _grabHandleTransformSave, false);
    }
    public void ResetSlicesTransform()
    {
        Converters.UpdateTransform(_slicingPlaneXNormalAxisObject.transform, _slicingPlaneXTransformSave,false);
        Converters.UpdateTransform(_slicingPlaneYNormalAxisObject.transform, _slicingPlaneYTransformSave,false);
        Converters.UpdateTransform(_slicingPlaneZNormalAxisObject.transform, _slicingPlaneZTransformSave,false);
    }
    private void SaveInitialTransforms()
    {
        _cutoutPlaneTransformSave = Converters.ConvertTransform(_cutoutPlane.transform);
        _cutoutSphereTransformSave = Converters.ConvertTransform(_cutoutSphere.transform);
        _grabHandleTransformSave= Converters.ConvertTransform(_controlHandle.transform);

        _slicingPlaneXTransformSave=Converters.ConvertTransform(_slicingPlaneXNormalAxisObject.transform);
        _slicingPlaneYTransformSave=Converters.ConvertTransform(_slicingPlaneYNormalAxisObject.transform);
        _slicingPlaneZTransformSave=Converters.ConvertTransform(_slicingPlaneZNormalAxisObject.transform);
    }
    public void SetVolumePosition(Vector3 position)
    {
        transform.position = position;
    }
    private void OnTFReset()
    {
        _saveSystem.SaveDataAsync(this);        //MRTK buttons do not trigger OnInputUp event from IMixedRealityInputHandler so we need to save it manually
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
