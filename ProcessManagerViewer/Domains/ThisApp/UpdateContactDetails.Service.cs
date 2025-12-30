using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ReactiveDomain;
using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

namespace ProcessManagerViewer.Domains.ThisApp;

public class UpdateContactDetailsService : ReactiveDomainServiceBase,
    IHandleCommand<UpdateContactDetailsMsgs.Start> {
    private readonly ICorrelatedRepository _repository;
    private readonly ContactLookup _lookup;
    private bool _hasBeenStarted = false;

    public UpdateContactDetailsService(
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
        if (_hasBeenStarted) {
            return;
        }
        _hasBeenStarted = true;

        Log.LogDebug("Starting {@ServiceName}.", GetType().Name);

        Subscribe<UpdateContactDetailsMsgs.Start>(this);
        Subscribe<UpdateContactDetails, AclRequests.UpdateCrmContactDetailsResp>((msg) => Guid.Empty);
        Subscribe<UpdateContactDetails, AclRequests.UpdateErpContactDetailsResp>((msg) => Guid.Empty);

        Log.LogDebug("{@ServiceName} started.", GetType().Name);
    }

    public CommandResponse Handle(UpdateContactDetailsMsgs.Start command) {
        Log.LogTrace("Handling {@Class}:{@Method}", GetType().Name, command.GetType().Name);
        if (!_lookup.TryToLookup(command.ContactId, out var xref)
            || xref is null) {
            return command.Fail(new InvalidOperationException("Contact xref value could not be found."));
        }

        try {
            var ucd = new UpdateContactDetails(
                Guid.NewGuid(),
                command.ContactId,
                xref,
                command.FirstName,
                command.LastName,
                command.Email,
                command.Source,
                command);
            _repository.Save(ucd);
            return command.Succeed();
        }
        catch (Exception exc) {
            return command.Fail(exc);
        }
    }
}
