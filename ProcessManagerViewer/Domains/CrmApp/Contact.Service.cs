using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ReactiveDomain;
using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

namespace ProcessManagerViewer.Domains.CrmApp;

public class ContactService : ReadModelBase, IReactiveDomainService,
    IHandleCommand<AclRequests.CreateCrmContactReq>,
    IHandleCommand<ContactMsgs.CreateContact>,

    IHandleCommand<AclRequests.UpdateCrmContactDetailsReq>,
    IHandleCommand<ContactMsgs.UpdateDetails>,

    IHandleCommand<AclRequests.ArchiveCrmContactReq>,
    IHandleCommand<ContactMsgs.ArchiveContact> {
    private readonly ICommandSubscriber _fromExternalApp;
    private readonly IPublisher _toExternalApp;
    private readonly ICommandPublisher _commandToExternalApp;
    private readonly ICommandSubscriber _commandSubscriber;
    private readonly ICommandPublisher _commandPublisher;
    private readonly ContactLookup _lookup;
    private readonly ICorrelatedRepository _repository;
    private readonly ILogger _log;
    private readonly CompositeDisposable _disposables = [];
    private bool _hasBeenStarted = false;
    private bool _hasBeenDisposed = false;

    public ContactService(
        [FromKeyedServices(Keys.ThisApp)]
        ICommandSubscriber fromExternalApp,
        [FromKeyedServices(Keys.ThisApp)]
        IPublisher toExternalApp,
        [FromKeyedServices(Keys.ThisApp)]
        ICommandPublisher commandToExternalApp,

        [FromKeyedServices(Keys.Crm)]
        ICommandSubscriber commandSubscriber,
        [FromKeyedServices(Keys.Crm)]
        ICommandPublisher commandPublisher,

        [FromKeyedServices(Keys.Crm)]
        ContactLookup lookup,

        ICorrelatedRepository repository,
        ILoggerFactory loggerFactory,
        IConfiguredConnection connection) : base(
            nameof(ContactService),
            connection) {

        _fromExternalApp = fromExternalApp;
        _toExternalApp = toExternalApp;
        _commandToExternalApp = commandToExternalApp;
        _commandPublisher = commandPublisher;
        _commandSubscriber = commandSubscriber;
        _lookup = lookup;
        _log = loggerFactory.CreateLogger<ContactService>();
        _repository = repository;
    }

    public void StartService() {
        if (_hasBeenStarted) {
            return;
        }
        _hasBeenStarted = true;

        _log.LogDebug("Starting {@ServiceName}.", GetType().Name);

        _fromExternalApp.Subscribe<AclRequests.CreateCrmContactReq>(this).DisposeWith(_disposables);
        _commandSubscriber.Subscribe<ContactMsgs.CreateContact>(this).DisposeWith(_disposables);

        _fromExternalApp.Subscribe<AclRequests.UpdateCrmContactDetailsReq>(this).DisposeWith(_disposables);
        _commandSubscriber.Subscribe<ContactMsgs.UpdateDetails>(this).DisposeWith(_disposables);

        _fromExternalApp.Subscribe<AclRequests.ArchiveCrmContactReq>(this).DisposeWith(_disposables);
        _commandSubscriber.Subscribe<ContactMsgs.ArchiveContact>(this).DisposeWith(_disposables);

        Start<Contact>(checkpoint: long.MaxValue - 1);

        _log.LogDebug("{@ServiceName} started.", GetType().Name);
    }

    public CommandResponse Handle(AclRequests.CreateCrmContactReq command) {
        _log.LogTrace("Handling {@Class}:{@Method}", GetType().Name, command.GetType().Name);

        if (_lookup.TryToFind(command.XrefId, out _)) {
            var resp = MessageBuilder.From(command)
                .Build(() => new AclRequests.CreateCrmContactResp(
                    command.XrefId,
                    true));
            _toExternalApp.Publish(resp);
            return command.Succeed();
        }

        var cmd = MessageBuilder.From(command)
            .Build(() => new ContactMsgs.CreateContact(
                Guid.NewGuid(),
                command.XrefId,
                command.FirstName,
                command.LastName,
                command.Email,
                command.Source));
        var succeeded = _commandPublisher.TrySendAsync(cmd);
        return succeeded
            ? command.Succeed()
            : command.Fail();
    }

    public CommandResponse Handle(ContactMsgs.CreateContact command) {
        _log.LogTrace("Handling {@Class}:{@Method}", GetType().Name, command.GetType().Name);

        _repository.Save(new Contact(
            command.ContactId,
            command.XrefId,
            command.FirstName,
            command.LastName,
            command.Email,
            command));
        _lookup.Set(command.XrefId, command.ContactId);
        if (command.Source == CommandSource.Crm) {
            _log.LogTrace("Contact created in CRM, propogate to other systems.");
            var req = MessageBuilder.From(command)
                .Build(() => new AclRequests.CreateContactReq(
                    command.XrefId,
                    command.FirstName,
                    command.LastName,
                    command.Email,
                    command.Source));
            _commandToExternalApp.TrySendAsync(req);
        } else {
            _log.LogTrace("Contact created from external command.  Respond with Ok.");
            var resp = MessageBuilder.From(command)
                .Build(() => new AclRequests.CreateCrmContactResp(
                    command.XrefId,
                    true));
            _toExternalApp.Publish(resp);
        }
        return command.Succeed();
    }

    public CommandResponse Handle(AclRequests.UpdateCrmContactDetailsReq command) {
        _log.LogTrace("Handling {@Class}:{@Method}", GetType().Name, command.GetType().Name);

        if (!_lookup.TryToFind(command.XrefId, out var contactId)) {
            var resp = MessageBuilder.From(command)
                .Build(() => new AclRequests.UpdateCrmContactDetailsResp(
                    command.XrefId,
                    false));
            return command.Succeed();
        }

        var cmd = MessageBuilder.From(command)
            .Build(() => new ContactMsgs.UpdateDetails(
                contactId,
                command.FirstName,
                command.LastName,
                command.Email,
                command.Source));

        var succeeded = _commandPublisher.TrySendAsync(cmd);
        return succeeded
            ? command.Succeed()
            : command.Fail();
    }

    public CommandResponse Handle(ContactMsgs.UpdateDetails command) {
        _log.LogTrace("Handling {@Class}:{@Method}", GetType().Name, command.GetType().Name);

        var contact = _repository.GetById<Contact>(command.ContactId, command);
        contact.UpdateDetails(
            command.FirstName,
            command.LastName,
            command.Email);

        _repository.Save(contact);

        if (command.Source == CommandSource.Crm) {
            // we are in-app, so we need to issue update contact req for other systems.
            var req = MessageBuilder.From(command)
                .Build(() => new AclRequests.UpdateContactDetailsReq(
                    _lookup.Lookup(command.ContactId),
                    command.FirstName,
                    command.LastName,
                    command.Email,
                    command.Source));
            _commandToExternalApp.TrySendAsync(req);
        } else {
            var resp = MessageBuilder.From(command)
                .Build(() => new AclRequests.UpdateCrmContactDetailsResp(
                    _lookup.Lookup(command.ContactId),
                    true));
            _toExternalApp.Publish(resp);
        }

        return command.Succeed();
    }

    public CommandResponse Handle(AclRequests.ArchiveCrmContactReq command) {
        _log.LogTrace("Handling {@Class}:{@Method}", GetType().Name, command.GetType().Name);

        if (!_lookup.TryToFind(command.XrefId, out var contactId)) {
            var resp = MessageBuilder.From(command)
                .Build(() => new AclRequests.ArchiveCrmContactResp(
                    command.XrefId,
                    false));
            return command.Succeed();
        }

        var cmd = MessageBuilder.From(command)
            .Build(() => new ContactMsgs.ArchiveContact(
                contactId,
                command.Source));

        var succeeded = _commandPublisher.TrySendAsync(cmd);
        return succeeded
            ? command.Succeed()
            : command.Fail();
    }

    public CommandResponse Handle(ContactMsgs.ArchiveContact command) {
        _log.LogTrace("Handling {@Class}:{@Method}", GetType().Name, command.GetType().Name);

        var contact = _repository.GetById<Contact>(command.ContactId, command);
        contact.Archive();

        _repository.Save(contact);

        if (command.Source == CommandSource.Crm) {
            var req = MessageBuilder.From(command)
                .Build(() => new AclRequests.ArchiveContactReq(
                    _lookup.Lookup(command.ContactId),
                    command.Source));
            _commandToExternalApp.TrySendAsync(req);
        } else {
            var resp = MessageBuilder.From(command)
                .Build(() => new AclRequests.ArchiveCrmContactResp(
                    _lookup.Lookup(command.ContactId),
                    true));
            _toExternalApp.Publish(resp);
        }


        return command.Succeed();
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (!disposing || _hasBeenDisposed) {
            return;
        }
        _hasBeenDisposed = true;

        _disposables?.Dispose();
    }
}
