using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Core.Middleware;

namespace TaskAPI.Core.Helpers
{
    public static class OperationExecutor
    {
        public static async Task<ResponseModel<T>> Execute<T>(Func<Task<T>> operation, ILogger logger, HttpContext httpContext, string operationName)
        {
            try
            {
                logger.LogInformation("{operationName} started at {Method} {Path} at {time}", operationName, httpContext.Request.Method, httpContext.Request.Path, DateTime.UtcNow);
                var result = await operation();
                logger.LogInformation("{operationName} completed successfully at {Method} {Path} at {time}", operationName, httpContext.Request.Method, httpContext.Request.Path, DateTime.UtcNow);
                return new ResponseModel<T>
                {
                    Success = true,
                    Result = result
                };
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning("{operationName} failed with ArgumentException: {message} at {Method} {Path} at {time}", operationName, ex.Message, httpContext.Request.Method, httpContext.Request.Path, DateTime.UtcNow);
                return new ResponseModel<T>
                {
                    Success = false,
                    Error = new Error(ErrorCodes.BadRequest, ex.Message)
                };
            }
            catch (Exception)
            {
                logger.LogError("{operationName} failed with unexpected error at {Method} {Path} at {time}", operationName, httpContext.Request.Method, httpContext.Request.Path, DateTime.UtcNow);
                return new ResponseModel<T>
                {
                    Success = false,
                    Error = new Error(ErrorCodes.GenericError, "An error occurred while executing the operation.")
                };
            }
        }
    }
}
