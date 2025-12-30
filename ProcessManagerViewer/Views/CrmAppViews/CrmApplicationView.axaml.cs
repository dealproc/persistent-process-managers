using Avalonia.Markup.Xaml;

using ProcessManagerViewer.ViewModels.CrmAppViewModels;

using ReactiveUI;
using ReactiveUI.Avalonia;

namespace ProcessManagerViewer.Views.CrmAppViews;

public partial class CrmApplicationView : ReactiveUserControl<CrmApplicationViewModel>
{
    public CrmApplicationView()
    {
        this.WhenActivated(d => {

        });
        AvaloniaXamlLoader.Load(this);
    }
}
