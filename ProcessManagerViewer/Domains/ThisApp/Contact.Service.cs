using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

namespace ProcessManagerViewer.Domains.ThisApp;

public class ContactService : ReadModelBase, IHostedLifecycleService,
    IHandleCommand<ContactMsgs.CreateContact>,
    IHandleCommand<ContactMsgs.UpdateDetails>,
    IHandleCommand<ContactMsgs.ArchiveContact>,

    IHandle<ContactMsgs.ContactCreated>,
    IHandle<ContactMsgs.DetailsUpdated>,
    IHandle<ContactMsgs.Archived> {

    private readonly ICorrelatedRepository _repository;
    private readonly ContactLookup _lookup;
    private readonly CompositeDisposable _disposables = [];

    public ContactService(
        [FromKeyedServices(Keys.ThisApp)]
        ICommandSubscriber commandSubscriber,

        ICorrelatedRepository repository,
        ContactLookup lookup,

        IConfiguredConnection connection) : base(
            nameof(ContactService),
            connection) {
        _repository = repository;
        _lookup = lookup;

        commandSubscriber.Subscribe<ContactMsgs.CreateContact>(this).DisposeWith(_disposables);
        commandSubscriber.Subscribe<ContactMsgs.UpdateDetails>(this).DisposeWith(_disposables);
        commandSubscriber.Subscribe<ContactMsgs.ArchiveContact>(this).DisposeWith(_disposables);

        EventStream.Subscribe<ContactMsgs.ContactCreated>(this).DisposeWith(_disposables);
        EventStream.Subscribe<ContactMsgs.DetailsUpdated>(this).DisposeWith(_disposables);
        EventStream.Subscribe<ContactMsgs.Archived>(this).DisposeWith(_disposables);
    }

    public CommandResponse Handle(ContactMsgs.CreateContact command) {
        _repository.Save(new Contact(
            command.ContactId,
            command.XrefId,
            command.FirstName,
            command.LastName,
            command.Email,
            command));
        return command.Succeed();
    }

    public CommandResponse Handle(ContactMsgs.UpdateDetails command) {
        var contact = _repository.GetById<Contact>(command.ContactId, command);
        contact.UpdateDetails(
            command.FirstName,
            command.LastName,
            command.Email);
        _repository.Save(contact);
        return command.Succeed();
    }

    public CommandResponse Handle(ContactMsgs.ArchiveContact command) {
        var contact = _repository.GetById<Contact>(command.ContactId, command);
        contact.Archive();
        _repository.Save(contact);
        return command.Succeed();
    }

    public void Handle(ContactMsgs.ContactCreated message) {
        try {
            _repository.Save(new AddContact(
                Guid.NewGuid(),
                message.ContactId,
                message.XrefId,
                message.FirstName,
                message.LastName,
                message.Email,
                message));
        }
        catch {
            // squelch for now, but needs to have a log error & a system message sent.
        }
    }

    public void Handle(ContactMsgs.DetailsUpdated message) {
        try {
            _repository.Save(new UpdateContactDetails(
                Guid.NewGuid(),
                message.ContactId,
                _lookup.Lookup(message.ContactId),
                message.FirstName,
                message.LastName,
                message.Email,
                message));
        }
        catch {
            // squelch for now, but needs to have a log error & a system message sent.
        }
    }

    public void Handle(ContactMsgs.Archived message) {
        try {
            _repository.Save(new ArchiveContact(
                Guid.NewGuid(),
                message.ContactId,
                _lookup.Lookup(message.ContactId),
                message));
        }
        catch {
            // squelch for now, but needs to have a log error & a system message sent.
        }
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StartedAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StartingAsync(CancellationToken cancellationToken) {
        Start<Contact>(checkpoint: long.MaxValue - 1);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StoppedAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StoppingAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}
