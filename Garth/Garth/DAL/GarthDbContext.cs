﻿using Microsoft.EntityFrameworkCore;
using Garth.DAL.DomainClasses;

namespace Garth.DAL;

public class GarthDbContext : DbContext
{
    public GarthDbContext(DbContextOptions<GarthDbContext> options) : base(options) { }

    public virtual DbSet<Tag>? Tags { get; set; }

    public virtual DbSet<Context>? Contexts { get; set; }

    public virtual DbSet<Blacklist>? Blacklist { get; set; }
    
    public virtual DbSet<CommandHistory>? CommandHistory { get; set; }
}