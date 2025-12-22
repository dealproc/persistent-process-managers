using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ProcessManagerViewer.Domains;

using ReactiveDomain;
using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging.Bus;

namespace ProcessManagerViewer;

public static class Extensions {
    public static IServiceCollection AddProcessManagerServices(this IServiceCollection services) {
        services.AddSingleton<ITimeSource, TimeSource>();
        services.AddSingleton<IStreamNameBuilder, NamespacedStreamNameBuilder>();
        services.AddSingleton<IEventSerializer, JsonMessageSerializer>();
        services.AddSingleton<IConfiguredConnection>((provider) =>
            new ConfiguredConnectionForPm(
                provider.GetRequiredKeyedService<IPublisher>(Keys.Erp),
                provider.GetRequiredKeyedService<ICommandPublisher>(Keys.Erp),
                provider.GetRequiredService<ITimeSource>(),
                provider.GetRequiredService<IStreamStoreConnection>(),
                provider.GetRequiredService<IStreamNameBuilder>(),
                provider.GetRequiredService<IEventSerializer>()));

        services.AddKeyedSingleton<IDispatcher>(Keys.ThisApp, (provider, _) => new Dispatcher("Main app."));
        services.AddKeyedSingleton<ISubscriber>(Keys.ThisApp, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.ThisApp));
        services.AddKeyedSingleton<ICommandSubscriber>(Keys.ThisApp, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.ThisApp));
        services.AddKeyedSingleton<IPublisher>(Keys.ThisApp, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.ThisApp));
        services.AddKeyedSingleton<ICommandPublisher>(Keys.ThisApp, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.ThisApp));
        services.AddHostedService<Domains.ThisApp.ContactService>();
        services.AddHostedService<Domains.ThisApp.AddContactService>();
        services.AddHostedService<Domains.ThisApp.UpdateContactDetailsService>();
        services.AddHostedService<Domains.ThisApp.ArchiveContactService>();
        services.AddSingleton<Domains.ThisApp.ContactLookup>();

        services.AddKeyedSingleton<IDispatcher>(Keys.Crm, (provider, _) => new Dispatcher($"{Keys.Crm} subsystem."));
        services.AddKeyedSingleton<ISubscriber>(Keys.Crm, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.Crm));
        services.AddKeyedSingleton<ICommandSubscriber>(Keys.Crm, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.Crm));
        services.AddKeyedSingleton<IPublisher>(Keys.Crm, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.Crm));
        services.AddKeyedSingleton<ICommandPublisher>(Keys.Crm, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.Crm));
        services.AddHostedService<Domains.CrmApp.ContactService>();
        services.AddSingleton<Domains.CrmApp.ContactLookup>();

        services.AddKeyedSingleton<IDispatcher>(Keys.Erp, (provider, _) => new Dispatcher($"{Keys.Erp} subsystem."));
        services.AddKeyedSingleton<ISubscriber>(Keys.Erp, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.Erp));
        services.AddKeyedSingleton<ICommandSubscriber>(Keys.Erp, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.Erp));
        services.AddKeyedSingleton<IPublisher>(Keys.Erp, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.Erp));
        services.AddKeyedSingleton<ICommandPublisher>(Keys.Erp, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.Erp));
        services.AddHostedService<Domains.ErpApp.ContactService>();
        services.AddSingleton<Domains.ErpApp.ContactLookup>();

        services.Configure<HostOptions>(x => {
            x.ServicesStartConcurrently = true;
            x.ServicesStopConcurrently = true;
        });

        return services;
    }
}
