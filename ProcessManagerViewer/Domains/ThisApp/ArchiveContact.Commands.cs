using System;

using ReactiveDomain.Messaging;

namespace ProcessManagerViewer.Domains.ThisApp;

public static partial class ArchiveContactMsgs {
    public class Start : Command {
        public readonly Guid ArchiveContactId;
        public readonly Guid ContactId;
        public readonly CommandSource Source;

        public Start(
            Guid archiveContactId,
            Guid contactId,
            CommandSource source) {
            ArchiveContactId = archiveContactId;
            ContactId = contactId;
            Source = source;
        }
    }
}
