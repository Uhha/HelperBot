using System.Collections.Concurrent;

namespace BotApi.Services
{
    public class ClientReplyStateService
    {
        private readonly ConcurrentDictionary<long, ExpectedReplyType> _clientStates = new();

        public void SetExpectedReply(long chatId, ExpectedReplyType replyType)
        {
            _clientStates[chatId] = replyType;
        }

        public ExpectedReplyType GetExpectedReply(long? chatId)
        {
            if (chatId is null)
                return ExpectedReplyType.None;

            return _clientStates.TryGetValue((long)chatId, out var type) ? type : ExpectedReplyType.None;
        }

        public void ClearExpectedReply(long chatId)
        {
            _clientStates.TryRemove(chatId, out _);
        }
    }

    public enum ExpectedReplyType
    {
        None,
        BandSearch,
        AddBand,
        RemoveBand,
    }
}
