using System;
using System.Collections.Generic;

namespace ReactiveDomain;

internal class GenericRecorder<T> {
    private readonly List<T> _items = [];

    public bool HasRecordedItems => _items.Count > 0;

    public T[] RecordedItems => [.. _items];

    public void Record(T item) {
        ArgumentException.ThrowIfNullOrEmpty(nameof(item));

        _items.Add(item);
    }

    public void Reset() => _items.Clear();
}
