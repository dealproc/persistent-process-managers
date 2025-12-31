using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading.Tasks;

using DynamicData;

using ProcessManagerViewer.Domains.ErpApp;

using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ProcessManagerViewer.ViewModels.ErpAppViewModels;

public sealed partial class ContactListViewModel : ViewModelBase, IContactListViewModel {
    private readonly IContactEditorViewModelFactory _contactEditorViewModelFactory;
    private readonly CompositeDisposable _d = [];
    private bool _hasBeenDisposed = false;
    private readonly ReadOnlyObservableCollection<ContactCollection.ContactDetails> _contacts;
    public ReadOnlyObservableCollection<ContactCollection.ContactDetails> Contacts => _contacts;

    public IScreen HostScreen { get; init; }

    [Reactive]
    private ContactCollection.ContactDetails _selectedContact;

    public ReactiveCommand<Unit, Unit> CreateContactCommand { get; init; }

    public ReactiveCommand<Unit, Unit> EditContactCommand { get; init; }

    public ReactiveCommand<Unit, Unit> ArchiveContactCommand { get; init; }

    public Interaction<string, Unit> ShowErrorMessage { get; init; } = new();

    public string? UrlPathSegment { get; } = $"ERP:{nameof(ContactListViewModel)}";

    private ContactListViewModel(
        IScreen hostScreen,
        IContactEditorViewModelFactory contactEditorViewModelFactory,
        ContactCollection contacts) {
        HostScreen = hostScreen;
        _contactEditorViewModelFactory = contactEditorViewModelFactory;
        contacts.Connect()
            .AutoRefresh(x => x.HasBeenArchived)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(x => !x.HasBeenArchived)
            .Bind(out _contacts)
            .Subscribe().DisposeWith(_d);

        CreateContactCommand = ReactiveCommand.Create(CreateContact);
        EditContactCommand = ReactiveCommand.Create(UpdateContact);
        ArchiveContactCommand = ReactiveCommand.CreateFromTask(ArchiveContact);

        SelectedContact = null!;
    }

    private void CreateContact() {
        var vm = _contactEditorViewModelFactory.Create(HostScreen);
        vm.Create();
        HostScreen.Router.Navigate.Execute(vm);
    }

    private void UpdateContact() {
        var vm = _contactEditorViewModelFactory.Create(HostScreen);
        vm.Edit(SelectedContact);
        HostScreen.Router.Navigate.Execute(vm);
    }

    private async Task ArchiveContact() {

    }

    public void Dispose() {
        if (_hasBeenDisposed) {
            return;
        }
        _d?.Dispose();
    }

    public class Factory : IContactListViewModelFactory {
        private readonly IContactEditorViewModelFactory _contactEditorViewModelFactory;
        private readonly ContactCollection _contacts;

        public Factory(
            IContactEditorViewModelFactory contactEditorViewModelFactory,
            ContactCollection contacts) {
            _contactEditorViewModelFactory = contactEditorViewModelFactory;
            _contacts = contacts;
        }

        public IContactListViewModel Create(IScreen screen)
            => new ContactListViewModel(
                screen,
                _contactEditorViewModelFactory,
                _contacts);
    }
}

public interface IContactListViewModel : IRoutableViewModel, IDisposable {
    ReadOnlyObservableCollection<ContactCollection.ContactDetails> Contacts { get; }
    ContactCollection.ContactDetails SelectedContact { get; set; }
}

public interface IContactListViewModelFactory {
    IContactListViewModel Create(IScreen screen);
}
