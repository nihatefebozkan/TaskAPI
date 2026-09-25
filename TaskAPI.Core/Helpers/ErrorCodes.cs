using System;
using System.Collections.Generic;
using System.Text;

namespace TaskAPI.Core.Helpers
{
    public static class ErrorCodes
    {
        public const string GenericError = "TASKAPI_GENERIC_ERROR";
        public const string InternalServerError = "INTERNAL_SERVER_ERROR";
        public const string BadRequest = "BAD_REQUEST";
        public const string NotFound = "NOT_FOUND";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string InvalidPassword = "INVALID_PASSWORD";
        public const string Conflict = "CONFLICT";

    }
}
