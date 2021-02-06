using System;
using System.Diagnostics;

namespace ElmahCore
{
    internal class TypeResolutionArgs
    {
        public TypeResolutionArgs(string typeName, bool throwOnError, bool ignoreCase)
        {
            TypeName = typeName;
            ThrowOnError = throwOnError;
            IgnoreCase = ignoreCase;
        }

        public string TypeName { get; }
        public bool ThrowOnError { get; }
        public bool IgnoreCase { get; }
    }

    internal static class TypeResolution
    {
        private static readonly Message<TypeResolutionArgs, Type> Message = new Message<TypeResolutionArgs, Type>();

        static TypeResolution()
        {
            PushHandler(next => (sender, input) => Type.GetType(input.TypeName, input.ThrowOnError, input.IgnoreCase));
        }

        private static void PushHandler(
            Func<Func<object, TypeResolutionArgs, Type>, Func<object, TypeResolutionArgs, Type>> binder)
        {
            Message.PushHandler(binder);
        }

        public static Type GetType(string typeName)
        {
            return GetType(typeName, false);
        }

        private static Type GetType(string typeName, bool ignoreCase)
        {
            return Send(new TypeResolutionArgs(typeName, true, ignoreCase));
        }

        private static Type Send(TypeResolutionArgs args)
        {
            Debug.Assert(args != null);
            return Message.Send(args);
        }
    }
}