using System;

using ReactiveDomain.Messaging;

namespace ProcessManagerViewer.Domains.ThisApp;

public static partial class ArchiveContactMsgs {
    public class Start : Command {
        public readonly Guid ArchiveContactId;
        public readonly Guid ContactId;

        public Start(
            Guid archiveContactId,
            Guid contactId) {
            ArchiveContactId = archiveContactId;
            ContactId = contactId;
        }
    }
}
