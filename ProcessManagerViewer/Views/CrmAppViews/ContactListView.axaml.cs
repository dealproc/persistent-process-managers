using Avalonia.Markup.Xaml;

using ProcessManagerViewer.ViewModels.CrmAppViewModels;

using ReactiveUI;
using ReactiveUI.Avalonia;

namespace ProcessManagerViewer.Views.CrmAppViews;

public partial class ContactListView : ReactiveUserControl<ContactListViewModel> {
    public ContactListView() {
        this.WhenActivated(d => {

        });
        AvaloniaXamlLoader.Load(this);
    }
}
