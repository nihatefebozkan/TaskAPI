using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Core.Helpers;

namespace TaskAPI.Core.Models
{
    public class ErrorModel
    {
        //public bool Success { get; }
        //public object? Result { get; }
        public string ErrorCode { get; }
        public string? Description { get; }
        public string Message { get; }
        public string CorrelationId { get; }
        public DateTime TimeStamp { get; }

        public ErrorModel(string message)
        {
            ErrorCode = ErrorCodes.GenericError;
            Message = message;
            CorrelationId = Guid.NewGuid().ToString();
            TimeStamp = DateTime.UtcNow;
        }


        public ErrorModel(string errorCode, string message)
        {
            ErrorCode = errorCode;
            Message = message;
            CorrelationId = Guid.NewGuid().ToString();
            TimeStamp = DateTime.UtcNow;
        }

        public ErrorModel(string errorCode, string description, string message)
        {
            ErrorCode = errorCode;
            Description = description;
            Message = message;
            CorrelationId = Guid.NewGuid().ToString();
            TimeStamp = DateTime.UtcNow;
        }

        public ErrorModel(string errorCode, string description, string message, string correlationId)
        {
            ErrorCode = errorCode;
            Description = description;
            Message = message;
            CorrelationId = correlationId;
            TimeStamp = DateTime.UtcNow;
        }

    }
}
