using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingModelManager : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] GameObject _emptyGameObjectPrefab;
    List<SliceData> _segments = new List<SliceData>();
    [SerializeField] GameObject _occlusionPlane;
    [SerializeField] GameObject _alphaSliderPrefab;


    public struct SliceData
    {
        public GameObject originalObject;
        public Slice slice;
        public GameObject fragmentRootContainer;
        public GameObject slicesParentObject;
    }


    void Awake()
    {
        int currentChildCount = transform.childCount;

        for (int i = 0; i < currentChildCount; i++)
        {
            GameObject current = transform.GetChild(i).gameObject;

            Slice currentSlice= current.AddComponent<Slice>();
            currentSlice.sliceOptions = new SliceOptions
            {
                insideMaterial = current.GetComponent<MeshRenderer>().sharedMaterial,
                enableReslicing = true
            };
            currentSlice.callbackOptions = new CallbackOptions();

            current.AddComponent<MeshCollider>();
            current.GetComponent<Rigidbody>().useGravity = false;


            SliceData data;
            data.slice = currentSlice;

            GameObject segmentContainer = Instantiate(_emptyGameObjectPrefab, current.transform.parent);
            segmentContainer.name = $"{current.name}SliceContainer";


            data.fragmentRootContainer = currentSlice.CreateFragmentRootObject(segmentContainer);
            data.slicesParentObject = segmentContainer.transform.Find($"{segmentContainer.name}Slices").gameObject;
            data.originalObject = current;
            _segments.Add(data);

            GameObject alphaSlider = Instantiate(_alphaSliderPrefab, transform.parent.transform.parent);

            alphaSlider.transform.localPosition = new Vector3(0, 0.03f * i, 0);
            alphaSlider.GetComponent<AlphaMat>().MainMaterial=currentSlice.sliceOptions.insideMaterial;
        }
    }

    public void UpdateSlicingData()
    {
        for (int i=0; i < _segments.Count;i++)
        {
            foreach(Transform j in _segments[i].slicesParentObject.transform)
            {
                Destroy(j.gameObject);
            }
            Tuple<GameObject,GameObject> slicedObjectFragments= _segments[i].slice.ComputeSlice(_occlusionPlane.transform.up, _occlusionPlane.transform.position, true, true, _segments[i].fragmentRootContainer);

            GameObject firstSlicedFragment=slicedObjectFragments.Item1;

            if(firstSlicedFragment!= null )
            {
                //TODO SLICE BY SLIDER
            }
        }
    }

}
