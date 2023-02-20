# FnO-Volume-visualisation

This branch is for raw medical volume data visualisation using AR/VR headsets connected to the PC. [UnityVolumeRendering](https://github.com/mlavik1/UnityVolumeRendering) library by mlavik is used for realtime RayMarching on medical data. Solution is tested to work on Hololens 2 (AR) and Oculus Quest 2 (VR).
## Important when loading project

<b>Simple ITK</b> - Assets/VolumeRendering/Assets/3rdparty/SimpleITK folder must contain SimpleITK binaries, otherwise project will not load correctly. Paste binaries from here: https://sourceforge.net/projects/simpleitk/files/SimpleITK/1.2.4/CSharp/. It is then possible to update these libraries thru Unity Menu->VolumeRendering->Settings->Disable/Enable SimpleITK, it asks to download it again, which should download the newest version

## Loading custom datasets

To view custom medical data (NRRD, NIFTI, DICOM, JPG), paste custom dataset to build folder:  <b>HospitalVisualisations_Data/StreamingAssets/DicomData</b>

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

##

