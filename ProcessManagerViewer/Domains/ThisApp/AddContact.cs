using System;

using ReactiveDomain;
using ReactiveDomain.Messaging;

namespace ProcessManagerViewer.Domains.ThisApp;

internal class AddContact : PersistentProcessManager {
    private bool _inErp = false;
    private bool _inCrm = false;

    public bool IsCompleted { get; private set; }

    public AddContact(
        Guid addContactId,
        Guid contactId,
        string xrefId,
        string firstName,
        string lastName,
        string email,
        ICorrelatedMessage msg) : base(
            msg) {
        RegisterEvents();

        Raise(new AddContactMsgs.Started(
            addContactId,
            contactId,
            xrefId,
            firstName,
            lastName,
            email));
        Raise(new AclRequests.CreateCrmContactReq(
            xrefId,
            firstName,
            lastName,
            email));
        Raise(new AclRequests.CreateErpContactReq(
            xrefId,
            firstName,
            lastName,
            email));
    }

    public AddContact() {
        RegisterEvents();
    }

    private void RegisterEvents() {
        Register<AddContactMsgs.Started>(Apply);
        Register<AddContactMsgs.CrmContactCreated>(Apply);
        Register<AddContactMsgs.ErpContactCreated>(Apply);
        Register<AddContactMsgs.Completed>(Apply);
    }

    public override void Handle(IMessage message) {
        if (IsCompleted) {
            return;
        }

        switch (message) {
            case AclRequests.CreateCrmContactResp resp:
                Raise(new AddContactMsgs.CrmContactCreated(
                    Id));
                break;
            case AclRequests.CreateErpContactReq resp:
                Raise(new AddContactMsgs.ErpContactCreated(
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
            Raise(new AddContactMsgs.Completed(
                Id));
        }
    }

    private void Apply(AddContactMsgs.Started msg) {
        Id = msg.AddContactId;
    }

    private void Apply(AddContactMsgs.CrmContactCreated _) {
        _inCrm = true;
    }

    private void Apply(AddContactMsgs.ErpContactCreated _) {
        _inErp = true;
    }

    private void Apply(AddContactMsgs.Completed _) {
        IsCompleted = true;
    }

    public override void OnTimeout(int numberOfRetries, TimeProvider tp) {
        throw new NotImplementedException();
    }
}
