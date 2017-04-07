using DonorGateway.Domain;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace DonorGateway.Data
{
    public class DataContext : IdentityDbContext<ApplicationUser>
    {
        public DataContext() : base("DefaultConnection")
        {
            Database.Log = msg => Debug.WriteLine(msg);
        }

        public static DataContext Create()
        {
            return new DataContext();
        }


        public DbSet<Event> Events { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Template> Templates { get; set; }

        public DbSet<DemographicChange> DemographicChanges { get; set; }

        public DbSet<Constituent> Constituents { get; set; }
        public DbSet<TaxItem> TaxItems { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<Mailer> Mailers { get; set; }
        public DbSet<SuppressReason> SuppressReasons { get; set; }

        public DbSet<CsvTaxRecord> CsvTaxRecord { get; set; }

        public override int SaveChanges()
        {
            AddTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync()
        {
            AddTimestamps();
            return await base.SaveChangesAsync();
        }


        private void AddTimestamps()
        {
            var entities = ChangeTracker.Entries().Where(x => x.Entity is BaseEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));

            var currentUsername = !string.IsNullOrEmpty(HttpContext.Current?.User?.Identity?.Name)
                ? HttpContext.Current.User.Identity.Name
                : "Anonymous";

            foreach (var entity in entities.Where(x => x.Entity is BaseEntity))
            {
                if (entity.State == EntityState.Added)
                {
                    ((BaseEntity)entity.Entity).CreatedDate = DateTime.UtcNow;
                    ((BaseEntity)entity.Entity).CreatedBy = currentUsername;
                }

                ((BaseEntity)entity.Entity).UpdatedDate = DateTime.UtcNow;
                ((BaseEntity)entity.Entity).UpdatedBy = currentUsername;
            }
        }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Properties<string>().Configure(c => c.HasColumnType("varchar"));
            builder.Properties<DateTime>().Configure(c => c.HasColumnType("smalldatetime"));

            builder.Entity<ApplicationUser>().ToTable("Users", "Security");
            builder.Entity<IdentityUserRole>().ToTable("UserRoles", "Security");
            builder.Entity<IdentityUserClaim>().ToTable("UserClaims", "Security");
            builder.Entity<IdentityUserLogin>().ToTable("UserLogins", "Security");
            builder.Entity<IdentityRole>().ToTable("Roles", "Security");

            builder.Entity<Event>().HasMany(x => x.Guests).WithRequired(x => x.Event).WillCascadeOnDelete(true);
            builder.Entity<Template>().Property(p => p.Image).IsMaxLength();
            //builder.Entity<TaxItem>().Property(p => p.DonationDate).HasColumnType("date");

            builder.Entity<Constituent>().HasKey(c => c.Id);
            builder.Entity<Constituent>().HasMany(t => t.TaxItems);

            builder.Entity<CsvTaxRecord>().ToTable("TaxStaging");
        }

    }
}