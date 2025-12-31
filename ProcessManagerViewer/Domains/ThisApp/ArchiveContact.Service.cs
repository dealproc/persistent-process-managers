using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ReactiveDomain;
using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

namespace ProcessManagerViewer.Domains.ThisApp;

public class ArchiveContactService : ReactiveDomainServiceBase,
    IHandleCommand<ArchiveContactMsgs.Start>,
    IHandleCommand<AclRequests.ArchiveContactReq> {
    private readonly ICorrelatedRepository _repository;
    private readonly ContactLookup _lookup;
    private readonly IDispatcher _dispatcher;

    public ArchiveContactService(
        ICorrelatedRepository repository,

        [FromKeyedServices(Keys.ThisApp)]
        ContactLookup lookup,

        [FromKeyedServices(Keys.ThisApp)]
        IDispatcher dispatcher,

        IConfiguredConnection connection,
        TimeProvider timeProvider,
        ILoggerFactory loggerFactory) : base(
            dispatcher,
            connection,
            timeProvider,
            loggerFactory) {
        _lookup = lookup;
        _repository = repository;
        _dispatcher = dispatcher;
    }

    public override void StartService() {
        Log.LogDebug("Starting {@ServiceName}.", GetType().Name);

        Subscribe<ArchiveContactMsgs.Start>(this);
        Subscribe<ArchiveContact, AclRequests.ArchiveCrmContactResp>((msg) => _lookup.Lookup(msg.XrefId));
        Subscribe<ArchiveContact, AclRequests.ArchiveErpContactResp>((msg) => _lookup.Lookup(msg.XrefId));
        _dispatcher.Subscribe<AclRequests.ArchiveContactReq>(this);

        Log.LogDebug("{@ServiceName} started.", GetType().Name);
    }

    public CommandResponse Handle(ArchiveContactMsgs.Start command) {
        Log.LogTrace("Handling {@Class}:{@Method}", GetType().Name, command.GetType().Name);
        if (!_lookup.TryToLookup(command.ContactId, out var xrefId) ||
            xrefId is null) {
            return command.Fail(new InvalidOperationException("Could not locate cross-reference value(s)."));
        }

        Log.LogTrace("Archive contact id: {@ArchiveContactId}", command.ArchiveContactId);

        var archive = new ArchiveContact(
            command.ArchiveContactId,
            command.ContactId,
            xrefId,
            command.Source,
            command);
        Log.LogTrace("Archive contact id before save: {@ArchiveContactId}", archive.Id);

        _repository.Save(archive);

        return command.Succeed();
    }

    public CommandResponse Handle(AclRequests.ArchiveContactReq command) {
        Log.LogTrace("Handling {@Class}:{@Method}", GetType().Name, command.GetType().Name);
        if (!_lookup.TryToLookup(command.XrefId, out var contactId) ||
            contactId == Guid.Empty) {
            return command.Fail(new InvalidOperationException("Could not locate cross-reference value(s)."));
        }

        var archive = new ArchiveContact(
            Guid.NewGuid(),
            contactId,
            command.XrefId,
            command.Source,
            command);
        Log.LogTrace("Archive contact id before save: {@ArchiveContactId}", archive.Id);

        _repository.Save(archive);

        return command.Succeed();
    }
}
