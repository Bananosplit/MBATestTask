using MBATestTask.Models;
using Microsoft.EntityFrameworkCore;

namespace MBATestTask.Data;

public partial class CitiesForecastContext : DbContext
{
    public CitiesForecastContext() {}

    public CitiesForecastContext(DbContextOptions<CitiesForecastContext> options)
        : base(options)
    { }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Forecast> Forecasts { get; set; }

    /*
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlite("Data Source = C:\\\\\\\\Program Files\\\\\\\\SqlLite\\\\\\\\DataBaseSQlite\\\\\\\\CitiesForecast.db");
        */

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Forecast>(entity =>
        {
            entity.ToTable("Forecast");

            entity.HasOne(d => d.City).WithMany(p => p.Forecasts).HasForeignKey(d => d.CityId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
