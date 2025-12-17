using System;
using System.Collections.Generic;

using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

namespace ReactiveDomain;

class CategoryStreamSubscription<TAggregate> : ISubscribeToStreams,
	IHandle<IMessage>,
	IHandle<Message>,
	IPublisher,
	IDisposable
	where TAggregate : AggregateRoot {
	private readonly Func<IListener> _getListener;
	private readonly List<IListener> _listeners = [];
	private readonly InMemoryBus _bus;
	private readonly QueuedHandler _queue;
	private bool _disposed = false;

	/// <summary>
	/// The stream of events that handlers should subscribe to.
	/// </summary>
	public ISubscriber EventStream => _bus;

	public CategoryStreamSubscription(IConfiguredConnection connection) {
		_getListener = () => connection.GetListener(GetType().Name);
		_bus = new InMemoryBus($"");
		_queue = new QueuedHandler(new AdHocHandler<IMessage>(_bus.Handle), $"");
		_queue.Start();
		AddNewListener().Start<TAggregate>(checkpoint: StreamPosition.End);
	}

	public void Handle(Message message) { ((IHandle<IMessage>)_queue).Handle(message); }
	public void Handle(IMessage message) { ((IHandle<IMessage>)_queue).Handle(message); }
	public void Publish(IMessage message) { ((IPublisher)_queue).Publish(message); }

	public IDisposable Subscribe<TMessage>(IHandle<TMessage> handle)
		where TMessage : Message
		=> EventStream.Subscribe(handle);

	private IListener AddNewListener() {
		var l = _getListener();
		lock (_listeners) {
			_listeners.Add(l);
		}
		l.EventStream.SubscribeToAll(_queue);
		return l;
	}

	public void Dispose() {
		if (_disposed) return;
		_disposed = true;

		_listeners.ForEach(l => l.Dispose());
		_listeners.Clear();
	}
}
