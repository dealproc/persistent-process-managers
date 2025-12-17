namespace ProcessManagerViewer.ViewModels;

public class ViewNotFoundViewModel : ViewModelBase {
    public string Message { get; init; }

    public ViewNotFoundViewModel(
        string message) {
        Message = message;
    }
}
