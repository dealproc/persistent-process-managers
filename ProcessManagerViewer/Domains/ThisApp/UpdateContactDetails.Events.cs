using System;

using ReactiveDomain.Messaging;

namespace ProcessManagerViewer.Domains.ThisApp;

public static partial class UpdateContactDetailsMsgs {
    public class Started : Event {
        public readonly Guid UpdateContactDetailsId;
        public readonly Guid ContactId;
        public readonly string XrefId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;

        public Started(
            Guid updateContactDetailsId,
            Guid contactId,
            string xrefId,
            string firstName,
            string lastName,
            string email) {
            UpdateContactDetailsId = updateContactDetailsId;
            ContactId = contactId;
            XrefId = xrefId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }
    }

    public class CrmContactDetailsUpdated : Event {
        public readonly Guid UpdateContactDetailsId;

        public CrmContactDetailsUpdated(
            Guid updateContactDetailsId) {
            UpdateContactDetailsId = updateContactDetailsId;
        }
    }

    public class ErpContactDetailsUpdated : Event {
        public readonly Guid UpdateContactDetailsId;

        public ErpContactDetailsUpdated(
            Guid updateContactDetailsId) {
            UpdateContactDetailsId = updateContactDetailsId;
        }
    }

    public class Completed : Event {
        public readonly Guid UpdateContactDetailsId;

        public Completed(
            Guid updateContactDetailsId) {
            UpdateContactDetailsId = updateContactDetailsId;
        }
    }
}
