using System;
using ProtoBuf.Meta;

namespace FluentProtobufNet.Exceptions
{
    public class FieldIdAlreadyUsedException : Exception
    {
        public FieldIdAlreadyUsedException(int fieldId, MetaType usedBy): base("The field ID " + fieldId + " has already been used by " + usedBy.Name)
        {
        }
    }
}