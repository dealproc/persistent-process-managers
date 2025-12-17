using System;
using System.Collections.Generic;
using System.Linq;

using ReactiveDomain.Messaging.Bus;

namespace ReactiveDomain;

public class ScheduledMessageRecorder {
	private readonly List<DelaySendEnvelope> _envelopes = [];

	public bool HasLetters => _envelopes.Any();

	public DelaySendEnvelope[] Envelopes => [.. _envelopes];

	public void Schedule(ITimeSource timeSource, TimeSpan delay, Messaging.IMessage message)
		=> _envelopes.Add(new DelaySendEnvelope(timeSource, delay, message));

	public void Schedule(TimePosition at, Messaging.IMessage message)
		=> _envelopes.Add(new(at, message));

	public void Reset() => _envelopes.Clear();
}
