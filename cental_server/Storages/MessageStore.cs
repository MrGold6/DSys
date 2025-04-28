using cental_server.Models;

namespace cental_server.Storages
{
    public static class MessageStore
    {
        //todo change to different inside type
        private static readonly List<MessageWrapper<string>> _messages = new();

        public static void Add(MessageWrapper<string> msg)
        {
            lock (_messages)
            {
                _messages.Add(msg);
            }
        }

        public static IReadOnlyList<MessageWrapper<string>> GetAll()
        {
            lock (_messages)
            {
                return _messages.ToList(); // захищаємо від зміни
            }
        }
    }


}
