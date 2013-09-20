using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluentProtobufNet
{
    public class FluentMappingsContainer
    {
        readonly IList<Assembly> assemblies = new List<Assembly>();
        readonly List<Type> types = new List<Type>();

        public FluentMappingsContainer AddFromAssemblyOf<T>()
        {
            return AddFromAssembly(typeof(T).Assembly);
        }

        public FluentMappingsContainer AddFromAssembly(Assembly assembly)
        {
            assemblies.Add(assembly);
            WasUsed = true;
            return this;
        }

        public bool WasUsed { get; set; }

        internal void Apply(PersistenceModel model)
        {
            foreach (var assembly in assemblies)
            {
                model.AddMappingsFromAssembly(assembly);
            }
        }
    }
}