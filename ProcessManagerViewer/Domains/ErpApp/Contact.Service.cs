using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;

using Microsoft.Extensions.DependencyInjection;

using ReactiveDomain;
using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

namespace ProcessManagerViewer.Domains.ErpApp;

public class ContactService : ReadModelBase, IReactiveDomainService,
    IHandleCommand<AclRequests.CreateErpContactReq>,
    IHandleCommand<ContactMsgs.CreateContact>,
    IHandle<ContactMsgs.ContactCreated>,

    IHandleCommand<AclRequests.UpdateErpContactDetailsReq>,
    IHandleCommand<ContactMsgs.UpdateDetails>,
    IHandle<ContactMsgs.DetailsUpdated>,

    IHandleCommand<AclRequests.ArchiveErpContactReq>,
    IHandleCommand<ContactMsgs.ArchiveContact>,
    IHandle<ContactMsgs.Archived> {
    private readonly ICommandSubscriber _fromExternalAppCommandSubscriber;
    private readonly IPublisher _toExternalApp;
    private readonly ICommandSubscriber _commandSubscriber;
    private readonly ICommandPublisher _commandPublisher;
    private readonly ContactLookup _lookup;
    private readonly ICorrelatedRepository _repository;
    private readonly CompositeDisposable _disposables = [];
    private bool _hasBeenStarted = false;
    private bool _hasBeenDisposed = false;

    public ContactService(
        [FromKeyedServices(Keys.ThisApp)]
        ICommandSubscriber fromExternalAppCommandSubscriber,
        [FromKeyedServices(Keys.ThisApp)]
        IPublisher toExternalApp,

        [FromKeyedServices(Keys.Erp)]
        ICommandSubscriber commandSubscriber,
        [FromKeyedServices(Keys.Erp)]
        ICommandPublisher commandPublisher,

        [FromKeyedServices(Keys.Erp)]
        ContactLookup lookup,

        ICorrelatedRepository repository,
        IConfiguredConnection connection) : base(
            nameof(ContactService),
            connection) {

        _fromExternalAppCommandSubscriber = fromExternalAppCommandSubscriber;
        _toExternalApp = toExternalApp;
        _commandPublisher = commandPublisher;
        _commandSubscriber = commandSubscriber;
        _lookup = lookup;
        _repository = repository;
    }

    public void StartService() {
        if (_hasBeenStarted) {
            return;
        }

        _fromExternalAppCommandSubscriber.Subscribe<AclRequests.CreateErpContactReq>(this).DisposeWith(_disposables);
        _commandSubscriber.Subscribe<ContactMsgs.CreateContact>(this).DisposeWith(_disposables);
        EventStream.Subscribe<ContactMsgs.ContactCreated>(this).DisposeWith(_disposables);

        _fromExternalAppCommandSubscriber.Subscribe<AclRequests.UpdateErpContactDetailsReq>(this).DisposeWith(_disposables);
        _commandSubscriber.Subscribe<ContactMsgs.UpdateDetails>(this).DisposeWith(_disposables);
        EventStream.Subscribe<ContactMsgs.DetailsUpdated>(this).DisposeWith(_disposables);

        _fromExternalAppCommandSubscriber.Subscribe<AclRequests.ArchiveErpContactReq>(this).DisposeWith(_disposables);
        _commandSubscriber.Subscribe<ContactMsgs.ArchiveContact>(this).DisposeWith(_disposables);
        EventStream.Subscribe<ContactMsgs.Archived>(this).DisposeWith(_disposables);

        Start<Contact>(checkpoint: long.MaxValue - 1);
    }

    public CommandResponse Handle(AclRequests.CreateErpContactReq command) {
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
                command.Email));

        var succeeded = _commandPublisher.TrySendAsync(cmd);
        return succeeded
            ? command.Succeed()
            : command.Fail();
    }

    public CommandResponse Handle(ContactMsgs.CreateContact command) {
        _repository.Save(new Contact(
            command.ContactId,
            command.XrefId,
            command.FirstName,
            command.LastName,
            command.Email,
            command));
        _lookup.Set(command.XrefId, command.ContactId);
        return command.Succeed();
    }

    public void Handle(ContactMsgs.ContactCreated message) {
        if (!_lookup.TryToFind(message.ContactId, out var xrefId) ||
            xrefId is null) {
            return;
        }

        var resp = MessageBuilder.From(message)
            .Build(() => new AclRequests.CreateCrmContactResp(
                xrefId,
                true));

        _toExternalApp.Publish(resp);
    }

    public CommandResponse Handle(AclRequests.UpdateErpContactDetailsReq command) {
        if (!_lookup.TryToFind(command.XrefId, out var contactId)) {
            var resp = MessageBuilder.From(command)
                .Build(() => new AclRequests.UpdateErpContactDetailsResp(
                    command.XrefId,
                    false));
            return command.Succeed();
        }

        var cmd = MessageBuilder.From(command)
            .Build(() => new ContactMsgs.UpdateDetails(
                contactId,
                command.FirstName,
                command.LastName,
                command.Email));

        var succeeded = _commandPublisher.TrySendAsync(cmd);
        return succeeded
            ? command.Succeed()
            : command.Fail();
    }

    public CommandResponse Handle(ContactMsgs.UpdateDetails command) {
        var contact = _repository.GetById<Contact>(command.ContactId, command);
        contact.UpdateDetails(
            command.FirstName,
            command.LastName,
            command.Email);

        if (!contact.HasRecordedEvents) {
            var resp = MessageBuilder.From(command)
                .Build(() => new AclRequests.UpdateErpContactDetailsResp(
                    _lookup.Lookup(command.ContactId),
                    true));
            _toExternalApp.Publish(resp);
        }

        _repository.Save(contact);

        return command.Succeed();
    }

    public void Handle(ContactMsgs.DetailsUpdated message) {
        if (!_lookup.TryToFind(message.ContactId, out var xrefId) ||
            xrefId is null) {
            return;
        }

        var resp = MessageBuilder.From(message)
            .Build(() => new AclRequests.UpdateErpContactDetailsResp(
                xrefId,
                true));

        _toExternalApp.Publish(resp);
    }

    public CommandResponse Handle(AclRequests.ArchiveErpContactReq command) {
        if (!_lookup.TryToFind(command.XrefId, out var contactId)) {
            var resp = MessageBuilder.From(command)
                .Build(() => new AclRequests.ArchiveErpContactResp(
                    command.XrefId,
                    false));
            return command.Succeed();
        }

        var cmd = MessageBuilder.From(command)
            .Build(() => new ContactMsgs.ArchiveContact(
                contactId));

        var succeeded = _commandPublisher.TrySendAsync(cmd);
        return succeeded
            ? command.Succeed()
            : command.Fail();
    }

    public CommandResponse Handle(ContactMsgs.ArchiveContact command) {
        var contact = _repository.GetById<Contact>(command.ContactId, command);
        contact.Archive();

        if (!contact.HasRecordedEvents) {
            var resp = MessageBuilder.From(command)
                .Build(() => new AclRequests.ArchiveErpContactResp(
                    _lookup.Lookup(command.ContactId),
                    true));
            _toExternalApp.Publish(resp);
        }

        _repository.Save(contact);

        return command.Succeed();
    }

    public void Handle(ContactMsgs.Archived message) {
        var resp = MessageBuilder.From(message)
            .Build(() => new AclRequests.ArchiveErpContactResp(
                _lookup.Lookup(message.ContactId),
                true));
        _toExternalApp.Publish(resp);
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (!disposing || _hasBeenDisposed) {
            return;
        }

        _disposables?.Dispose();
    }
}
