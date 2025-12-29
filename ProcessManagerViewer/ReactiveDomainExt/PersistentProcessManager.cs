//using System;

//using ReactiveDomain.Messaging;
//using ReactiveDomain.Messaging.Bus;

//namespace ReactiveDomain;

//public abstract class PersistentProcessManager : AggregateRoot, IHandle<IMessage> {
//	private readonly Recorder<ICommand> _commandRecorder = new();
//	private readonly Recorder<DelaySendEnvelopeParameters> _envelopeRecorder = new();

//	protected PersistentProcessManager(ICorrelatedMessage msg) : base(msg) {

//	}

//	protected PersistentProcessManager() : base() {

//	}

//	public abstract void Handle(IMessage message);

//	public abstract void OnTimeout(int numberOfRetries, TimeProvider tp);

//	public ICommand[] TakeCommands() {
//		OnTakeCommandsStarted();
//		var cmds = _commandRecorder.RecordedItems;
//		_commandRecorder.Reset();
//		OnTakeCommandsCompleted();
//		return cmds;
//	}

//	protected virtual void OnTakeCommandsStarted() {

//	}

//	protected virtual void OnTakeCommandsCompleted() {

//	}

//	protected void Raise(ICommand cmd) {
//		OnRaised(cmd);
//		_commandRecorder.Record(cmd);
//	}

//	protected virtual void OnRaised(ICommand cmd) {
//		if (Source.CorrelationId == Guid.Empty) {
//			throw new InvalidOperationException("Cannot raise events without valid source.");
//		}

//		if (cmd.CorrelationId != Guid.Empty || cmd.CausationId != Guid.Empty) {
//			throw new InvalidOperationException("Cannot raise events with a different source.");
//		}

//		cmd.CorrelationId = Source.CorrelationId;
//		cmd.CausationId = Source.MsgId;
//	}

//	public DelaySendEnvelopeParameters[] TakeEnvelopes() {
//		OnTakeEnvelopesStarted();
//		var envelopes = _envelopeRecorder.RecordedItems;
//		_envelopeRecorder.Reset();
//		OnTakeEnvelopesCompleted();
//		return envelopes;
//	}

//	protected virtual void OnTakeEnvelopesStarted() {

//	}

//	protected virtual void OnTakeEnvelopesCompleted() {

//	}

//	protected void Raise(DelaySendEnvelopeParameters envelope) {
//		OnSendEnvelopeRaised(envelope);
//		_envelopeRecorder.Record(envelope);
//	}

//	protected virtual void OnSendEnvelopeRaised(DelaySendEnvelopeParameters envelope) {
//		if (envelope is ICorrelatedMessage msg) {
//			if (Source.CorrelationId == Guid.Empty) {
//				throw new InvalidOperationException("Cannot raise events without valid source.");
//			}

//			if (msg.CorrelationId != Guid.Empty || msg.CausationId != Guid.Empty) {
//				throw new InvalidOperationException("Cannot raise events with a different source.");
//			}

//			msg.CorrelationId = Source.CorrelationId;
//			msg.CausationId = Source.MsgId;
//		}
//	}
//}
