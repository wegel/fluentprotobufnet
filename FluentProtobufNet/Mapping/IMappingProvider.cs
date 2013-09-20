using ProtoBuf.Meta;

namespace FluentProtobufNet
{
    public interface IMappingProvider
    {
        RuntimeTypeModel GetRuntimeTypeModel(RuntimeTypeModel protobufModel);
        // HACK: In place just to keep compatibility until verdict is made
        //HibernateMapping GetHibernateMapping();
        //IEnumerable<Member> GetIgnoredProperties();
        bool CanBeResolvedUsing(RuntimeTypeModel protobufModel);
    }
}