using System;

using ReactiveDomain;
using ReactiveDomain.Messaging;

namespace ProcessManagerViewer.Domains.ThisApp;

public class ArchiveContact : PmProcessManager {
    private bool _inErp = false;
    private bool _inCrm = false;

    public bool IsCompleted { get; private set; }

    public ArchiveContact(
        Guid archiveContactId,
        Guid contactId,
        string xrefId,
        CommandSource source,
        ICorrelatedMessage msg) : base(msg) {
        RegisterEvents();

        Raise(new ArchiveContactMsgs.Started(
            archiveContactId,
            contactId));

        Raise(new ContactMsgs.ArchiveContact(
            contactId));

        if (source == CommandSource.Crm) {
            Raise(new ArchiveContactMsgs.CrmContactArchived(archiveContactId));
        } else {
            Raise(new AclRequests.ArchiveCrmContactReq(
                xrefId,
                source));
        }

        if (source == CommandSource.Erp) {
            Raise(new ArchiveContactMsgs.ErpContactArchived(archiveContactId));
        } else {
            Raise(new AclRequests.ArchiveErpContactReq(
                xrefId,
                source));
        }
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

    public override void Timeout(int retryCount) {
        throw new NotImplementedException();
    }

    protected override void OnHandle(IMessage message) {
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
