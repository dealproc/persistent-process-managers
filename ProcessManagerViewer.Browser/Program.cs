using Avalonia;
using Avalonia.Browser;

using Microsoft.Extensions.DependencyInjection;

using ProcessManagerViewer;

using ReactiveDomain;

using ReactiveUI.Avalonia;

var collection = new ServiceCollection();
collection.AddSingleton<IStreamStoreConnection>((_) => {
    var store = new ProcessManagerViewer.Storage.DataStore("browser-store");
    store.Connect();
    return store;
});

await AppBuilder.Configure(() => new App(collection))
    .UseReactiveUI()
    .WithInterFont()
    .StartBrowserAppAsync("out");
