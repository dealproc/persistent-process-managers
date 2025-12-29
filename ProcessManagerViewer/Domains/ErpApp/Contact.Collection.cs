using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;

using DynamicData;

using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging.Bus;

using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ProcessManagerViewer.Domains.ErpApp;

public partial class ContactCollection : ReadModelBase,
    IHandle<ContactMsgs.ContactCreated>,
    IHandle<ContactMsgs.DetailsUpdated>,
    IHandle<ContactMsgs.Archived> {

    private readonly CompositeDisposable _disposables = [];
    private readonly SourceCache<ContactDetails, Guid> _cache = new((obj) => obj.ContactId);

    public ContactCollection(
        IConfiguredConnection connection) : base(
            nameof(ContactCollection),
            connection) {

        EventStream.Subscribe<ContactMsgs.ContactCreated>(this).DisposeWith(_disposables);
        EventStream.Subscribe<ContactMsgs.DetailsUpdated>(this).DisposeWith(_disposables);
        EventStream.Subscribe<ContactMsgs.Archived>(this).DisposeWith(_disposables);

        Start<Contact>();
    }

    public IObservable<IChangeSet<ContactDetails, Guid>> Connect(
        Func<ContactDetails, bool>? predicate = null,
        bool suppressEmptyChangeSets = true)
        => _cache.Connect(
            predicate,
            suppressEmptyChangeSets);

    public void Handle(ContactMsgs.ContactCreated message) {
        _cache.AddOrUpdate(new ContactDetails(
            message.ContactId,
            message.XrefId,
            message.FirstName,
            message.LastName,
            message.Email));
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

    public partial class ContactDetails : ReactiveObject, IContactRm {
        public Guid ContactId { get; init; }
        public string XrefId { get; init; }

        [Reactive]
        private string _firstName = "";

        [Reactive]
        private string _lastName = "";

        [Reactive]
        private string _email = "";

        [Reactive]
        private bool _hasBeenArchived = false;

        public ContactDetails(
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
