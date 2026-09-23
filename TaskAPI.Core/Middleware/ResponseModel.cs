using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Core.Helpers;

namespace TaskAPI.Core.Middleware
{
    public class ResponseModel<T>
    {
        public bool Success { get; set; }
        public T? Result { get; set; }
        public Error? Error { get; set; } = null;
    }
}
