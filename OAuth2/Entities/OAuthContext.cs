using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace OAuth2.Entities;

public partial class OAuthContext : DbContext
{
    public OAuthContext()
    {
    }

    public OAuthContext(DbContextOptions<OAuthContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_100_BIN2_UTF8");

        OnModelCreatingPartial(modelBuilder);
        modelBuilder.UseOpenIddict();
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
