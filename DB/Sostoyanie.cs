using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

using ParseService.Elements;

namespace ParseService.DB
{
    class SostoyanieContext : DbContext
    {
        public SostoyanieContext()
            :base("name = CollectInfo")
        { }
        public DbSet<Process> Processes { get; set; } 
    }
}
