using System;

namespace ServerTest.Attribute
{
    /// <summary>
    /// Marks a property or field to be optionally injected by the custom DI container.
    /// If the service cannot be resolved, the property/field will remain its default value (or null).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class TryInjectAttribute : System.Attribute
    {
    }
}