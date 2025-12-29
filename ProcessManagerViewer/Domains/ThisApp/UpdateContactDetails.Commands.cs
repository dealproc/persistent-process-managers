using System;

using ReactiveDomain.Messaging;

namespace ProcessManagerViewer.Domains.ThisApp;

public static partial class UpdateContactDetailsMsgs {
    public class Start : Command {
        public readonly Guid UpdateContactDetailsId;
        public readonly Guid ContactId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;

        public Start(
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
}
