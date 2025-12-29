using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging;

namespace ReactiveDomain;

class ProcessManagerStreamHandler<TAggregate, TStream> : IHandleProcessManagerMessagesFromStream
    where TAggregate : PmProcessManager
    where TStream : AggregateRoot {
    private readonly Dictionary<Type, Func<IMessage, Guid>> _idResolvers = [];
    private readonly IDisposable _subscription;
    private readonly Channel<(Event, Func<IMessage, Guid>)> _queue = Channel.CreateBounded<(Event, Func<IMessage, Guid>)>(new BoundedChannelOptions(30) {
        FullMode = BoundedChannelFullMode.Wait
    });
    private readonly CancellationTokenSource _cts;

    public ProcessManagerStreamHandler(IConfiguredConnection connection, CancellationToken token = default) {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        _subscription = connection.Connection.SubscribeToStream(
            connection.StreamNamer.GenerateForCategory(typeof(TStream)),
            (recorded) => {
                if (recorded is null) {
                    return;
                }

                var deserialized = connection.Serializer.Deserialize(recorded);

                if (deserialized is not Event @event) {
                    return;
                }

                if (!_idResolvers.TryGetValue(@event.GetType(), out var resolver)) {
                    return;
                }

                _queue.Writer.TryWrite((@event, resolver));
            });
        _ = Task.Factory.StartNew(async () => {
            var repository = connection.GetCorrelatedRepository();

            while (!_cts.IsCancellationRequested) {
                try {
                    if (!(await _queue.Reader.WaitToReadAsync(_cts.Token))) {
                        continue;
                    }

                    var (@event, resolver) = await _queue.Reader.ReadAsync(_cts.Token);
                    if (!repository.TryGetById<TAggregate>(resolver(@event), out var pm, @event)) {
                        continue;
                    }

                    pm.Handle(@event);
                    repository.Save(pm);
                }
                catch {
                    _cts.Cancel();
                }
            }

        }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    public void AddResolver<TEvent>(Func<TEvent, Guid> getId)
        where TEvent : Event {
        _idResolvers[typeof(TEvent)] = (msg) => getId((TEvent)msg);
    }

    public void Dispose() {
        _subscription.Dispose();
    }
}
