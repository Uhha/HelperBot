using System.Threading.Tasks;

namespace BotApi.Interfaces
{
    public interface IAIChatService
    {
        Task<bool> IsServerAvailableAsync();
        Task<string> ChatAsync(string prompt);
    }
}