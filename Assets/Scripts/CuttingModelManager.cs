using System;
using System.Collections.Generic;
using UnityEngine;

public class CuttingModelManager : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] GameObject _emptyGameObjectPrefab;
    [SerializeField] GameObject _slicingPlaneObject;
    [SerializeField] GameObject _alphaSliderPrefab;
    [SerializeField] GameObject _alphaSliderContainer;
    [SerializeField] GameObject _parentSpawnObject;
    [SerializeField] GameObject _grabHandle;

    TransformSave _mainStartTransform;
    TransformSave _planeStartTransform;
    TransformSave _grabHandleStartTransform;

    GameObject _mainObject;
    List<SliceData> _segmentObjects = new List<SliceData>();


    public struct SliceData
    {
        public GameObject originalObject;
        public Slice slice;
        public GameObject fragmentRootContainer;
        public GameObject slicesParentObject;
        public Segment segment;
    }

    void Start()
    {
        GameObject[] obj=Resources.LoadAll<GameObject>("");
       
        _mainObject = Instantiate(obj[0], _parentSpawnObject.transform);        //Loading first item from resources folder
        _mainObject.transform.localPosition = new Vector3(0, 0, 0);
        _mainObject.transform.localRotation = Quaternion.identity;

        int currentChildCount = _mainObject.transform.childCount;

        for (int i = 0; i < currentChildCount; i++)
        {
            GameObject currentSegment = _mainObject.transform.GetChild(i).gameObject;
            MeshRenderer renderer = currentSegment.GetComponent<MeshRenderer>();

            Slice currentSlice= currentSegment.AddComponent<Slice>();
            currentSlice.sliceOptions = new SliceOptions
            {
                insideMaterial = renderer.sharedMaterial,
                enableReslicing = true
            };

            currentSegment.AddComponent<MeshCollider>();
            Rigidbody bod = currentSegment.GetComponent<Rigidbody>();
            bod.useGravity = false;
            bod.isKinematic = true;

            GameObject segmentContainer = Instantiate(_emptyGameObjectPrefab, currentSegment.transform.parent);
            segmentContainer.name = $"{currentSegment.name}SliceContainer";  
            


            GameObject alphaSlider = Instantiate(_alphaSliderPrefab, _alphaSliderContainer.transform);

            alphaSlider.transform.localPosition = new Vector3(19.5f,3.2f- (0.17f * i), 5);
            alphaSlider.transform.localRotation = Quaternion.Euler(new Vector3(0, -90, 0));

            Segment seg= alphaSlider.GetComponent<Segment>();
            seg.InitSegment(renderer);

            SliceData data;
            data.slice = currentSlice;
            data.fragmentRootContainer = currentSlice.CreateFragmentRootObject(segmentContainer);
            data.slicesParentObject = segmentContainer.transform.Find($"{segmentContainer.name}Slices").gameObject;
            data.originalObject = currentSegment;
            data.segment = seg;

            _segmentObjects.Add(data);
        }
        SetInitialPosition();
        UpdateSlicingData();

    }

    public void UpdateSlicingData()
    {
        for (int i=0; i < _segmentObjects.Count;i++)
        {
            foreach(Transform j in _segmentObjects[i].slicesParentObject.transform)
            {
                Destroy(j.gameObject);
            }
            Tuple<GameObject,GameObject> parts=_segmentObjects[i].slice.ComputeSlice(sliceNormalWorld: _slicingPlaneObject.transform.up, sliceOriginWorld: _slicingPlaneObject.transform.position+ (_slicingPlaneObject.transform.up*0.001f),instantiateOnlyLeftFragment: true,isKinematic: true, fragmentRootObject: _segmentObjects[i].fragmentRootContainer);
            _segmentObjects[i].segment.MeshRenderer = parts.Item1.GetComponent<MeshRenderer>();
        }
    }
    public void ResetObjectTransform()
    {
        Converters.UpdateTransform(transform, _mainStartTransform);
        Converters.UpdateTransform(_slicingPlaneObject.transform.parent.transform, _planeStartTransform);
        Converters.UpdateTransform(_grabHandle.transform, _grabHandleStartTransform);

        UpdateSlicingData();
    }

    private void SetInitialPosition()
    {
        _mainStartTransform = Converters.ConvertTransform(transform);
        _planeStartTransform= Converters.ConvertTransform(_slicingPlaneObject.transform.parent.transform);
        _grabHandleStartTransform = Converters.ConvertTransform(_grabHandle.transform);
    }
    public void TurnAllAlphas(bool value)
    {
        _segmentObjects.ForEach(x => x.segment.UpdateSlider(value ? 1 : 0));
    }

}
