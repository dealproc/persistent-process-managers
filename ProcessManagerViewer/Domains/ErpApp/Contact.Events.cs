using System;

using ReactiveDomain.Messaging;

namespace ProcessManagerViewer.Domains.ErpApp;

public static partial class ContactMsgs {
    public class ContactCreated : Event {
        public readonly Guid ContactId;
        public readonly string XrefId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;

        public ContactCreated(
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

    public class DetailsUpdated : Event {
        public readonly Guid ContactId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;

        public DetailsUpdated(
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

    public class Archived : Event {
        public readonly Guid ContactId;

        public Archived(
            Guid contactId) {
            ContactId = contactId;
        }
    }
}
