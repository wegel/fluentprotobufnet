using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluentProtobufNet
{
    public class AssemblyTypeSource : ITypeSource
    {
        readonly Assembly _source;

        public AssemblyTypeSource(Assembly source)
        {
            if (source == null) throw new ArgumentNullException("source");

            _source = source;
        }

        #region ITypeSource Members

        public IEnumerable<Type> GetTypes()
        {
            return _source.GetTypes().OrderBy(x => x.FullName);
        }

        public void LogSource(IDiagnosticLogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");

            logger.LoadedFluentMappingsFromSource(this);
        }

        public string GetIdentifier()
        {
            return _source.GetName().FullName;
        }

        #endregion

        public override int GetHashCode()
        {
            return _source.GetHashCode();
        }
    }

    public interface ITypeSource
    {
        IEnumerable<Type> GetTypes();
        //void LogSource(IDiagnosticLogger logger);
        string GetIdentifier();
    }
}