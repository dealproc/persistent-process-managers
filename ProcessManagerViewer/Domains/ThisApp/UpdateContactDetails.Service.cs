using System;

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

    public UpdateContactDetailsService(
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
        _lookup = lookup;

        Subscribe<UpdateContactDetailsMsgs.Start>(this);
        Subscribe<UpdateContactDetails, AclRequests.UpdateCrmContactDetailsResp>((msg) => Guid.Empty);
        Subscribe<UpdateContactDetails, AclRequests.UpdateErpContactDetailsResp>((msg) => Guid.Empty);
    }

    public CommandResponse Handle(UpdateContactDetailsMsgs.Start command) {
        _repository.Save(new UpdateContactDetails(
            command.UpdateContactDetailsId,
            command.ContactId,
            command.XrefId,
            command.FirstName,
            command.LastName,
            command.Email,
            command));
        return command.Succeed();
    }
}
