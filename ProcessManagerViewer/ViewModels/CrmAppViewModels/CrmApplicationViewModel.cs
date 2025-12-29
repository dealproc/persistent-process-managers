using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;

using DynamicData;

using Microsoft.Extensions.DependencyInjection;

using ProcessManagerViewer.Domains;
using ProcessManagerViewer.Domains.CrmApp;

using ReactiveUI;

namespace ProcessManagerViewer.ViewModels.CrmAppViewModels;

public sealed class CrmApplicationViewModel : ViewModelBase, ICrmApplicationViewModel {
    private readonly CompositeDisposable _d = [];
    private ReadOnlyObservableCollection<ContactCollection.ContactDetails> _contacts;
    public ReadOnlyObservableCollection<ContactCollection.ContactDetails> Contacts => _contacts;

    private CrmApplicationViewModel(ContactCollection contacts) {
        contacts.Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _contacts)
            .Subscribe().DisposeWith(_d);
    }


    public class Factory : ICrmApplicationViewModelFactory {
        private readonly ContactCollection _collection;

        public Factory(
            [FromKeyedServices(Keys.Crm)]
            ContactCollection collection) {
            _collection = collection;
        }

        public ICrmApplicationViewModel Create() {
            return new CrmApplicationViewModel(_collection);
        }
    }

    public void Dispose() {
        _d?.Dispose();
    }
}

public interface ICrmApplicationViewModel : IDisposable {
    ReadOnlyObservableCollection<ContactCollection.ContactDetails> Contacts { get; }
}

public interface ICrmApplicationViewModelFactory {
    ICrmApplicationViewModel Create();
}
