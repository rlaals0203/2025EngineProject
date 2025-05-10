using System;

namespace Blade.Core.Dependencies
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Field)]
    public class InjectAttribute : Attribute
    {
        
    }
}