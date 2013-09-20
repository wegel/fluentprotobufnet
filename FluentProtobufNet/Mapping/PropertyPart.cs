using System;

namespace FluentProtobufNet.Mapping
{
    public class PropertyPart
    {
        public Member Member { get; set; }
        public Type Type { get; set; }

        public PropertyPart(Member member, Type type)
        {
            Member = member;
            Type = type;
        }
    }
}
