using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OCCCPrinting.Models;

namespace OCCCPrinting.Persistence
{
    class OCCCPrintingDbContext : DbContext
    {
        public OCCCPrintingDbContext() : base("name=DefaultConnection")
        {
            
        }
        
        public DbSet<PrintTrack> PrintTracks { get; set; }
    }
}
