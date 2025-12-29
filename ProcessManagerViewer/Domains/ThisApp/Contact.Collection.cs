using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;

using DynamicData;

using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging.Bus;

using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ProcessManagerViewer.Domains.ThisApp;

public class ContactCollection : ReadModelBase,
    IHandle<ContactMsgs.ContactCreated>,
    IHandle<ContactMsgs.DetailsUpdated>,
    IHandle<ContactMsgs.Archived> {
    private readonly CompositeDisposable _disposables = [];
    private readonly SourceCache<ContactRm, Guid> _cache = new((o) => o.ContactId);

    public ContactCollection(
        IConfiguredConnection connection) : base(nameof(ContactCollection), connection) {

        EventStream.Subscribe<ContactMsgs.ContactCreated>(this).DisposeWith(_disposables);
        EventStream.Subscribe<ContactMsgs.DetailsUpdated>(this).DisposeWith(_disposables);
        EventStream.Subscribe<ContactMsgs.Archived>(this).DisposeWith(_disposables);

        Start<Contact>();
    }

    public IObservable<IChangeSet<ContactRm, Guid>> Connect(
        Func<ContactRm, bool>? predicate = null,
        bool suppressEmptyChangeSets = true)
        => _cache.Connect(
            predicate,
            suppressEmptyChangeSets);

    public void Handle(ContactMsgs.ContactCreated message) {
        try {
            _cache.AddOrUpdate(new ContactRm(
                message.ContactId,
                message.FirstName,
                message.LastName,
                message.Email));
        } catch (Exception exc) {
            var x = 0;
        }
    }

    public void Handle(ContactMsgs.DetailsUpdated message) {
        var contact = _cache.Lookup(message.ContactId);
        if (contact.HasValue) {
            contact.Value.FirstName = message.FirstName;
            contact.Value.LastName = message.LastName;
            contact.Value.Email = message.Email;
        }
    }

    public void Handle(ContactMsgs.Archived message) {
        var contact = _cache.Lookup(message.ContactId);
        if (contact.HasValue) {
            contact.Value.HasBeenArchived = true;
        }
    }
}

public partial class ContactRm : ReactiveObject {
    public Guid ContactId { get; init; }

    [Reactive]
    private string _firstName = "";

    [Reactive]
    private string _lastName = "";

    [Reactive]
    private string _email = "";

    [Reactive]
    private bool _hasBeenArchived = false;

    public ContactRm(
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
