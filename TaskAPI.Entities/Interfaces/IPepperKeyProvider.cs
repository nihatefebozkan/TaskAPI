using System;
using System.Collections.Generic;
using System.Text;

namespace TaskAPI.Entities.Interfaces
{
    public interface IPepperKeyProvider
    {
        byte[] GetKey();
    }
}
