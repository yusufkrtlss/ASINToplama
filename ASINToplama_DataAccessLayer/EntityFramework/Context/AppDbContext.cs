using ASINToplama_EntityLayer.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ASINToplama_DataAccessLayer.EntityFramework.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Tüm entity'lerde IsDeleted = false global filtresi (soft delete)
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var isDeletedProp = entityType.FindProperty("IsDeleted");
                if (isDeletedProp is not null && isDeletedProp.ClrType == typeof(bool))
                {
                    var param = Expression.Parameter(entityType.ClrType, "e");
                    var prop = Expression.Property(param, "IsDeleted");
                    var body = Expression.Equal(prop, Expression.Constant(false));
                    var lambda = Expression.Lambda(body, param);
                    entityType.SetQueryFilter(lambda);
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
