using System;

using ReactiveDomain.Messaging;

namespace ProcessManagerViewer.Domains.ThisApp;

public static partial class ArchiveContactMsgs {
    public class Started : Command {
        public readonly Guid ArchiveContactId;
        public readonly Guid ContactId;
        public readonly string XrefId;

        public Started(
            Guid archiveContactId,
            Guid contactId,
            string xrefId) {
            ArchiveContactId = archiveContactId;
            ContactId = contactId;
            XrefId = xrefId;
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
