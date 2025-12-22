using System;

using ReactiveDomain.Messaging;

namespace ProcessManagerViewer.Domains.ThisApp;

public static partial class AddContactMsgs {
    public class Started : Event {
        public readonly Guid AddContactId;
        public readonly Guid ContactId;
        public readonly string XrefId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;

        public Started(
            Guid addContactId,
            Guid contactId,
            string xrefId,
            string firstName,
            string lastName,
            string email) {
            AddContactId = addContactId;
            ContactId = contactId;
            XrefId = xrefId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }
    }

    public class CrmContactCreated : Event {
        public readonly Guid AddContactId;

        public CrmContactCreated(
            Guid addContactId) {
            AddContactId = addContactId;
        }
    }

    public class ErpContactCreated : Event {
        public readonly Guid AddContactId;

        public ErpContactCreated(
            Guid addContactId) {
            AddContactId = addContactId;
        }
    }

    public class Completed : Event {
        public readonly Guid AddContactId;

        public Completed(
            Guid addContactId) {
            AddContactId = addContactId;
        }
    }
}
