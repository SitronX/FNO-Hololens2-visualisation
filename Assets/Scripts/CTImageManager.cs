using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class CTImageManager : MonoBehaviour
{
    List<GameObject> ctImages=new List<GameObject>();
    List<GameObject> slicedObjects=new List<GameObject>();
    [SerializeField] GameObject liver;
    [SerializeField] GameObject sliceObjectParent;
    [SerializeField] PinchSlider pinchSlider;

    Vector3 savedPos;
    Quaternion savedRot;
     string slicedObjectName;
    void Start()
    {
        Slice sliceScript = liver.GetComponent<Slice>();

        savedPos = transform.parent.localPosition;
        savedRot = transform.parent.localRotation;

        for (int i= 0; i < transform.childCount; i++)
        {
            GameObject ctImage = transform.GetChild(i).gameObject;
            ctImages.Add(ctImage);

            Vector3 slicePos = ctImage.transform.position+(ctImage.transform.forward * 0.001f);

            sliceScript.ComputeSlice(ctImage.transform.forward, slicePos, true, true,sliceObjectParent);
        }


        slicedObjectName = liver.name+"Slices";
        GameObject slicedParent = GameObject.Find(slicedObjectName);
        for(int i=0;i<slicedParent.transform.childCount;i++)
        {
            GameObject slicedObject = slicedParent.transform.GetChild(i).gameObject;
            slicedObjects.Add(slicedObject);
        }

        pinchSlider.SliderValue = 0;
        

    }

    public void OnSliderUpdated(SliderEventData data)
    {
        ctImages.ForEach(x => x.SetActive(false));
        slicedObjects.ForEach(x => x.SetActive(false));

        int index =  (int)(data.NewValue*ctImages.Count)-1;

        if(index==-1)
        {
            liver.SetActive(true);
        }
        else
        {
            liver.SetActive(false);
            ctImages[index].SetActive(true);
            slicedObjects[index].SetActive(true);
        }
        
    }
    public void ResetPosition()
    {
        transform.parent.transform.localPosition = savedPos;
        transform.parent.transform.localRotation = savedRot;
    }
}
