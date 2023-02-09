# FnO-Hololens2-visualisation
## Important

<b>Simple ITK</b> - Assets/VolumeRendering/Assets/3rdparty/SimpleITK folder must contain SimpleITK binaries, otherwise there will be compilation errors. Paste binaries from here: https://sourceforge.net/projects/simpleitk/files/SimpleITK/1.2.4/CSharp/. It is then possible to update these libraries thru Unity Menu->VolumeRendering->Settings->Disable/Enable SimpleITK, it asks to download it again, which should download the newest version

## IMPORTANT AFTER BUILD!!!

Due to issue with dlls in build, which is issue with [QR tracker](https://github.com/microsoft/MixedReality-QRCode-Sample) sample, all these dlls must be placed after the build to the root folder with exe. Majority of these dlls are in <b>HospitalVisualisations_Data/Plugins/x86_64</b>. 


![dlls](https://user-images.githubusercontent.com/68167377/217945899-341667ac-3ea2-499f-b08c-5f90a15029e9.png)


MonoSupport.dll is not present in build. 

It can be found in project <b>Assets/Packages/Microsoft.Windows.MixedReality.DotNetWinRT.0.5.1049/Unity/x64/MonoSupport.dll<b/>
