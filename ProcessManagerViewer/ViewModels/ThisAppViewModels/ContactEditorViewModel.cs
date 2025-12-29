using System;
using System.Reactive;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using ProcessManagerViewer.Domains;

using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ProcessManagerViewer.ViewModels.ThisAppViewModels;

public sealed partial class ContactEditorViewModel : ViewModelBase, IContactEditorViewModel {
    private readonly ICommandPublisher _commandPublisher;
    private Guid _contactId;
    private bool _isEditingContact = false;

    public string? UrlPathSegment { get; private set; }

    public IScreen HostScreen { get; init; }

    [Reactive]
    private string _firstName = "";

    [Reactive]
    private string _lastName = "";

    [Reactive]
    private string _email = "";

    public ReactiveCommand<Unit, Unit> AddOrUpdateCommand { get; init; }

    public ReactiveCommand<Unit, IRoutableViewModel> CancelCommand { get; init; }

    public Interaction<string, Unit> ShowErrorMessage { get; } = new();

    public ContactEditorViewModel(
        ICommandPublisher commandPublisher,
        IScreen hostScreen) {
        HostScreen = hostScreen;
        _commandPublisher = commandPublisher;

        AddOrUpdateCommand = ReactiveCommand.CreateFromTask(AddOrUpdate);
        CancelCommand = ReactiveCommand.CreateFromObservable<Unit, IRoutableViewModel>(Cancel);
    }

    public void Create() {
        _isEditingContact = false;
        _contactId = Guid.NewGuid();

        FirstName = "";
        LastName = "";
        Email = "";

        SetPathSegment();
    }

    public void Edit(Domains.ThisApp.ContactRm contact) {
        _isEditingContact = true;
        _contactId = contact.ContactId;

        FirstName = contact.FirstName;
        LastName = contact.LastName;
        Email = contact.Email;

        SetPathSegment();
    }

    private async Task AddOrUpdate() {
        var cmd = MessageBuilder.New<ICommand>(() => _isEditingContact
            ? new Domains.ThisApp.UpdateContactDetailsMsgs.Start(
                _contactId,
                FirstName,
                LastName,
                Email)
            : new Domains.ThisApp.AddContactMsgs.Start(
                _contactId,
                FirstName,
                LastName,
                Email));

        if (_commandPublisher.TrySend(cmd, out var response)) {
            await HostScreen.Router.NavigateBack.Execute().ToTask();
            return;
        }

        var failedMsg = (response as Fail)?.Exception.Message ?? "Something failed, but we don't know what happened.";
        await ShowErrorMessage.Handle(failedMsg).ToTask();
    }

    private IObservable<IRoutableViewModel> Cancel(Unit _)
        => HostScreen.Router.NavigateBack.Execute();

    private void SetPathSegment() {
        UrlPathSegment = $"{nameof(ContactEditorViewModel)}:{_contactId:N}";
    }

    public class Factory : IContactEditorViewModelFactory {
        private readonly ICommandPublisher _commandPublisher;

        public IContactEditorViewModel CreateContact(IScreen hostScreen) {
            var vm = new ContactEditorViewModel(
                _commandPublisher,
                hostScreen);
            vm.Create();
            return vm;
        }

        public IContactEditorViewModel EditContact(IScreen hostScreen, Domains.ThisApp.ContactRm contact) {
            var vm = new ContactEditorViewModel(
                _commandPublisher,
                hostScreen);
            vm.Edit(contact);
            return vm;
        }

        public Factory(
            [FromKeyedServices(Keys.ThisApp)]
            ICommandPublisher commandPublisher) {
            _commandPublisher = commandPublisher;
        }
    }
}

public interface IContactEditorViewModel : IRoutableViewModel {
    string FirstName { get; set; }

    string LastName { get; set; }

    string Email { get; set; }

    ReactiveCommand<Unit, Unit> AddOrUpdateCommand { get; }

    ReactiveCommand<Unit, IRoutableViewModel> CancelCommand { get; }

    Interaction<string, Unit> ShowErrorMessage { get; }
}

public interface IContactEditorViewModelFactory {
    IContactEditorViewModel CreateContact(IScreen hostScreen);

    IContactEditorViewModel EditContact(IScreen hostScreen, Domains.ThisApp.ContactRm contact);
}
