using Avalonia;

using Microsoft.Extensions.DependencyInjection;

using ProcessManagerViewer;

using ReactiveDomain;

using ReactiveUI.Avalonia;

var collection = new ServiceCollection();
collection.AddSingleton<IStreamStoreConnection>((_) => new ProcessManagerViewer.Storage.DataStore("desktop-store"));
collection.AddProcessManagerServices();

// configure desktop-specific services.

var services = collection.BuildServiceProvider();

AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .WithInterFont()
    .LogToTrace()
    .UseReactiveUI()
    .StartWithClassicDesktopLifetime(args);
