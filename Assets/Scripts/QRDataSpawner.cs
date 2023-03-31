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
        if (HandMenu.Instance.ActiveQRDataset != null)
        {
            if (HandMenu.Instance.ActiveQRDataset.VolumeControlObject == null)
            {
                HandMenu.Instance.ActiveQRDataset.LoadDataset();
                HandMenu.Instance.ActiveQRDataset.TryUpdateQRVolume();
            }
            else
            {
                HandMenu.Instance.ActiveQRDataset.SetButtonState(DatasetButton.LoadButtonState.Active);
                ChangeVolumeData(HandMenu.Instance.ActiveQRDataset.VolumeControlObject);
            }
        }
        QrCodeSpawned?.Invoke();
    }
    public void ChangeVolumeData(VolumeDataControl volume)
    {
        if (_activeVolume != null)
        {
            _activeVolume.transform.position = transform.position + new Vector3(1, 0, 0);
            _activeVolume.transform.parent = null;
        }

        _activeVolume = volume.gameObject;
        _activeVolume.transform.parent = transform;
        _activeVolume.transform.localPosition= Vector3.zero;
        _activeVolume.transform.localRotation= Quaternion.identity;
        _activeVolume.transform.localScale = _defaultVolumScale;

        volume.ResetCrossSectionToolsTransform();
        volume.ResetHandleTransform();
        volume.ResetSlicesTransform();
        
    }
}
