//using System;
//using System.Collections.Generic;
//using System.Text;
//using Microsoft.AspNetCore.Mvc;
//using TaskAPI.Core.Helpers;

//namespace TaskAPI.Core.Middleware
//{
//    public class ErrorResponse
//    {
//        public static IActionResult Create(ActionContext context)
//        {
//            var messages = context.ModelState.Values
//                .SelectMany(v => v.Errors)
//                .Select(e => e.ErrorMessage);
//            var error = new Error(ErrorCodes.BadRequest, string.Join("...", messages));
//            return new BadRequestObjectResult(error);
//        }
//    }
//}