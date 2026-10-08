namespace TaskAPI.Core.Models
{
    public class ErrorModel
    {
        public string ErrorCode { get; set; }
        public int StatusCode { get; set; }
        public string? Description { get; set; }
        public string Message { get; set; }
        public string CorrelationId { get; set; }
        public DateTime TimeStamp { get; set; }


        public ErrorModel()
        {

        }
        public ErrorModel(string errorCode, string message)
        {
            ErrorCode = errorCode;
            Message = message;
            StatusCode = 500; // Default to Internal Server Error
            CorrelationId = Guid.NewGuid().ToString();
            TimeStamp = DateTime.UtcNow;
        }


        public ErrorModel(string errorCode, string message, int statusCode)
        {
            ErrorCode = errorCode;
            Message = message;
            StatusCode = statusCode;
            CorrelationId = Guid.NewGuid().ToString();
            TimeStamp = DateTime.UtcNow;
        }

        public ErrorModel(string errorCode, string description, string message, int statusCode)
        {
            ErrorCode = errorCode;
            Description = description;
            Message = message;
            StatusCode = statusCode;
            CorrelationId = Guid.NewGuid().ToString();
            TimeStamp = DateTime.UtcNow;
        }

        public ErrorModel(string errorCode, string description, string message, string correlationId, int statusCode)
        {
            ErrorCode = errorCode;
            Description = description;
            Message = message;
            CorrelationId = correlationId;
            StatusCode = statusCode;
            TimeStamp = DateTime.UtcNow;
        }
    }
}
