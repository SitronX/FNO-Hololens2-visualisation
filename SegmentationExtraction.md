# Segmentation extraction from patient - Slicer3D manual

### 1. Open Slicer3D with your segmented patient. You should see segmentation like this.
 
 <img src="https://github.com/SitronX/FnO-Hololens2-visualisation/assets/68167377/f1ebf93b-7826-4f73-b36d-83fc67f18f1f" width=512>

### 2. Open the export window, like shown below.

 <img src="https://github.com/SitronX/FnO-Hololens2-visualisation/assets/68167377/4a5bf8a2-5034-49d1-b495-efad741e096d" width=512>

### 4. Export the segmentation. This segmentation can then be placed into the Labels folder in Dataset.

 <img src="https://user-images.githubusercontent.com/68167377/229525711-ccb53594-aac3-4adc-b4a0-44639121e99a.jpg" width=512>

<b>Note 1:</b> Make sure that segmentation and medical dataset have same dimensions. For example, if you have cropped your dataset, the segmentation should also be cropped.

<b>Note 2:</b> Use NRRD format for segmentation, app can also read segment names from this format.

<b>Note 3:</b> The multilayer label map up to 8 layers is supported from app version 1.1
