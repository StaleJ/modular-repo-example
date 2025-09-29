using System;
using Microsoft.EntityFrameworkCore;

namespace LoanApplication.Infrastructure;

public class LoanAppDbContext : DbContext
{
    public LoanAppDbContext(DbContextOptions<LoanAppDbContext> options) : base(options) { }

    public DbSet<LoanApp> LoanApplications => Set<LoanApp>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LoanApp>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.CustomerId).IsRequired();
            b.Property(x => x.Amount).IsRequired();
            b.Property(x => x.Status).IsRequired();
        });

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Type).IsRequired();
            b.Property(x => x.Payload).IsRequired();
            b.Property(x => x.OccurredOnUtc).IsRequired();
        });
    }
}

public class LoanApp
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Submitted";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedOnUtc { get; set; }
}
