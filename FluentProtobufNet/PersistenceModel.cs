using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentProtobufNet.Helpers;
using ProtoBuf.Meta;

namespace FluentProtobufNet
{
    public class PersistenceModel
    {
        protected readonly IList<IMappingProvider> classProviders = new List<IMappingProvider>();
        protected IDiagnosticLogger log = new NullDiagnosticsLogger();
        private RuntimeTypeModel _protobufModel;
        private IList<IMappingProvider> subclassProviders = new List<IMappingProvider>();

        public void AddMappingsFromAssembly(Assembly assembly)
        {
            AddMappingsFromSource(new AssemblyTypeSource(assembly));
        }

        public void AddMappingsFromSource(ITypeSource source)
        {
            source.GetTypes()
                .Where(x => IsMappingOf<IMappingProvider>(x))
                .Each(Add);

            log.LoadedFluentMappingsFromSource(source);
        }

        private bool IsMappingOf<T>(Type type)
        {
            return !type.IsGenericType && typeof(T).IsAssignableFrom(type);
        }

        public void Add(IMappingProvider provider)
        {
            classProviders.Add(provider);
        }

        public void AddSubclassMap(IMappingProvider provider)
        {
            subclassProviders.Add(provider);
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
                        log.FluentMappingDiscovered(type);
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
            _protobufModel = ProtoBuf.Meta.TypeModel.Create();
            foreach (var classMap in classProviders)
                classMap.GetRuntimeTypeModel(_protobufModel);

            var subclassProvidersCopy = subclassProviders.ToList();
            IMappingProvider subclassMap = null;
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

    public class MissingConstructorException : Exception
    {
        public Type Type { get; set; }

        public MissingConstructorException(Type type)
        {
            Type = type;
        }
    }

    public class ReflectHelper
    {
        public const BindingFlags AnyVisibilityInstance = BindingFlags.Instance | BindingFlags.Public |
                                                           BindingFlags.NonPublic;
        private static readonly System.Type[] NoClasses = System.Type.EmptyTypes;

        public static ConstructorInfo GetDefaultConstructor(System.Type type)
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

    public interface IMappingProvider
    {
        RuntimeTypeModel GetRuntimeTypeModel(RuntimeTypeModel protobufModel);
        // HACK: In place just to keep compatibility until verdict is made
        //HibernateMapping GetHibernateMapping();
        //IEnumerable<Member> GetIgnoredProperties();
        bool CanBeResolvedUsing(RuntimeTypeModel protobufModel);
    }
}