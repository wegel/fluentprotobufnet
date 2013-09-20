using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluentProtobufNet
{
    public class FluentMappingsContainer
    {
        readonly IList<Assembly> _assemblies = new List<Assembly>();
        readonly List<Type> _types = new List<Type>();

        /// <summary>
        /// Add all fluent mappings in the assembly that contains T.
        /// </summary>
        /// <typeparam name="T">Type from the assembly</typeparam>
        /// <returns>Fluent mappings configuration</returns>
        public FluentMappingsContainer AddFromAssemblyOf<T>()
        {
            return AddFromAssembly(typeof(T).Assembly);
        }

        /// <summary>
        /// Add all fluent mappings in the assembly
        /// </summary>
        /// <param name="assembly">Assembly to add mappings from</param>
        /// <returns>Fluent mappings configuration</returns>
        public FluentMappingsContainer AddFromAssembly(Assembly assembly)
        {
            _assemblies.Add(assembly);
            WasUsed = true;
            return this;
        }

        /// <summary>
        /// Adds a single <see cref="IMappingProvider" /> represented by the specified type.
        /// </summary>
        /// <returns>Fluent mappings configuration</returns>
        public FluentMappingsContainer Add<T>()
        {
            return Add(typeof(T));
        }

        /// <summary>
        /// Adds a single <see cref="IMappingProvider" /> represented by the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Fluent mappings configuration</returns>
        public FluentMappingsContainer Add(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            _types.Add(type);
            WasUsed = true;
            return this;
        }

        public bool WasUsed { get; set; }

        internal void Apply(PersistenceModel model)
        {
            foreach (var assembly in _assemblies)
            {
                model.AddMappingsFromAssembly(assembly);
            }

            foreach (var type in _types)
            {
                model.Add(type);
            }
        }
    }
}