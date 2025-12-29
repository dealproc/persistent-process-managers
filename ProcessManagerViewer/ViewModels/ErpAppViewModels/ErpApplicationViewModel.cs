using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;

using DynamicData;

using Microsoft.Extensions.DependencyInjection;

using ProcessManagerViewer.Domains;
using ProcessManagerViewer.Domains.ErpApp;

using ReactiveUI;

namespace ProcessManagerViewer.ViewModels.ErpAppViewModels;

public sealed partial class ErpApplicationViewModel : ReactiveObject, IErpApplicationViewModel {
    private readonly CompositeDisposable _d = [];

    private ReadOnlyObservableCollection<ContactCollection.ContactDetails> _contacts;
    public ReadOnlyObservableCollection<ContactCollection.ContactDetails> Contacts => _contacts;

    public ErpApplicationViewModel(
        ContactCollection contacts) {
        contacts.Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _contacts)
            .Subscribe().DisposeWith(_d);
    }

    public void Dispose() {
        _d?.Dispose();
    }

    public class Factory : IErpApplicationViewModelFactory {
        private readonly ContactCollection _contacts;

        public Factory(
            [FromKeyedServices(Keys.Erp)]
            ContactCollection contacts) {
            _contacts = contacts;
        }

        public IErpApplicationViewModel Create()
            => new ErpApplicationViewModel(_contacts);
    }
}

public interface IErpApplicationViewModel : IDisposable {
    ReadOnlyObservableCollection<ContactCollection.ContactDetails> Contacts { get; }
}

public interface IErpApplicationViewModelFactory {
    IErpApplicationViewModel Create();
}
