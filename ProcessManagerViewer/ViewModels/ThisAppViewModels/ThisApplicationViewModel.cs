using ReactiveUI;

namespace ProcessManagerViewer.ViewModels.ThisAppViewModels;

public partial class ThisApplicationViewModel : ViewModelBase, IThisApplicationViewModel {
    private ThisApplicationViewModel(IContactListViewModelFactory contactListViewModelFactory) {
        var clvm = contactListViewModelFactory.Create(this);
        Router.NavigateAndReset.Execute(clvm);
    }

    public RoutingState Router { get; } = new(RxApp.MainThreadScheduler);

    public class Factory : IThisApplicationViewModelFactory {
        private readonly IContactListViewModelFactory _factory;

        public Factory(IContactListViewModelFactory factory) {
            _factory = factory;
        }

        public IThisApplicationViewModel Create()
            => new ThisApplicationViewModel(_factory);
    }
}

public interface IThisApplicationViewModel : IScreen {

}

public interface IThisApplicationViewModelFactory {
    IThisApplicationViewModel Create();
}
