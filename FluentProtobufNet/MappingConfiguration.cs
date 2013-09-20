
namespace FluentProtobufNet
{
    public class MappingConfiguration
    {
        readonly PersistenceModel _model;

        public MappingConfiguration(IDiagnosticLogger logger)
        {
            _model = new PersistenceModel();
            FluentMappings = new FluentMappingsContainer();
        }

        public FluentMappingsContainer FluentMappings { get; set; }

        public void Apply(Configuration cfg)
        {
            FluentMappings.Apply(_model);

            _model.Configure(cfg);

        }
    }
}