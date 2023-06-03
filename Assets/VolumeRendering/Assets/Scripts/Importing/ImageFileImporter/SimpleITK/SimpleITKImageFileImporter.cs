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
using System.Xml;
using Unity.Jobs;
using Microsoft.MixedReality.Toolkit;
using Unity.Collections;

namespace UnityVolumeRendering
{
    /// <summary>
    /// SimpleITK-based DICOM importer.
    /// </summary>
    public class SimpleITKImageFileImporter : IImageFileImporter
    {
        public VolumeDataset Import(string filePath)
        {
            ImageFileReader reader = new ImageFileReader();

            reader.SetFileName(filePath);

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

            VectorDouble spacing = image.GetSpacing();

            // Create dataset
            VolumeDataset volumeDataset = new VolumeDataset();
            volumeDataset.data = pixelData;
            volumeDataset.dimX = (int)size[0];
            volumeDataset.dimY = (int)size[1];
            volumeDataset.dimZ = (int)size[2];
            volumeDataset.datasetName = "test";
            volumeDataset.filePath = filePath;
            volumeDataset.scaleX = (float)(spacing[0] * size[0]);
            volumeDataset.scaleY = (float)(spacing[1] * size[1]);
            volumeDataset.scaleZ = (float)(spacing[2] * size[2]);

            volumeDataset.FixDimensions();

            return volumeDataset;
        }
        public async Task<VolumeDataset> ImportAsync(string filePath, string datasetName)
        {
            float[] pixelData = null;
            VectorUInt32 size = null;
            VectorDouble spacing = null;
            // Create dataset
            VolumeDataset volumeDataset = new VolumeDataset();


            await Task.Run(() =>
            {
                ImageFileReader reader = new ImageFileReader();

                reader.SetFileName(filePath);

                Image image = reader.Execute();

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

                spacing = image.GetSpacing();

                volumeDataset.data = pixelData.Reverse().ToArray();
                volumeDataset.dimX = (int)size[0];
                volumeDataset.dimY = (int)size[1];
                volumeDataset.dimZ = (int)size[2];

                volumeDataset.datasetName = datasetName;
                volumeDataset.filePath = filePath;
                volumeDataset.scaleX = (float)(spacing[0] * size[0]);
                volumeDataset.scaleY = (float)(spacing[1] * size[1]);
                volumeDataset.scaleZ = (float)(spacing[2] * size[2]);

                volumeDataset.FixDimensions();
            });

            return volumeDataset;
        }

        public async Task ImportSegmentationAsync(string filePath, VolumeDataset volumeDataset)
        {
            float[] pixelData = null;
            VectorUInt32 size = null;
            Image image = null;
            ImageFileReader reader = null;

            await Task.Run(() =>
            {
                reader = new ImageFileReader();
                reader.SetFileName(filePath);
                image = reader.Execute();
            });
            
            await Task.Run(() =>
            {
                int segmentNumber = 0;
                List<string> metaDataKeys = reader.GetMetaDataKeys().ToList();
                int numOfChannels = (int)image.GetNumberOfComponentsPerPixel();

                for(int i=0;i<numOfChannels;i++)
                {
                    volumeDataset.LabelNames.Add(new Dictionary<float, string>());
                    volumeDataset.LabelValues.Add(new Dictionary<float, float>());
                }

                while (true)
                {
                    string key = $"Segment{segmentNumber}_Name";
                    string keyValue = $"Segment{segmentNumber}_LabelValue";
                    string layerValue = $"Segment{segmentNumber}_Layer";

                    if (metaDataKeys.Contains(key))
                    {
                        float segmentValue = float.Parse(reader.GetMetaData(keyValue),CultureInfo.InvariantCulture);
                        string segmentName = reader.GetMetaData(key);
                        int layer = int.Parse(reader.GetMetaData(layerValue));

                        volumeDataset.LabelNames[layer].Add(segmentValue, segmentName);
                        segmentNumber++;
                    }
                    else
                    {
                        break;
                    }
                }


                // Cast to 32-bit float
                image = SimpleITK.Cast(image, PixelIDValueEnum.sitkVectorFloat32);
                size=image.GetSize();

                int numPixels = numOfChannels;

                for (int dim = 0; dim < image.GetDimension(); dim++)
                    numPixels *= (int)size[dim];

                // Read pixel data
                pixelData = new float[numPixels];
                IntPtr imgBuffer = image.GetBufferAsFloat();
                Marshal.Copy(imgBuffer, pixelData, 0, numPixels);


                pixelData = pixelData.Reverse().ToArray();

                NativeArray<float>[] labelData= new NativeArray<float>[numOfChannels];
                int pixelsPerLayer = numPixels / numOfChannels;

                for (int i=0;i<numOfChannels;i++)       //Extracting 3D images by channels/layers
                {
                    labelData[i] = new NativeArray<float>(pixelData.Where((x, index) => (index +i ) % numOfChannels == 0).ToArray(), Allocator.Persistent);
                }


                volumeDataset.nativeLabelData = labelData;
                volumeDataset.labelDimX = (int)size[0];
                volumeDataset.labelDimY = (int)size[1];
                volumeDataset.labelDimZ = (int)size[2];
                volumeDataset.HowManyLabelMapLayers= numOfChannels;
            });

        }
    }
}
#endif
