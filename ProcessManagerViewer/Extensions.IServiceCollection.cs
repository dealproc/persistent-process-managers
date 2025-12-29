using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

using ProcessManagerViewer.Domains;

using ReactiveDomain;
using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging.Bus;

namespace ProcessManagerViewer;

public static class Extensions {
    private static bool _processManagerServicesHaveBeenAdded = false;

    public static IServiceCollection AddProcessManagerServices(this IServiceCollection services) {
        if (_processManagerServicesHaveBeenAdded) {
            return services;
        }

        IConfiguration Configuration = new ConfigurationBuilder()
          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
          .AddEnvironmentVariables()
          //.AddCommandLine(args)
          .Build();

        services.AddLogging((builder) => {
            builder.ClearProviders();

            builder.AddConfiguration(Configuration.GetSection("Logging"));

            builder.AddSimpleConsole();
            builder.AddDebug();
        });

        //services

        services.AddSingleton<ITimeSource, TimeSource>();
        services.AddSingleton((_) => TimeProvider.System);
        services.AddSingleton<IStreamNameBuilder, NamespacedStreamNameBuilder>();
        services.AddSingleton<IEventSerializer, JsonMessageSerializer>();
        services.AddSingleton<IConfiguredConnection>((provider) =>
            new ConfiguredConnectionForPm(
                provider.GetRequiredKeyedService<IPublisher>(Keys.ThisApp),
                provider.GetRequiredKeyedService<ICommandPublisher>(Keys.ThisApp),
                provider.GetRequiredService<ITimeSource>(),
                provider.GetRequiredService<IStreamStoreConnection>(),
                provider.GetRequiredService<IStreamNameBuilder>(),
                provider.GetRequiredService<IEventSerializer>()));
        services.AddSingleton((provider) => provider.GetRequiredService<IConfiguredConnection>().GetCorrelatedRepository(caching: true));

        services.AddKeyedSingleton<IDispatcher>(Keys.ThisApp, (provider, _) => new Dispatcher("Main app."));
        services.AddKeyedSingleton<ISubscriber>(Keys.ThisApp, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.ThisApp));
        services.AddKeyedSingleton<ICommandSubscriber>(Keys.ThisApp, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.ThisApp));
        services.AddKeyedSingleton<IPublisher>(Keys.ThisApp, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.ThisApp));
        services.AddKeyedSingleton<ICommandPublisher>(Keys.ThisApp, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.ThisApp));
        services.AddKeyedSingleton(Keys.ThisApp, (provider, _) => new Domains.ThisApp.ContactCollection(provider.GetRequiredService<IConfiguredConnection>()));
        services.AddReactiveDomainService<Domains.ThisApp.ContactService>();
        services.AddReactiveDomainService<Domains.ThisApp.AddContactService>();
        services.AddReactiveDomainService<Domains.ThisApp.UpdateContactDetailsService>();
        services.AddReactiveDomainService<Domains.ThisApp.ArchiveContactService>();
        services.AddKeyedSingleton<Domains.ThisApp.ContactLookup>(Keys.ThisApp);
        services.AddSingleton<ViewModels.ThisAppViewModels.IThisApplicationViewModelFactory, ViewModels.ThisAppViewModels.ThisApplicationViewModel.Factory>();
        services.AddSingleton<ViewModels.ThisAppViewModels.IContactEditorViewModelFactory, ViewModels.ThisAppViewModels.ContactEditorViewModel.Factory>();
        services.AddSingleton<ViewModels.ThisAppViewModels.IContactListViewModelFactory, ViewModels.ThisAppViewModels.ContactsListViewModel.Factory>();

        services.AddKeyedSingleton<IDispatcher>(Keys.Crm, (provider, _) => new Dispatcher($"{Keys.Crm} subsystem."));
        services.AddKeyedSingleton<ISubscriber>(Keys.Crm, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.Crm));
        services.AddKeyedSingleton<ICommandSubscriber>(Keys.Crm, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.Crm));
        services.AddKeyedSingleton<IPublisher>(Keys.Crm, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.Crm));
        services.AddKeyedSingleton<ICommandPublisher>(Keys.Crm, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.Crm));
        services.AddKeyedSingleton(Keys.Crm, (provider, _) => new Domains.CrmApp.ContactCollection(provider.GetRequiredService<IConfiguredConnection>()));
        services.AddReactiveDomainService<Domains.CrmApp.ContactService>();
        services.AddKeyedSingleton<Domains.CrmApp.ContactLookup>(Keys.Crm);
        services.AddSingleton<ViewModels.CrmAppViewModels.ICrmApplicationViewModelFactory, ViewModels.CrmAppViewModels.CrmApplicationViewModel.Factory>();

        services.AddKeyedSingleton<IDispatcher>(Keys.Erp, (provider, _) => new Dispatcher($"{Keys.Erp} subsystem."));
        services.AddKeyedSingleton<ISubscriber>(Keys.Erp, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.Erp));
        services.AddKeyedSingleton<ICommandSubscriber>(Keys.Erp, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.Erp));
        services.AddKeyedSingleton<IPublisher>(Keys.Erp, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.Erp));
        services.AddKeyedSingleton<ICommandPublisher>(Keys.Erp, (provider, _) => provider.GetRequiredKeyedService<IDispatcher>(Keys.Erp));
        services.AddKeyedSingleton(Keys.Erp, (provider, _) => new Domains.ErpApp.ContactCollection(provider.GetRequiredService<IConfiguredConnection>()));
        services.AddReactiveDomainService<Domains.ErpApp.ContactService>();
        services.AddKeyedSingleton<Domains.ErpApp.ContactLookup>(Keys.Erp);
        services.AddSingleton<ViewModels.ErpAppViewModels.IErpApplicationViewModelFactory, ViewModels.ErpAppViewModels.ErpApplicationViewModel.Factory>();

        services.Configure<HostOptions>(x => {
            x.ServicesStartConcurrently = true;
            x.ServicesStopConcurrently = true;
        });

        return services;
    }

    public static IServiceCollection AddReactiveDomainService<TImplementation>(this IServiceCollection services)
        where TImplementation : class, IReactiveDomainService {
        services.AddSingleton<TImplementation>();
        services.AddSingleton<IReactiveDomainService, TImplementation>(provider => provider.GetRequiredService<TImplementation>());
        return services;
    }

    public static IServiceProvider InitializeReactiveDomain(this IServiceProvider provider) {
        var autoStartServcies = provider.GetServices<IReactiveDomainService>();
        //up for discussion as to whether this should be started linearly, or in parallel.
        Parallel.ForEach(autoStartServcies, (service) => service.StartService());
        return provider;
    }
}
