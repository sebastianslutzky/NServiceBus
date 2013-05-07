﻿namespace NServiceBus.Features
{
    using AutomaticSubscriptions;
    using Config;
    using Settings;
    using Transports;
    using Transports.RabbitMQ;
    using Transports.RabbitMQ.Config;
    using Transports.RabbitMQ.Routing;
    using Unicast.Queuing.Installers;
    using Unicast.Subscriptions;

    public class RabbitMqTransport : ConfigureTransport<RabbitMQ>, IFeature
    {
        public void Initialize()
        {
            var connectionString = SettingsHolder.Get<string>("NServiceBus.Transport.ConnectionString");
            var connectionConfiguration = new ConnectionStringParser().Parse(connectionString);

            NServiceBus.Configure.Instance.Configurer.RegisterSingleton<IConnectionConfiguration>(connectionConfiguration);

            NServiceBus.Configure.Component<RabbitMqDequeueStrategy>(DependencyLifecycle.InstancePerCall)
                 .ConfigureProperty(p => p.PurgeOnStartup, ConfigurePurging.PurgeRequested)
                 .ConfigureProperty(p => p.PrefetchCount, connectionConfiguration.PrefetchCount);

            NServiceBus.Configure.Component<RabbitMqUnitOfWork>(DependencyLifecycle.InstancePerCall)
                  .ConfigureProperty(p => p.UsePublisherConfirms, connectionConfiguration.UsePublisherConfirms)
                  .ConfigureProperty(p => p.MaxWaitTimeForConfirms, connectionConfiguration.MaxWaitTimeForConfirms);


            NServiceBus.Configure.Component<RabbitMqMessageSender>(DependencyLifecycle.InstancePerCall);

            NServiceBus.Configure.Component<RabbitMqMessagePublisher>(DependencyLifecycle.InstancePerCall);

            NServiceBus.Configure.Component<RabbitMqSubscriptionManager>(DependencyLifecycle.SingleInstance)
             .ConfigureProperty(p => p.EndpointQueueName, Address.Local.Queue);

            NServiceBus.Configure.Component<RabbitMqQueueCreator>(DependencyLifecycle.InstancePerCall);

            InfrastructureServices.Enable<IRoutingTopology>();
            InfrastructureServices.Enable<IManageRabbitMqConnections>();

            EndpointInputQueueCreator.Enabled = true;
        }


        protected override void InternalConfigure(Configure config, string connectionString)
        {
            Feature.Enable<RabbitMqTransport>();
        }

        protected override string ExampleConnectionStringForErrorMessage
        {
            get { return "host=localhost"; }
        }
    }
}