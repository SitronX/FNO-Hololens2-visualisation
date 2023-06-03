using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.ParticleSystemJobs;


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

        private NativeArray<float> nativeData;

        
        public NativeArray<float>[] nativeLabelData;
        public int HowManyLabelMapLayers { get; set; }


        public List<Dictionary<float, float>> LabelValues { get; set; } = new List<Dictionary<float, float>>();      //Label value and index
        public List<Dictionary<float, string>> LabelNames { get; set; } = new List<Dictionary<float, string>>();      //Names correction

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

        public float MinDataValue { get; set; } = float.MaxValue;
        public float MaxDataValue { get; set; } = float.MinValue;

        private Texture3D dataTexture = null;
        private Texture3D gradientTexture = null;

        //TODO
        private Texture3D labelTexture = null;
        public int labelDimX, labelDimY, labelDimZ;


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

        public float GetMinDataValue()
        {
            if (MinDataValue == float.MaxValue)
                CalculateValueBounds();
            return MinDataValue;
        }

        public float GetMaxDataValue()
        {
            if (MaxDataValue == float.MinValue)
                CalculateValueBounds();
            return MaxDataValue;
        }

        [BurstCompile]
        public struct FindSegmentJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float> labelData;
            [WriteOnly] public NativeParallelHashSet<float>.ParallelWriter allSegments;

            public void Execute(int index)
            {
                allSegments.Add(labelData[index]);
            }
        }
        public async Task FindAllSegments(ProgressHandler progressHandler)
        {
            if (nativeLabelData != null)
            {
                progressHandler.ReportProgress(0, "Finding all segments...");
                int maxNumberOfSegments = 256;

                NativeParallelHashSet<float>[] hashSets= new NativeParallelHashSet<float>[nativeLabelData.Length];
                NativeArray<JobHandle> handles = new NativeArray<JobHandle>(nativeLabelData.Length, Allocator.TempJob);

                for (int i=0;i<nativeLabelData.Length;i++)
                {
                    hashSets[i] = new NativeParallelHashSet<float>(maxNumberOfSegments, Allocator.TempJob);
                    FindSegmentJob segmentJob = new FindSegmentJob()
                    {
                        allSegments = hashSets[i].AsParallelWriter(),
                        labelData = nativeLabelData[i],
                    };
                    handles[i] = segmentJob.Schedule(nativeLabelData[i].Length, 64);
                }

                JobHandle combinedHandles = JobHandle.CombineDependencies(handles);

                while (!combinedHandles.IsCompleted)
                    await Task.Delay(1000);

                combinedHandles.Complete();
        

                for(int i=0;i<hashSets.Length;i++)
                {
                    foreach (var j in hashSets[i])
                        LabelValues[i].Add(j, 0);
                }

                for (int i = 0; i < hashSets.Length; i++)
                    hashSets[i].Dispose();

                handles.Dispose();
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
                        downScaledData[x + y * halfDimX + z * (halfDimX * halfDimY)] = math.round(GetAvgerageVoxelValues(x * 2, y * 2, z * 2));
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

            if(nativeLabelData!=null)
            {
                for(int i=0;i<nativeLabelData.Length;i++)
                {
                    float[] arr = nativeLabelData[i].Reverse().ToArray();
                    nativeLabelData[i].Dispose();
                    nativeLabelData[i] = new NativeArray<float>(arr, Allocator.TempJob);
                }
            }
        }

        [BurstCompile]
        public struct DownscaleDataJob : IJobParallelFor
        {
            [WriteOnly] public NativeArray<float> downscaledData;
            [ReadOnly] public NativeArray<float> originalData;
            [ReadOnly] public int halfDimX;
            [ReadOnly] public int halfDimY;
            [ReadOnly] public int dimX;
            [ReadOnly] public int dimY;
            [ReadOnly] public int dimZ;

            public void Execute(int index)
            {
                int z = index / (halfDimX * halfDimY);
                int y = (index % (halfDimX * halfDimY)) / halfDimX;
                int x = index % halfDimX;

                downscaledData[x + y * halfDimX + z * (halfDimX * halfDimY)] = math.round(GetAvgerageVoxelValues(originalData, x * 2, y * 2, z * 2, dimX, dimY, dimZ));
            }
        }
        public async Task DownScaleDataAsync(ProgressHandler progressHandler)
        {
            int halfDimX = dimX / 2 + dimX % 2;
            int halfDimY = dimY / 2 + dimY % 2;
            int halfDimZ = dimZ / 2 + dimZ % 2;
            
            progressHandler.ReportProgress(0, "Downscaling dataset...");
            NativeArray<float> downScaledData = default;

            await Task.Run(() => downScaledData = new NativeArray<float>(halfDimX * halfDimY * halfDimZ, Allocator.Persistent));

            DownscaleDataJob downscaleJob = new DownscaleDataJob()
            {
                dimX = dimX,
                dimY = dimY,
                dimZ = dimZ,
                downscaledData = downScaledData,
                originalData=nativeData,
                halfDimX = halfDimX,
                halfDimY = halfDimY,
            };

            JobHandle handle=downscaleJob.Schedule(halfDimX * halfDimY * halfDimZ, 64);

            while (!handle.IsCompleted)
                await Task.Delay(1000);

            //Update data & data dimensions
            data = downScaledData.ToArray();

            nativeData.Dispose();
            nativeData = downScaledData;

            dimX = halfDimX;
            dimY = halfDimY;
            dimZ = halfDimZ;   
        }

        [BurstCompile]
        public struct MinMaxArrayJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float> data;
            public NativeArray<float> minValues;
            public NativeArray<float> maxValues;

            public void Execute(int index)
            {
                float minValue = float.MaxValue;
                float maxValue = float.MinValue;
                int chunkSize = data.Length / minValues.Length;

                for (int i = chunkSize * index; i < chunkSize * (index + 1); i++)
                {
                    float val = data[i];
                    minValue = math.min(minValue, val);
                    maxValue = math.max(maxValue, val);

                    minValues[index] = minValue;
                    maxValues[index] = maxValue;
                }
            }
        }
        private async Task CalculateValueBounds(ProgressHandler progressHandler)
        {
            MinDataValue = float.MaxValue;
            MaxDataValue = float.MinValue;

            if (data != null)
            {
                progressHandler.ReportProgress(0, "Calculating Boundaries...");

                int numChunks = 64;
                NativeArray<float> minValues = default;
                NativeArray<float> maxValues = default; 

                await Task.Run(() => {
                    minValues= new NativeArray<float>(numChunks, Allocator.TempJob);
                    maxValues= new NativeArray<float>(numChunks, Allocator.TempJob);
                });
                
                MinMaxArrayJob boundsJob = new MinMaxArrayJob()
                {
                    data = nativeData,
                    minValues = minValues,
                    maxValues = maxValues
                };
                JobHandle handle = boundsJob.Schedule(numChunks, 1);
                
                while (!handle.IsCompleted)
                    await Task.Delay(1000);
                
                handle.Complete();
  
                MinDataValue = minValues[0];
                MaxDataValue = maxValues[0];

                for (int i = 1; i < numChunks; i++)
                {
                    MinDataValue = math.min(MinDataValue, minValues[i]);
                    MaxDataValue = math.max(MaxDataValue, maxValues[i]);
                }

                minValues.Dispose();
                maxValues.Dispose();
                   
            }
        }
        private void CalculateValueBounds()
        {
            MinDataValue = float.MaxValue;
            MaxDataValue = float.MinValue;

            if (data != null)
            {
                int totalCount = dimX * dimY * dimZ;
              
                for (int i = 0; i < totalCount; i++)
                {
                    float val = data[i];
                    MinDataValue = Mathf.Min(MinDataValue, val);
                    MaxDataValue = Mathf.Max(MaxDataValue, val);
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
        [BurstCompile]
        public struct DataTextureProcess : IJobParallelFor
        {
            [WriteOnly] public NativeArray<half> pixelBytes;
            [ReadOnly]
            public NativeArray<float> data;
            [ReadOnly]
            public float minValue;
            [ReadOnly]
            public float maxRange;
            public void Execute(int index)
            {
                pixelBytes[index] = math.half((float)(data[index] - minValue) / maxRange);
            }
        }
        private async Task CreateTextureInternalAsync(ProgressHandler progressHandler)                                             //This method can be also called in custom logic to load it before continuing
        {
            Debug.Log("Async texture generation. Hold on.");

            Texture3D.allowThreadedTextureCreation = true;
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RHalf) ? TextureFormat.RHalf : TextureFormat.RFloat;

            await Task.Run(() => { nativeData = new NativeArray<float>(data, Allocator.Persistent); });    //At start create this native array

            
            if (MinDataValue == float.MaxValue&& MaxDataValue == float.MinValue)
                await CalculateValueBounds(progressHandler);
               
            float maxRange = MaxDataValue - MinDataValue;
            

            bool isHalfFloat = texformat == TextureFormat.RHalf;

            try
            {
                if (isHalfFloat)
                {
                    progressHandler.ReportProgress(0, "Creating Data...");
                    NativeArray<half> pixelBytes = default;

                    await Task.Run(() =>pixelBytes = new NativeArray<half>(data.Length, Allocator.TempJob));

                    DataTextureProcess dataJob = new DataTextureProcess()
                    {
                        data = nativeData,
                        pixelBytes = pixelBytes,
                        minValue = MinDataValue,
                        maxRange = maxRange
                    };

                    JobHandle handle= dataJob.Schedule(data.Length, 64);

                    while(!handle.IsCompleted)
                        await Task.Delay(1000);

                    handle.Complete();

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
                            pixelBytes[iData] = (float)(data[iData] - MinDataValue) / maxRange;

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
                            texture.SetPixel(x, y, z, new Color((float)(data[x + y * dimX + z * (dimX * dimY)] - MinDataValue) / maxRange, 0.0f, 0.0f, 0.0f));

                texture.Apply();
                dataTexture = texture;
            }

            Debug.Log("Texture generation done.");
        }

        [BurstCompile]
        public struct LabelTextureProcess : IJobParallelFor         //Each label map layer is processed
        {
            [WriteOnly] public NativeArray<byte> pixelBytes;
            [ReadOnly]
            public NativeArray<float> labelData;
            [ReadOnly]
            public NativeParallelHashMap<float, float> labelValues;

            public void Execute(int index)
            {
                pixelBytes[index] =(byte)labelValues[labelData[index]];
            }
        }
        [BurstCompile]
        public struct ColorTextureJoining:IJobParallelFor       //Label map layers are quickly joined to one Color32 with 4channels
        {
            [ReadOnly] public NativeArray<byte> redChannel;
            [ReadOnly] public NativeArray<byte> greenChannel;
            [ReadOnly] public NativeArray<byte> blueChannel;
            [ReadOnly] public NativeArray<byte> alphaChannel;

            public NativeArray<Color32> result;

            public void Execute(int index)
            {
                result[index] = new Color32(redChannel[index], greenChannel[index], blueChannel[index], alphaChannel[index]);
            }
        }
        private async Task CreateLabelTextureInternalAsync(ProgressHandler progressHandler)                                         //It would be ideal to represent label values as Int, but i didnt manage to get it working
        {
            Debug.Log("Async label texture generation. Hold on.");

            Texture3D.allowThreadedTextureCreation = true;

            await FindAllSegments(progressHandler);       //Althought LabelNames should contain this info, it is not always the case (empty metadatas), so we still need to check it manually

            await Task.Run(() =>OrderLabelDictionaríes());

            try
            {
                progressHandler.ReportProgress(0, "Creating Segmentation Data...");

                labelTexture = await CreateLayerTexture(0,Mathf.Min(HowManyLabelMapLayers,4));
                
            }
            catch (OutOfMemoryException)
            {
                ErrorNotifier.Instance.AddErrorMessageToUser("Not enough of memory when creating label map!!!");
            }

            Debug.Log("Label Texture generation done.");
        }
        public async Task<Texture3D> CreateLayerTexture(int startLayer,int endLayer)
        {
            int layersCount = endLayer - startLayer;

            NativeArray<byte>[] pixelBytes = new NativeArray<byte>[layersCount];
            NativeArray<byte> emptyArray = new NativeArray<byte>(nativeLabelData[0].Length, Allocator.TempJob);

            NativeParallelHashMap<float, float>[] nativeHashmap = new NativeParallelHashMap<float, float>[layersCount];
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(layersCount, Allocator.TempJob);
            NativeArray<Color32> pixelColors = new NativeArray<Color32>(nativeLabelData[0].Length, Allocator.TempJob);


            for (int i = 0; i < layersCount; i++)        
            {
                nativeHashmap[i] = new NativeParallelHashMap<float, float>(LabelValues[startLayer + i].Keys.Count, Allocator.TempJob);
                pixelBytes[i] = new NativeArray<byte>(nativeLabelData[0].Length, Allocator.TempJob);

                foreach (var item in LabelValues[startLayer + i])
                    nativeHashmap[i].Add(item.Key, item.Value);

                LabelTextureProcess labelJob = new LabelTextureProcess()
                {
                    pixelBytes = pixelBytes[i],
                    labelData = nativeLabelData[startLayer+ i],
                    labelValues = nativeHashmap[i]
                };
                jobHandles[i] = labelJob.Schedule(nativeLabelData[startLayer+ i].Length, 64);
            }

            JobHandle combinedHandles = JobHandle.CombineDependencies(jobHandles);

            while (!combinedHandles.IsCompleted)
                await Task.Delay(1000);

            combinedHandles.Complete();


            ColorTextureJoining joining = new ColorTextureJoining()
            {
                redChannel = pixelBytes[0],
                greenChannel = layersCount>1?pixelBytes[1]: emptyArray,
                blueChannel = layersCount>2? pixelBytes[2]: emptyArray,
                alphaChannel = layersCount>3? pixelBytes[3]: emptyArray,
                result = pixelColors

            };

            JobHandle joiningHandle = joining.Schedule(nativeLabelData[0].Length, 64);

            while (!joiningHandle.IsCompleted)
                await Task.Delay(1000);

            joiningHandle.Complete();


            Texture3D texture = new Texture3D(labelDimX, labelDimY, labelDimZ, TextureFormat.RGBA32, false);                  //Grouped texture stuff so it doesnt freezes twice, but only once
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels32(pixelColors.ToArray(), 0);
            texture.filterMode = FilterMode.Point;
            texture.Apply();


            jobHandles.Dispose();
            pixelColors.Dispose();
            emptyArray.Dispose();

            for (int i = 0; i < layersCount; i++)
            {
                pixelBytes[i].Dispose();
                nativeHashmap[i].Dispose();
            }

            return texture;
        }
        private void OrderLabelDictionaríes()
        {
            for(int i=0;i<LabelValues.Count;i++)
            {
                List<float> orderedKeys = LabelValues[i].Keys.OrderBy(x => x).ToList();

                for (int j = 0; j < orderedKeys.Count; j++)
                    LabelValues[i][orderedKeys[j]] = j;
            }         
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

        [BurstCompile]
        public struct GradientTextureProcess : IJobParallelFor
        {
            [WriteOnly] public NativeArray<Color32> resultColors;
            [ReadOnly]
            public NativeArray<float> data;
            [ReadOnly]
            public int dimX;
            [ReadOnly]
            public int dimY;
            [ReadOnly]
            public int dimZ;
            [ReadOnly]
            public float minValue;
            [ReadOnly]
            public float maxRange;

            public void Execute(int index)
            {
                int z = index / (dimX * dimY);
                int y = (index % (dimX * dimY)) / dimX;
                int x = index % dimX;

                Vector3 grad = GetGrad(data, x, y, z, minValue, maxRange,dimX,dimY,dimZ);

                resultColors[index] = new Color(grad.x, grad.y, grad.z, (float)(data[index] - minValue) / maxRange);
            }
        }
        private async Task CreateGradientTextureInternalAsync(ProgressHandler progressHandler)
        {
            Debug.Log("Async gradient generation. Hold on.");

            Texture3D.allowThreadedTextureCreation = true;
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;

            Color[] cols = null;

            if (MinDataValue == float.MaxValue && MaxDataValue == float.MinValue)
                await CalculateValueBounds(progressHandler);

            float maxRange = MaxDataValue - MinDataValue;


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
                            Vector3 grad = GetGrad(x, y, z, MinDataValue, maxRange);

                            textureTmp.SetPixel(x, y, z, new Color(grad.x, grad.y, grad.z, (float)(data[iData] - MinDataValue) / maxRange));

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

            progressHandler.ReportProgress(0, "Creating Gradient...");
            NativeArray<Color32> colors = default;

            await Task.Run(() =>  colors= new NativeArray<Color32>(data.Length, Allocator.TempJob));

            GradientTextureProcess gradientJob = new GradientTextureProcess()
            {
                resultColors = colors,
                dimX = dimX,
                dimY = dimY,
                dimZ = dimZ,
                data = nativeData,
                maxRange = maxRange,
                minValue = MinDataValue
            };
            JobHandle handle = gradientJob.Schedule(data.Length, 64);

            while (!handle.IsCompleted)
                await Task.Delay(1000);

            handle.Complete();

            Texture3D texture = new Texture3D(dimX, dimY, dimZ, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels32(colors.ToArray());
            texture.Apply();
            gradientTexture = texture;

            colors.Dispose();

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
        public static Vector3 GetGrad(NativeArray<float> data, int x, int y, int z, float minValue, float maxRange,int dimX,int dimY,int dimZ)
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
        public static float GetAvgerageVoxelValues(NativeArray<float> data, int x, int y, int z,int dimX,int dimY,int dimZ)
        {
            float GetData(NativeArray<float> localData, int x, int y, int z)
            {
                return localData[x + y * dimX + z * (dimX * dimY)];
            }

            // if a dimension length is not an even number
            bool xC = x + 1 == dimX;
            bool yC = y + 1 == dimY;
            bool zC = z + 1 == dimZ;

            //if expression can only be true on the edges of the texture
            if (xC || yC || zC)
            {
                if (!xC && yC && zC) return (GetData(data,x, y, z) + GetData(data,x + 1, y, z)) / 2.0f;
                else if (xC && !yC && zC) return (GetData(data,x, y, z) + GetData(data,x, y + 1, z)) / 2.0f;
                else if (xC && yC && !zC) return (GetData(data,x, y, z) + GetData(data,x, y, z + 1)) / 2.0f;
                else if (!xC && !yC && zC) return (GetData(data,x, y, z) + GetData(data,x + 1, y, z) + GetData(data,x, y + 1, z) + GetData(data,x + 1, y + 1, z)) / 4.0f;
                else if (!xC && yC && !zC) return (GetData(data,x, y, z) + GetData(data,x + 1, y, z) + GetData(data,x, y, z + 1) + GetData(data,x + 1, y, z + 1)) / 4.0f;
                else if (xC && !yC && !zC) return (GetData(data,x, y, z) + GetData(data,x, y + 1, z) + GetData(data,x, y, z + 1) + GetData(data,x, y + 1, z + 1)) / 4.0f;
                else return GetData(data,x, y, z); // if xC && yC && zC
            }
            return (GetData(data,x, y, z) + GetData(data,x + 1, y, z) + GetData(data,x, y + 1, z) + GetData(data,x + 1, y + 1, z)
                    + GetData(data,x, y, z + 1) + GetData(data,x, y + 1, z + 1) + GetData(data,x + 1, y, z + 1) + GetData(data,x + 1, y + 1, z + 1)) / 8.0f;
        }

        public float GetData(int x, int y, int z)
        {
            return data[x + y * dimX + z * (dimX * dimY)];
        }
        private void OnDestroy()
        {
            if (nativeData.IsCreated)
                nativeData.Dispose();

            foreach(var data in nativeLabelData)
            {
                if (data.IsCreated)
                    data.Dispose();
            }
                       
        }

    }
}