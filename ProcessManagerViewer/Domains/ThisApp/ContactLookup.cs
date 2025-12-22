using System;
using System.Collections.Generic;

using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging.Bus;

namespace ProcessManagerViewer.Domains.ThisApp;

public class ContactLookup : ReadModelBase,
    IHandle<ContactMsgs.ContactCreated> {
    private Dictionary<Guid, string> _internalIdToXref = [];
    private Dictionary<string, Guid> _xrefToInternalId = [];

    public ContactLookup(string name, IConfiguredConnection connection) : base(name, connection) {
        EventStream.Subscribe<ContactMsgs.ContactCreated>(this);

        Start<Contact>();
    }

    public void Handle(ContactMsgs.ContactCreated message) {
        _internalIdToXref[message.ContactId] = message.XrefId;
        _xrefToInternalId[message.XrefId] = message.ContactId;
    }

    public string Lookup(Guid contactId)
        => _internalIdToXref[contactId];

    public Guid Lookup(string xrefId)
        => _xrefToInternalId[xrefId];
}
