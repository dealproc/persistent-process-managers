using ReactiveUI.SourceGenerators;

namespace ProcessManagerViewer.ViewModels;

public partial class CommandProducerViewModel : ViewModelBase {
    [Reactive]
    private int _numberOfClicks = 0;

    [ReactiveCommand]
    private void OnButtonPressed() {
        NumberOfClicks++;
    }
}
