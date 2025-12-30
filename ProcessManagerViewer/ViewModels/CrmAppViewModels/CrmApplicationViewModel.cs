using System.Reactive.Disposables;

using ReactiveUI;

namespace ProcessManagerViewer.ViewModels.CrmAppViewModels;

public sealed class CrmApplicationViewModel : ViewModelBase, ICrmApplicationViewModel {
    private readonly CompositeDisposable _d = [];

    public RoutingState Router { get; } = new();

    private CrmApplicationViewModel(
        IContactListViewModelFactory contactListViewModelFactory) {
        var vm = contactListViewModelFactory.Create(this);
        Router.NavigateAndReset.Execute(vm);
    }


    public class Factory : ICrmApplicationViewModelFactory {
        private readonly IContactListViewModelFactory _listViewModelFactory;

        public Factory(
            IContactListViewModelFactory listViewModelFactory) {
            _listViewModelFactory = listViewModelFactory;
        }

        public ICrmApplicationViewModel Create() {
            return new CrmApplicationViewModel(_listViewModelFactory);
        }
    }

    public void Dispose() {
        _d?.Dispose();
    }
}

public interface ICrmApplicationViewModel : IScreen {
}

public interface ICrmApplicationViewModelFactory {
    ICrmApplicationViewModel Create();
}
