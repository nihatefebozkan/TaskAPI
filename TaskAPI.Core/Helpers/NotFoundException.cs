using System;
using System.Collections.Generic;
using System.Text;

namespace TaskAPI.Core.Helpers
{
    public class NotFoundException : Exception
    {
        internal string Message;

        public NotFoundException(string? message) : base(message)
        {
        }
    }
}
