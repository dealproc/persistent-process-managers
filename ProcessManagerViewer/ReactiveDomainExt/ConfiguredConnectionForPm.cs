using System;

using ReactiveDomain.Foundation;
using ReactiveDomain.Foundation.StreamStore;
using ReactiveDomain.Messaging.Bus;

using StreamReader = ReactiveDomain.Foundation.StreamReader;

namespace ReactiveDomain;

public class ConfiguredConnectionForPm : IConfiguredConnection {
    private readonly IPublisher _publisher;
	private readonly ICommandPublisher _cmdPublisher;
	private readonly ITimeSource _timeSource;

	public IStreamStoreConnection Connection { get; }

	public IStreamNameBuilder StreamNamer { get; }

	public IEventSerializer Serializer { get; }

	public ConfiguredConnectionForPm(IPublisher publisher, ICommandPublisher cmdPublisher, ITimeSource timeSource, IStreamStoreConnection connection, IStreamNameBuilder namer, IEventSerializer serializer) {
		_publisher = publisher;
		_cmdPublisher = cmdPublisher;
		_timeSource = timeSource;
		Connection = connection;
		StreamNamer = namer;
		Serializer = serializer;
	}

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type. Signature is coming from `IConfiguredConnection`
	public ICorrelatedRepository GetCorrelatedRepository(IRepository baseRepository = null, bool caching = false, Func<Guid> currentPolicyUserId = null) {
		return new CorrelatedStreamStoreRepositoryForPm(_publisher, _cmdPublisher, _timeSource, baseRepository ?? GetRepository(caching, currentPolicyUserId));
	}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type. Signature is coming from `IConfiguredConnection`

	public IListener GetListener(string name) {
		return new StreamListener(name, Connection, StreamNamer, Serializer);
	}

	public IListener GetQueuedListener(string name) {
		return new QueuedStreamListener(name, Connection, StreamNamer, Serializer);
	}

	public IStreamReader GetReader(string name, Action<ReactiveDomain.Messaging.IMessage> handle) {
		return new StreamReader(name, Connection, StreamNamer, Serializer, handle);
	}

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type. Signature is coming from `IConfiguredConnection`
	public IRepository GetRepository(bool caching = false, Func<Guid> currentPolicyUserId = null) {
		IRepository repository = new StreamStoreRepository(StreamNamer, Connection, Serializer, currentPolicyUserId);
		if (!caching) {
			return repository;
		}

		return new ReadThroughAggregateCache(repository);
	}
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type. Signature is coming from `IConfiguredConnection`
}
