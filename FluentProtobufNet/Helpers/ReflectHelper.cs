using System;
using System.Reflection;

namespace FluentProtobufNet.Helpers
{
    public class ReflectHelper
    {
        public const BindingFlags AnyVisibilityInstance = BindingFlags.Instance | BindingFlags.Public |
                                                          BindingFlags.NonPublic;
        private static readonly Type[] NoClasses = Type.EmptyTypes;

        public static ConstructorInfo GetDefaultConstructor(Type type)
        {
            if (IsAbstractClass(type))
                return null;

            try
            {
                ConstructorInfo constructor =
                    type.GetConstructor(AnyVisibilityInstance, null, CallingConventions.HasThis, NoClasses, null);
                return constructor;
            }
            catch (Exception e)
            {
                throw new InstantiationException("A default (no-arg) constructor could not be found for: ", e, type);
            }
        }

        public static bool IsAbstractClass(System.Type type)
        {
            return (type.IsAbstract || type.IsInterface);
        }
    }
}