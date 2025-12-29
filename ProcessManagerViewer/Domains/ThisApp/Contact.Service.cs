using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ReactiveDomain;
using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

namespace ProcessManagerViewer.Domains.ThisApp;

public class ContactService : ReadModelBase, IReactiveDomainService,
    IHandleCommand<ContactMsgs.CreateContact>,
    IHandleCommand<ContactMsgs.UpdateDetails>,
    IHandleCommand<ContactMsgs.ArchiveContact> {

    private readonly ICommandSubscriber _commandSubscriber;
    private readonly ICorrelatedRepository _repository;
    private readonly ContactLookup _lookup;
    private readonly CompositeDisposable _disposables = [];
    private readonly ILogger _log;
    private bool _hasBeenStarted = false;
    private bool _hasBeenDisposed = false;

    public ContactService(
        [FromKeyedServices(Keys.ThisApp)]
        ICommandSubscriber commandSubscriber,

        ICorrelatedRepository repository,

        [FromKeyedServices(Keys.ThisApp)]
        ContactLookup lookup,

        ILoggerFactory loggerFactory,
        IConfiguredConnection connection) : base(
            nameof(ContactService),
            connection) {
        _repository = repository;
        _commandSubscriber = commandSubscriber;
        _lookup = lookup;
        _log = loggerFactory.CreateLogger<ContactService>();
    }

    public void StartService() {
        if (_hasBeenStarted) {
            return;
        }
        _hasBeenStarted = true;

        _log.LogDebug("Starting {@ServiceName}.", GetType().Name);

        _commandSubscriber.Subscribe<ContactMsgs.CreateContact>(this).DisposeWith(_disposables);
        _commandSubscriber.Subscribe<ContactMsgs.UpdateDetails>(this).DisposeWith(_disposables);
        _commandSubscriber.Subscribe<ContactMsgs.ArchiveContact>(this).DisposeWith(_disposables);

        Start<Contact>(checkpoint: long.MaxValue - 1);

        _log.LogDebug("{@ServiceName} started.", GetType().Name);
    }

    public CommandResponse Handle(ContactMsgs.CreateContact command) {
        _log.LogTrace("Handling {@Class}:{@Method}", GetType().Name, command.GetType().Name);
        var contact = new Contact(
            command.ContactId,
            command.XrefId,
            command.FirstName,
            command.LastName,
            command.Email,
            command);
        _repository.Save(contact);
        return command.Succeed();
    }

    public CommandResponse Handle(ContactMsgs.UpdateDetails command) {
        _log.LogTrace("Handling {@Class}:{@Method}", GetType().Name, command.GetType().Name);
        var contact = _repository.GetById<Contact>(command.ContactId, command);
        contact.UpdateDetails(
            command.FirstName,
            command.LastName,
            command.Email);
        _repository.Save(contact);
        return command.Succeed();
    }

    public CommandResponse Handle(ContactMsgs.ArchiveContact command) {
        _log.LogTrace("Handling {@Class}:{@Method}", GetType().Name, command.GetType().Name);
        var contact = _repository.GetById<Contact>(command.ContactId, command);
        contact.Archive();
        _repository.Save(contact);
        return command.Succeed();
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (_hasBeenDisposed || !disposing) {
            return;
        }
        _hasBeenDisposed = true;
        _disposables?.Dispose();
    }
}
