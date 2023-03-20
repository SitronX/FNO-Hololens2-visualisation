</p>
<p align="center">
  <img src="https://user-images.githubusercontent.com/68167377/221692005-8307fb03-7175-4b71-8095-1ed2c3d85d4b.png"/>
</p>





# FnO-Volume-visualisation

This branch is for raw medical volume data visualisation using AR/VR headsets connected to the PC. [UnityVolumeRendering](https://github.com/mlavik1/UnityVolumeRendering) library by mlavik is used for realtime RayMarching on medical data. Solution is tested to work on Hololens 2 (AR) and Oculus Quest 2 (VR).

Downloadable builds are present in [Release](https://github.com/SitronX/FnO-Hololens2-visualisation/releases) section.

<table>
  <tr>  
<th>
  <img src="https://user-images.githubusercontent.com/68167377/226212896-2f515a08-a887-495d-9d79-06f41a4e37eb.jpg" width="1024">
</th>
<th>  
   <img src="https://user-images.githubusercontent.com/68167377/226200142-53190216-13ea-47c3-9184-e05875096922.jpeg" width="1024">
</th>
  </tr>
  <tr>
    <th> Hololens 2 </th>
    <th> PCVR </th>
  </tr>
  </table>
  
# User manual
## Running the app
### Hololens 2
Make sure you have [Remoting app](https://apps.microsoft.com/store/detail/holographic-remoting-player/9NBLGGH4SV40?hl=cs-cz&gl=cz&rtc=1) opened on Hololens 2. Set hololens IP in the build and click connect. Headset can be connected either via USB-C or Wifi.

### PCVR
Connect to the pc with VR headset before launching the app (eg: [AirLink](https://www.meta.com/blog/quest/introducing-oculus-air-link-a-wireless-way-to-play-pc-vr-games-on-oculus-quest-2-plus-infinite-office-updates-support-for-120-hz-on-quest-2-and-more/)). On Quest 2 it is best to set Oculus OpenXR runtime as default, SteamVR has some performance issue with the device.

<img src="https://user-images.githubusercontent.com/68167377/220205065-01c349e3-70ac-4937-b07f-08f988869e65.jpg" width=720>

## Opening the menu
### Hololens 2
To open the hand menu with controls, show your palm to the camera. Menu can be also grabbed and anchored in the space

<img src="https://user-images.githubusercontent.com/68167377/221321745-1d85ceb1-d1c1-4cd9-8363-0b3ebc626974.gif" width=512>

Additionaly voice commands "Show hand menu" and "Close hand menu" also work.
### PCVR
To open the controller menu with additional controls, hold <b>Button 1</b> on the left controller. On Quest 2, it is the X button

![quest2controllers](https://user-images.githubusercontent.com/68167377/221322807-1cbd76a1-9683-422a-95f5-278ebebd5908.png)

Menu can be similarly grabbed and anchored in the space with second controller

<img src="https://user-images.githubusercontent.com/68167377/221322940-1d4d1100-47ef-4470-8cc0-8599dcd3c310.jpeg" width=720>

## Working with new Datasets
To add custom dataset to build with option to spawn it at runtime, follow these steps:

1. Create new Folder in the following directory (name of the folder will be displayed in the app):
    - In build, the path is: HospitalVisualisations_Data/StreamingAssets/Datasets
    - In editor, the path is: Assets/StreamingAssets/Datasets
2. Create two additional folders in previously created folder that are named: <b> Data </b> and <b> Thumbnail </b>
3. Paste medical data into the <b>Data</b> folder. Supported files are: <b>NRRD,NIFTI,DICOM,JPG sequence</b>. Files need to have corresponding suffix matching the file they represent (eg: Dicom data will have .dcm suffix). If you have a lot of files without suffix, you can create suffixes in my simple bulk renaming tool, which is available [here](https://github.com/SitronX/FileRenamer)
4. Paste some thumbnail into the <b>Snapshot</b> folder (.jpg or .png) so dataset is recognizable in spawn menu
5. (Optional) Create third folder named <b>Labels</b> for segmentation support. Paste segmentation [label map](https://slicer.readthedocs.io/en/latest/user_guide/modules/segmentations.html#export-segmentation-to-labelmap-volume) in this folder. Supported files are same as mentioned in 3rd point.

### Spawning dataset in app

Open the hand menu. Datasets can be scrolled horizontally.

Double click the dataset you want to spawn, datasets are differentiated with thumbnails and folder name you previously set.

<img src="https://user-images.githubusercontent.com/68167377/226214129-0902cc3d-da77-4714-8229-eea4af8b02a4.gif" width=512>

When the dataset is spawned, it can be reseted by clicking same previous button.

<img src="https://user-images.githubusercontent.com/68167377/226214137-bf5b4928-0567-40d2-80b5-824463ec1050.gif" width=512>

Specifically on Hololens 2 datasets can be set as <b> QR active</b>. QR active datasets are placed into exact position where QR code was detected. It is possible to switch between loaded datasets as shown below

<img src="https://user-images.githubusercontent.com/68167377/221687075-d32c3aee-1407-49df-8f2f-504c9ddce989.gif" width=512>

## Segmentation module

If you placed correct label map to corresponding <b>Labels</b> folder, the segmentation module is available for that dataset. 

After dataset loading, you can open segmentation module by checking the segmentation checkbox

<img src="https://user-images.githubusercontent.com/68167377/226214793-e0e8074b-9f04-40f2-8391-7e9bf02f62d7.jpg" width=512>

List of segments will appear. Segments are differentiated via color. You can control segments opacity by corresponding sliders as shown below.

![SegmentControl](https://user-images.githubusercontent.com/68167377/226215616-12a93ab8-6ed5-4337-8343-c359e7364432.gif)

You can also change segment color by pressing the color button

![ColorChange](https://user-images.githubusercontent.com/68167377/226215618-b020f276-95f3-4aec-9d1f-a27dcb70b995.gif)

Segmentation module can be exited by uncheching segmentation checkbox

## Slice planes

You can enable slice view from hand menu. When enabled, you can move with the slice planes.

![Slices](https://user-images.githubusercontent.com/68167377/226216517-54128e09-7516-45f2-9a50-c803bbe011a8.gif)


## Cutout methods

Cutout methods are selectable in <b>Additional settings </b>

![Cutouts](https://user-images.githubusercontent.com/68167377/226217175-80e0391c-f703-4be6-9d3f-07ee8a61e382.gif)

## Downsampling datasets

Some very large datasets can bring even powerfull computers to its knees. When this happens, it is probably best to downsample the dataset. 

By downsampling very large datasets, the quality loss is usually negligible with a real boost in performance. Downsampling option is available in <b>Additional settings</b>.

1. Grab previously spawned datasets you want to downsample. The dataset will then become active and you will see its name, thumbnail and dimensions.
2. Press the downscale button (dataset must finish previous loading)

![Downscale](https://user-images.githubusercontent.com/68167377/226219048-e3ccf381-aa02-4b3d-ade1-7fc156817720.gif)

# Developer notes
## Important when loading project

### SimpleITK
Assets/VolumeRendering/Assets/3rdparty/SimpleITK folder must contain SimpleITK binaries, otherwise project will not load correctly. Paste binaries from here: https://sourceforge.net/projects/simpleitk/files/SimpleITK/1.2.4/CSharp/. It is then possible to update these libraries thru Unity Menu->VolumeRendering->Settings->Disable/Enable SimpleITK, it asks to download it again, which should download the newest version

### Quantum Console
Project has working developer console with usefull commands. The asset is called [Quantum Console](https://assetstore.unity.com/packages/tools/utilities/quantum-console-211046) and is not present in this repository due to licensing reasons. 

If you are not going to use the console, you can disable this console by deleting console prefab in scene and deleting [Console.cs](Assets/Scripts/Console.cs) script

<img src="https://user-images.githubusercontent.com/68167377/226320485-2bc453d7-9ab3-488d-bcb6-073211c7310a.png" width=256>

If you want to use a Console and have a licence, simply download the package from [Unity package manager](https://docs.unity3d.com/Manual/upm-ui.html) and all errors should dissapear. Console can then be opened by pressing <b>F1</b> key when the app is running.


## App build
### Hololens 2 
Make sure settings in XR Plug-in Management are same as shown here

<img src="https://user-images.githubusercontent.com/68167377/220190194-086fab19-f7b3-4196-9fbc-80d420de2879.jpg" width=720>

Then in scene make sure target platform is set to <b>Hololens 2</b>

<img src="https://user-images.githubusercontent.com/68167377/220190434-54a27ce9-136e-4b85-aac1-0e057654db9c.jpg" width=720>

#### After the build

Due to issue with dlls in build, which is issue with [QR tracker](https://github.com/microsoft/MixedReality-QRCode-Sample) sample, all these dlls below must be placed after the build to the root folder with exe (do not copy all dlls from folder, only these shown below, otherwise there is issue with remoting connection). These dlls are in <b>HospitalVisualisations_Data/Plugins/x86_64</b> in build. 

<img src="https://user-images.githubusercontent.com/68167377/217945899-341667ac-3ea2-499f-b08c-5f90a15029e9.png" width=256>

### PC-VR
Make sure settings in XR Plug-in Management are same as shown here

<img src="https://user-images.githubusercontent.com/68167377/220191043-fc1e0d22-d22c-446e-aca6-722334eb53b9.jpg" width=720>

Then in scene make sure target platform is set to <b>PCVR</b>

<img src="https://user-images.githubusercontent.com/68167377/220191084-e8e752e5-b67e-4b9e-838b-73793a59647a.jpg" width=720>

