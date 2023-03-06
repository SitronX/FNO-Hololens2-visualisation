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

    public List<ScrollableButton> AllButtons { get; set; } = new List<ScrollableButton>();
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

            currentScroll.ChangeBackButtonSprite(_datasetDirectories[i] + "/Snapshot/");
            currentScroll.DatasetPath = _datasetDirectories[i];
            currentScroll.ButtonIndex= i;

            currentScroll.QrCodeDatasetActivated += OnAnyQrActivated;
            currentScroll.LoadButtonPressed += OnLoadButtonClicked;
            AllButtons.Add(currentScroll);
        }
        yield return new WaitForEndOfFrame();
        _gridObjectCollection.UpdateCollection();
        yield return new WaitForEndOfFrame();
        _scrollingObjectCollection.UpdateContent();     //This needs to be here like this in coroutine due to bug : https://github.com/microsoft/MixedRealityToolkit-Unity/issues/10350
    }

    public void OnAnyQrActivated(int index)
    {
        for(int i=0;i< AllButtons.Count;i++)
        {
            if (i == index)
            {
                ActiveQR = AllButtons[i];
                AllButtons[i].SetQrActiveState(true);
                AllButtons[i].TryUpdateQRVolume();
            }
            else
            {
                AllButtons[i].SetQrActiveState(false);
            }
        }
    }
    private void OnLoadButtonClicked(int index)
    {
        for (int i = 0; i < AllButtons.Count; i++)
        {
            if (i == index)
            {
                ScrollableButton button = AllButtons[i];

                if (button.ButtonState == ScrollableButton.LoadButtonState.Selectable)
                {
                    button.SetButtonState(ScrollableButton.LoadButtonState.ReadyToLoad);
                }
                else if (button.ButtonState == ScrollableButton.LoadButtonState.ReadyToLoad)
                {
                    button.SetButtonState(ScrollableButton.LoadButtonState.Active);
                    button.LoadDataset();
                }
                else if (button.ButtonState == ScrollableButton.LoadButtonState.Active)
                {
                    button.SetButtonState(ScrollableButton.LoadButtonState.Disabled);
                    button.VolumeGameObject.SetActive(false);
                }
                else if (button.ButtonState == ScrollableButton.LoadButtonState.Disabled)
                {
                    button.SetButtonState(ScrollableButton.LoadButtonState.Active);
                    button.VolumeGameObject.SetActive(true);
                }
            }
            else
            {
                if (AllButtons[i].ButtonState == ScrollableButton.LoadButtonState.ReadyToLoad)
                    AllButtons[i].SetButtonState(ScrollableButton.LoadButtonState.Selectable);
            }
        }
    }

    


}
