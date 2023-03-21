</p>
<p align="center">
 <img src="https://user-images.githubusercontent.com/68167377/226200142-53190216-13ea-47c3-9184-e05875096922.jpeg" width="512">
</p>

# FnO-Manual

Hospital manual for Quest 2 version of the app.

## Running the app

Connect the VR headset to the PC with AirLink. After sucesfull connection, app can be directly launched from the desktop. The app looks like this.

![appIcon](https://user-images.githubusercontent.com/68167377/226698519-8b72c5fa-d1a3-417f-977c-93b427c15e7d.png)

In case of connecting issues, make sure that Oculus OpenXR runtime is set as the default. If OpenXR is not set as the default, a prompt will appear in the Oculus app to assist you in correctly setting it. The prompt is shown below.

<img src="https://user-images.githubusercontent.com/68167377/220205065-01c349e3-70ac-4937-b07f-08f988869e65.jpg" width=720>

## Opening the menu

To open the control menu inside the app, hold the X button on <b>left</b> controller.

![quest2controllers](https://user-images.githubusercontent.com/68167377/221322807-1cbd76a1-9683-422a-95f5-278ebebd5908.png)

The menu can be grabbed and anchored in the space using the second controller.

<img src="https://user-images.githubusercontent.com/68167377/226731942-d9b4f7f3-bd60-4a2a-86eb-a4182198416e.gif" width=512>

## Working with new Datasets
To add a custom dataset, follow these steps:

1. On desktop there is folder named <b>Datasets</b>. In this folder, create new folder with appropriate name that will hold your new dataset.

<img src="https://user-images.githubusercontent.com/68167377/226700616-7e8bf396-c91c-4197-90b7-67177fcb5248.png" width=512>

2. Create two additional folders in your previously created folder, named <b> Data </b> and <b> Thumbnail</b>.
3. Paste the medical data into the <b>Data</b> folder. Supported file types are: <b>NRRD,NIFTI,DICOM and JPG sequence</b>. The files need to have corresponding suffix matching the file they represent (eg: DICOM data will have a .dcm suffix). If you have a lot of files without suffix, you can create suffixes with with my simple renaming tool, which is available [here](https://github.com/SitronX/FileRenamer).
4. Paste a thumbnail into the <b>Thumbnail</b> folder (.jpg or .png) so that the dataset is recognizable in the spawn menu.
5. (Optional) Create a third folder named <b>Labels</b> for segmentation support. Paste the segmentation [label map](https://slicer.readthedocs.io/en/latest/user_guide/modules/segmentations.html#export-segmentation-to-labelmap-volume) in this folder. The supported file types are the same as mentioned in step 3.

### Spawning dataset in app

1. Open the hand menu. Datasets can be scrolled horizontally.
2. Double-click the dataset you want to spawn. Datasets are differentiated by thumbnails and the folder name you previously set.

<img src="https://user-images.githubusercontent.com/68167377/226214129-0902cc3d-da77-4714-8229-eea4af8b02a4.gif" width=512>

When the dataset is spawned, it can be reset by clicking the same previous button.

<img src="https://user-images.githubusercontent.com/68167377/226214137-bf5b4928-0567-40d2-80b5-824463ec1050.gif" width=512>

## Segmentation module

If you placed the correct label map in the corresponding <b>Labels</b> folder, the segmentation module is available for that dataset. 

After loading the dataset, you can open the segmentation module by checking the segmentation checkbox.

<img src="https://user-images.githubusercontent.com/68167377/226214793-e0e8074b-9f04-40f2-8391-7e9bf02f62d7.jpg" width=512>

List of segments will appear. Segments are differentiated via color. You can control segments opacity by corresponding sliders as shown below.

![SegmentControl](https://user-images.githubusercontent.com/68167377/226215616-12a93ab8-6ed5-4337-8343-c359e7364432.gif)

You can also change segment color by pressing the color button.

![ColorChange](https://user-images.githubusercontent.com/68167377/226215618-b020f276-95f3-4aec-9d1f-a27dcb70b995.gif)

You can exit the segmentation module by uncheching the segmentation checkbox.

Note: The segmentation module only works in DVR render mode.

## Slice planes

You can enable the slice view from the hand menu. When enabled, you can move the slice planes.

![Slices](https://user-images.githubusercontent.com/68167377/226216517-54128e09-7516-45f2-9a50-c803bbe011a8.gif)

## Cutout methods

Cutout methods are selectable in <b>Additional settings </b>.

![Cutouts](https://user-images.githubusercontent.com/68167377/226217175-80e0391c-f703-4be6-9d3f-07ee8a61e382.gif)

## Downsampling datasets

Some very large datasets can bring even powerful computers to their knees. When this happens, it's best to downsample the dataset.

By downsampling very large datasets, the quality loss is usually negligible with a real boost in performance. The downsampling option is available in the <b>Additional settings</b>.

1. Grab the previously spawned dataset you want to downsample. The dataset will then become active, and you will see its name, thumbnail and dimensions.
2. Press the downscale button (the dataset must finish previous loading).

![Downscale](https://user-images.githubusercontent.com/68167377/226219048-e3ccf381-aa02-4b3d-ade1-7fc156817720.gif)

## Changing background

It is possible to change the background according to the user's preference.

<table>
  <tr>  
<th>Default backgroung</th>
<th>Dark background</th>
<th>Light background</th>
<tr>  
<th>
  <img src="https://user-images.githubusercontent.com/68167377/226741250-eae58191-67f5-4b5f-aa80-b72b26d995d7.jpg" width=512>
</th>
<th>

  <img src="https://user-images.githubusercontent.com/68167377/226741372-54374c71-0e28-4f0a-93c4-0005c1949d66.jpg" width=512>
</th>
<th>

  <img src="https://user-images.githubusercontent.com/68167377/226741461-834e394c-e9e5-480b-9c08-57614d7a6d94.jpg" width=512>
</th>
  </tr>
  <tr>
    <th>Press <b> F2 </b> to activate </th>
    <th>Press <b> F3 </b> to activate </th>
    <th>Press <b> F4 </b> to activate </th>
  </tr>
  </table>
  
  
Note: <b>F1</b> button opens developer console for additional commands that you can use.

