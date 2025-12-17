using System;
using System.Collections.Generic;

namespace ReactiveDomain;

internal class Recorder<T> {
	private readonly List<T> _items = [];

	public T[] RecordedItems => [.. _items];
	public bool HasRecordedItems => _items.Count > 0;

	public void Record(T item) {
		ArgumentNullException.ThrowIfNull(item, nameof(item));

		_items.Add(item);
	}

	public void Reset() => _items.Clear();
}
