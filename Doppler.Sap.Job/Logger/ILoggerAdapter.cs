using System;

namespace Doppler.Sap.Job.Service.Logger
{
    public interface ILoggerAdapter<T>
    {
        void LogInformation(string message);
        void LogError(Exception ex, string message, params object[] args);
    }
}
