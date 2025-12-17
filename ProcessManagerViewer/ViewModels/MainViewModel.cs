using ReactiveUI.SourceGenerators;

namespace ProcessManagerViewer.ViewModels;

public partial class MainViewModel : ViewModelBase {
    [Reactive]
    private string _greeting = "Welcome to Avalonia!";

    [Reactive]
    private CommandProducerViewModel _commandProducer = new();
}
