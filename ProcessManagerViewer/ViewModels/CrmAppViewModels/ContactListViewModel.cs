using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;

using DynamicData;

using Microsoft.Extensions.DependencyInjection;

using ProcessManagerViewer.Domains.CrmApp;

using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ProcessManagerViewer.ViewModels.CrmAppViewModels;

public sealed partial class ContactListViewModel : ViewModelBase, IContactListViewModel {
    private readonly CompositeDisposable _d = [];

    private ReadOnlyObservableCollection<ContactCollection.ContactDetails> _contacts;

    public ReadOnlyObservableCollection<ContactCollection.ContactDetails> Contacts => _contacts;
    public IScreen HostScreen { get; init; }

    [Reactive]
    private ContactCollection.ContactDetails _selectedContact;

    public string? UrlPathSegment { get; } = $"crm-{nameof(ContactListViewModel)}";

    private ContactListViewModel(
        IScreen hostScreen,
        ContactCollection contacts) {
        HostScreen = hostScreen;
        contacts.Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _contacts)
            .Subscribe().DisposeWith(_d);
        SelectedContact = null!;
    }

    public class Factory : IContactListViewModelFactory {
        private readonly ContactCollection _contacts;

        public Factory(
            ContactCollection contacts) {
            _contacts = contacts;
        }

        public IContactListViewModel Create(IScreen screen)
            => new ContactListViewModel(
                screen,
                _contacts);
    }
}

public interface IContactListViewModel : IRoutableViewModel {
    ReadOnlyObservableCollection<ContactCollection.ContactDetails> Contacts { get; }
    ContactCollection.ContactDetails SelectedContact { get; set; }
}

public interface IContactListViewModelFactory {
    IContactListViewModel Create(IScreen screen);
}
