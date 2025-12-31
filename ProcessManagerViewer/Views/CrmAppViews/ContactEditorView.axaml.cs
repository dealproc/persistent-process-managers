using System.Reactive;

using Avalonia.Markup.Xaml;

using ProcessManagerViewer.ViewModels.CrmAppViewModels;

using ReactiveUI;
using ReactiveUI.Avalonia;

namespace ProcessManagerViewer.Views.CrmAppViews;

public partial class ContactEditorView : ReactiveUserControl<ContactEditorViewModel> {
    public ContactEditorView() {
        this.WhenActivated(d => {
            ViewModel!.ShowErrorMessage.RegisterHandler((ctx) => {
                var mb = new MessageBox();
                mb.Title = "Test";
                mb.Message.Text = "This is a test message.";
                mb.Closed += (sender, args) => {
                    ctx.SetOutput(Unit.Default);
                };
                mb.Show();
            });
        });
        AvaloniaXamlLoader.Load(this);
    }
}
