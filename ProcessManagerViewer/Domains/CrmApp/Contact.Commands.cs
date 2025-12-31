using System;

using ReactiveDomain.Messaging;

namespace ProcessManagerViewer.Domains.CrmApp;

public static partial class ContactMsgs {
    public class CreateContact : Command {
        public readonly Guid ContactId;
        public readonly string XrefId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;
        public readonly CommandSource Source;

        public CreateContact(
            Guid contactId,
            string xrefId,
            string firstName,
            string lastName,
            string email,
            CommandSource source) {
            ContactId = contactId;
            XrefId = xrefId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Source = source;
        }
    }

    public class UpdateDetails : Command {
        public readonly Guid ContactId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;
        public readonly CommandSource Source;

        public UpdateDetails(
            Guid contactId,
            string firstName,
            string lastName,
            string email,
            CommandSource source) {
            ContactId = contactId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Source = source;
        }
    }

    public class ArchiveContact : Command {
        public readonly Guid ContactId;
        public readonly CommandSource Source;

        public ArchiveContact(
            Guid contactId,
            CommandSource source) {
            ContactId = contactId;
            Source = source;
        }
    }
}
