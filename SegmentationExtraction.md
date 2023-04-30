# Segmentation extraction from patient - Slicer3D manual

### 1. Open Slicer3D with your segmented patient. You should see segmentation like this.
 
 <img src="https://user-images.githubusercontent.com/68167377/229519204-963d005b-d4ec-43a2-beae-9aaf9462db8e.jpg" width=512>

### 2. Make sure the segmentation only has one layer, it is shown on image below.

 <img src="https://user-images.githubusercontent.com/68167377/229522065-36fb3401-f296-4513-8ed2-26016c323549.jpg" width=512>

### 3. Open the export window, like shown below.

 <img src="https://user-images.githubusercontent.com/68167377/229524916-5a0d7060-d186-4f5d-bfd9-62e1cff439bc.jpg" width=512>

### 4. Export the segmentation. This segmentation can then be placed into the Labels folder in Dataset.

 <img src="https://user-images.githubusercontent.com/68167377/229525711-ccb53594-aac3-4adc-b4a0-44639121e99a.jpg" width=512>

<b>Note 1:</b> Make sure that segmentation and medical dataset have same dimensions. For example, if you have cropped your dataset, the segmentation should also be cropped.

<b>Note 2:</b> Use NRRD format for segmentation, app can also read segment names from this format.

<b>Note 3:</b> By collapsing label map layers, there is inevitable loss of data on certain label map segments. You can try to avoid it by manually disabling overlapping segments you dont specifically need.
