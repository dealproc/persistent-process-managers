using System;

using ReactiveDomain.Messaging;

namespace ReactiveDomain;

interface IHandleProcessManagerMessagesFromStream : IDisposable {
	void AddResolver<TEvent>(Func<TEvent, Guid> getId)
		where TEvent : Event;
}
