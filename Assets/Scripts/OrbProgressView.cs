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

    ConcurrentQueue<ProgressData> progressQueue= new ConcurrentQueue<ProgressData>();       //Queue due to the fact that background thread cannot update ui, which is an issue with async tasks

    struct ProgressData
    {
        public float progress;
        public string description;
    }
    public void FinishProgress(ProgressStatus status = ProgressStatus.Succeeded)
    {
        CloseOrbView();
    }

    public void StartProgress(string title, string description)
    {
        OpenOrbView(title);      
    }

    public void UpdateProgress(float progress, string description)
    {
        EnqeueReport(progress,description);
    }

    private async void OpenOrbView(string title)
    {
        await _orbObject.OpenAsync();
       
        EnqeueReport(0,title);
    }
    private async void CloseOrbView()
    {
        await _orbObject.CloseAsync();
    }
    private void EnqeueReport(float progress, string description)
    {
        ProgressData progressData;
        progressData.progress = progress;
        progressData.description = description;
        progressQueue.Enqueue(progressData);
    }

    private void FixedUpdate()
    {
        if(progressQueue.TryDequeue(out ProgressData progressData))
        {
            _orbObject.Message=progressData.description;
            _textIndicator.text = $"{(int)(progressData.progress*100)} %";
        }
    }
}
