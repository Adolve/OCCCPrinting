using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCCCPrinting.Models
{
    public class PrintTrack
    {
        public int Id { get; set; }
        public string StudentId { get; set; }
        public int PagesPrinted { get; set; }
        public bool IsCurrentlyPrinting { get; set; }
        public DateTime Date { get; set; }
    }
}
