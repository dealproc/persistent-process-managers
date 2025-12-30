using ProcessManagerViewer.ViewModels.ErpAppViewModels;

using ReactiveUI.Avalonia;

namespace ProcessManagerViewer.Views.ErpAppViews;

public partial class ErpAppView : ReactiveUserControl<ErpApplicationViewModel>
{
    public ErpAppView()
    {
        InitializeComponent();
    }
}
