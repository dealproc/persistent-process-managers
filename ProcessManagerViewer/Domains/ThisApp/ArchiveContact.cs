using System;

using ReactiveDomain;
using ReactiveDomain.Messaging;

namespace ProcessManagerViewer.Domains.ThisApp;

public class ArchiveContact : PersistentProcessManager {
    private bool _inErp = false;
    private bool _inCrm = false;

    public bool IsCompleted { get; private set; }

    public ArchiveContact(
        Guid archiveContactId,
        Guid contactId,
        string xrefId,
        ICorrelatedMessage msg) : base(msg) {
        RegisterEvents();

        Raise(new ArchiveContactMsgs.Started(
            archiveContactId,
            contactId,
            xrefId));
        Raise(new AclRequests.ArchiveCrmContactReq(
            xrefId));
        Raise(new AclRequests.ArchiveErpContactReq(
            xrefId));
    }

    public ArchiveContact() : base() {
        RegisterEvents();
    }

    private void RegisterEvents() {
        Register<ArchiveContactMsgs.Started>(Apply);
        Register<ArchiveContactMsgs.CrmContactArchived>(Apply);
        Register<ArchiveContactMsgs.ErpContactArchived>(Apply);
        Register<ArchiveContactMsgs.Completed>(Apply);
    }

    public override void Handle(IMessage message) {
        if (IsCompleted) {
            return;
        }

        switch (message) {
            case AclRequests.ArchiveCrmContactResp resp:
                Raise(new ArchiveContactMsgs.CrmContactArchived(
                    Id));
                break;
            case AclRequests.ArchiveErpContactResp resp:
                Raise(new ArchiveContactMsgs.ErpContactArchived(
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
            Raise(new ArchiveContactMsgs.Completed(
                Id));
        }
    }

    public override void OnTimeout(int numberOfRetries, TimeProvider tp) {
        throw new NotImplementedException();
    }

    private void Apply(ArchiveContactMsgs.Started msg) {
        Id = msg.ArchiveContactId;
    }

    private void Apply(ArchiveContactMsgs.CrmContactArchived _) {
        _inCrm = true;
    }

    private void Apply(ArchiveContactMsgs.ErpContactArchived _) {
        _inErp = true;
    }

    private void Apply(ArchiveContactMsgs.Completed _) {
        IsCompleted = true;
    }
}
