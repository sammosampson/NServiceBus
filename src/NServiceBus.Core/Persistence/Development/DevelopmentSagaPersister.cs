namespace NServiceBus.Persistence.Development
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Threading.Tasks;
    using Extensibility;
    using Sagas;

    class DevelopmentSagaPersister : ISagaPersister
    {
        public DevelopmentSagaPersister(string basePath)
        {
            this.basePath = basePath;
        }

        public Task Save(IContainSagaData sagaData, SagaCorrelationProperty correlationProperty, SynchronizedStorageSession session, ContextBag context)
        {
            var correlationId = correlationProperty.Value.ToString();

            var sagaType = sagaData.GetType();
            var serializer = new DataContractJsonSerializer(sagaType);
            var path = GetInstancePath(sagaType, correlationId);


            //todo: need to be persisted
            sagaIdLookup.TryAdd(sagaData.Id, correlationId);

            using (var fs = new FileStream(path, FileMode.CreateNew))
            {
                serializer.WriteObject(fs, sagaData);
            }

            return TaskEx.CompletedTask;
        }

        public Task Update(IContainSagaData sagaData, SynchronizedStorageSession session, ContextBag context)
        {
            string correlationId;

            if (!sagaIdLookup.TryGetValue(sagaData.Id, out correlationId))
            {
                throw new Exception("$Could not lookup correlation id for {sagaData.Id}");
            }

            var sagaType = sagaData.GetType();
            var serializer = new DataContractJsonSerializer(sagaType);
            var path = GetInstancePath(sagaType, correlationId);

            using (var fs = new FileStream(path, FileMode.Truncate))
            {
                serializer.WriteObject(fs, sagaData);
            }

            return TaskEx.CompletedTask;
        }

        public Task<TSagaData> Get<TSagaData>(Guid sagaId, SynchronizedStorageSession session, ContextBag context) where TSagaData : IContainSagaData
        {
            string correlationId;

            if (!sagaIdLookup.TryGetValue(sagaId, out correlationId))
            {
                return Task.FromResult(default(TSagaData));
            }

            return Read<TSagaData>(correlationId);
        }

        public Task<TSagaData> Get<TSagaData>(string propertyName, object propertyValue, SynchronizedStorageSession session, ContextBag context) where TSagaData : IContainSagaData
        {
            var correlationId = propertyValue.ToString();

            return Read<TSagaData>(correlationId);
        }

        public Task Complete(IContainSagaData sagaData, SynchronizedStorageSession session, ContextBag context)
        {
            string correlationId;

            if (!sagaIdLookup.TryGetValue(sagaData.Id, out correlationId))
            {
                return TaskEx.CompletedTask;
            }
            var path = GetInstancePath(sagaData.GetType(), correlationId);

            File.Delete(path);

            return TaskEx.CompletedTask;
        }

        Task<TSagaData> Read<TSagaData>(string correlationId) where TSagaData : IContainSagaData
        {
            var sagaType = typeof(TSagaData);
            var serializer = new DataContractJsonSerializer(sagaType);
            var path = GetInstancePath(sagaType, correlationId);

            if (!File.Exists(path))
            {
                return Task.FromResult(default(TSagaData));
            }

            using (var fs = new FileStream(path, FileMode.Open))
            {
                return Task.FromResult((TSagaData) serializer.ReadObject(fs));
            }
        }

        string GetInstancePath(Type sagaType, string correlationId)
        {
            //todo: folder per sagatype?
            return Path.Combine(basePath, sagaType.FullName + correlationId + ".json");
        }

        string basePath;
        ConcurrentDictionary<Guid, string> sagaIdLookup = new ConcurrentDictionary<Guid, string>();
    }
}