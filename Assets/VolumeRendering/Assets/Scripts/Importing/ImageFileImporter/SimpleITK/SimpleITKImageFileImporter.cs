#if UVR_USE_SIMPLEITK
using UnityEngine;
using System;
using itk.simple;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
        public async Task<VolumeDataset> ImportAsync(string filePath,string datasetName)
        {
            float[] pixelData = null;
            VectorUInt32 size = null;
            VectorDouble spacing = null;
            // Create dataset
            VolumeDataset volumeDataset = new VolumeDataset();

            await Task.Run(() => {  
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

                volumeDataset.data = pixelData;
                volumeDataset.dimX = (int)size[0];
                volumeDataset.dimY = (int)size[1];
                volumeDataset.dimZ = (int)size[2];

                volumeDataset.labelDimX = (int)size[0];
                volumeDataset.labelDimY = (int)size[1];
                volumeDataset.labelDimZ = (int)size[2];

                volumeDataset.datasetName = datasetName;
                volumeDataset.filePath = filePath;
                volumeDataset.scaleX = (float)(spacing[0] * size[0]);
                volumeDataset.scaleY = (float)(spacing[1] * size[1]);
                volumeDataset.scaleZ = (float)(spacing[2] * size[2]);

                volumeDataset.FixDimensions();
            });

            return volumeDataset;
        }
        public async Task ImportSegmentationAsync(string filePath,VolumeDataset volumeDataset)
        {
            float[] pixelData = null;
            VectorUInt32 size = null;

            await Task.Run(() => {
                ImageFileReader reader = new ImageFileReader();

                reader.SetFileName(filePath);

                Image image = reader.Execute();

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

                volumeDataset.labelData = pixelData;            
            });
        }
    }
}
#endif
