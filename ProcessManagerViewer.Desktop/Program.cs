using Avalonia;

using Microsoft.Extensions.DependencyInjection;

using ProcessManagerViewer;

using ReactiveDomain;

using ReactiveUI.Avalonia;

var collection = new ServiceCollection();
collection.AddSingleton<IStreamStoreConnection>((_) => {
    var store = new ProcessManagerViewer.Storage.DataStore("desktop-store");
    store.Connect();
    return store;
});

AppBuilder.Configure(() => new App(collection))
    .UsePlatformDetect()
    .WithInterFont()
    //.LogToTrace()
    .UseReactiveUI()
    .StartWithClassicDesktopLifetime(args);
