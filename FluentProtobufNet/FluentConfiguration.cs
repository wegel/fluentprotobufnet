using System;
using System.Collections.Generic;

namespace FluentProtobufNet
{
    public class FluentConfiguration
    {
        private readonly Configuration _cfg;
        private readonly IDiagnosticLogger _logger;
        readonly List<Action<MappingConfiguration>> _mappingsBuilders = new List<Action<MappingConfiguration>>();

        internal FluentConfiguration()
            : this(new Configuration())
        { }

        public FluentConfiguration(Configuration cfg)
        {
            _cfg = cfg;
            _logger = new NullDiagnosticsLogger();
        }

        public FluentConfiguration Mappings(Action<MappingConfiguration> mappings)
        {
            _mappingsBuilders.Add(mappings);
            return this;
        }


        public Configuration BuildConfiguration()
        {
            var mappingCfg = new MappingConfiguration(_logger);

            foreach (var builder in _mappingsBuilders)
                builder(mappingCfg);

            mappingCfg.Apply(Configuration);

            return Configuration;
        }

        internal Configuration Configuration
        {
            get { return _cfg; }
        }
    }
}