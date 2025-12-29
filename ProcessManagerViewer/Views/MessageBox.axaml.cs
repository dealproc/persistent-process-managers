using Avalonia.Controls;

namespace ProcessManagerViewer;

public partial class MessageBox : Window {
    public MessageBox() {
        InitializeComponent();

        btnOk.Click += (s, e) => Close();
    }
}
