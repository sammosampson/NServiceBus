namespace NServiceBus.Persistence.Development
{
    using System.IO;
    using Features;

    class DevelopmentSagaPersistence : Feature
    {
        public DevelopmentSagaPersistence()
        {
            DependsOn<Sagas>();
        }

        protected internal override void Setup(FeatureConfigurationContext context)
        {
            var path = Path.Combine(@"c:\persistence", "sagas");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            context.Container.ConfigureComponent(b => new DevelopmentSagaPersister(path), DependencyLifecycle.SingleInstance);
        }
    }
}