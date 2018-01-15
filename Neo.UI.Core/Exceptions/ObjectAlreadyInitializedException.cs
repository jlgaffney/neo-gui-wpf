using System;

namespace Neo.UI.Core.Exceptions
{
    public class ObjectAlreadyInitializedException : Exception
    {
        public ObjectAlreadyInitializedException(string instanceName)
            : base(instanceName + " has already been initialized!")
        {
        }
    }
}
