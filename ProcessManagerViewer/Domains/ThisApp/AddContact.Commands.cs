using System;

using ReactiveDomain.Messaging;

namespace ProcessManagerViewer.Domains.ThisApp;

public static partial class AddContactMsgs {
    public class Start : Command {
        public readonly Guid ContactId;
        public readonly string XrefId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;

        public Start(
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
}
