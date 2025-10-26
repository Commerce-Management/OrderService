using OrderService.Core.Interfaces;

namespace OrderService.Application.Services;

public class EmailService: IEmailService
{
    private readonly Dictionary<string, string?> _emails = new();
    
    public async Task CompileEmail(string emailName, string emailPath)
    {
        var email = await File.ReadAllTextAsync(emailPath);
    }

    public string? GetEmail(string emailName)
    {
        return _emails.GetValueOrDefault(emailName);
    }
}