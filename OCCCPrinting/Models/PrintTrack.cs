using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCCCPrinting.Models
{
    public class PrintTrack
    {
        public int Id { get; set; }
        [Required]
        public string StudentId { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string ComputerName { get; set; }
        public int PagesPrinted { get; set; }
        [Required]
        public DateTime Date { get; set; }
    }
}
