using System;

namespace Neo.Gui.Base.Exceptions
{
    public class ObjectAlreadyInitializedException : Exception
    {
        public ObjectAlreadyInitializedException(string instanceName)
            : base(instanceName + " has already been initialized!")
        {
        }
    }
}
