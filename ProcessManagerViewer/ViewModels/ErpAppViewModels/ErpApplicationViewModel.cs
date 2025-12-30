using ReactiveUI;

namespace ProcessManagerViewer.ViewModels.ErpAppViewModels;

public sealed partial class ErpApplicationViewModel : ReactiveObject, IErpApplicationViewModel {

    public RoutingState Router { get; } = new();

    private ErpApplicationViewModel(
        IContactListViewModelFactory factory) {
        var vm = factory.Create(this);
        Router.NavigateAndReset.Execute(vm);
    }

    public class Factory : IErpApplicationViewModelFactory {
        private readonly IContactListViewModelFactory _factory;

        public Factory(
            IContactListViewModelFactory factory) {
            _factory = factory;
        }

        public IErpApplicationViewModel Create()
            => new ErpApplicationViewModel(_factory);
    }
}

public interface IErpApplicationViewModel : IScreen {

}

public interface IErpApplicationViewModelFactory {
    IErpApplicationViewModel Create();
}
