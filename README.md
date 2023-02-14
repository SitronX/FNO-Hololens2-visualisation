# FnO-Hololens2-visualisation
## Important

<b>Simple ITK</b> - Assets/VolumeRendering/Assets/3rdparty/SimpleITK folder must contain SimpleITK binaries, otherwise there will be compilation errors. Paste binaries from here: https://sourceforge.net/projects/simpleitk/files/SimpleITK/1.2.4/CSharp/. It is then possible to update these libraries thru Unity Menu->VolumeRendering->Settings->Disable/Enable SimpleITK, it asks to download it again, which should download the newest version

## IMPORTANT WHEN BUILDING!!!

Due to issue with dlls in build, which is issue with [QR tracker](https://github.com/microsoft/MixedReality-QRCode-Sample) sample, all these dlls below must be placed after the build to the root folder with exe (do not copy all dlls from folder, only these shown below, otherwise there is issue with remoting connection). These dlls are in <b>HospitalVisualisations_Data/Plugins/x86_64</b> in build. 


![dlls](https://user-images.githubusercontent.com/68167377/217945899-341667ac-3ea2-499f-b08c-5f90a15029e9.png)

