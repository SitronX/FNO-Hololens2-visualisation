using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DatasetLister : MonoBehaviour
{
    [SerializeField] GameObject _scrollableMenuContainer;
    [SerializeField] GameObject _scrollablePrefab;
    [SerializeField] GridObjectCollection _gridObjectCollection;
    [SerializeField] ScrollingObjectCollection _scrollingObjectCollection;

    List<ScrollableButton> _allButtons = new List<ScrollableButton>();
    public ScrollableButton ActiveQR { get; set; }

    public static DatasetLister Instance { get; private set; }              //Singleton

    IEnumerator Start()
    {
        if (Instance != null && Instance != this)               
            Destroy(this);
        else 
            Instance= this;


        List<string> _datasetDirectories = Directory.EnumerateDirectories(Application.streamingAssetsPath + "/DicomData/").ToList();

        for(int i=0;i<_datasetDirectories.Count;i++)
        {
            GameObject current= Instantiate(_scrollablePrefab, _scrollableMenuContainer.transform);
            ScrollableButton currentScroll= current.GetComponent<ScrollableButton>();

            if (i == 0)
            {
                currentScroll.SetQrLabelActive(true);
                ActiveQR = currentScroll;
            }

            currentScroll.ChangeBackButtonSprite(_datasetDirectories[i] + "/Snapshot/");
            currentScroll.DatasetPath = _datasetDirectories[i];
            currentScroll.ButtonIndex= i;

            currentScroll.QrCodeDatasetActivated += OnAnyQrActivated;
            _allButtons.Add(currentScroll);
        }
        yield return new WaitForEndOfFrame();
        _gridObjectCollection.UpdateCollection();
        yield return new WaitForEndOfFrame();
        _scrollingObjectCollection.UpdateContent();     //This needs to be here like this in coroutine due to bug : https://github.com/microsoft/MixedRealityToolkit-Unity/issues/10350
    }

    private void OnAnyQrActivated(int index)
    {
        for(int i=0;i< _allButtons.Count;i++)
        {
            if (i == index)
            {
                ActiveQR = _allButtons[i];
                _allButtons[i].SetQrLabelActive(true);
                _allButtons[i].TryUpdateQRVolume();
            }
            else
            {
                _allButtons[i].SetQrLabelActive(false);
            }
        }
    }

    


}
