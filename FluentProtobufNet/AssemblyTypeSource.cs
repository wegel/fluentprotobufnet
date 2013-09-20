using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluentProtobufNet
{
    public class AssemblyTypeSource : ITypeSource
    {
        readonly Assembly source;

        public AssemblyTypeSource(Assembly source)
        {
            if (source == null) throw new ArgumentNullException("source");

            this.source = source;
        }

        #region ITypeSource Members

        public IEnumerable<Type> GetTypes()
        {
            return source.GetTypes().OrderBy(x => x.FullName);
        }

        public void LogSource(IDiagnosticLogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");

            logger.LoadedFluentMappingsFromSource(this);
        }

        public string GetIdentifier()
        {
            return source.GetName().FullName;
        }

        #endregion

        public override int GetHashCode()
        {
            return source.GetHashCode();
        }
    }

    public interface ITypeSource
    {
        IEnumerable<Type> GetTypes();
        //void LogSource(IDiagnosticLogger logger);
        string GetIdentifier();
    }

    public interface IDiagnosticLogger
    {
        void Flush();
        void FluentMappingDiscovered(Type type);
        void ConventionDiscovered(Type type);
        void LoadedFluentMappingsFromSource(ITypeSource source);
        void LoadedConventionsFromSource(ITypeSource source);
        void AutomappingSkippedType(Type type, string reason);
        void AutomappingCandidateTypes(IEnumerable<Type> types);
        void BeginAutomappingType(Type type);
    }
}