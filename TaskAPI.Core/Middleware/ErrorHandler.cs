using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Core.Helpers;

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
                var error = new Error(ErrorCodes.BadRequest, ex.Message);
                var response = new ResponseModel<object> { Success = false, Result = (object?)null, Error = error };
                await context.Response.WriteAsJsonAsync(response);
            }
            catch (Exception)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError; // Set the response status code to 500 Internal Server Error
                context.Response.ContentType = "application/json"; // Set the response content type to JSON
                var error = new Error(ErrorCodes.InternalServerError, "An unexpected error occurred.");
                var response = new ResponseModel<object> { Success = false, Result = (object?)null, Error = error };
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
