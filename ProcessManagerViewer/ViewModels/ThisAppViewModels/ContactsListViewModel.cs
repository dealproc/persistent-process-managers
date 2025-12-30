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
using ProcessManagerViewer.Domains.ThisApp;

using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ProcessManagerViewer.ViewModels.ThisAppViewModels;

public sealed partial class ContactsListViewModel : ViewModelBase, IContactListViewModel {
    private readonly CompositeDisposable _d = [];

    private readonly ICommandPublisher _commandPublisher;
    private readonly IContactEditorViewModelFactory _contactEditorViewModelFactory;
    private ReadOnlyObservableCollection<ContactRm> _contacts;

    public ReadOnlyObservableCollection<ContactRm> Contacts => _contacts;

    [Reactive]
    private ContactRm _selectedContact = null!;

    public ReactiveCommand<Unit, Unit> CreateContactCommand { get; init; }

    public ReactiveCommand<Unit, Unit> EditContactCommand { get; init; }

    public ReactiveCommand<Unit, Unit> ArchiveContactCommand { get; init; }

    public Interaction<string, Unit> ShowErrorMessage { get; init; }

    public string? UrlPathSegment { get; } = nameof(ContactsListViewModel);

    public IScreen HostScreen { get; init; }

    private ContactsListViewModel(
        ICommandPublisher commandPublisher,
        IScreen hostScreen,

        ContactCollection contacts,

        IContactEditorViewModelFactory contactEditorViewModelFactory) {
        _commandPublisher = commandPublisher;
        HostScreen = hostScreen;
        _contactEditorViewModelFactory = contactEditorViewModelFactory;
        contacts.Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _contacts)
            .Subscribe().DisposeWith(_d);

        var hasBeenSelected = this.WhenAnyValue(
            vm => vm.SelectedContact,
            (ContactRm? c) => c is not null);

        CreateContactCommand = ReactiveCommand.Create(CreateContact);
        EditContactCommand = ReactiveCommand.Create(UpdateContact, hasBeenSelected);
        ArchiveContactCommand = ReactiveCommand.CreateFromTask(ArchiveContact, hasBeenSelected);
        ShowErrorMessage = new();
    }

    private void CreateContact() {
        var vm = _contactEditorViewModelFactory.CreateContact(
            HostScreen);
        HostScreen.Router.Navigate.Execute(vm);
    }

    private void UpdateContact() {
        var vm = _contactEditorViewModelFactory.EditContact(
            HostScreen,
            SelectedContact);
        HostScreen.Router.Navigate.Execute(vm);
    }

    private async Task ArchiveContact() {
        var cmd = MessageBuilder.New(() => new ArchiveContactMsgs.Start(
            Guid.NewGuid(),
            SelectedContact!.ContactId,
            CommandSource.ThisApp));

        if (!_commandPublisher.TrySend(cmd, out var response)) {
            var msg = (response as Fail)?.Exception.Message ?? "Unknown failure. Check logs";
            await ShowErrorMessage.Handle(msg).ToTask();
        }
    }

    public void Dispose() {
        _d?.Dispose();
    }

    public class Factory : IContactListViewModelFactory {
        private readonly ICommandPublisher _commandPublisher;
        private readonly ContactCollection _contacts;
        private readonly IContactEditorViewModelFactory _contactEditorViewModelFactory;

        public Factory(
            [FromKeyedServices(Keys.ThisApp)]
            ICommandPublisher commandPublisher,

            ContactCollection contacts,

            IContactEditorViewModelFactory contactEditorViewModelFactory) {
            _commandPublisher = commandPublisher;
            _contacts = contacts;
            _contactEditorViewModelFactory = contactEditorViewModelFactory;
        }

        public IContactListViewModel Create(IScreen screen) {
            return new ContactsListViewModel(
                _commandPublisher,
                screen,
                _contacts,
                _contactEditorViewModelFactory);
        }
    }
}

public interface IContactListViewModel : IRoutableViewModel, IDisposable {
    ReadOnlyObservableCollection<ContactRm> Contacts { get; }

    ContactRm SelectedContact { get; set; }

    ReactiveCommand<Unit, Unit> CreateContactCommand { get; init; }

    ReactiveCommand<Unit, Unit> EditContactCommand { get; init; }

    ReactiveCommand<Unit, Unit> ArchiveContactCommand { get; init; }

    Interaction<string, Unit> ShowErrorMessage { get; }
}

public interface IContactListViewModelFactory {
    IContactListViewModel Create(IScreen screen);
}
