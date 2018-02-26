using System;

namespace Neo.UI.Core.Services.Implementations.Exceptions
{
    public class ObjectAlreadyInitializedException : Exception
    {
        public ObjectAlreadyInitializedException(string instanceName)
            : base(instanceName + " has already been initialized!")
        {
        }
    }
}
