using System;

namespace CodeHealthHub.Services
{
    public class ErrorStateService
    {
        public Exception? LastException { get; set; }
        public string? ErrorMessage { get; set; }

        public void SetError(Exception ex)
        {
            LastException = ex;
            ErrorMessage = ex?.Message;
        }

        public void ClearError()
        {
            LastException = null;
            ErrorMessage = null;
        }
    }
}