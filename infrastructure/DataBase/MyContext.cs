using Core.Entitites;
using infrastructure.DataBase.EntityConfigutation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace infrastructure.DataBase
{
    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions<MyContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new PostConfigutation());
            modelBuilder.ApplyConfiguration(new UserConfiguation());
        }
        public DbSet<Post>Post{ get; set; }
        public DbSet<User>User { get; set; }
    }
}
