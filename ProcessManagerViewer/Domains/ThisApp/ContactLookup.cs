using System;
using System.Collections.Generic;

using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging.Bus;

namespace ProcessManagerViewer.Domains.ThisApp;

public class ContactLookup : ReadModelBase,
    IHandle<ContactMsgs.ContactCreated> {
    private Dictionary<Guid, string> _internalIdToXref = [];
    private Dictionary<string, Guid> _xrefToInternalId = [];

    public ContactLookup(IConfiguredConnection connection) : base($"ThisApp:{nameof(ContactLookup)}", connection) {
        EventStream.Subscribe<ContactMsgs.ContactCreated>(this);

        Start<Contact>();
    }

    public void Handle(ContactMsgs.ContactCreated message) {
        _internalIdToXref[message.ContactId] = message.XrefId;
        _xrefToInternalId[message.XrefId] = message.ContactId;
    }

    public void SetValues(Guid contactId, string xrefId) {
        _internalIdToXref[contactId] = xrefId;
        _xrefToInternalId[xrefId] = contactId;
    }

    public string Lookup(Guid contactId)
        => _internalIdToXref[contactId];

    public Guid Lookup(string xrefId)
        => _xrefToInternalId[xrefId];

    public bool TryToLookup(Guid contactId, out string? xrefId)
        => _internalIdToXref.TryGetValue(contactId, out xrefId);

    public bool TryToLookup(string xrefId, out Guid contactId)
        => _xrefToInternalId.TryGetValue(xrefId, out contactId);
}
