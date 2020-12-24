using System;
using System.Diagnostics;

namespace ElmahCore
{

    internal class TypeResolutionArgs
    {
        public string TypeName { get; }
        public bool ThrowOnError { get; }
        public bool IgnoreCase { get; }

        public TypeResolutionArgs(string typeName, bool throwOnError, bool ignoreCase)
        {
            TypeName = typeName;
            ThrowOnError = throwOnError;
            IgnoreCase = ignoreCase;
        }
    }

    internal static class TypeResolution
    {
        static readonly Message<TypeResolutionArgs, Type> Message = new Message<TypeResolutionArgs, Type>();

        private static void PushHandler(
            Func<Func<object, TypeResolutionArgs, Type>, Func<object, TypeResolutionArgs, Type>> binder)
        {
            Message.PushHandler(binder);
        }
        
        static TypeResolution()
        {
            PushHandler(next => (sender, input) => Type.GetType(input.TypeName, input.ThrowOnError, input.IgnoreCase));
        }

	    public static Type GetType(string typeName)
        {
            return GetType(typeName, false);
        }

        private static Type GetType(string typeName, bool ignoreCase)
        {
            return Send(new TypeResolutionArgs(typeName, true, ignoreCase));
        }

        static Type Send(TypeResolutionArgs args)
        {
            Debug.Assert(args != null);
            return Message.Send(args);
        }
    }
}
