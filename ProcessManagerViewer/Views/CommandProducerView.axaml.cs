using Avalonia.Markup.Xaml;

using ReactiveUI;
using ReactiveUI.Avalonia;

namespace ProcessManagerViewer.Views;

public partial class CommandProducerView : ReactiveUserControl<ViewModels.CommandProducerViewModel> {
    public CommandProducerView() {
        this.WhenActivated(d => {

        });
        AvaloniaXamlLoader.Load(this);
    }
}
