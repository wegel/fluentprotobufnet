using System;
using System.Collections.Generic;

namespace FluentProtobufNet
{
    public class FluentConfiguration
    {
        private readonly Configuration _cfg;
        private IDiagnosticLogger _logger;
        readonly List<Action<MappingConfiguration>> mappingsBuilders = new List<Action<MappingConfiguration>>();
        private bool _mappingsSet;

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
            mappingsBuilders.Add(mappings);
            _mappingsSet = true;
            return this;
        }


        public Configuration BuildConfiguration()
        {
            var mappingCfg = new MappingConfiguration(_logger);

            foreach (var builder in mappingsBuilders)
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