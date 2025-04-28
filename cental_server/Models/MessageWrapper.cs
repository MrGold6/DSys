namespace cental_server.Models
{
    public class MessageWrapper<T>
    {
        public int ClientId { get; set; }
        public string Sender { get; set; }
        public T Response { get; set; }
        public DateTime Time { get; set; }

        public MessageWrapper(T response)
        {
            Response = response;
        }
    }

}
