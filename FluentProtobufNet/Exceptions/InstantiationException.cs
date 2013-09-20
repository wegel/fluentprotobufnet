using System;

namespace FluentProtobufNet
{
    public class InstantiationException : Exception
    {
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public Type Type { get; set; }

        public InstantiationException(string message, Exception exception, Type type)
        {
            Message = message;
            Exception = exception;
            Type = type;
        }
    }
}