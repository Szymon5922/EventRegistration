using Contracts;

namespace Infrastructure.Email;

public class FakeEmailSender : IEmailSender
{
    public async Task<bool> SendSingleAsync(IEmailRequest message)
    {
        Console.WriteLine($"[EMAIL] Sending to {message.Recipient}...");
        await Task.Delay(500);
        Console.WriteLine($"[EMAIL] Sent to {message.Recipient}");

        return true;
    }

    public async Task<bool> SendBatchAsync(IEnumerable<IEmailRequest> messages)
    {
        var list = messages.ToList();
        Console.WriteLine($"[EMAIL BATCH] Sending batch of {list.Count} messages...");
        await Task.Delay(1000);
        foreach (var m in list)
            Console.WriteLine($"  * Sent to {m.Recipient}");

        return true;
    }
}
