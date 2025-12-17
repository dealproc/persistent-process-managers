using System;

using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

namespace ReactiveDomain;

interface ISubscribeToStreams : IDisposable {
	IDisposable Subscribe<TMessage>(IHandle<TMessage> handle)
		where TMessage : Message;
}
