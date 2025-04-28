using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client_wpf_app.Models
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
