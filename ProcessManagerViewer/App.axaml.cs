using System;
using System.Linq;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using Microsoft.Extensions.DependencyInjection;

using ProcessManagerViewer.ViewModels;
using ProcessManagerViewer.Views;

namespace ProcessManagerViewer;

public partial class App : Application {
    private readonly IServiceProvider _services;

    public App(IServiceCollection services) {
        services.AddProcessManagerServices();
        _services = services.BuildServiceProvider();
        _services.InitializeReactiveDomain();
    }

    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
        var vm = new MainViewModel(
            _services.GetRequiredService<ViewModels.ThisAppViewModels.IThisApplicationViewModelFactory>(),
            _services.GetRequiredService<ViewModels.CrmAppViewModels.ICrmApplicationViewModelFactory>(),
            _services.GetRequiredService<ViewModels.ErpAppViewModels.IErpApplicationViewModelFactory>()
        );

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow {
                DataContext = vm
            };
        } else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
            singleViewPlatform.MainView = new MainView {
                DataContext = vm
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation() {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove) {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
