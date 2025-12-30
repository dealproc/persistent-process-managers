using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ReactiveDomain;
using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

namespace ProcessManagerViewer.Domains.ThisApp;

public class ArchiveContactService : ReactiveDomainServiceBase,
    IHandleCommand<ArchiveContactMsgs.Start> {
    private readonly ICorrelatedRepository _repository;
    private readonly ContactLookup _lookup;

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
    }

    public override void StartService() {
        Log.LogDebug("Starting {@ServiceName}.", GetType().Name);

        Subscribe<ArchiveContactMsgs.Start>(this);
        Subscribe<ArchiveContact, AclRequests.ArchiveCrmContactResp>((msg) => _lookup.Lookup(msg.XrefId));
        Subscribe<ArchiveContact, AclRequests.ArchiveErpContactResp>((msg) => _lookup.Lookup(msg.XrefId));

        Log.LogDebug("{@ServiceName} started.", GetType().Name);
    }

    public CommandResponse Handle(ArchiveContactMsgs.Start command) {
        Log.LogTrace("Handling {@Class}:{@Method}", GetType().Name, command.GetType().Name);
        if (!_lookup.TryToLookup(command.ContactId, out var xrefId) ||
            xrefId is null) {
            return command.Fail(new InvalidOperationException("Could not locate cross-reference value(s)."));
        }

        _repository.Save(new ArchiveContact(
            command.ArchiveContactId,
            command.ContactId,
            xrefId,
            command.Source,
            command));

        return command.Succeed();
    }
}
