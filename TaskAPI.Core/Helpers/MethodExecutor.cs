using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using TaskAPI.Core.Middleware;

namespace TaskAPI.Core.Helpers
{
    public static class MethodExecutor
    {
        //genel logger tanımlaması ve httpcontext erişimi ile birlikte çalışacak bir metot oluşturuyoruz. Bu metot, verilen işlemi çalıştırır ve işlem sırasında oluşabilecek hataları yakalar ve loglar.
        public static async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, ILogger logger, IHttpContextAccessor httpContextAccessor, string operationName)
        {
            var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP request found");
            try
            {
                logger.LogInformation("{operationName} started at {Method} {Path} at {time}", operationName, httpContext.Request.Method, httpContext.Request.Path, DateTime.UtcNow);
                var result = await operation();
                logger.LogInformation("{operationName} completed successfully at {Method} {Path} at {time}", operationName, httpContext.Request.Method, httpContext.Request.Path, DateTime.UtcNow);
                return result;
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
        public static T Execute<T>(Func<T> operation, ILogger logger, IHttpContextAccessor httpContextAccessor, string operationName)
        {
            var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP request found");
            try
            {
                logger.LogInformation("{operationName} started at {Method} {Path} at {time}", operationName, httpContext.Request.Method, httpContext.Request.Path, DateTime.UtcNow);
                var result = operation();
                logger.LogInformation("{operationName} completed successfully at {Method} {Path} at {time}", operationName, httpContext.Request.Method, httpContext.Request.Path, DateTime.UtcNow);
                return result;
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

        public static async Task RunAsync(Func<Task> operation, ILogger logger, IHttpContextAccessor httpContextAccessor, string operationName)
        {
            var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP request found");
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
        public static void Run(Action operation, ILogger logger, IHttpContextAccessor httpContextAccessor, string operationName)
        {
            var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP request found");
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
