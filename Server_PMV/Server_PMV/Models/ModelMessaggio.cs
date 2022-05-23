using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_PMV.Models
{
    public class ModelMessaggio
    {
        public int IDMessaggio { get; set; }
        public bool Visualizza { get; set; }
        public string Testo { get; set; }

        public override string ToString()
        {
            return $@"<message> <IDMessaggio>{IDMessaggio}</IDMessaggio> <Visualizza>{Visualizza}</Visualizza> <Testo>{Testo}</Testo> </message>";
        }

        public string ToTinyString()
        {
            return $@"<message> <IDMessaggio>{IDMessaggio}</IDMessaggio><Testo>{Testo}</Testo> </message>";
        }
    }
}
