namespace UnityVolumeRendering
{
    public interface IProgressHandler
    {
        void ReportProgress(float progress, string description);
        void ReportProgress(int currentStep, int totalSteps, string description);
        void UpdateTotalNumberOfParts(int numberOfParts);
    }
}
