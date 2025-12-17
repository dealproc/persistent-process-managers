using System;

using ReactiveDomain.Foundation;
using ReactiveDomain.Foundation.StreamStore;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

namespace ReactiveDomain;

public class CorrelatedStreamStoreRepositoryForPm : ICorrelatedRepository, IDisposable {
	private readonly IPublisher _publisher;
	private readonly ICommandPublisher _cmdPublisher;
	private readonly ITimeSource _timeSource;
	private readonly IRepository _repository;

	private readonly IAggregateCache _cache = null!;

	public CorrelatedStreamStoreRepositoryForPm(IPublisher publisher, ICommandPublisher cmdPublisher, ITimeSource timeSource, IRepository repository, Func<IRepository, IAggregateCache> cacheFactory = null!) {
		_publisher = publisher;
		_cmdPublisher = cmdPublisher;
		_timeSource = timeSource;
		_repository = repository;
		if (cacheFactory != null) {
			_cache = cacheFactory(_repository);
		}
	}

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.  Signature is coming from `ICorrelatedRepository`
	public CorrelatedStreamStoreRepositoryForPm(IPublisher publisher, ICommandPublisher cmdPublisher, ITimeSource timeSource, IStreamNameBuilder streamNameBuilder, ReactiveDomain.IStreamStoreConnection streamStoreConnection, IEventSerializer eventSerializer, Func<IRepository, IAggregateCache> cacheFactory = null) {
		_publisher = publisher;
		_cmdPublisher = cmdPublisher;
		_timeSource = timeSource;
		_repository = new StreamStoreRepository(streamNameBuilder, streamStoreConnection, eventSerializer);
		if (cacheFactory != null) {
			_cache = cacheFactory(_repository);
		}
	}
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.  Signature is coming from `ICorrelatedRepository`

	public bool TryGetById<TAggregate>(Guid id, out TAggregate aggregate, ICorrelatedMessage source) where TAggregate : ReactiveDomain.AggregateRoot, ReactiveDomain.IEventSource {
		return TryGetById(id, int.MaxValue, out aggregate, source);
	}

	public TAggregate GetById<TAggregate>(Guid id, ICorrelatedMessage source) where TAggregate : ReactiveDomain.AggregateRoot, ReactiveDomain.IEventSource {
		return GetById<TAggregate>(id, int.MaxValue, source);
	}

	public bool TryGetById<TAggregate>(Guid id, int version, out TAggregate aggregate, ICorrelatedMessage source) where TAggregate : ReactiveDomain.AggregateRoot, ReactiveDomain.IEventSource {
		try {
			aggregate = GetById<TAggregate>(id, version, source);
			return true;
		}
		catch (Exception) {
			aggregate = null;
			return false;
		}
	}

	public TAggregate GetById<TAggregate>(Guid id, int version, ICorrelatedMessage source) where TAggregate : ReactiveDomain.AggregateRoot, ReactiveDomain.IEventSource {
		var cache = _cache;
		var val = cache != null ? cache.GetById<TAggregate>(id, version) : null!;
		if (val == null || val.Version > version) {
			val = _repository.GetById<TAggregate>(id, version);
			if (val != null) {
				_cache?.Save(val);
			}
		}

		if (val != null) {
			((ReactiveDomain.ICorrelatedEventSource)val).Source = source;
		}

#pragma warning disable CS8603 // Possible null reference return.  Expected from signature of `ICorrelatedRepository`
		return val;
#pragma warning disable CS8603 // Possible null reference return.  Expected from signature of `ICorrelatedRepository`
	}

	public void Save(ReactiveDomain.IEventSource aggregate) {
		if (_cache != null) {
			_cache.Save(aggregate);
		}
		else {
			_repository.Save(aggregate);
		}

		if (aggregate is PmProcessManager pm) {
			var scheduledEvents = pm.TakeScheduledEvents();
			foreach (var scheduledEventParameters in scheduledEvents) {
				var envelope = new DelaySendEnvelope(_timeSource, scheduledEventParameters.After, scheduledEventParameters.MessageToSend);
				_publisher.Publish(envelope);
			}

			var commands = pm.TakeCommands();
			foreach (var cmd in commands) {
				_cmdPublisher.TrySendAsync(cmd);
			}
		}
	}

	public void Delete(ReactiveDomain.IEventSource aggregate) {
		if (_cache != null) {
			_cache.Delete(aggregate);
		}
		else {
			_repository.Delete(aggregate);
		}
	}

	public void HardDelete(ReactiveDomain.IEventSource aggregate) {
		if (_cache != null) {
			_cache.HardDelete(aggregate);
		}
		else {
			_repository.HardDelete(aggregate);
		}
	}

	protected virtual void Dispose(bool disposing) {
		if (disposing) {
			_cache?.Dispose();
		}
	}

	public void Dispose() {
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
