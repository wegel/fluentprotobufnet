using System;
using System.Linq;
using FluentProtobufNet.Exceptions;
using ProtoBuf.Meta;

namespace FluentProtobufNet.Mapping
{
    public class SubclassMap<T>: ClassMap<T>
    {
        private int _subclassFieldId;
        private bool _fieldIdSet;

        public void SubclassFieldId(int fieldNumber)
        {
            ///­OnMemberMapped(member);

            _subclassFieldId = fieldNumber;
            _fieldIdSet = true;
        }

        public override RuntimeTypeModel GetRuntimeTypeModel(RuntimeTypeModel protobufModel)
        {
            if (!_fieldIdSet)
                throw new SubclassFieldIdNotSetException("Field ID of subclass " + typeof (T).Name + " not set");
            base.GetRuntimeTypeModel(protobufModel);

            var types = protobufModel.GetTypes().Cast<MetaType>();
            var baseType =
                types.SingleOrDefault(t => t.Type == typeof(T).BaseType);

            if (baseType.GetSubtypes().Any(s => s.FieldNumber == _subclassFieldId))
                throw new FieldIdAlreadyUsedException(_subclassFieldId,
                    baseType.GetSubtypes().First(s => s.FieldNumber == _subclassFieldId).DerivedType);

            baseType.AddSubType(_subclassFieldId, typeof (T));

            return protobufModel;
        }

        public override bool CanBeResolvedUsing(RuntimeTypeModel protobufModel)
        {
            var types = protobufModel.GetTypes().Cast<MetaType>();
            var baseType =
                types.SingleOrDefault(t => t.Type == typeof(T).BaseType);

            return baseType != null;
        }

        public class SubclassFieldIdNotSetException : Exception
        {
            public SubclassFieldIdNotSetException(string message) : base(message)
            {
                
            }
        }
    }
}