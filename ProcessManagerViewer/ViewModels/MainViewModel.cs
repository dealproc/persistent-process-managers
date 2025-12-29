using ReactiveUI.SourceGenerators;

namespace ProcessManagerViewer.ViewModels;

public partial class MainViewModel : ViewModelBase {
    [Reactive]
    ThisAppViewModels.IThisApplicationViewModel _thisApplication = null!;

    [Reactive]
    CrmAppViewModels.ICrmApplicationViewModel _crmApplication = null!;

    [Reactive]
    ErpAppViewModels.IErpApplicationViewModel _erpApplication = null!;

    public MainViewModel(
        ThisAppViewModels.IThisApplicationViewModelFactory thisApplicationViewModelFactory,
        CrmAppViewModels.ICrmApplicationViewModelFactory crmApplicationViewModelFactory,
        ErpAppViewModels.IErpApplicationViewModelFactory erpApplicationViewModelFactory) {
        ThisApplication = thisApplicationViewModelFactory.Create();
        CrmApplication = crmApplicationViewModelFactory.Create();
        ErpApplication = erpApplicationViewModelFactory.Create();
    }
}
