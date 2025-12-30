using Avalonia.Markup.Xaml;

using ProcessManagerViewer.ViewModels.ErpAppViewModels;

using ReactiveUI;
using ReactiveUI.Avalonia;

namespace ProcessManagerViewer.Views.ErpAppViews;

public partial class ErpApplicationView : ReactiveUserControl<ErpApplicationViewModel>
{
    public ErpApplicationView()
    {
        this.WhenActivated(d => {

        });
        AvaloniaXamlLoader.Load(this);
    }
}
