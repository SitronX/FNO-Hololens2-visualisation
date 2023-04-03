#if UVR_USE_SIMPLEITK
using UnityEngine;
using System;
using itk.simple;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;

namespace UnityVolumeRendering
{
    /// <summary>
    /// SimpleITK-based DICOM importer.
    /// Has support for JPEG2000 and more.
    /// </summary>
    public class SimpleITKImageSequenceImporter : IImageSequenceImporter
    {
        public class ImageSequenceSlice : IImageSequenceFile
        {
            public string filePath;

            public string GetFilePath()
            {
                return filePath;
            }
        }

        public class ImageSequenceSeries : IImageSequenceSeries
        {
            public List<ImageSequenceSlice> files = new List<ImageSequenceSlice>();

            public IEnumerable<IImageSequenceFile> GetFiles()
            {
                return files;
            }
        }

        public IEnumerable<IImageSequenceSeries> LoadSeries(IEnumerable<string> files)
        {
            HashSet<string>  directories = new HashSet<string>();

            foreach (string file in files)
            {
                string dir = Path.GetDirectoryName(file);
                if (!directories.Contains(dir))
                    directories.Add(dir);
            }

            List<ImageSequenceSeries> seriesList = new List<ImageSequenceSeries>();
            Dictionary<string, VectorString> directorySeries = new Dictionary<string, VectorString>();
            foreach (string directory in directories)
            {
                VectorString seriesIDs = ImageSeriesReader.GetGDCMSeriesIDs(directory);
                directorySeries.Add(directory, seriesIDs);

            }

            foreach(var dirSeries in directorySeries)
            {
                foreach(string seriesID in dirSeries.Value)
                {
                    VectorString dicom_names = ImageSeriesReader.GetGDCMSeriesFileNames(dirSeries.Key, seriesID);
                    ImageSequenceSeries series = new ImageSequenceSeries();
                    foreach(string file in dicom_names)
                    {
                        ImageSequenceSlice sliceFile = new ImageSequenceSlice();
                        sliceFile.filePath = file;
                        series.files.Add(sliceFile);
                    }
                    seriesList.Add(series);
                }
            }

            return seriesList;
        }
        public async Task<IEnumerable<IImageSequenceSeries>> LoadSeriesAsync(IEnumerable<string> files,ProgressHandler progressHandler,bool segmentation)
        {
            List<ImageSequenceSeries> seriesList = null;
            await Task.Run(() => {
                HashSet<string> directories = new HashSet<string>();

                int totalCount = files.Count();
                int onePercent=totalCount/100;
                int percentCounter=0;
                int overall = 0;

                foreach (string file in files)
                {
                    string dir = Path.GetDirectoryName(file);
                    if (!directories.Contains(dir))
                        directories.Add(dir);

                    if(percentCounter > onePercent)
                    {
                        progressHandler.ReportProgress(overall, totalCount, segmentation?"Loading segmentation slices...": "Loading main slices...");
                        percentCounter = 0;
                    }
                    percentCounter++;
                    overall++;
                }

                seriesList = new List<ImageSequenceSeries>();
                Dictionary<string, VectorString> directorySeries = new Dictionary<string, VectorString>();
                foreach (string directory in directories)
                {
                    VectorString seriesIDs = ImageSeriesReader.GetGDCMSeriesIDs(directory);
                    directorySeries.Add(directory, seriesIDs);

                }

                foreach (var dirSeries in directorySeries)
                {
                    foreach (string seriesID in dirSeries.Value)
                    {
                        VectorString dicom_names = ImageSeriesReader.GetGDCMSeriesFileNames(dirSeries.Key, seriesID);
                        ImageSequenceSeries series = new ImageSequenceSeries();
                        foreach (string file in dicom_names)
                        {
                            ImageSequenceSlice sliceFile = new ImageSequenceSlice();
                            sliceFile.filePath = file;
                            series.files.Add(sliceFile);
                        }
                        seriesList.Add(series);
                    }
                }

                
            });

            return seriesList;
        }

        public VolumeDataset ImportSeries(IImageSequenceSeries series)
        {
            ImageSequenceSeries sequenceSeries = (ImageSequenceSeries)series;
            if (sequenceSeries.files.Count == 0)
            {
                Debug.LogError("Empty series. No files to load.");
                return null;
            }

            ImageSeriesReader reader = new ImageSeriesReader();

            VectorString dicomNames = new VectorString();
            foreach (var dicomFile in sequenceSeries.files)
                dicomNames.Add(dicomFile.filePath);
            reader.SetFileNames(dicomNames);

            Image image = reader.Execute();

            // Cast to 32-bit float
            image = SimpleITK.Cast(image, PixelIDValueEnum.sitkFloat32);

            VectorUInt32 size = image.GetSize();

            int numPixels = 1;
            for (int dim = 0; dim < image.GetDimension(); dim++)
                numPixels *= (int)size[dim];

            // Read pixel data
            float[] pixelData = new float[numPixels];
            IntPtr imgBuffer = image.GetBufferAsFloat();
            Marshal.Copy(imgBuffer, pixelData, 0, numPixels);

            for (int i = 0; i < pixelData.Length; i++)
                pixelData[i] = Mathf.Clamp(pixelData[i], -1024, 3071);

            VectorDouble spacing = image.GetSpacing();

            // Create dataset
            VolumeDataset volumeDataset = new VolumeDataset();
            volumeDataset.data = pixelData;
            volumeDataset.dimX = (int)size[0];
            volumeDataset.dimY = (int)size[1];
            volumeDataset.dimZ = (int)size[2];
            volumeDataset.datasetName = "test";
            volumeDataset.filePath = dicomNames[0];
            volumeDataset.scaleX = (float)(spacing[0] * size[0]);
            volumeDataset.scaleY = (float)(spacing[1] * size[1]);
            volumeDataset.scaleZ = (float)(spacing[2] * size[2]);

            volumeDataset.FixDimensions();

            return volumeDataset;
        }
        public async Task<(VolumeDataset,bool)> ImportSeriesAsync(IImageSequenceSeries series,string datasetName)
        {
            Image image = null;
            float[] pixelData = null;
            VectorUInt32 size = null;
            VectorString dicomNames = null;
            bool isDatasetReversed = true;

            // Create dataset
            VolumeDataset volumeDataset = new VolumeDataset();

            ImageSequenceSeries sequenceSeries = (ImageSequenceSeries)series;
            if (sequenceSeries.files.Count == 0)
            {
                Debug.LogError("Empty series. No files to load.");
                return (null,false);
            }

            await Task.Run(() => {
               
                ImageSeriesReader reader = new ImageSeriesReader();

                string first = sequenceSeries.files.First().filePath;
                string last = sequenceSeries.files.Last().filePath;

                Image firstImage = SimpleITK.ReadImage(first);
                Image lastImage = SimpleITK.ReadImage(last);

                isDatasetReversed = !Utils.IsHeadFeetDataset(firstImage, lastImage);

                dicomNames = new VectorString();
          
                foreach (var dicomFile in sequenceSeries.files)
                    dicomNames.Add(dicomFile.filePath);
                reader.SetFileNames(dicomNames);
            
                image = reader.Execute();

                // Cast to 32-bit float
                image = SimpleITK.Cast(image, PixelIDValueEnum.sitkFloat32);

                size = image.GetSize();

                int numPixels = 1;
                for (int dim = 0; dim < image.GetDimension(); dim++)
                    numPixels *= (int)size[dim];

                // Read pixel data
                pixelData = new float[numPixels];
                IntPtr imgBuffer = image.GetBufferAsFloat();
                Marshal.Copy(imgBuffer, pixelData, 0, numPixels);

                //int onePercent=pixelData.Length / 100;
                //int percentCounter = 0;
                //for (int i = 0; i < pixelData.Length; i++)
                //{
                //    pixelData[i] = Mathf.Clamp(pixelData[i], -1024, 3071);      //Hounsfield values clamp
                //
                //    if(percentCounter>=onePercent)
                //    {
                //        progressHandler.ReportProgress(i, pixelData.Length, "Clamping data...");
                //        percentCounter =0;
                //    }
                //    percentCounter++;
                //}

                VectorDouble spacing = image.GetSpacing();


                volumeDataset.data = isDatasetReversed? pixelData.Reverse().ToArray():pixelData;
                volumeDataset.dimX = (int)size[0];
                volumeDataset.dimY = (int)size[1];
                volumeDataset.dimZ = (int)size[2];

                volumeDataset.datasetName = datasetName;
                volumeDataset.filePath = dicomNames[0];
                volumeDataset.scaleX = (float)(spacing[0] * size[0]);
                volumeDataset.scaleY = (float)(spacing[1] * size[1]);
                volumeDataset.scaleZ = (float)(spacing[2] * size[2]);

                volumeDataset.FixDimensions();
            });
            
            return (volumeDataset,isDatasetReversed);
        }
        public async Task ImportSeriesSegmentationAsync(IImageSequenceSeries series,VolumeDataset volumeDataset,bool isDatasetReversed)
        {
            Image image = null;
            float[] pixelData = null;
            VectorUInt32 size = null;
            VectorString dicomNames = null;


            ImageSequenceSeries sequenceSeries = (ImageSequenceSeries)series;
            if (sequenceSeries.files.Count == 0)
            {
                Debug.LogError("Empty series. No files to load.");
                return;
            }

            await Task.Run(() => {

                ImageSeriesReader reader = new ImageSeriesReader();

                dicomNames = new VectorString();

                foreach (var dicomFile in sequenceSeries.files)
                    dicomNames.Add(dicomFile.filePath);
                reader.SetFileNames(dicomNames);

                image = reader.Execute();

                try
                {
                    Image labelImage = SimpleITK.Cast(image, PixelIDValueEnum.sitkUInt16);

                    Image binary = SimpleITK.BinaryThreshold(labelImage, 1, int.MaxValue, 1, 0);
                    LabelStatisticsImageFilter stats = null;
                    stats = new LabelStatisticsImageFilter();
                    stats.Execute(binary, labelImage);

                    ulong numSegments = stats.GetNumberOfLabels();

                    List<string> segmentNames = new List<string>();
                    for (ulong i = 0; i < numSegments; i++)
                    {
                        //string segmentName = reader.GetMetaData($"Segment{i}_Name");
                        //segmentNames.Add(segmentName);
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }

                // Cast to 32-bit float
                image = SimpleITK.Cast(image, PixelIDValueEnum.sitkFloat32);

                size = image.GetSize();

                if (size[0] != volumeDataset.dimX || size[1] != volumeDataset.dimY || size[2] != volumeDataset.dimZ)
                    ErrorNotifier.Instance.AddErrorMessageToUser($"Segmentation file in dataset named: {volumeDataset.datasetName} in folder Data has other dimensions than base dataset.");

                int numPixels = 1;
                for (int dim = 0; dim < image.GetDimension(); dim++)
                    numPixels *= (int)size[dim];

                // Read pixel data
                pixelData = new float[numPixels];
                IntPtr imgBuffer = image.GetBufferAsFloat();
                Marshal.Copy(imgBuffer, pixelData, 0, numPixels);

                for (int i = 0; i < pixelData.Length; i++)
                    pixelData[i] = Mathf.Clamp(pixelData[i], -1024, 3071);

                VectorDouble spacing = image.GetSpacing();

                volumeDataset.labelData = isDatasetReversed? pixelData.Reverse().ToArray():pixelData;

                volumeDataset.labelDimX = (int)size[0];
                volumeDataset.labelDimY = (int)size[1];
                volumeDataset.labelDimZ = (int)size[2];
            });

        }
      

    }
}
#endif
