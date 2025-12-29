using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ReactiveDomain;
using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

namespace ProcessManagerViewer.Domains.ThisApp;

public class AddContactService : ReactiveDomainServiceBase,
    IHandleCommand<AddContactMsgs.Start> {
    private readonly ICorrelatedRepository _repository;
    private readonly ContactLookup _lookup;

    public AddContactService(
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
        _repository = repository;
        _lookup = lookup;
    }

    public override void StartService() {
        Log.LogDebug("Starting {@ServiceName}.", GetType().Name);

        Subscribe<AddContactMsgs.Start>(this);
        Handle<AddContact, Contact, ContactMsgs.ContactCreated>((msg) => _lookup.Lookup(msg.XrefId));
        Subscribe<AddContact, AclRequests.CreateCrmContactResp>((msg) => _lookup.Lookup(msg.XrefId));
        Subscribe<AddContact, AclRequests.CreateErpContactResp>((msg) => _lookup.Lookup(msg.XrefId));

        Log.LogDebug("{@ServiceName} started.", GetType().Name);
    }

    public CommandResponse Handle(AddContactMsgs.Start command) {
        Log.LogTrace("Handling {@Class}:{@Method}", GetType().Name, command.GetType().Name);
        var add = new AddContact(
            Guid.NewGuid(),
            command.ContactId,
            Guid.NewGuid().ToString("N"),
            command.FirstName,
            command.LastName,
            command.Email,
            command);
        _repository.Save(add);
        return command.Succeed();
    }
}
