using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

namespace ReactiveDomain;

public abstract partial class ReactiveDomainServiceBase : IReactiveDomainService {
    private readonly IConfiguredConnection _connection;
    private readonly List<IDisposable> _subscriptions = [];
    private readonly Dictionary<Type, ISubscribeToStreams> _streamListeners = [];
    private readonly Dictionary<Type, IHandleProcessManagerMessagesFromStream> _processManagerListeners = [];

    protected ILogger Log { get; init; }
    protected TimeProvider TimeProvider { get; init; }
    protected ICorrelatedRepository Repository { get; init; }
    protected ICommandSubscriber CommandSubscriber { get; init; }
    protected ISubscriber Subscriber { get; init; }

    protected ReactiveDomainServiceBase(
        IDispatcher dispatcher,
        IConfiguredConnection connection,
        TimeProvider timeProvider,
        ILoggerFactory loggerFactory) {
        _connection = connection;

        Log = loggerFactory.CreateLogger(GetType());
        TimeProvider = timeProvider;
        Repository = connection.GetCorrelatedRepository();
        CommandSubscriber = dispatcher;
        Subscriber = dispatcher;
    }

    public abstract void StartService();

    /// <summary>
    /// Creates a subscription to handle commands on the provided <see cref="ICommandSubscriber"/>.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    /// <param name="handle"></param>
    protected void Subscribe<TCommand>(IHandleCommand<TCommand> handle)
        where TCommand : Command
        => _subscriptions.Add(CommandSubscriber.Subscribe(handle));

    /// <summary>
    /// Creates a subscription on the provided <see cref="ISubscriber" />.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="handle"></param>
    protected void Subscribe<TEvent>(IHandle<TEvent> handle)
        where TEvent : Event
        => _subscriptions.Add(Subscriber.Subscribe(handle));

    /// <summary>
    /// Creates a subscription on the provided <see cref="ISubscriber"/> to listen for events and apply
    /// them to the provided <typeparamref name="TProcessManager"/> using the value <paramref name="getId"/>
    /// returns as the key to retrieve the correct <typeparamref name="TProcessManager"/> to be acted upon.
    /// </summary>
    /// <typeparam name="TProcessManager"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="getId"></param>
    protected void Subscribe<TProcessManager, TEvent>(Func<TEvent, Guid> getId)
        where TProcessManager : PmProcessManager, IHandle<IMessage>
        where TEvent : Event
        => _subscriptions.Add(new ProcessManagerSubscriberHanlder<TProcessManager, TEvent>(Subscriber, _connection, getId));

    /// <summary>
    /// This listens on the provided <see cref="IStreamStoreConnection"/> for <typeparamref name="TMessage"/> messages
    /// published on the <typeparamref name="TAggregate"/> category strema and allows for handling them in "your" code.
    /// </summary>
    /// <typeparam name="TAggregate"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="handle"></param>
    protected void Handle<TAggregate, TMessage>(IHandle<TMessage> handle)
        where TAggregate : AggregateRoot
        where TMessage : Message {
        if (!_streamListeners.TryGetValue(typeof(TAggregate), out var subscriber)) {
            subscriber = new CategoryStreamSubscription<TAggregate>(_connection);
            _subscriptions.Add(subscriber);
            _streamListeners[typeof(TAggregate)] = subscriber;
        }

        _subscriptions.Add(subscriber.Subscribe(handle));
    }


    /// <summary>
    /// This listens on the provided <see cref="IStreamStoreConnection"/> to capture <typeparamref name="TEvent"/> messages and handle them within the 
    /// <typeparamref name="TProcessManager"/> process manager.
    /// </summary>
    /// <typeparam name="TProcessManager"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="getId"></param>
    protected void Handle<TProcessManager, TEvent>(Func<TEvent, Guid> getId)
        where TProcessManager : PmProcessManager, IHandle<IMessage>
        where TEvent : Event
        => Handle<TProcessManager, TProcessManager, TEvent>(getId);

    /// <summary>
    /// This listens on the provided <see cref="IStreamStoreConnection"/> to capture <typeparamref name="TEvent"/> messages that are published
    /// on <typeparamref name="TStream"/> category streams and handles them within the specified <typeparamref name="TProcessManager"/> using 
    /// a <see cref="ProcessManagerStreamHandler{TProcessManager, TStream}"/>.
    /// </summary>
    /// <typeparam name="TProcessManager"></typeparam>
    /// <typeparam name="TStream"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="getId"></param>
    protected void Handle<TProcessManager, TStream, TEvent>(Func<TEvent, Guid> getId)
        where TProcessManager : PmProcessManager
        where TStream : AggregateRoot
        where TEvent : Event {
        if (!_processManagerListeners.TryGetValue(typeof(TProcessManager), out var listener)) {
            listener = new ProcessManagerStreamHandler<TProcessManager, TStream>(_connection);
            _subscriptions.Add(listener);
            _processManagerListeners[typeof(TProcessManager)] = listener;
        }

        listener.AddResolver(getId);
    }
}
