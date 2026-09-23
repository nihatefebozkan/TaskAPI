using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskAPI.Core.Helpers
{
    public class Error
    {
        //public bool Success { get; }
        //public object? Result { get; }
        public string ErrorCode { get; }
        public string? Description { get; }
        public string Message { get; }
        public string CorrelationId { get; }
        public DateTime TimeStamp { get; }

        public Error(string message)
        {
            ErrorCode = ErrorCodes.GenericError;
            Message = message;
            CorrelationId = Guid.NewGuid().ToString();
            TimeStamp = DateTime.UtcNow;
        }


        public Error(string errorCode, string message)
        {
            ErrorCode = errorCode;
            Message = message;
            CorrelationId = Guid.NewGuid().ToString();
            TimeStamp = DateTime.UtcNow;
        }

        public Error(string errorCode, string description, string message)
        {
            ErrorCode = errorCode;
            Description = description;
            Message = message;
            CorrelationId = Guid.NewGuid().ToString();
            TimeStamp = DateTime.UtcNow;
        }

        public Error(string errorCode, string description, string message, string correlationId)
        {
            ErrorCode = errorCode;
            Description = description;
            Message = message;
            CorrelationId = correlationId;
            TimeStamp = DateTime.UtcNow;
        }

    }
}
