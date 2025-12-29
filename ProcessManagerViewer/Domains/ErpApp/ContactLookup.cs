using System;
using System.Collections.Generic;

using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging.Bus;

namespace ProcessManagerViewer.Domains.ErpApp;

public class ContactLookup : ReadModelBase,
    IHandle<ContactMsgs.ContactCreated> {
    private Dictionary<Guid, string> _internalIdToXref = [];
    private Dictionary<string, Guid> _xrefToInternalId = [];

    public ContactLookup(IConfiguredConnection connection) : base($"ErpApp:{nameof(ContactLookup)}", connection) {
        EventStream.Subscribe<ContactMsgs.ContactCreated>(this);

        Start<Contact>();
    }

    public void Handle(ContactMsgs.ContactCreated message) {
        _internalIdToXref[message.ContactId] = message.XrefId;
        _xrefToInternalId[message.XrefId] = message.ContactId;
    }

    public string Lookup(Guid contactId)
        => _internalIdToXref[contactId];

    public bool TryToFind(Guid contactId, out string? xrefId)
        => _internalIdToXref.TryGetValue(contactId, out xrefId);

    public Guid Lookup(string xrefId)
        => _xrefToInternalId[xrefId];

    public bool TryToFind(string xrefId, out Guid contactId)
        => _xrefToInternalId.TryGetValue(xrefId, out contactId);

    internal void Set(string xrefId, Guid contactId) {
        _internalIdToXref[contactId] = xrefId;
        _xrefToInternalId[xrefId] = contactId;
    }
}
