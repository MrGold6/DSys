using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHybridApp.Models
{
    public class PeerFileModel
    {
        public List<string> Nodes { get; set; } = new();
        public string? CurrentCenter { get; set; }
    }

}
