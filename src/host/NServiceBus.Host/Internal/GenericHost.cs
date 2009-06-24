using System;
using Common.Logging;
using NServiceBus.ObjectBuilder;
using System.Linq;

namespace NServiceBus.Host.Internal
{
    public class GenericHost : MarshalByRefObject
    {
        private IMessageEndpoint messageEndpoint;
        private readonly Type endpointType;
        private readonly IMessageEndpointConfiguration messageEndpointConfiguration;

        public GenericHost(Type endpointType)
        {
            this.endpointType = endpointType;

            messageEndpointConfiguration = GetEndpointConfiguration();
        }

        public void Start()
        {
            Logger.Debug("Starting host for " + endpointType.Name);
            var config = messageEndpointConfiguration.Configure();

            //register the endpoint so that the user can get DI for the endpoint itself
            config.Configurer.ConfigureComponent(endpointType, ComponentCallModelEnum.Singleton);

            //build the endpoint
            messageEndpoint = config.Builder.Build<IMessageEndpoint>();

            messageEndpoint.OnStart();
        }

        public void Stop()
        {
            messageEndpoint.OnStop();
        }

        private IMessageEndpointConfiguration GetEndpointConfiguration()
        {
            var endpointConfigurationType = endpointType.Assembly.GetTypes()
                .Where(t => typeof(IMessageEndpointConfiguration).IsAssignableFrom(t)).FirstOrDefault();

            if (endpointConfigurationType == null)
                throw new InvalidOperationException("No endpoint configuration found in assembly " +
                                                    endpointType.Assembly);
            return Activator.CreateInstance(endpointConfigurationType) as IMessageEndpointConfiguration;
        }

        private static readonly ILog Logger = LogManager.GetLogger(typeof(GenericHost));
    }
}