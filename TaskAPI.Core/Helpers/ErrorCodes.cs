namespace TaskAPI.Core.Helpers
{
    public class ErrorCodes
    {
        public const string GenericException = "TASKAPI_GENERIC_EXCEPTION";
        public const string InternalServerError = "INTERNAL_SERVER_ERROR";
        public const string BadRequest = "BAD_REQUEST";
        public const string TaskNotFound = "TASK_NOT_FOUND";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string InvalidPassword = "INVALID_PASSWORD";
        public const string Conflict = "CONFLICT";
        public const string AuthNotFound = "AUTH_NOT_FOUND";
        public const string InvalidTaskFormat = "INVALID_TASK_FORMAT";

    }
}
