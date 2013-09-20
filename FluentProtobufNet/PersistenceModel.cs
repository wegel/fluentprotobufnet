using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentProtobufNet.Helpers;
using FluentProtobufNet.Mapping;
using ProtoBuf.Meta;

namespace FluentProtobufNet
{
    public class PersistenceModel
    {
        protected readonly IList<IMappingProvider> ClassProviders = new List<IMappingProvider>();
        protected IDiagnosticLogger Log = new NullDiagnosticsLogger();
        private RuntimeTypeModel _protobufModel;
        private readonly IList<IMappingProvider> _subclassProviders = new List<IMappingProvider>();

        public void AddMappingsFromAssembly(Assembly assembly)
        {
            AddMappingsFromSource(new AssemblyTypeSource(assembly));
        }

        public void AddMappingsFromSource(ITypeSource source)
        {
            source.GetTypes()
                .Where(IsMappingOf<IMappingProvider>)
                .Each(Add);

            Log.LoadedFluentMappingsFromSource(source);
        }

        private static bool IsMappingOf<T>(Type type)
        {
            return !type.IsGenericType && typeof(T).IsAssignableFrom(type);
        }

        public void Add(IMappingProvider provider)
        {
            ClassProviders.Add(provider);
        }

        public void AddSubclassMap(IMappingProvider provider)
        {
            _subclassProviders.Add(provider);
        }

        public void Add(Type type)
        {
            var mapping = type.InstantiateUsingParameterlessConstructor();

            if (mapping is IMappingProvider)
            {
                if (mapping.GetType().BaseType != null && mapping.GetType().BaseType.IsGenericType)
                {
                    if (mapping.GetType().BaseType.GetGenericTypeDefinition() == typeof (ClassMap<>))
                    {
                        Log.FluentMappingDiscovered(type);
                        Add((IMappingProvider)mapping);
                    }
                    else if (mapping.GetType().BaseType.GetGenericTypeDefinition() == typeof (SubclassMap<>))
                    {
                        AddSubclassMap((IMappingProvider)mapping);
                    }
                }
            }
            else
                throw new InvalidOperationException("Unsupported mapping type '" + type.FullName + "'");
        }

        public virtual void Configure(Configuration cfg)
        {
            _protobufModel = TypeModel.Create();
            foreach (var classMap in ClassProviders)
                classMap.GetRuntimeTypeModel(_protobufModel);

            var subclassProvidersCopy = _subclassProviders.ToList();
            IMappingProvider subclassMap;
            while ((subclassMap = subclassProvidersCopy.FirstOrDefault(sc => sc.CanBeResolvedUsing(_protobufModel))) != null)
            {
                subclassMap.GetRuntimeTypeModel(_protobufModel);
                subclassProvidersCopy.Remove(subclassMap);
            }

            if(subclassProvidersCopy.Any())
                throw new Exception("Couldn't resolve all subclassed");

            cfg.RuntimeTypeModel = _protobufModel;
        }
    }
}