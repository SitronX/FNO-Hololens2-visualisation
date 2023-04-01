using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatasetSaveSystem : MonoBehaviour, IMixedRealityInputHandler
{
    [SerializeField] DatasetSaveData _saveData;
    [SerializeField] VolumeDataControl _volumeControl;
    public void OnInputDown(InputEventData eventData)
    {
        //
    }

    public void OnInputUp(InputEventData eventData)
    {
        //throw new System.NotImplementedException();
    }
    private void UpdateSaveData()
    {

    }
}
