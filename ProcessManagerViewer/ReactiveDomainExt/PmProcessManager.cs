using System;

using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

namespace ReactiveDomain;

public abstract class PmProcessManager : AggregateRoot, IHandle<IMessage> {
	protected PmProcessManager(ICorrelatedMessage msg = null!) : base(msg) { }

	public void Handle(IMessage message) {
		Raise(new InputMsg(message));
		OnHandle(message);
	}

	protected abstract void OnHandle(IMessage message);

	public abstract void Timeout(int retryCount);

	private readonly GenericRecorder<ICommand> _commandRecorder = new();
	private readonly GenericRecorder<DelaySendEnvelopeParameters> _eventsToSend = new();

	public bool HasRecordedCommands => _commandRecorder.HasRecordedItems;

	public ICommand[] TakeCommands() {
		OnTakeEventsStarted();
		var recordedCommands = _commandRecorder.RecordedItems;
		_commandRecorder.Reset();
		OnTakeEventsCompleted();
		return recordedCommands;
	}

	public virtual void OnTakeEventsStarted() { }
	public virtual void OnTakeEventsCompleted() { }

	/// <summary>
	/// Allows for a command to be sent once the Pm has been stored successfully.
	/// </summary>
	/// <param name="command"></param>
	protected void Raise(ICommand command) {
		OnRaise(command);
		_commandRecorder.Record(command);
	}

	protected virtual void OnRaise(ICommand command) {
		if (Source.CorrelationId == Guid.Empty) {
			throw new InvalidOperationException("Cannot raise events without valid source.");
		}

		if (command.CorrelationId != Guid.Empty || command.CausationId != Guid.Empty) {
			throw new InvalidOperationException("Cannot raise events with a different source.");
		}

		command.CorrelationId = Source.CorrelationId;
		command.CausationId = Source.MsgId;
	}


	public DelaySendEnvelopeParameters[] TakeScheduledEvents() {
		OnTakeEnvelopesStarted();
		var scheduledEvents = _eventsToSend.RecordedItems;
		_eventsToSend.Reset();
		OnTakeEnvelopesCompleted();
		return scheduledEvents;
	}

	public virtual void OnTakeEnvelopesStarted() { }
	public virtual void OnTakeEnvelopesCompleted() { }

	/// <summary>
	/// Stores a message that should be sent at a future time. (We'll create a delay send envelope underneath).
	/// </summary>
	/// <param name="message"></param>
	/// <param name="after"></param>
	/// 
	protected void Raise(IMessage message, TimeSpan after) {
		OnSendRaised(message);
		_eventsToSend.Record(new DelaySendEnvelopeParameters(message, after));
	}

	protected virtual void OnSendRaised(IMessage message) {
		if (message is ICorrelatedMessage msg) {
			if (Source.CorrelationId == Guid.Empty) {
				throw new InvalidOperationException("Cannot raise events without valid source.");
			}

			if (msg.CorrelationId != Guid.Empty || msg.CausationId != Guid.Empty) {
				throw new InvalidOperationException("Cannot raise events with a different source.");
			}

			msg.CorrelationId = Source.CorrelationId;
			msg.CausationId = Source.MsgId;
		}
	}

	public class InputMsg : Event {
		public IMessage Received { get; private set; }

		public InputMsg(IMessage received) {
			Received = received;
		}
	}
}
