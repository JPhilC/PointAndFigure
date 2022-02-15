using Microsoft.EntityFrameworkCore;

namespace PnFData.Model
{
    public class PnFDataContext : DbContext
    {
        private const string ConnectionString =
            @"Server=localhost\SQLEXPRESS;Database=PnFData;Trusted_Connection=True;";
        public DbSet<Share> Shares { get; set; }
        public DbSet<Eod> EodPrices { get; set; }
        public DbSet<Index> Indices { get; set; }

        public DbSet<IndexDate> IndexDates { get; set; }

        public DbSet<IndexShare> IndexShares { get; set; }

        public DbSet<PnFChart> PnFCharts { get; set; }

        public DbSet<PnFColumn> PnFColumns { get; set; }

        public DbSet<PnFBox> PnFBoxes { get; set; }


        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(ConnectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Share>()
                .Property(b => b.CreatedAt)
                .HasDefaultValueSql("getdate()");
            modelBuilder.Entity<Eod>()
                .Property(b => b.CreatedAt)
                .HasDefaultValueSql("getdate()");
            modelBuilder.Entity<Index>()
                .Property(b => b.CreatedAt)
                .HasDefaultValueSql("getdate()");
            modelBuilder.Entity<IndexDate>()
                .Property(b => b.CreatedAt)
                .HasDefaultValueSql("getdate()");
            modelBuilder.Entity<IndexShare>()
                .Property(b => b.CreatedAt)
                .HasDefaultValueSql("getdate()");
            modelBuilder.Entity<PnFChart>()
                .Property(b => b.CreatedAt)
                .HasDefaultValueSql("getdate()");
            modelBuilder.Entity<PnFColumn>()
                .Property(b => b.CreatedAt)
                .HasDefaultValueSql("getdate()");
            modelBuilder.Entity<PnFBox>()
                .Property(b => b.CreatedAt)
                .HasDefaultValueSql("getdate()");
            base.OnModelCreating(modelBuilder);
        }
    }
}
