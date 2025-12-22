using System;

using ReactiveDomain.Messaging;

namespace ProcessManagerViewer.Domains.ThisApp;

public static partial class ContactMsgs {
    public class CreateContact : Command {
        public readonly Guid ContactId;
        public readonly string XrefId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;

        public CreateContact(
            Guid contactId,
            string xrefId,
            string firstName,
            string lastName,
            string email) {
            ContactId = contactId;
            XrefId = xrefId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }
    }

    public class UpdateDetails : Command {
        public readonly Guid ContactId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;

        public UpdateDetails(
            Guid contactId,
            string firstName,
            string lastName,
            string email) {
            ContactId = contactId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }
    }

    public class ArchiveContact : Command {
        public readonly Guid ContactId;

        public ArchiveContact(
            Guid contactId) {
            ContactId = contactId;
        }
    }
}
