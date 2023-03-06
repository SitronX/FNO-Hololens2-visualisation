using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QRDataSpawner : MonoBehaviour
{
    GameObject _activeVolume = null;
    Vector3 _defaultVolumScale = new Vector3(0.3f, 0.3f, 0.3f);

    public static Action QrCodeSpawned { get; set; }

    private void Start()
    {
        if (DatasetLister.Instance.ActiveQR != null)
        {
            if (DatasetLister.Instance.ActiveQR.VolumeGameObject == null)
            {
                DatasetLister.Instance.ActiveQR.LoadDataset();
                DatasetLister.Instance.ActiveQR.TryUpdateQRVolume();
            }
            else
            {
                DatasetLister.Instance.ActiveQR.VolumeGameObject.SetActive(true);
                DatasetLister.Instance.ActiveQR.SetButtonState(ScrollableButton.LoadButtonState.Active);
                ChangeVolumeData(DatasetLister.Instance.ActiveQR.VolumeGameObject);
            }
        }
        QrCodeSpawned?.Invoke();
    }
    public void ChangeVolumeData(GameObject volume)
    {
        if (_activeVolume != null)
        {
            _activeVolume.transform.position = transform.position + new Vector3(1, 0, 0);
            _activeVolume.transform.parent = null;
        }

        _activeVolume = volume;
        _activeVolume.transform.parent = transform;
        _activeVolume.transform.localPosition= Vector3.zero;
        _activeVolume.transform.localRotation= Quaternion.identity;
        _activeVolume.transform.localScale = _defaultVolumScale;

        VolumeDataControl contr=_activeVolume.GetComponent<VolumeDataControl>();
        contr.ResetCrossSectionToolsTransform();
        contr.ResetHandleTransform();
        contr.ResetSlicesTransform();
        
    }
}
