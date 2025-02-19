using System.Collections.Concurrent;

namespace Money.Business;

public class QueueHolder
{
    public ConcurrentQueue<MailMessage> MailMessages { get; } = new();
}
