using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace RadioWeb.Models.Repos
{
    public class UsersDBContext : DbContext
    {
        public UsersDBContext()
            : base("name=ConexionUsuarios")
        {
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }

        public class MyContextInitializer : CreateDatabaseIfNotExists<UsersDBContext>
        {
            protected override void Seed(UsersDBContext context)
            {
                // Add defaults to certain tables in the database

                base.Seed(context);
            }
        }

        public DbSet<Models.LOGERRORS> LOGERRORS { get; set; }
        public DbSet<Models.USUARIO> UCCADUSER { get; set; }
        public DbSet<Models.UCCADPERM> UCCADPERM { get; set; }
        public DbSet<Models.UCCADMENU> UCCADMENU { get; set; }
        public DbSet<Models.UCCADMENUPERM> UCCADMENUPERM { get; set; }

 
    }
}