using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// An imported dataset. Has a dimension and a 3D pixel array.
    /// </summary>
    [Serializable]
    public class VolumeDataset : ScriptableObject
    {
        public string filePath;

        // Flattened 3D array of data sample values.
        [SerializeField]
        public float[] data;

        [SerializeField]
        public float[] labelData;

        public Dictionary<float,float> LabelValues { get; set; } =new Dictionary<float,float>();      //Label value and position

        [SerializeField]
        public int dimX, dimY, dimZ;

        [SerializeField]
        public float scaleX = 1.0f;
        [SerializeField]
        public float scaleY = 1.0f;
        [SerializeField]
        public float scaleZ = 1.0f;

        public float volumeScale;

        [SerializeField]
        public string datasetName;

        private float minDataValue = float.MaxValue;
        private float maxDataValue = float.MinValue;

        private Texture3D dataTexture = null;
        private Texture3D gradientTexture = null;

        //TODO
        private Texture3D labelTexture = null;
        public int labelDimX, labelDimY, labelDimZ;
        public List<string> LabelNames { get; set; } = new List<string>();


        public Texture3D GetDataTexture()
        {
            if (dataTexture == null)
                dataTexture = CreateTextureInternal();
            return dataTexture;
        }
        public async Task<Texture3D> GetDataTextureAsync(bool generateNew,ProgressHandler progressHandler)
        {
            if (dataTexture == null||generateNew)
            {
                await CreateTextureInternalAsync(progressHandler);
            }
            return dataTexture;
        }

        public Texture3D GetGradientTexture()
        {
            if (gradientTexture == null)
                gradientTexture = CreateGradientTextureInternal();
            return gradientTexture;
        }
        public async Task<Texture3D> GetGradientTextureAsync(bool generateNew,ProgressHandler progressHandler)
        {
            if (gradientTexture == null||generateNew)
            {
                await CreateGradientTextureInternalAsync(progressHandler);
            }
            return gradientTexture;
        }
        public async Task<Texture3D> GetLabelTextureAsync(bool generateNew,ProgressHandler progressHandler)
        {
            if (labelTexture == null||generateNew)
            {
                await CreateLabelTextureInternalAsync(progressHandler);
            }
            return labelTexture;
        }

        public float GetMinDataValue(ProgressHandler progressHandler)
        {
            if (minDataValue == float.MaxValue)
                CalculateValueBounds(progressHandler);
            return minDataValue;
        }
        public float GetMinDataValue()
        {
            if (minDataValue == float.MaxValue)
                CalculateValueBounds();
            return minDataValue;
        }

        public float GetMaxDataValue(ProgressHandler progressHandler)
        {
            if (maxDataValue == float.MinValue)
                CalculateValueBounds(progressHandler);
            return maxDataValue;
        }
        public float GetMaxDataValue()
        {
            if (maxDataValue == float.MinValue)
                CalculateValueBounds();
            return maxDataValue;
        }

        public void FindAllSegments(ProgressHandler progressHandler)
        {
            if (labelData != null)
            {
                int totalCount = labelData.Length;
                int onePercent=totalCount/100;
                int percentCounter = 0;
                for (int i = 0; i < totalCount; i++)
                {
                    float val = labelData[i];
                    
                    if (!LabelValues.ContainsKey(val))
                        LabelValues.Add(val, 0);

                    if (percentCounter >= onePercent)
                    {
                        progressHandler.ReportProgress(i, totalCount, "Finding all segments...");
                        percentCounter = 0;
                    }
                    percentCounter++;
                }
            }
        }

        /// <summary>
        /// Ensures that the dataset is not too large.
        /// </summary>
        public void FixDimensions()
        {
            int MAX_DIM = 2048; // 3D texture max size. See: https://docs.unity3d.com/Manual/class-Texture3D.html

            while (Mathf.Max(dimX, dimY, dimZ) > MAX_DIM)
            {
                Debug.LogWarning("Dimension exceeds limits (maximum: " + MAX_DIM + "). Dataset is downscaled by 2 on each axis!");
                DownScaleData();
            }
        }

        /// <summary>
        /// Downscales the data by averaging 8 voxels per each new voxel,
        /// and replaces downscaled data with the original data
        /// </summary>
        public void DownScaleData()
        {
            int halfDimX = dimX / 2 + dimX % 2;
            int halfDimY = dimY / 2 + dimY % 2;
            int halfDimZ = dimZ / 2 + dimZ % 2;
            float[] downScaledData = new float[halfDimX * halfDimY * halfDimZ];

            for (int x = 0; x < halfDimX; x++)
            {
                for (int y = 0; y < halfDimY; y++)
                {
                    for (int z = 0; z < halfDimZ; z++)
                    {
                        downScaledData[x + y * halfDimX + z * (halfDimX * halfDimY)] = Mathf.Round(GetAvgerageVoxelValues(x * 2, y * 2, z * 2));
                    }
                }
            }

            //Update data & data dimensions
            data = downScaledData;
            dimX = halfDimX;
            dimY = halfDimY;
            dimZ = halfDimZ;
        }
        public void FlipTextureArrays()
        {
            data=data.Reverse().ToArray();

            if(labelData!=null)
                labelData= labelData.Reverse().ToArray();
        }
        public async Task DownScaleDataAsync()
        {
            await Task.Run(() => {
                int halfDimX = dimX / 2 + dimX % 2;
                int halfDimY = dimY / 2 + dimY % 2;
                int halfDimZ = dimZ / 2 + dimZ % 2;
                float[] downScaledData = new float[halfDimX * halfDimY * halfDimZ];

                for (int x = 0; x < halfDimX; x++)
                {
                    for (int y = 0; y < halfDimY; y++)
                    {
                        for (int z = 0; z < halfDimZ; z++)
                        {
                            downScaledData[x + y * halfDimX + z * (halfDimX * halfDimY)] = Mathf.Round(GetAvgerageVoxelValues(x * 2, y * 2, z * 2));
                        }
                    }
                }

                //Update data & data dimensions
                data = downScaledData;
                dimX = halfDimX;
                dimY = halfDimY;
                dimZ = halfDimZ;
            });
        }

        private void CalculateValueBounds(ProgressHandler progressHandler)
        {
            minDataValue = float.MaxValue;
            maxDataValue = float.MinValue;

            if (data != null)
            {
                int totalCount=dimX * dimY * dimZ;
                int onePercent=totalCount/100;
                int percentCounter = 0;
                for (int i = 0; i < totalCount; i++)
                {
                    float val = data[i];
                    minDataValue = Mathf.Min(minDataValue, val);
                    maxDataValue = Mathf.Max(maxDataValue, val);

                    if (percentCounter >= onePercent)
                    {
                        progressHandler.ReportProgress(i, totalCount, "Calculating Boundaries...");
                        percentCounter = 0;
                    }
                    percentCounter++;
                }
            }
        }
        private void CalculateValueBounds()
        {
            minDataValue = float.MaxValue;
            maxDataValue = float.MinValue;

            if (data != null)
            {
                int totalCount = dimX * dimY * dimZ;
              
                for (int i = 0; i < totalCount; i++)
                {
                    float val = data[i];
                    minDataValue = Mathf.Min(minDataValue, val);
                    maxDataValue = Mathf.Max(maxDataValue, val);
                }
            }
        }


        private Texture3D CreateTextureInternal()
        {
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RHalf) ? TextureFormat.RHalf : TextureFormat.RFloat;
            Texture3D texture = new Texture3D(dimX, dimY, dimZ, texformat, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            float minValue = GetMinDataValue();
            float maxValue = GetMaxDataValue();
            float maxRange = maxValue - minValue;

            bool isHalfFloat = texformat == TextureFormat.RHalf;
            try
            {
                if (isHalfFloat)
                {
                    NativeArray<ushort> pixelBytes = new NativeArray<ushort>(data.Length, Allocator.Temp);
                    for (int iData = 0; iData < data.Length; iData++)
                        pixelBytes[iData] = Mathf.FloatToHalf((float)(data[iData] - minValue) / maxRange);
                    texture.SetPixelData(pixelBytes, 0);
                }
                else
                {
                    NativeArray<float> pixelBytes = new NativeArray<float>(data.Length, Allocator.Temp);
                    for (int iData = 0; iData < data.Length; iData++)
                        pixelBytes[iData] = (float)(data[iData] - minValue) / maxRange;
                    texture.SetPixelData(pixelBytes, 0);
                }
            }
            catch (OutOfMemoryException)
            {
                Debug.LogWarning("Out of memory when creating texture. Using fallback method.");
                for (int x = 0; x < dimX; x++)
                    for (int y = 0; y < dimY; y++)
                        for (int z = 0; z < dimZ; z++)
                            texture.SetPixel(x, y, z, new Color((float)(data[x + y * dimX + z * (dimX * dimY)] - minValue) / maxRange, 0.0f, 0.0f, 0.0f));
            }
            texture.Apply();
            return texture;
        }
        private async Task CreateTextureInternalAsync(ProgressHandler progressHandler)                                             //This method can be also called in custom logic to load it before continuing
        {
            Debug.Log("Async texture generation. Hold on.");

            Texture3D.allowThreadedTextureCreation = true;
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RHalf) ? TextureFormat.RHalf : TextureFormat.RFloat;

            float minValue = 0;
            float maxValue = 0;
            float maxRange = 0;

            await Task.Run(() =>
            {
                minValue = GetMinDataValue(progressHandler);
                maxValue = GetMaxDataValue(progressHandler);
                maxRange = maxValue - minValue;
            });

            bool isHalfFloat = texformat == TextureFormat.RHalf;

            try
            {
                if (isHalfFloat)
                {
                    NativeArray<ushort> pixelBytes = default;

                    await Task.Run(() => {
                        pixelBytes = new NativeArray<ushort>(data.Length, Allocator.TempJob);

                        int onePercentVal = data.Length / 100;
                        int percentCounter = 0;
                        for (int iData = 0; iData < data.Length; iData++)
                        {
                            pixelBytes[iData] = Mathf.FloatToHalf((float)(data[iData] - minValue) / maxRange);
                            if(percentCounter>=onePercentVal)
                            {
                                progressHandler.ReportProgress(iData, data.Length, "Creating Data...");
                                percentCounter = 0;
                            }
                            percentCounter++;
                        }
                    });

                    Texture3D texture = new Texture3D(dimX, dimY, dimZ, texformat, false);                  //Grouped texture stuff so it doesnt freezes twice, but only once
                    texture.wrapMode = TextureWrapMode.Clamp;
                    texture.SetPixelData(pixelBytes, 0);
                    texture.Apply();
                    dataTexture = texture;

                    pixelBytes.Dispose();
                }
                else
                {
                    NativeArray<float> pixelBytes = default;

                    int onePercentVal = data.Length / 100;
                    int percentCounter = 0;

                    await Task.Run(() => {
                        pixelBytes = new NativeArray<float>(data.Length, Allocator.TempJob);
                        for (int iData = 0; iData < data.Length; iData++)
                        {
                            pixelBytes[iData] = (float)(data[iData] - minValue) / maxRange;

                            if (percentCounter >= onePercentVal)
                            {
                                progressHandler.ReportProgress(iData, data.Length, "Creating Data...");
                                percentCounter = 0;
                            }
                            percentCounter++;
                        }
                    });

                    Texture3D texture = new Texture3D(dimX, dimY, dimZ, texformat, false);                  //Grouped texture stuff so it doesnt freezes twice, but only once
                    texture.wrapMode = TextureWrapMode.Clamp;
                    texture.SetPixelData(pixelBytes, 0);
                    texture.Apply();
                    dataTexture = texture;

                    pixelBytes.Dispose();
                }
            }
            catch (OutOfMemoryException)
            {
                Texture3D texture = new Texture3D(dimX, dimY, dimZ, texformat, false);                  //Grouped texture stuff so it doesnt freezes twice, but only once
                texture.wrapMode = TextureWrapMode.Clamp;


                Debug.LogWarning("Out of memory when creating texture. Using fallback method.");
                for (int x = 0; x < dimX; x++)
                    for (int y = 0; y < dimY; y++)
                        for (int z = 0; z < dimZ; z++)
                            texture.SetPixel(x, y, z, new Color((float)(data[x + y * dimX + z * (dimX * dimY)] - minValue) / maxRange, 0.0f, 0.0f, 0.0f));

                texture.Apply();
                dataTexture = texture;
            }

            Debug.Log("Texture generation done.");
        }
        private async Task CreateLabelTextureInternalAsync(ProgressHandler progressHandler)                                         //It would be ideal to represent label values as Int, but i didnt manage to get it working
        {
            Debug.Log("Async label texture generation. Hold on.");

            Texture3D.allowThreadedTextureCreation = true;
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RHalf) ? TextureFormat.RHalf : TextureFormat.RFloat;

            await Task.Run(() =>
            {
                FindAllSegments(progressHandler);
                OrderLabelDictionary();
            });

            try
            {
                if (texformat == TextureFormat.RHalf)
                {
                    NativeArray<ushort> pixelBytes = default;

                    await Task.Run(() =>
                    {
                       
                        int onePercentVal = labelData.Length / 100;
                        int percentCounter = 0;

                        pixelBytes = new NativeArray<ushort>(labelData.Length, Allocator.TempJob);
                        for (int iData = 0; iData < labelData.Length; iData++)
                        {
                            pixelBytes[iData] = Mathf.FloatToHalf(LabelValues[labelData[iData]]);                             //Assigning correct label map values

                            if (percentCounter >= onePercentVal)
                            {
                                progressHandler.ReportProgress(iData, labelData.Length, "Creating Segmentation Data...");
                                percentCounter = 0;
                            }
                            percentCounter++;
                        }
                    });

                    Texture3D texture = new Texture3D(labelDimX, labelDimY, labelDimZ, texformat, false);                  //Grouped texture stuff so it doesnt freezes twice, but only once
                    texture.wrapMode = TextureWrapMode.Clamp;
                    texture.SetPixelData(pixelBytes, 0);
                    texture.filterMode= FilterMode.Point;           //Main culprit of impossible shader issue. Without point interpolation, it refused to round out to whole int number and was giving strangest results
                    texture.Apply();
                    labelTexture = texture;

                    pixelBytes.Dispose();
                }
                else
                {
                    NativeArray<float> pixelBytes = default;

                    await Task.Run(() =>
                    {
                        int onePercentVal = labelData.Length / 100;
                        int percentCounter = 0;

                        pixelBytes = new NativeArray<float>(labelData.Length, Allocator.TempJob);
                        for (int iData = 0; iData < labelData.Length; iData++)
                        {
                            pixelBytes[iData] = LabelValues[labelData[iData]];                             //Assigning correct label map values

                            if (percentCounter >= onePercentVal)
                            {
                                progressHandler.ReportProgress(iData, labelData.Length, "Creating Segmentation Data...");
                                percentCounter = 0;
                            }
                            percentCounter++;
                        }
                    });

                    Texture3D texture = new Texture3D(labelDimX, labelDimY, labelDimZ, texformat, false);                  //Grouped texture stuff so it doesnt freezes twice, but only once
                    texture.wrapMode = TextureWrapMode.Clamp;
                    texture.SetPixelData(pixelBytes, 0);
                    //texture.filterMode = FilterMode.Point;
                    texture.Apply();
                    labelTexture = texture;

                    pixelBytes.Dispose();
                }
            }
            catch (OutOfMemoryException)
            {
                Texture3D texture = new Texture3D(labelDimX, labelDimY, labelDimZ, TextureFormat.RFloat, false);                  //Grouped texture stuff so it doesnt freezes twice, but only once
                texture.wrapMode = TextureWrapMode.Clamp;


                Debug.LogWarning("Out of memory when creating texture. Using fallback method.");
                for (int x = 0; x < labelDimX; x++)
                    for (int y = 0; y < labelDimY; y++)
                        for (int z = 0; z < labelDimZ; z++)
                            texture.SetPixel(x, y, z, new Color(LabelValues[labelData[x + y * labelDimX + z * (labelDimX * labelDimY)]], 0.0f, 0.0f, 0.0f));

                texture.Apply();
                labelTexture = texture;
            }

            Debug.Log("Label Texture generation done.");
        }
        private void OrderLabelDictionary()
        {
            List<float> values = new List<float>();

            foreach(float i in LabelValues.Keys)
                values.Add(i);

            values=values.OrderBy(x=>x).ToList();

            for (int i = 0; i < values.Count; i++)
                LabelValues[values[i]] = i;
        }

        private Texture3D CreateGradientTextureInternal()
        {
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;
            Texture3D texture = new Texture3D(dimX, dimY, dimZ, texformat, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            float minValue = GetMinDataValue();
            float maxValue = GetMaxDataValue();
            float maxRange = maxValue - minValue;

            Color[] cols;
            try
            {
                cols = new Color[data.Length];
            }
            catch (OutOfMemoryException)
            {
                cols = null;
            }
            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        int iData = x + y * dimX + z * (dimX * dimY);

                        Vector3 grad = GetGrad(x, y, z, minValue, maxRange);

                        if (cols == null)
                        {
                            texture.SetPixel(x, y, z, new Color(grad.x, grad.y, grad.z, (float)(data[iData] - minValue) / maxRange));
                        }
                        else
                        {
                            cols[iData] = new Color(grad.x, grad.y, grad.z, (float)(data[iData] - minValue) / maxRange);
                        }
                    }
                }
            }
            if (cols != null) texture.SetPixels(cols);
            texture.Apply();
            return texture;
        }
        private async Task CreateGradientTextureInternalAsync(ProgressHandler progressHandler)
        {
            Debug.Log("Async gradient generation. Hold on.");

            Texture3D.allowThreadedTextureCreation = true;
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;

            float minValue = 0;
            float maxValue = 0;
            float maxRange = 0;
            Color[] cols = null;

            await Task.Run(() => {

                minValue = GetMinDataValue(progressHandler);
                maxValue = GetMaxDataValue(progressHandler);
                maxRange = maxValue - minValue;
            });

            try
            {
                await Task.Run(() => cols = new Color[data.Length]);
            }
            catch (OutOfMemoryException)
            {
                Texture3D textureTmp = new Texture3D(dimX, dimY, dimZ, texformat, false);
                textureTmp.wrapMode = TextureWrapMode.Clamp;

                int totalCount = dimX * dimY * dimZ;
                int onePercentVal = totalCount / 100;
                int percentCounter = 0;
                int overall = 0;

                for (int x = 0; x < dimX; x++)
                {
                    for (int y = 0; y < dimY; y++)
                    {
                        for (int z = 0; z < dimZ; z++)
                        {
                            int iData = x + y * dimX + z * (dimX * dimY);
                            Vector3 grad = GetGrad(x, y, z, minValue, maxRange);

                            textureTmp.SetPixel(x, y, z, new Color(grad.x, grad.y, grad.z, (float)(data[iData] - minValue) / maxRange));

                            if (percentCounter >= onePercentVal)
                            {
                                progressHandler.ReportProgress(overall, totalCount, "Creating Gradient...");
                                percentCounter = 0;
                            }
                            percentCounter++;
                            overall++;
                        }
                    }
                }
                textureTmp.Apply();
                gradientTexture = textureTmp;

                Debug.Log("Gradient gereneration done.");
                return;
            }

            await Task.Run(() => {

                int totalCount = dimX * dimY * dimZ;
                int onePercentVal = totalCount / 100;
                int percentCounter = 0;
                int overall = 0;

                for (int x = 0; x < dimX; x++)
                {
                    for (int y = 0; y < dimY; y++)
                    {
                        for (int z = 0; z < dimZ; z++)
                        {
                            int iData = x + y * dimX + z * (dimX * dimY);
                            Vector3 grad = GetGrad(x, y, z, minValue, maxRange);

                            cols[iData] = new Color(grad.x, grad.y, grad.z, (float)(data[iData] - minValue) / maxRange);

                            if (percentCounter >= onePercentVal)
                            {
                                progressHandler.ReportProgress(overall, totalCount, "Creating Gradient...");
                                percentCounter = 0;
                            }
                            percentCounter++;
                            overall++;
                        }
                    }
                }
            });

            Texture3D texture = new Texture3D(dimX, dimY, dimZ, texformat, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(cols);
            texture.Apply();
            gradientTexture = texture;

            Debug.Log("Gradient gereneration done.");
        }
        public Vector3 GetGrad(int x, int y, int z, float minValue, float maxRange)
        {
            float x1 = data[Math.Min(x + 1, dimX - 1) + y * dimX + z * (dimX * dimY)] - minValue;
            float x2 = data[Math.Max(x - 1, 0) + y * dimX + z * (dimX * dimY)] - minValue;
            float y1 = data[x + Math.Min(y + 1, dimY - 1) * dimX + z * (dimX * dimY)] - minValue;
            float y2 = data[x + Math.Max(y - 1, 0) * dimX + z * (dimX * dimY)] - minValue;
            float z1 = data[x + y * dimX + Math.Min(z + 1, dimZ - 1) * (dimX * dimY)] - minValue;
            float z2 = data[x + y * dimX + Math.Max(z - 1, 0) * (dimX * dimY)] - minValue;

            return new Vector3((x2 - x1) / maxRange, (y2 - y1) / maxRange, (z2 - z1) / maxRange);
        }

        public float GetAvgerageVoxelValues(int x, int y, int z)
        {
            // if a dimension length is not an even number
            bool xC = x + 1 == dimX;
            bool yC = y + 1 == dimY;
            bool zC = z + 1 == dimZ;

            //if expression can only be true on the edges of the texture
            if (xC || yC || zC)
            {
                if (!xC && yC && zC) return (GetData(x, y, z) + GetData(x + 1, y, z)) / 2.0f;
                else if (xC && !yC && zC) return (GetData(x, y, z) + GetData(x, y + 1, z)) / 2.0f;
                else if (xC && yC && !zC) return (GetData(x, y, z) + GetData(x, y, z + 1)) / 2.0f;
                else if (!xC && !yC && zC) return (GetData(x, y, z) + GetData(x + 1, y, z) + GetData(x, y + 1, z) + GetData(x + 1, y + 1, z)) / 4.0f;
                else if (!xC && yC && !zC) return (GetData(x, y, z) + GetData(x + 1, y, z) + GetData(x, y, z + 1) + GetData(x + 1, y, z + 1)) / 4.0f;
                else if (xC && !yC && !zC) return (GetData(x, y, z) + GetData(x, y + 1, z) + GetData(x, y, z + 1) + GetData(x, y + 1, z + 1)) / 4.0f;
                else return GetData(x, y, z); // if xC && yC && zC
            }
            return (GetData(x, y, z) + GetData(x + 1, y, z) + GetData(x, y + 1, z) + GetData(x + 1, y + 1, z)
                    + GetData(x, y, z + 1) + GetData(x, y + 1, z + 1) + GetData(x + 1, y, z + 1) + GetData(x + 1, y + 1, z + 1)) / 8.0f;
        }

        public float GetData(int x, int y, int z)
        {
            return data[x + y * dimX + z * (dimX * dimY)];
        }
    }
}