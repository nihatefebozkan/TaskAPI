using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using TaskAPI.Core.Middleware;

namespace TaskAPI.Core.Helpers
{
    public static class OperationExecutor
    {
        public static async Task<ResponseModel<T>> ExecuteAsync<T>(Func<Task<T>> operation, ILogger logger, HttpContext httpContext, string operationName)
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
            catch (NotFoundException ex)
            {
                logger.LogWarning("{operationName} failed with NotFoundException: {message} at {Method} {Path} at {time}", operationName, ex.Message, httpContext.Request.Method, httpContext.Request.Path, DateTime.UtcNow);
                return new ResponseModel<T>
                {
                    Success = false,
                    Error = new Error(ErrorCodes.NotFound, ex.Message)
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{operationName} failed with unexpected error at {Method} {Path} at {time}", operationName, httpContext.Request.Method, httpContext.Request.Path, DateTime.UtcNow);
                return new ResponseModel<T>
                {
                    Success = false,
                    Error = new Error(ErrorCodes.GenericError, "An error occurred while executing the operation.")
                };
            }
        }
        public static ResponseModel<T> Execute<T>(Func<T> operation, ILogger logger, HttpContext httpContext, string operationName)
        {
            try
            {
                logger.LogInformation("{operationName} started at {Method} {Path} at {time}", operationName, httpContext.Request.Method, httpContext.Request.Path, DateTime.UtcNow);
                var result = operation();
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
            catch (Exception ex)
            {
                logger.LogError(ex, "{operationName} failed with unexpected error at {Method} {Path} at {time}", operationName, httpContext.Request.Method, httpContext.Request.Path, DateTime.UtcNow);
                return new ResponseModel<T>
                {
                    Success = false,
                    Error = new Error(ErrorCodes.GenericError, "An error occurred while executing the operation.")
                };
            }
        }

        public static async Task RunAsync(Func<Task> operation, ILogger logger, HttpContext httpContext, string operationName)
        {
            try
            {
                logger.LogInformation("{operationName} started at {Method} {Path} at {time}", operationName, httpContext.Request.Method, httpContext.Request.Path, DateTime.UtcNow);
                await operation();
                logger.LogInformation("{operationName} completed successfully at {Method} {Path} at {time}", operationName, httpContext.Request.Method, httpContext.Request.Path, DateTime.UtcNow);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning("{operationName} failed with ArgumentException: {message} at {Method} {Path} at {time}", operationName, ex.Message, httpContext.Request.Method, httpContext.Request.Path, DateTime.UtcNow);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{operationName} failed with unexpected error at {Method} {Path} at {time}", operationName, httpContext.Request.Method, httpContext.Request.Path, DateTime.UtcNow);
                throw;
            }
        }
        public static void Run(Action operation, ILogger logger, HttpContext httpContext, string operationName)
        {
            try
            {
                logger.LogInformation("{operationName} started at {Method} {Path} at {time}", operationName, httpContext.Request.Method, httpContext.Request.Path, DateTime.UtcNow);
                operation();
                logger.LogInformation("{operationName} completed successfully at {Method} {Path} at {time}", operationName, httpContext.Request.Method, httpContext.Request.Path, DateTime.UtcNow);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning("{operationName} failed with ArgumentException: {message} at {Method} {Path} at {time}", operationName, ex.Message, httpContext.Request.Method, httpContext.Request.Path, DateTime.UtcNow);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{operationName} failed with unexpected error at {Method} {Path} at {time}", operationName, httpContext.Request.Method, httpContext.Request.Path, DateTime.UtcNow);
                throw;
            }
        }
        
    }
}
