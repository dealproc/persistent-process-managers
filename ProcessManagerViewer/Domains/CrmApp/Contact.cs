using System;

using ReactiveDomain;
using ReactiveDomain.Messaging;

namespace ProcessManagerViewer.Domains.CrmApp;

public class Contact : AggregateRoot {
    private string _firstName = "";
    private string _lastName = "";
    private string _email = "";

    public bool IsArchived { get; private set; }

    public Contact(
        Guid contactId,
        string xrefId,
        string firstName,
        string lastName,
        string email,
        ICorrelatedMessage msg)
        : base(
            msg) {
        RegisterEvents();
        Raise(new ContactMsgs.ContactCreated(
            contactId,
            xrefId,
            firstName,
            lastName,
            email));
    }

    public Contact() : base() {
        RegisterEvents();
    }

    private void RegisterEvents() {
        Register<ContactMsgs.ContactCreated>(Apply);
        Register<ContactMsgs.DetailsUpdated>(Apply);
        Register<ContactMsgs.Archived>(Apply);
    }

    public void UpdateDetails(
        string firstName,
        string lastName,
        string email) {
        if (_firstName.Equals(firstName) &&
            _lastName.Equals(lastName) &&
            _email.Equals(email)) {
            return;
        }

        Raise(new ContactMsgs.DetailsUpdated(
            Id,
            firstName,
            lastName,
            email));
    }

    public void Archive() {
        if (IsArchived) {
            return;
        }

        Raise(new ContactMsgs.Archived(
            Id));
    }

    private void Apply(ContactMsgs.ContactCreated msg) {
        Id = msg.ContactId;
        _firstName = msg.FirstName;
        _lastName = msg.LastName;
        _email = msg.Email;
    }

    private void Apply(ContactMsgs.DetailsUpdated msg) {
        _firstName = msg.FirstName;
        _lastName = msg.LastName;
        _email = msg.Email;
    }

    private void Apply(ContactMsgs.Archived _) {
        IsArchived = true;
    }
}
