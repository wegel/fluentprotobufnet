using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentProtobufNet.Helpers;
using ProtoBuf.Meta;

namespace FluentProtobufNet.Mapping
{

    public class ClassMap<T> : IMappingProvider
    {
        public IList<PropertyMapping> Fields { get; set; }

        public ClassMap()
        {
            Fields = new List<PropertyMapping>();
        }


        internal Type EntityType
        {
            get { return typeof(T); }
        }

        public PropertyMapping Map(Expression<Func<T, object>> memberExpression, int fieldNumber)
        {
            return Map(memberExpression.ToMember(), fieldNumber);
        }

        PropertyMapping Map(Member member, int fieldNumber)
        {
            //­OnMemberMapped(member);

            var field = new PropertyMapping
                        {
                            Member = member,
                            FieldNumber = fieldNumber,
                            AsReference = false,
                            Type = typeof (T)
                        };
            Fields.Add(field);

            return field;
        }


        public PropertyMapping References<TOther>(Expression<Func<T, TOther>> memberExpression, int fieldNumber)
        {
            return References<TOther>(memberExpression.ToMember(), fieldNumber);
        }

        public PropertyMapping References<TOther>(Expression<Func<T, object>> memberExpression, int fieldNumber)
        {
            return References<TOther>(memberExpression.ToMember(), fieldNumber);
        }

        PropertyMapping References<TOther>(Member member, int fieldNumber)
        {
            //OnMemberMapped(member);

            var field = new PropertyMapping
            {
                Member = member,
                FieldNumber = fieldNumber,
                AsReference = true,
                Type = typeof(T)
            };
            Fields.Add(field);

            return field;
        }

        public virtual RuntimeTypeModel GetRuntimeTypeModel(RuntimeTypeModel protobufModel)
        {
            var protoType = protobufModel.Add(typeof(T), false);
            foreach (var f in Fields)
            {
                protoType.Add(f.FieldNumber, f.Member.Name);
                protoType.GetFields().Single(newField => newField.FieldNumber == f.FieldNumber).AsReference = f.AsReference;
            }

            return protobufModel;
        }

        public virtual bool CanBeResolvedUsing(RuntimeTypeModel protobufModel)
        {
            return true;
        }
    }

    public class PropertyMapping
    {
        public Member Member { get; set; }
        public int FieldNumber { get; set;  }
        public bool AsReference { get; set; }
        public Type Type { get; set; }
    }

}