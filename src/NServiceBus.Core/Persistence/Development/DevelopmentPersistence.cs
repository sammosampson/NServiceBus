namespace NServiceBus
{
    using Features;
    using Persistence;
    using Persistence.Development;

    /// <summary>
    ///
    /// </summary>
    public class DevelopmentPersistence: PersistenceDefinition
    {
        internal DevelopmentPersistence()
        {
            Supports<StorageType.Sagas>(s => s.EnableFeatureByDefault<DevelopmentSagaPersistence>());
        }
    }
}