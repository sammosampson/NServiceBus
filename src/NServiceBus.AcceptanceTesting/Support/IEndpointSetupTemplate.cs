﻿namespace NServiceBus.AcceptanceTesting.Support
{
    using System;
    using Config.ConfigurationSource;

    public interface IEndpointSetupTemplate
    {
        Configure GetConfiguration(RunDescriptor runDescriptor, EndpointConfiguration endpointConfiguration, IConfigurationSource configSource, Action<ConfigurationBuilder> configurationBuilderCustomization);
    }
}