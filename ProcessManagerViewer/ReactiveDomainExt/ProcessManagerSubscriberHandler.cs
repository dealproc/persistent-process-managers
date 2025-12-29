using System;

using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

namespace ReactiveDomain;

class ProcessManagerSubscriberHanlder<TProcessManager, TEvent> : IHandleProcessManagerSubsriberHandler, IHandle<TEvent>
	where TProcessManager : PmProcessManager, IHandle<IMessage>
	where TEvent : Event {
	private readonly IConfiguredConnection _connection;
	private readonly Func<TEvent, Guid> _getId;
	private readonly IDisposable _subscription;
	private bool _disposed = false;

	public ProcessManagerSubscriberHanlder(
		ISubscriber subscriber,
		IConfiguredConnection connection,
		Func<TEvent, Guid> getId) {
		_connection = connection;
		_getId = getId;
		_subscription = subscriber.Subscribe(this);
	}

	public void Handle(TEvent message) {
		var repository = _connection.GetCorrelatedRepository();
		if (!repository.TryGetById<TProcessManager>(_getId(message), out var pm, message)) { return; }

		pm.Handle(message);
		repository.Save(pm);
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_subscription.Dispose();
	}
}
