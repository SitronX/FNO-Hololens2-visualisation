using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QRDataSpawner : MonoBehaviour
{
    GameObject _activeVolume = null;
    Vector3 _defaultVolumScale = new Vector3(0.3f, 0.3f, 0.3f);

    private void Start()
    {
        if (DatasetLister.Instance.ActiveQR.VolumeGameObject == null)
        {
            DatasetLister.Instance.ActiveQR.LoadDataset();
            DatasetLister.Instance.ActiveQR.TryUpdateQRVolume();
        }
        else
        {
            DatasetLister.Instance.ActiveQR.EnableVolume();
            ChangeVolumeData(DatasetLister.Instance.ActiveQR.VolumeGameObject);
        }
    }
    public void ChangeVolumeData(GameObject volume)
    {
        if (_activeVolume != null)
        {
            _activeVolume.transform.position+= new Vector3(0, 0, 2);        //Move it away little bit
            _activeVolume.transform.parent = null;
        }

        _activeVolume = volume;
        _activeVolume.transform.parent = transform;
        _activeVolume.transform.localPosition= Vector3.zero;
        _activeVolume.transform.localRotation= Quaternion.identity;
        _activeVolume.transform.localScale = _defaultVolumScale;
        
    }
}
