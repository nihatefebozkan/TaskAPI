using Microsoft.AspNetCore.Http;
using TaskAPI.Core.Helpers;
using TaskAPI.Core.Models;


namespace TaskAPI.Core.Middleware
{
    public class ErrorHandler(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);

            }
            catch (ArgumentException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest; // Set the response status code to 400 Bad Request   
                context.Response.ContentType = "application/json"; // Set the response content type to JSON
                var code = ErrorCodes.BadRequest;
                var error = new ErrorModel(code, ex.Message, StatusCodes.Status400BadRequest);
                var response = new ResponseModel<object> { Success = false, Result = (object?)null, Error = error };
                await context.Response.WriteAsJsonAsync(response);
            }
            catch (TaskApiException ex)
            {
                context.Response.StatusCode = ex.errorModel.StatusCode; // Set the response status code to the one specified in the exception
                context.Response.ContentType = "application/json"; // Set the response content type to JSON
                var error = ex.errorModel;
                var response = new ResponseModel<object> { Success = false, Result = (object?)null, Error = error };
                await context.Response.WriteAsJsonAsync(response);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError; // Set the response status code to 500 Internal Server Error
                context.Response.ContentType = "application/json"; // Set the response content type to JSON
                var error = new ErrorModel(ErrorCodes.InternalServerError, ex.Message, StatusCodes.Status500InternalServerError);
                var response = new ResponseModel<object> { Success = false, Result = (object?)null, Error = error };
                await context.Response.WriteAsJsonAsync(response);
            }

        }
    }
}
