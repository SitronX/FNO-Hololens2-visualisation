using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingModelManager : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] GameObject _emptyGameObjectPrefab;
    List<SliceData> _segments = new List<SliceData>();
    [SerializeField] GameObject _slicingPlaneObject;
    [SerializeField] GameObject _alphaSliderPrefab;
    [SerializeField] GameObject _alphaSliderContainer;

    Vector3 _startLocalPosition;
    Vector3 _startLocalRotation;
    Vector3 _startLocalScale;

    Vector3 _startLocalPlanePosition;
    Vector3 _startLocalPlaneRotation;
    Vector3 _startLocalPlaneScale;


    public struct SliceData
    {
        public GameObject originalObject;
        public Slice slice;
        public GameObject fragmentRootContainer;
        public GameObject slicesParentObject;
    }


    void Start()
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

            GameObject alphaSlider = Instantiate(_alphaSliderPrefab, _alphaSliderContainer.transform);

            alphaSlider.transform.localPosition = new Vector3(19.5f,3.2f- (0.15f * i), 5);
            alphaSlider.transform.localRotation = Quaternion.Euler(new Vector3(0, -90, 0));

            alphaSlider.GetComponent<AlphaMat>().MainMaterial=currentSlice.sliceOptions.insideMaterial;

            ResetInitialPosition();
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
            Tuple<GameObject,GameObject> slicedObjectFragments= _segments[i].slice.ComputeSlice(_slicingPlaneObject.transform.up, _slicingPlaneObject.transform.position+ (_slicingPlaneObject.transform.up*0.001f), true, true, _segments[i].fragmentRootContainer);

            GameObject firstSlicedFragment=slicedObjectFragments.Item1;

            if(firstSlicedFragment!= null )
            {
                //TODO SLICE BY SLIDER
            }
        }
    }
    public void ResetObjectTransform()
    {
        transform.parent.localPosition = _startLocalPosition;
        transform.parent.localRotation = Quaternion.Euler(_startLocalRotation);
        transform.parent.localScale = _startLocalScale;

        _slicingPlaneObject.transform.parent.localPosition = _startLocalPlanePosition;
        _slicingPlaneObject.transform.parent.localRotation = Quaternion.Euler(_startLocalPlaneRotation);
        _slicingPlaneObject.transform.parent.localScale = _startLocalPlaneScale;

        UpdateSlicingData();
    }

    private void ResetInitialPosition()
    {
        _startLocalPosition = new Vector3(transform.parent.localPosition.x, transform.parent.localPosition.y, transform.parent.localPosition.z);
        _startLocalRotation = new Vector3(transform.parent.localRotation.eulerAngles.x, transform.parent.localRotation.eulerAngles.y, transform.parent.localRotation.eulerAngles.z);
        _startLocalScale = new Vector3(transform.parent.localScale.x, transform.parent.localScale.y, transform.parent.localScale.z);

        _startLocalPlanePosition = new Vector3(_slicingPlaneObject.transform.parent.localPosition.x, _slicingPlaneObject.transform.parent.localPosition.y, _slicingPlaneObject.transform.parent.localPosition.z);
        _startLocalPlaneRotation = new Vector3(_slicingPlaneObject.transform.parent.localRotation.eulerAngles.x, _slicingPlaneObject.transform.parent.localRotation.eulerAngles.y, _slicingPlaneObject.transform.parent.localRotation.eulerAngles.z);
        _startLocalPlaneScale = new Vector3(_slicingPlaneObject.transform.parent.localScale.x, _slicingPlaneObject.transform.parent.localScale.y, _slicingPlaneObject.transform.parent.localScale.z);
    }

}
