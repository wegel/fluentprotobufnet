using System;

namespace FluentProtobufNet.Exceptions
{
    public class MissingConstructorException : Exception
    {
        public Type Type { get; set; }

        public MissingConstructorException(Type type)
        {
            Type = type;
        }
    }
}