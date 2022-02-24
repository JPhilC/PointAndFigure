using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PnFData.Model
{
    public class PnFDataContext : DbContext
    {
        private const string ConnectionString =
            @"Server=localhost\SQLEXPRESS;Database=PnFData;Trusted_Connection=True;";

        public static readonly ILoggerFactory loggerFactory = LoggerFactory.Create(builder => {
                builder.AddFilter("Database.Command", LogLevel.None)
                    .AddDebug();
            }
        );


        public DbSet<Share> Shares { get; set; }
        public DbSet<Eod> EodPrices { get; set; }
        public DbSet<Index> Indices { get; set; }

        public DbSet<IndexValue> IndexValues { get; set; }

        public DbSet<PnFChart> PnFCharts { get; set; }

        public DbSet<PnFColumn> PnFColumns { get; set; }

        public DbSet<PnFBox> PnFBoxes { get; set; }

        public DbSet<ShareChart> ShareCharts { get; set; }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            //options.UseLoggerFactory(loggerFactory)  //tie-up DbContext with LoggerFactory object
            //    .EnableSensitiveDataLogging()
            //    .UseSqlServer(ConnectionString);

            options.UseSqlServer(ConnectionString, 
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount:2,
                        maxRetryDelay:TimeSpan.FromSeconds(5),
                        errorNumbersToAdd:null
                        );
                    
                });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define Db generated default values for Id
            modelBuilder.Entity<Share>()
                .Property(b => b.Id)
                .HasDefaultValueSql("newid()");
            modelBuilder.Entity<Eod>()
                .Property(b => b.Id)
                .HasDefaultValueSql("newid()");
            modelBuilder.Entity<Index>()
                .Property(b => b.Id)
                .HasDefaultValueSql("newid()");
            modelBuilder.Entity<IndexValue>()
                .Property(b => b.Id)
                .HasDefaultValueSql("newid()");
            modelBuilder.Entity<PnFChart>()
                .Property(b => b.Id)
                .HasDefaultValueSql("newid()");
            modelBuilder.Entity<PnFColumn>()
                .Property(b => b.Id)
                .HasDefaultValueSql("newid()");
            modelBuilder.Entity<PnFBox>()
                .Property(b => b.Id)
                .HasDefaultValueSql("newid()");
            modelBuilder.Entity<ShareChart>()
                .Property(b => b.Id)
                .HasDefaultValueSql("newid()");

            // Define Db generated default values for CreatedAt
            modelBuilder.Entity<Share>()
                .Property(b => b.CreatedAt)
                .HasDefaultValueSql("getdate()");
            modelBuilder.Entity<Eod>()
                .Property(b => b.CreatedAt)
                .HasDefaultValueSql("getdate()");
            modelBuilder.Entity<Index>()
                .Property(b => b.CreatedAt)
                .HasDefaultValueSql("getdate()");
            modelBuilder.Entity<IndexValue>()
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
            modelBuilder.Entity<ShareChart>()
                .Property(b => b.CreatedAt)
                .HasDefaultValueSql("getdate()");

            // Define relationships to trigger creation or cascading deletes
            modelBuilder.Entity<Eod>()
                .HasOne(p => p.Share)
                .WithMany(b => b.EodPrices)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<IndexValue>()
                .HasOne(p => p.Index)
                .WithMany(b => b.IndexValues)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PnFBox>()
                .HasOne(p => p.Column)
                .WithMany(b => b.Boxes)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PnFColumn>()
                .HasOne(p => p.PnFChart)
                .WithMany(b => b.Columns)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ShareChart>()
                .HasOne(p => p.Share)
                .WithMany(b => b.Charts)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PnFChart>()
                .HasOne(c => c.ShareChart)
                .WithOne(sc => sc.Chart)
                .HasForeignKey<ShareChart>(sc => sc.ChartId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
