using Avalonia.Markup.Xaml;

using ProcessManagerViewer.ViewModels.ThisAppViewModels;

using ReactiveUI;
using ReactiveUI.Avalonia;

namespace ProcessManagerViewer.Views.ThisAppViews;

public partial class ThisApplicationView : ReactiveUserControl<ThisApplicationViewModel> {
    public ThisApplicationView() {
        this.WhenActivated(d => {

        });
        AvaloniaXamlLoader.Load(this);
    }
}
