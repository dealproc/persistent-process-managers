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
    IHandle<AclRequests.CreateErpContactReq>,
    IHandleCommand<ContactMsgs.CreateContact>,
    IHandle<ContactMsgs.ContactCreated>,

    IHandle<AclRequests.UpdateErpContactDetailsReq>,
    IHandleCommand<ContactMsgs.UpdateDetails>,
    IHandle<ContactMsgs.DetailsUpdated>,

    IHandle<AclRequests.ArchiveErpContactReq>,
    IHandleCommand<ContactMsgs.ArchiveContact>,
    IHandle<ContactMsgs.Archived> {
    private readonly ISubscriber _fromExternalApp;
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
        ISubscriber fromExternalApp,
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

        _fromExternalApp = fromExternalApp;
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

        _fromExternalApp.Subscribe<AclRequests.CreateErpContactReq>(this).DisposeWith(_disposables);
        _commandSubscriber.Subscribe<ContactMsgs.CreateContact>(this).DisposeWith(_disposables);
        EventStream.Subscribe<ContactMsgs.ContactCreated>(this).DisposeWith(_disposables);

        _fromExternalApp.Subscribe<AclRequests.UpdateErpContactDetailsReq>(this).DisposeWith(_disposables);
        _commandSubscriber.Subscribe<ContactMsgs.UpdateDetails>(this).DisposeWith(_disposables);
        EventStream.Subscribe<ContactMsgs.DetailsUpdated>(this).DisposeWith(_disposables);

        _fromExternalApp.Subscribe<AclRequests.ArchiveErpContactReq>(this).DisposeWith(_disposables);
        _commandSubscriber.Subscribe<ContactMsgs.ArchiveContact>(this).DisposeWith(_disposables);
        EventStream.Subscribe<ContactMsgs.Archived>(this).DisposeWith(_disposables);

        Start<Contact>(checkpoint: long.MaxValue - 1);
    }

    public void Handle(AclRequests.CreateErpContactReq message) {
        if (_lookup.TryToFind(message.XrefId, out _)) {
            var resp = MessageBuilder.From(message)
                .Build(() => new AclRequests.CreateCrmContactResp(
                    message.XrefId,
                    true));
            _toExternalApp.Publish(resp);
            return;
        }

        var cmd = MessageBuilder.From(message)
            .Build(() => new ContactMsgs.CreateContact(
                Guid.NewGuid(),
                message.XrefId,
                message.FirstName,
                message.LastName,
                message.Email));
        _commandPublisher.Send(cmd);
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

    public void Handle(AclRequests.UpdateErpContactDetailsReq message) {
        if (!_lookup.TryToFind(message.XrefId, out var contactId)) {
            var resp = MessageBuilder.From(message)
                .Build(() => new AclRequests.UpdateErpContactDetailsResp(
                    message.XrefId,
                    false));
            return;
        }

        var cmd = MessageBuilder.From(message)
            .Build(() => new ContactMsgs.UpdateDetails(
                contactId,
                message.FirstName,
                message.LastName,
                message.Email));

        _commandPublisher.Send(cmd);
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

    public void Handle(AclRequests.ArchiveErpContactReq message) {
        if (!_lookup.TryToFind(message.XrefId, out var contactId)) {
            var resp = MessageBuilder.From(message)
                .Build(() => new AclRequests.ArchiveErpContactResp(
                    message.XrefId,
                    false));
            return;
        }

        var cmd = MessageBuilder.From(message)
            .Build(() => new ContactMsgs.ArchiveContact(
                contactId));
        _commandPublisher.Send(cmd);
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
