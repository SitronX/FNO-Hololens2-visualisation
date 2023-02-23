# FnO-Model-visualisation
This branch is for data visualisation that are represented by 3D mesh/model using AR/VR headsets running on target devices without the PC. [OpenFracture](https://github.com/dgreenheck/OpenFracture) library from dgreenheck is used for reality mesh slicing providing similar functionality as cutout plane in [VolumeBranch](https://github.com/SitronX/FnO-Hololens2-visualisation/tree/QRCodeRecognition-VolumetricData). Solution is tested to work on Hololens 2 (AR) and Oculus Quest 2 (VR).
## Note

Imported model must have generated UVS for [OpenFracture](https://github.com/dgreenheck/OpenFracture) to work correctly. Probably simplest way is to use SmartUV project in blender. It is tested with Island Margin 0.03 and it works.

# Hololens 2
![20230223_165132_HoloLens](https://user-images.githubusercontent.com/68167377/220963407-7225ab6e-96db-44ab-bc17-ab663449b0c7.jpg)

## Build

In the scene, make sure the Hololens2 is selected here as target platform
![hololensStandalone](https://user-images.githubusercontent.com/68167377/220965466-def9d8e6-4548-4c2a-a499-f89210b64484.jpg)

Make sure the UWP is selected in build and all settings are same as on next picture
![hololensBuild](https://user-images.githubusercontent.com/68167377/220967860-7e2aabeb-c191-4f59-b7c7-d614db96489e.jpg)

The visual-studio installation process is then described [here](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/advanced-concepts/using-visual-studio?tabs=hl2). The application should start running automatically on device after Visual-studio finishes building the app.

# Quest 2 / Quest 1
![com DefaultCompany HospitalVisualisations-20230223-145033](https://user-images.githubusercontent.com/68167377/220968858-e87f215a-ce64-4a28-9687-3bec5498fdf0.jpg)

## Build
In the scene, make sure the Quest is selected here as target platform
![questPlatform](https://user-images.githubusercontent.com/68167377/220969351-728b47f9-d943-4f9c-998c-a39885270cbb.jpg)

Make sure the Android is selected in build and all settings are same as on next picture
![questBuild](https://user-images.githubusercontent.com/68167377/220969725-bc977142-38f1-4549-837d-9ced841c6e22.jpg)

Simply select <b>build and run</b> option and app should automatically install on Quest device.
