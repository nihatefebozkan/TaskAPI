using TaskAPI.Core.Models;

namespace TaskAPI.Core.Helpers
{
    public class TaskApiException : Exception
    {
        public ErrorModel errorModel;
        public TaskApiException(string errorCode, string message) : base(message)
        {
            errorModel = new ErrorModel(errorCode, message, 500);
        }
        public TaskApiException(string message, int statusCode) : base(message)
        {
            errorModel = new ErrorModel(ErrorCodes.GenericException, message, statusCode);
        }
        public TaskApiException(string errorCode, string message, int statusCode) : base(message)
        {
            errorModel = new ErrorModel(errorCode, message, statusCode);
        }
        public TaskApiException(string errorCode, string message, int statusCode, Exception innerException) : base(message, innerException)
        {
            errorModel = new ErrorModel(errorCode, message, statusCode);
        }

    }
}
