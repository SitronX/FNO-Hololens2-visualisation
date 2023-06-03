using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Concurrent;
using TMPro;
using UnityEngine;
using UnityVolumeRendering;

public class OrbProgressView : MonoBehaviour, IProgressView
{
    [SerializeField] ProgressIndicatorOrbsRotator _orbObject;
    [SerializeField] TMP_Text _textIndicator;
    [SerializeField] TMP_Text _textPartNumber;

    int _numberOfParts;
    Camera _mainCamera;
    ConcurrentQueue<ProgressData> _progressQueue = new ConcurrentQueue<ProgressData>();       //Queue due to the fact that background thread cannot update ui, which is an issue with async tasks

    struct ProgressData
    {
        public float progress;
        public string description;
        public int partNumber;
    }
    private void Start()
    {
        _mainCamera = FindObjectOfType<Camera>();
    }
    public void FinishProgress(ProgressStatus status = ProgressStatus.Succeeded)
    {
        _orbObject.CloseAsync();
    }

    public void StartProgress(string description, int numberOfParts)
    {
        _numberOfParts = numberOfParts;
        OpenOrbView(description);
    }

    public void UpdateProgress(float progress, string description, int partNumber)
    {
        EnqeueReport(progress, description, partNumber);
    }

    private async void OpenOrbView(string description)
    {
        await _orbObject.OpenAsync();

        EnqeueReport(0, description, 1);
    }
    private void EnqeueReport(float progress, string description, int partNumber)
    {
        ProgressData progressData;
        progressData.progress = progress;
        progressData.description = description;
        progressData.partNumber = partNumber;
        _progressQueue.Enqueue(progressData);
    }

    private void FixedUpdate()
    {
        if (_progressQueue.TryDequeue(out ProgressData progressData))
        {
            _orbObject.Message = progressData.description;
            _textIndicator.text = progressData.progress == 0 ? "" : $"{(int)(progressData.progress * 100)} %";
            _textPartNumber.text = $"{progressData.partNumber}/{_numberOfParts}";
        }
        transform.rotation = _mainCamera.transform.rotation;
    }
    public void UpdateTotalNumberOfParts(int numberOfParts)
    {
        _numberOfParts = numberOfParts;
    }
}