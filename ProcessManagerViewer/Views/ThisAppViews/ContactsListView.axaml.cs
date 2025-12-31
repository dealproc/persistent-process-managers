using System.Reactive;

using Avalonia.Markup.Xaml;

using ProcessManagerViewer.ViewModels.ThisAppViewModels;

using ReactiveUI;
using ReactiveUI.Avalonia;

namespace ProcessManagerViewer.Views.ThisAppViews;

public partial class ContactsListView : ReactiveUserControl<ContactsListViewModel>
{
    public ContactsListView()
    {
        this.WhenActivated(d => {
            ViewModel!.ShowErrorMessage.RegisterHandler((ctx) => {
                var box = new MessageBox {
                    Title = "Error"
                };
                box.Message.Text = ctx.Input;
                box.Closed += (sender, args) => ctx.SetOutput(Unit.Default);
                box.Show();
            });
        });
        AvaloniaXamlLoader.Load(this);
    }
}
