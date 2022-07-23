namespace System.Runtime.CompilerServices
{
#pragma warning disable 0436
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false, AllowMultiple = false)]
    public sealed class AsyncMethodBuilderAttribute : Attribute
    {
        public AsyncMethodBuilderAttribute(Type builderType)
        {
            this.BuilderType = builderType;
        }

        public Type BuilderType { get; }
    }
#pragma warning restore
}