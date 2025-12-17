using System;

namespace ReactiveDomain;

public record DelaySendEnvelopeParameters(Messaging.IMessage MessageToSend, TimeSpan After);
