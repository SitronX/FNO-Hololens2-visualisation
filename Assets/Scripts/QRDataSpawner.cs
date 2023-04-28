using UnityEngine;

public class QRDataSpawner : MonoBehaviour
{
    GameObject _activeVolume = null;
    Vector3 _defaultVolumScale = new Vector3(0.33f, 0.33f, 0.33f);

    private void Start()
    {
        if (HandMenu.Instance.ActiveQRDataset != null)        
            ChangeVolumeData(HandMenu.Instance.ActiveQRDataset.VolumeControlObject);
    }
    public void ChangeVolumeData(VolumeDataControl volume)
    {
        if (_activeVolume != null)
        {
            _activeVolume.transform.position = transform.position + new Vector3(1, 0, 0);   //if some dataset was previously on qr position, move it out the way
            _activeVolume.transform.parent = null;
        }

        _activeVolume = volume.gameObject;
        _activeVolume.transform.parent = transform;
        _activeVolume.transform.localPosition= Vector3.zero;
        _activeVolume.transform.localRotation= Quaternion.identity;
        _activeVolume.transform.localScale = _defaultVolumScale;

        volume.SetQRRotation();
        volume.ResetCrossSectionToolsTransform();
        volume.ResetHandleTransform();
        volume.ResetSlicesTransform();      
    }
}
