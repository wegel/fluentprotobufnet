using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentProtobufNet.Helpers;
using FluentProtobufNet.Mapping;
using ProtoBuf.Meta;

namespace FluentProtobufNet
{

    public class ClassMap<T> : IMappingProvider
    {
        public IList<NameAndFieldNumber> Fields { get; set; }

        public ClassMap()
        {
            Fields = new List<NameAndFieldNumber>();
        }


        internal Type EntityType
        {
            get { return typeof(T); }
        }

        public NameAndFieldNumber Map(Expression<Func<T, object>> memberExpression, int fieldNumber)
        {
            return Map(memberExpression.ToMember(), fieldNumber);
        }

        NameAndFieldNumber Map(Member member, int fieldNumber)
        {
            ///­OnMemberMapped(member);

            var field = new NameAndFieldNumber
                        {
                            Member = member,
                            FieldNumber = fieldNumber,
                            AsReference = false,
                            Type = typeof (T)
                        };
            Fields.Add(field);

            return field;
        }


        public NameAndFieldNumber References<TOther>(Expression<Func<T, TOther>> memberExpression, int fieldNumber)
        {
            return References<TOther>(memberExpression.ToMember(), fieldNumber);
        }

        public NameAndFieldNumber References<TOther>(Expression<Func<T, object>> memberExpression, int fieldNumber)
        {
            return References<TOther>(memberExpression.ToMember(), fieldNumber);
        }

        NameAndFieldNumber References<TOther>(Member member, int fieldNumber)
        {
            //OnMemberMapped(member);

            var field = new NameAndFieldNumber
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

    public class NameAndFieldNumber
    {
        public Member Member { get; set; }
        public int FieldNumber { get; set;  }
        public bool AsReference { get; set; }
        public Type Type { get; set; }
    }

}