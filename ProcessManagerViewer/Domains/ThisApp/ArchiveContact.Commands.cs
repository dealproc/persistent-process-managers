using System;

using ReactiveDomain.Messaging;

namespace ProcessManagerViewer.Domains.ThisApp;

public static partial class ArchiveContactMsgs {
    public class Start : Command {
        public readonly Guid ArchiveContactId;
        public readonly Guid ContactId;
        public readonly string XrefId;

        public Start(
            Guid archiveContactId,
            Guid contactId,
            string xrefId) {
            ArchiveContactId = archiveContactId;
            ContactId = contactId;
            XrefId = xrefId;
        }
    }
}
