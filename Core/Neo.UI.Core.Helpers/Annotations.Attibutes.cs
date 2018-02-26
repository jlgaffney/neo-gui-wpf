using System;

namespace Neo.UI.Core.Helpers
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public sealed class ValidatedNotNullAttribute : Attribute
    {
    }
}
