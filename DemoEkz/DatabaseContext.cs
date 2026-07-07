using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoEkz.Pages
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() : base("DemoEkz.Properties.Settings.DemoDBConnectionString") { }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
    }
}
