using Avalonia.Markup.Xaml;

using ProcessManagerViewer.ViewModels.ThisAppViewModels;

using ReactiveUI;
using ReactiveUI.Avalonia;

namespace ProcessManagerViewer.Views.ThisAppViews;

public partial class ContactEditorView : ReactiveUserControl<ContactEditorViewModel> {
    public ContactEditorView() {
        this.WhenActivated(d => {
            ViewModel!.ShowErrorMessage.RegisterHandler((ctx) => {
                var box = new MessageBox {
                    Title = "Error"
                };
                box.Message.Text = ctx.Input;
                box.Show();
            });
        });
        AvaloniaXamlLoader.Load(this);
    }
}
