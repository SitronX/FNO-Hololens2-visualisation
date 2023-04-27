using System;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Progress handler, for tracking the progress of long (async) actions, such as import.
    /// How to use:
    /// - Create instace with the "using" statement, to ensure that failure callback is called on unhandled exceptions. 
    /// - Call Start() when starting
    /// - Call ReportProgress() to update progress
    /// - (optionally) call StartStage() and EndStage() to create a sub-stage to track the progress of.
    /// - Call Finish() or Fail() when done.
    /// </summary>
    public class ProgressHandler : IProgressHandler, IDisposable
    {
        private class ProgressStage
        {
            public float start;
            public float end;
        }

        private string description = "";
        private float currentProgress = 0.0f;
        private IProgressView progressView;
        private int _activePart;
        private string _lastDescription = "";

        public ProgressHandler(IProgressView progressView)
        {
            this.progressView = progressView;
        }

        /// <summary>
        /// Start the processing.
        /// </summary>
        public void Start(string description,int numberOfParts)
        {
            this.progressView.StartProgress(description,numberOfParts);
            this.description = description;
            currentProgress = 0.0f;
            _activePart = 1;
            _lastDescription = description;
        }

        /// <summary>
        /// Report current progress.
        /// <param name="progress">Current progress. Value between 0.0 and 1.0 (0-100%)</param>
        /// <param name="description">Description of the work being done</param>
        /// </summary>
        public void ReportProgress(float progress, string description)
        {
            if (description != "")
                this.description = description;
            currentProgress = progress;

            if(description!=_lastDescription)
            {
                _activePart++;
                _lastDescription = description;
            }

            UpdateProgressView();
        }

        /// <summary>
        /// Report current progress, by step.
        /// <param name="currentStep">Index of current step (must be less than or equal to totalSteps)</param>
        /// <param name="totalSteps">Total number of steps</param>
        /// <param name="description">Description of the work being done</param>
        /// </summary>
        public void ReportProgress(int currentStep, int totalSteps, string description)
        {
            if (description != "")
                this.description = description;
            currentProgress = (currentStep / (float)totalSteps);

            if (description!= _lastDescription)
            {
                _activePart++;
                _lastDescription = description;
            }

            UpdateProgressView();
        }

        public void Dispose()
        {
            this.progressView.FinishProgress();
        }

        private void UpdateProgressView()
        {
            this.progressView.UpdateProgress(currentProgress, description,_activePart);
        }

        public void UpdateTotalNumberOfParts(int numberOfParts)
        {
            progressView.UpdateTotalNumberOfParts(numberOfParts);
        }
    }
}
