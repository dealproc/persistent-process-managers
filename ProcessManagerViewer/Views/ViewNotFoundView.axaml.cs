using Avalonia.Markup.Xaml;

using ReactiveUI;
using ReactiveUI.Avalonia;

namespace ProcessManagerViewer.Views;

public partial class ViewNotFoundView : ReactiveUserControl<ViewModels.ViewNotFoundViewModel> {
    public ViewNotFoundView() {
        this.WhenActivated(d => {

        });
        AvaloniaXamlLoader.Load(this);
    }
}
