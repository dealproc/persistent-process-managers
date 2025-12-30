using Avalonia.Markup.Xaml;

using ProcessManagerViewer.ViewModels.ErpAppViewModels;

using ReactiveUI;
using ReactiveUI.Avalonia;

namespace ProcessManagerViewer.Views.ErpAppViews;

public partial class ContactListView : ReactiveUserControl<ContactListViewModel> {
    public ContactListView() {
        this.WhenActivated(d => {

        });
        AvaloniaXamlLoader.Load(this);
    }
}
