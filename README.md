</p>
<p align="center">
  <img src="https://user-images.githubusercontent.com/68167377/221692005-8307fb03-7175-4b71-8095-1ed2c3d85d4b.png"/>
</p>



# FnO-Volume-visualisation

This branch is for raw medical volume data visualisation using AR/VR headsets connected to the PC. [UnityVolumeRendering](https://github.com/mlavik1/UnityVolumeRendering) library by mlavik is used for realtime RayMarching on medical data. Solution is tested to work on Hololens 2 (AR) and Oculus Quest 2 (VR).
## Important when loading project

<b>Simple ITK</b> - Assets/VolumeRendering/Assets/3rdparty/SimpleITK folder must contain SimpleITK binaries, otherwise project will not load correctly. Paste binaries from here: https://sourceforge.net/projects/simpleitk/files/SimpleITK/1.2.4/CSharp/. It is then possible to update these libraries thru Unity Menu->VolumeRendering->Settings->Disable/Enable SimpleITK, it asks to download it again, which should download the newest version

# Hololens 2 

![20230220_224320_HoloLens](https://user-images.githubusercontent.com/68167377/220206419-51091b6d-4ae4-4e67-be44-0ef05039be00.jpg)

## Build
Make sure settings in XR Plug-in Management are same as shown here
![hl](https://user-images.githubusercontent.com/68167377/220190194-086fab19-f7b3-4196-9fbc-80d420de2879.jpg)

Then in scene make sure target platform is set to <b>Hololens 2</b>
![hl2](https://user-images.githubusercontent.com/68167377/220190434-54a27ce9-136e-4b85-aac1-0e057654db9c.jpg)

## After the build

Due to issue with dlls in build, which is issue with [QR tracker](https://github.com/microsoft/MixedReality-QRCode-Sample) sample, all these dlls below must be placed after the build to the root folder with exe (do not copy all dlls from folder, only these shown below, otherwise there is issue with remoting connection). These dlls are in <b>HospitalVisualisations_Data/Plugins/x86_64</b> in build. 

![dlls](https://user-images.githubusercontent.com/68167377/217945899-341667ac-3ea2-499f-b08c-5f90a15029e9.png)

## Running the app
Make sure you have [Remoting app](https://apps.microsoft.com/store/detail/holographic-remoting-player/9NBLGGH4SV40?hl=cs-cz&gl=cz&rtc=1) opened on Hololens 2. Set hololens IP in the build and click connect. Data is placed when headset scans any QR code.

### Opening the menu
To open the hand menu with additional controls, show your palm to the camera. Menu can be also grabbed and anchored in the space

![ezgif com-optimize (1)](https://user-images.githubusercontent.com/68167377/221321745-1d85ceb1-d1c1-4cd9-8363-0b3ebc626974.gif)

Additionaly voice commands "Show hand menu" and "Close hand menu" also work.

# PC-VR

![OculusScreenshot1676931261](https://user-images.githubusercontent.com/68167377/220206498-51574994-79db-4e7d-9172-24713f4bf68f.jpeg)

## Build
Make sure settings in XR Plug-in Management are same as shown here
![pcvr1](https://user-images.githubusercontent.com/68167377/220191043-fc1e0d22-d22c-446e-aca6-722334eb53b9.jpg)

Then in scene make sure target platform is set to <b>PCVR</b>
![pcvr2](https://user-images.githubusercontent.com/68167377/220191084-e8e752e5-b67e-4b9e-838b-73793a59647a.jpg)

## Running the app
Connect to the pc with VR headset before launching the app (eg: [AirLink](https://www.meta.com/blog/quest/introducing-oculus-air-link-a-wireless-way-to-play-pc-vr-games-on-oculus-quest-2-plus-infinite-office-updates-support-for-120-hz-on-quest-2-and-more/)). On Quest 2 it is best to set Oculus OpenXR runtime as default, SteamVR has some performance issue with the device.
![openXR](https://user-images.githubusercontent.com/68167377/220205065-01c349e3-70ac-4937-b07f-08f988869e65.jpg)

Data is placed instantly infront of the user.

### Opening the menu
To open the controller menu with additional controls, hold <b>Button 1</b> on the left controller. On Quest 2, it is the X button

![quest2controllers](https://user-images.githubusercontent.com/68167377/221322807-1cbd76a1-9683-422a-95f5-278ebebd5908.png)

Menu can be similarly grabbed and anchored in the space with second controller

![OculusScreenshot1677282138](https://user-images.githubusercontent.com/68167377/221322940-1d4d1100-47ef-4470-8cc0-8599dcd3c310.jpeg)

# Working with new Datasets

### Adding new data
To add custom dataset to build with option to spawn it at runtime, follow these steps:

1. Create new Folder in the following directory:
    - In build, the path is: HospitalVisualisations_Data/StreamingAssets/DicomData
    - In editor, the path is: Assets/StreamingAssets/DicomData
2. Create two additional folders in previously created folder that are named: <b> Data </b> and <b> Snapshot </b>
3. Paste medical data into the <b>Data</b> folder. Supported files are: <b>NRRD,NIFTI,DICOM,JPG sequence</b>. Files need to have corresponding suffix matching the file they represent (eg: Dicom data will have .dcm suffix)
4. Paste some thumbnail into the <b>Snapshot</b> folder (.jpg or .png) so dataset is recognizable in spawn menu

### Spawning dataset in app

Open the hand menu. Datasets can be scrolled horizontally.

Hold button with data you want to spawn, datasets are differentiated with thumbnails you set previously

![ezgif com-optimize (2)](https://user-images.githubusercontent.com/68167377/221684028-57a0bfc6-2383-491c-bb37-731611036e99.gif)

When the dataset is spawned, it can be enabled/disabled by clicking the previous dataset button

![EnableDisable](https://user-images.githubusercontent.com/68167377/221686384-7e8c374b-4003-4c6d-b214-51807eb2633c.gif)

Dataset can be set as <b> QR active</b>. QR active datasets are placed into exact position where QR code was detected. It is possible to switch between loaded datasets as shown below

![ezgif com-optimize (3)](https://user-images.githubusercontent.com/68167377/221687075-d32c3aee-1407-49df-8f2f-504c9ddce989.gif)



