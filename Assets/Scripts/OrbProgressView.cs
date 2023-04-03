using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityVolumeRendering;

public class OrbProgressView :MonoBehaviour,IProgressView
{
    [SerializeField] ProgressIndicatorOrbsRotator _orbObject;
    [SerializeField] TMP_Text _textIndicator;
    [SerializeField] TMP_Text _textPartNumber;

    ConcurrentQueue<ProgressData> progressQueue= new ConcurrentQueue<ProgressData>();       //Queue due to the fact that background thread cannot update ui, which is an issue with async tasks
    int _numberOfParts;

    Camera _mainCamera;

    private void Start()
    {
        _mainCamera = FindObjectOfType<Camera>();
    }

    struct ProgressData
    {
        public float progress;
        public string description;
        public int partNumber;
    }
    public void FinishProgress(ProgressStatus status = ProgressStatus.Succeeded)
    {
        _orbObject.CloseAsync();
    }

    public void StartProgress(string description,int numberOfParts)
    {
        _numberOfParts = numberOfParts;
        OpenOrbView(description);   
    }

    public void UpdateProgress(float progress, string description,int partNumber)
    {
        EnqeueReport(progress,description, partNumber);
    }

    private async void OpenOrbView(string description)
    {
        await _orbObject.OpenAsync();
       
        EnqeueReport(0, description, 1);
    }
    private void EnqeueReport(float progress, string description,int partNumber)
    {
        ProgressData progressData;
        progressData.progress = progress;
        progressData.description = description;
        progressData.partNumber = partNumber;
        progressQueue.Enqueue(progressData);
    }

    private void FixedUpdate()
    {
        if(progressQueue.TryDequeue(out ProgressData progressData))
        {
            _orbObject.Message=progressData.description;
            _textIndicator.text = $"{(int)(progressData.progress*100)} %";
            _textPartNumber.text = $"{progressData.partNumber}/{_numberOfParts}";
        }
        transform.rotation = _mainCamera.transform.rotation;
    }

    public void UpdateTotalNumberOfParts(int numberOfParts)
    {
        _numberOfParts = numberOfParts;
    }
}
