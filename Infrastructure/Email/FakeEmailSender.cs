using Application.Interfaces;
using Application.Models;
using System.Diagnostics;

namespace Infrastructure.Email;

public class FakeEmailSender : IEmailSender
{
    public async Task<bool> SendSingleAsync(EmailMessage message)
    {
        Console.WriteLine($"[EMAIL] Sending to {message.To}...");
        await Task.Delay(500);
        Console.WriteLine($"[EMAIL] Sent to {message.To}");

        return true;
    }

    public async Task<bool> SendBatchAsync(IEnumerable<EmailMessage> messages)
    {
        var list = messages.ToList();
        Console.WriteLine($"[EMAIL BATCH] Sending batch of {list.Count} messages...");
        await Task.Delay(1000);
        foreach (var m in list)
            Console.WriteLine($"  * Sent to {m.To}");

        return true;
    }
}
