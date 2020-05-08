using Core.Entitites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace infrastructure.DataBase.EntityConfigutation
{
    public class UserConfiguation : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(x => x.Id).IsRequired();
            builder.Property(x => x.PassWord).IsRequired();
            builder.Property(x => x.Name).IsRequired();
        }
    }
}
