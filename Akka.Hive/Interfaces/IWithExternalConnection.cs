using System;

namespace Akka.Hive.Interfaces
{
    public interface IWithExternalConnection
    {
        Action<object> SendExtern { get; set; }
    }
}