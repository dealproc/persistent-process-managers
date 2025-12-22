using System;

using Microsoft.Extensions.Logging;

using ReactiveDomain;
using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

namespace ProcessManagerViewer.Domains.ThisApp;

public class AddContactService : ReactiveDomainServiceBase,
    IHandleCommand<AddContactMsgs.Start> {
    private readonly ICorrelatedRepository _repository;

    public AddContactService(
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
        _repository = repository;

        Subscribe<AddContactMsgs.Start>(this);
        Subscribe<AddContact, AclRequests.CreateCrmContactResp>((msg) => lookup.Lookup(msg.XrefId));
        Subscribe<AddContact, AclRequests.CreateErpContactResp>((msg) => lookup.Lookup(msg.XrefId));
    }

    public CommandResponse Handle(AddContactMsgs.Start command) {
        _repository.Save(new AddContact(
            Guid.NewGuid(),
            command.ContactId,
            command.XrefId,
            command.FirstName,
            command.LastName,
            command.Email,
            command));
        return command.Succeed();
    }
}
