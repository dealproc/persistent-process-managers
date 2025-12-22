using System;

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
        ContactLookup lookup,
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

        Subscribe<ArchiveContactMsgs.Start>(this);
        Subscribe<ArchiveContact, AclRequests.ArchiveCrmContactResp>((msg) => _lookup.Lookup(msg.XrefId));
        Subscribe<ArchiveContact, AclRequests.ArchiveErpContactResp>((msg) => _lookup.Lookup(msg.XrefId));
    }

    public CommandResponse Handle(ArchiveContactMsgs.Start command) {
        _repository.Save(new ArchiveContact(
            command.ArchiveContactId,
            command.ContactId,
            command.XrefId,
            command));
        return command.Succeed();
    }
}
