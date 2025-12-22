using System;

using ReactiveDomain;
using ReactiveDomain.Messaging;

namespace ProcessManagerViewer.Domains.ThisApp;

public class UpdateContactDetails : PersistentProcessManager {
    private bool _inErp = false;
    private bool _inCrm = false;

    public bool IsCompleted { get; private set; }

    public UpdateContactDetails(
        Guid updateContactDetailsId,
        Guid contactId,
        string xrefId,
        string firstName,
        string lastName,
        string email,
        ICorrelatedMessage msg) : base(msg) {
        RegisterEvents();

        Raise(new UpdateContactDetailsMsgs.Started(
            updateContactDetailsId,
            contactId,
            xrefId,
            firstName,
            lastName,
            email));
        Raise(new AclRequests.UpdateCrmContactDetailsReq(
            xrefId,
            firstName,
            lastName,
            email));
        Raise(new AclRequests.UpdateErpContactDetailsReq(
            xrefId,
            firstName,
            lastName,
            email));
    }

    public UpdateContactDetails() : base() {
        RegisterEvents();
    }

    private void RegisterEvents() {
        Register<UpdateContactDetailsMsgs.Started>(Apply);
        Register<UpdateContactDetailsMsgs.CrmContactDetailsUpdated>(Apply);
        Register<UpdateContactDetailsMsgs.ErpContactDetailsUpdated>(Apply);
        Register<UpdateContactDetailsMsgs.Completed>(Apply);
    }

    public override void Handle(IMessage message) {
        if (IsCompleted) {
            return;
        }

        switch (message) {
            case AclRequests.UpdateCrmContactDetailsResp:
                Raise(new UpdateContactDetailsMsgs.CrmContactDetailsUpdated(
                    Id));
                break;
            case AclRequests.UpdateErpContactDetailsResp:
                Raise(new UpdateContactDetailsMsgs.ErpContactDetailsUpdated(
                    Id));
                break;
            default:
                return;
        }

        TryToComplete();
    }

    private void TryToComplete() {
        if (IsCompleted) {
            return;
        }

        if (_inCrm && _inErp) {
            Raise(new UpdateContactDetailsMsgs.Completed(
                Id));
        }
    }

    public override void OnTimeout(int numberOfRetries, TimeProvider tp) {
        throw new NotImplementedException();
    }

    private void Apply(UpdateContactDetailsMsgs.Started msg) {
        Id = msg.UpdateContactDetailsId;
    }

    private void Apply(UpdateContactDetailsMsgs.CrmContactDetailsUpdated _) {
        _inCrm = true;
    }

    private void Apply(UpdateContactDetailsMsgs.ErpContactDetailsUpdated _) {
        _inErp = true;
    }

    private void Apply(UpdateContactDetailsMsgs.Completed msg) {
        IsCompleted = true;
    }
}
