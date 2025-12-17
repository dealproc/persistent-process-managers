using System.Collections;
using System.Collections.Generic;

using ReactiveDomain;

namespace ProcessManagerViewer.Storage;

public class Stream : IStream {
    private readonly SinglyLinkedList _inner;

    public Stream() {
        _inner = new SinglyLinkedList();
    }

    public RecordedEvent Add(RecordedEvent re) {
        _inner.Add(re);
        return re;
    }

    public int Count => _inner.Count;

    public RecordedEvent this[int index] => _inner[index];
    public IEnumerator<RecordedEvent> GetEnumerator() {
        return _inner.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable)_inner).GetEnumerator();
    }
}
