using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client_Terminal_PMV.Models
{
    public class ModelMessaggio
    {
        public int IDMessaggio { get; set; }
        public DateTime? Data { get; set; }
        public bool Visualizza { get; set; }
        public string Testo { get; set; }

        public override string ToString()
        {
            string data = Data != null ? $"<Data>{Data}</Data>" : "";
            return $@"<message> <IDMessaggio>{IDMessaggio}</IDMessaggio> {data} <Visualizza>{Visualizza}</Visualizza> <Testo>{Testo}</Testo> </message>";
        }
    }
}
