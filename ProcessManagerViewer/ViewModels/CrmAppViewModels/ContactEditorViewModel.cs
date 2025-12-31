using System;
using System.Reactive;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ProcessManagerViewer.ViewModels.CrmAppViewModels;

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

    public Interaction<string, Unit> ShowErrorMessage { get; init; } = new();

    public ContactEditorViewModel(
        ICommandPublisher commandPublisher,
        IScreen hostScreen
        ) {
        _commandPublisher = commandPublisher;
        HostScreen = hostScreen;

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

    public void Edit(Domains.CrmApp.ContactCollection.ContactDetails contact) {
        _isEditingContact = true;
        _contactId = contact.ContactId;

        FirstName = contact.FirstName;
        LastName = contact.LastName;
        Email = contact.Email;

        SetPathSegment();
    }

    private async Task AddOrUpdate() {
        ICommand cmd = _isEditingContact
            ? MessageBuilder.New(() => new Domains.CrmApp.ContactMsgs.UpdateDetails(
                _contactId,
                FirstName,
                LastName,
                Email,
                Domains.CommandSource.Crm))
            : MessageBuilder.New(() => new Domains.CrmApp.ContactMsgs.CreateContact(
                _contactId,
                Guid.NewGuid().ToString("N"),
                FirstName,
                LastName,
                Email,
                Domains.CommandSource.Crm));

        if (_commandPublisher.TrySend(cmd, out var response)) {
            await HostScreen.Router.NavigateBack.Execute().ToTask();
            return;
        }

        ShowErrorMessage.Handle((response as Fail)?.Exception.Message ?? "Something failed.");
    }

    private IObservable<IRoutableViewModel> Cancel(Unit _)
        => HostScreen.Router.NavigateBack.Execute();


    private void SetPathSegment() {
        UrlPathSegment = $"{nameof(ContactEditorViewModel)}:{_contactId:N}";
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

}
