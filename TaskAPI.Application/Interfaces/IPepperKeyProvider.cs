using System;
using System.Collections.Generic;
using System.Text;

namespace TaskAPI.Application.Interfaces
{
    public interface IPepperKeyProvider
    {
        byte[] GetKey(string key);
    }
}
