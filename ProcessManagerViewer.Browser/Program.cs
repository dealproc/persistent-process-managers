using Avalonia;
using Avalonia.Browser;

using ProcessManagerViewer;

using ReactiveUI.Avalonia;

await AppBuilder.Configure<App>()
    .UseReactiveUI()
    .WithInterFont()
    .StartBrowserAppAsync("out");
