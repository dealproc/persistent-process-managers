using System;

using ReactiveDomain.Messaging;

namespace ProcessManagerViewer.Domains.ThisApp;

public static partial class ArchiveContactMsgs {
    public class Started : Command {
        public readonly Guid ArchiveContactId;
        public readonly Guid ContactId;

        public Started(
            Guid archiveContactId,
            Guid contactId) {
            ArchiveContactId = archiveContactId;
            ContactId = contactId;
        }
    }

    public class CrmContactArchived : Event {
        public readonly Guid ArchiveContactId;

        public CrmContactArchived(
            Guid archiveContactId) {
            ArchiveContactId = archiveContactId;
        }
    }

    public class ErpContactArchived : Event {
        public readonly Guid ArchiveContactId;

        public ErpContactArchived(
            Guid archiveContactId) {
            ArchiveContactId = archiveContactId;
        }
    }

    public class Completed : Event {
        public readonly Guid ArchiveContactId;

        public Completed(
            Guid archiveContactId) {
            ArchiveContactId = archiveContactId;
        }
    }
}
