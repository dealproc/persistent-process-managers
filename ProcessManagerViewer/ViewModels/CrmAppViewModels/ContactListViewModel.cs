using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

using DynamicData;

using Microsoft.Extensions.DependencyInjection;

using ProcessManagerViewer.Domains;
using ProcessManagerViewer.Domains.CrmApp;

using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ProcessManagerViewer.ViewModels.CrmAppViewModels;

public sealed partial class ContactListViewModel : ViewModelBase, IContactListViewModel {
    private readonly IContactEditorViewModelFactory _contactEditorViewModelFactory;
    private readonly ICommandPublisher _commandPublisher;
    private readonly CompositeDisposable _d = [];

    private ReadOnlyObservableCollection<ContactCollection.ContactDetails> _contacts;
    public ReadOnlyObservableCollection<ContactCollection.ContactDetails> Contacts => _contacts;

    public IScreen HostScreen { get; init; }

    [Reactive]
    private ContactCollection.ContactDetails _selectedContact;

    public string? UrlPathSegment { get; } = $"crm-{nameof(ContactListViewModel)}";

    public ReactiveCommand<Unit, Unit> CreateContactCommand { get; init; }

    public ReactiveCommand<Unit, Unit> EditContactCommand { get; init; }

    public ReactiveCommand<Unit, Unit> ArchiveContactCommand { get; init; }

    public Interaction<string, Unit> ShowErrorMessage { get; init; } = new();

    private ContactListViewModel(
        IScreen hostScreen,
        ICommandPublisher commandPublisher,
        IContactEditorViewModelFactory contactEditorViewModelFactory,
        ContactCollection contacts) {
        HostScreen = hostScreen;
        _commandPublisher = commandPublisher;
        _contactEditorViewModelFactory = contactEditorViewModelFactory;
        contacts.Connect()
            .AutoRefresh(x => x.HasBeenArchived)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(x => !x.HasBeenArchived)
            .Bind(out _contacts)
            .Subscribe().DisposeWith(_d);

        var hasBeenSelected = this.WhenAnyValue(
            vm => vm.SelectedContact,
            (ContactCollection.ContactDetails? c) => c is not null);

        CreateContactCommand = ReactiveCommand.Create(CreateContact);
        EditContactCommand = ReactiveCommand.Create(EditContact, hasBeenSelected);
        ArchiveContactCommand = ReactiveCommand.CreateFromTask(ArchiveContact, hasBeenSelected);

        SelectedContact = null!;
    }

    private void CreateContact() {
        var vm = _contactEditorViewModelFactory.Create(HostScreen);
        vm.Create();
        HostScreen.Router.Navigate.Execute(vm);
    }

    private void EditContact() {
        var vm = _contactEditorViewModelFactory.Create(HostScreen);
        vm.Edit(SelectedContact);
        HostScreen.Router.Navigate.Execute(vm);
    }

    private async Task ArchiveContact() {
        var cmd = MessageBuilder.New(() => new ContactMsgs.ArchiveContact(
            SelectedContact.ContactId,
            CommandSource.Crm));

        if (!_commandPublisher.TrySend(cmd, out var response)) {
            var msg = ((response as Fail)?.Exception.Message ?? "Unknown");
            await ShowErrorMessage.Handle(msg).ToTask();
        }
    }

    public class Factory : IContactListViewModelFactory {
        private readonly IContactEditorViewModelFactory _contactEditorViewModelFactory;
        private readonly ICommandPublisher _commandPublisher;
        private readonly ContactCollection _contacts;

        public Factory(
            IContactEditorViewModelFactory contactEditorViewModelFactory,
            [FromKeyedServices(Keys.Crm)]
            ICommandPublisher commandPublisher,
            ContactCollection contacts) {
            _contactEditorViewModelFactory = contactEditorViewModelFactory;
            _commandPublisher = commandPublisher;
            _contacts = contacts;
        }

        public IContactListViewModel Create(IScreen screen)
            => new ContactListViewModel(
                screen,
                _commandPublisher,
                _contactEditorViewModelFactory,
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
