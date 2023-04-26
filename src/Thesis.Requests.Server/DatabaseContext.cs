using Microsoft.EntityFrameworkCore;
using Thesis.Requests.Model;

namespace Thesis.Requests.Server;

/// <summary>
/// Контекст базы данных
/// </summary>
public sealed class DatabaseContext : DbContext
{
    #region Tables

    /// <summary>
    /// Таблица заявок
    /// </summary>
    public DbSet<Request> Requests { get; set; } = null!;
    
    /// <summary>
    /// Таблица комментариев к заявке
    /// </summary>
    public DbSet<RequestComment> RequestComments { get; set; } = null!;
    
    /// <summary>
    /// Таблица статусов заявок
    /// </summary>
    public DbSet<RequestStatus> RequestStatuses { get; set; } = null!;

    #endregion
    
    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public DatabaseContext() { }
    
    /// <summary>
    /// Конструктор с параметрами
    /// </summary>
    /// <param name="options">Параметры</param>
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) 
    {
        if (Database.GetPendingMigrations().Any())
            Database.Migrate();
    }

    /// <inheritdoc cref="DbContext.OnModelCreating(ModelBuilder)"/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Request>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Number).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Images).IsRequired();
            entity.Property(e => e.CreatorId).IsRequired();
            entity.Property(e => e.CreatorName).IsRequired();
            entity.Property(e => e.Created).IsRequired().HasDefaultValueSql("now()");
            entity.Property(e => e.IncidentPointList).IsRequired();
            entity.Property(e => e.IncidentPointListAsString).IsRequired();

            entity.HasMany(e => e.Comments).WithOne(c => c.Request).HasForeignKey(c => c.RequestId);
            entity.HasMany(e => e.Statuses).WithOne(c => c.Request).HasForeignKey(c => c.RequestId);
        });

        modelBuilder.Entity<RequestComment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RequestId).IsRequired();
            entity.Property(e => e.Text).IsRequired();
            entity.Property(e => e.Images).IsRequired();
            entity.Property(e => e.CreatorId).IsRequired();
            entity.Property(e => e.CreatorName).IsRequired();
            entity.Property(e => e.Created).IsRequired().HasDefaultValueSql("now()");

            entity.HasOne(e => e.Request).WithMany(r => r.Comments).HasForeignKey(r => r.RequestId);
        });
        
        modelBuilder.Entity<RequestStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RequestId).IsRequired();
            entity.Property(e => e.State).IsRequired();
            entity.Property(e => e.Comment).IsRequired();
            entity.Property(e => e.CreatorId).IsRequired();
            entity.Property(e => e.CreatorName).IsRequired();
            entity.Property(e => e.Created).IsRequired().HasDefaultValueSql("now()");

            entity.HasOne(e => e.Request).WithMany(r => r.Statuses).HasForeignKey(r => r.RequestId);
        });
        
        base.OnModelCreating(modelBuilder);
    }
}