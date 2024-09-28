using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AppFoods.Models;

namespace AppFoods.Models
{
    // AppFoods.Models.AppDbContext
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public DbSet<Menu> Menus {get; set;}
        public DbSet<Chef> Chefs {get; set;}
        public DbSet<Group> Groups {get; set; }
        public DbSet<Combo> Combos {get; set; }
        public DbSet<Restaurant> Restaurants {get; set; }
        public DbSet<Table> Tables {get; set; }
        public DbSet<MenuOrder> MenuOrders {get; set;}
        public DbSet<ComboOrder> ComboOrders {get; set;}
        public DbSet<Summary> Summarys {get; set;}
       
        

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Chef>(entity =>
            {
                entity.HasOne(c => c.Restaurant)
                    .WithMany()
                    .HasForeignKey(c => c.RestaurantId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // 
            builder.Entity<MenuOrder>(entity => {
                entity.HasIndex(m => m.TableId);
                entity.HasIndex(m => m.Time);
            });

            builder.Entity<ComboOrder>(entity => {
                entity.HasIndex(m => m.TableId);
                entity.HasIndex(m => m.Time);
            });
           

            var entityTypes = builder.Model.GetEntityTypes();

            foreach (var entityType in entityTypes)
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }
        }
    // AppFoods.Models.AppDbContext
public DbSet<AppFoods.Models.Restaurant> Restaurant { get; set; } = default!;
    }
}
