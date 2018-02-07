using System;
using Microsoft.EntityFrameworkCore;
using MySql.Data.EntityFrameworkCore;



namespace ParseServiceNC2.DB
{
    class MyDBContext : DbContext
    {
        private string connectionString;
        private ConfigClass.Sql sqlProvider;

        public MyDBContext(string connectionString, ConfigClass.Sql sqlProvider) : base() {
            this.connectionString = connectionString;
            this.sqlProvider = sqlProvider;
        }
        public DbSet<PechStatus> PechStatus { get; set; }
        public DbSet<Splavy> Splavy { get; set; }
        public DbSet<Contracts> Contracts { get; set; }
        public DbSet<Ukazaniya> Ukazaniya { get; set; }
        public DbSet<Operations> Operations { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (sqlProvider == ConfigClass.Sql.MSSql)
                optionsBuilder.UseSqlServer(connectionString);
            if (sqlProvider == ConfigClass.Sql.MySql)
                optionsBuilder.UseMySQL(connectionString);
        }
    }
}
