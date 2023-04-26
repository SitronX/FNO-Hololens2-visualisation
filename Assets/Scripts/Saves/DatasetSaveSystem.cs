using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class DatasetSaveSystem : MonoBehaviour
{
    [SerializeField] Transform _mainDatasetT;
    [SerializeField] Transform _grabHandleT;
    [SerializeField] Transform _crossPlaneT;
    [SerializeField] Transform _crossSphereT;

    [SerializeField] Transform _sliceXT;
    [SerializeField] Transform _sliceYT;
    [SerializeField] Transform _sliceZT;

    DatasetSaveData _saveData;

    string _savePath = "";

    public async void SaveDataAsync(VolumeDataControl volumeControl)
    {
        if (volumeControl.HasBeenLoaded)
        {
            if (_saveData == null)
                _saveData = new DatasetSaveData();

            _saveData.MainDatasetTransform = Converters.ConvertTransform(_mainDatasetT);
            _saveData.GrabHandleTransform = Converters.ConvertTransform(_grabHandleT);
            _saveData.CrossPlaneTransform = Converters.ConvertTransform(_crossPlaneT);
            _saveData.CrossSphereTransform = Converters.ConvertTransform(_crossSphereT);

            _saveData.SliceXTransform = Converters.ConvertTransform(_sliceXT);
            _saveData.SliceYTransform = Converters.ConvertTransform(_sliceYT);
            _saveData.SliceZTransform = Converters.ConvertTransform(_sliceZT);

            await Task.Run(() =>
            {
                _saveData.DensityIntervalSliders = Converters.ConvertDensitySliders(volumeControl.DensityIntervalSliders);
                _saveData.SegmentColors = Converters.ConvertColors(volumeControl.Segments.Select(x => x.SegmentColor).ToList());
                _saveData.TransferFunction = Converters.ConvertTransferFunction(volumeControl.TransferFunction);
                _saveData.SliceWindowRange=Converters.ConvertVector(new Vector3(volumeControl.SliceWindowMin, volumeControl.SliceWindowMax,0));

                string jsonText = JsonConvert.SerializeObject(_saveData);
                File.WriteAllText(_savePath, jsonText);
            });
            
        }
    }
    public async Task TryLoadSaveFileAsync(string datasetPath)
    {
        await Task.Run(() =>
        {
            string folderPath = datasetPath + "/Saves";
            _savePath = folderPath + "/SaveFile.json";

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            if (File.Exists(_savePath))
            {
                try
                {
                    string jsonText = File.ReadAllText(_savePath);
                    _saveData = JsonConvert.DeserializeObject<DatasetSaveData>(jsonText);           
                }
                catch { }       //If it is somehow corrupted, just ignore it, save will be overidden eventually
            }
        });
    }
    public bool TryLoadSaveTransformData()
    {
        try
        {
            if (_saveData != null)
            {
                Converters.UpdateTransform(_mainDatasetT.transform, _saveData.MainDatasetTransform);
                Converters.UpdateTransform(_grabHandleT.transform, _saveData.GrabHandleTransform);
                Converters.UpdateTransform(_crossPlaneT.transform, _saveData.CrossPlaneTransform);
                Converters.UpdateTransform(_crossSphereT.transform, _saveData.CrossSphereTransform);

                Converters.UpdateTransform(_sliceXT.transform, _saveData.SliceXTransform);
                Converters.UpdateTransform(_sliceYT.transform, _saveData.SliceYTransform);
                Converters.UpdateTransform(_sliceZT.transform, _saveData.SliceZTransform);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
    public bool TryLoadSaveDensitySliders(VolumeDataControl volumeControl)
    {
        try
        {
            if (_saveData != null&& _saveData.DensityIntervalSliders.Count>0)
            {
                for (int i = 0; i < _saveData.DensityIntervalSliders.Count; i++)
                    volumeControl.AddValueDensitySlider(_saveData.DensityIntervalSliders[i].MinValue, _saveData.DensityIntervalSliders[i].MaxValue,false);

                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
    public bool TryLoadSaveSegmentData(VolumeDataControl volumeControl)
    {
        try
        {
            if (_saveData != null&& _saveData.SegmentColors.Count>0)
            {
                for (int i = 0; i < _saveData.SegmentColors.Count; i++)
                {
                    Color col = Converters.ConvertColor(_saveData.SegmentColors[i]);
                    volumeControl.Segments[i].InitColor(col);
                    volumeControl.Segments[i].AlphaUpdate(_saveData.SegmentColors[i].A);
                }
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
    public bool TryLoadSliceWindow(VolumeDataControl volumeControl)
    {
        try
        {
            if (_saveData != null&&!_saveData.SliceWindowRange.IsVectorEmpty())
            {
                volumeControl.SliceRendererWindow.SetInitvalue(_saveData.SliceWindowRange.X,_saveData.SliceWindowRange.Y);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
    public bool TryLoadTFData(TFColorUpdater tfUpdater)
    {
        try
        {
            if (_saveData != null&& _saveData.TransferFunction.ColourControlPoints.Count>0)
            {
                for (int i = 0; i < _saveData.TransferFunction.ColourControlPoints.Count; i++)
                {
                    tfUpdater.UpdateSliderColorPosition(i, _saveData.TransferFunction.ColourControlPoints[i].SliderValue);
                }
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
}
