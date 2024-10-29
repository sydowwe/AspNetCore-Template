using AspNetCore_Template.model.entity;
using AspNetCore_Template.model.entity.abs;
using AspNetCore_Template.security;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore_Template;

public class AppDbContext(DbContextOptions<AppDbContext> options, ILoggedUserService loggedUserService)
    : IdentityDbContext<User, UserRole, long>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AbstractEntityWithUser>().UseTpcMappingStrategy();


        // modelBuilder.ApplyConfiguration(new ActivityConfiguration());

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        base.OnModelCreating(modelBuilder);
    }
    // public override int SaveChanges()
    // {
    //     foreach (var entry in ChangeTracker.Entries<AbstractEntity>())
    //     {
    //         if (entry.State == EntityState.Added)
    //         {
    //             entry.Entity.createdTimestamp = DateTime.UtcNow;
    //         }
    //
    //         if (entry.State == EntityState.Modified)
    //         {
    //             entry.Entity.modifiedTimestamp = DateTime.UtcNow;
    //         }
    //     }
    //     
    //     return base.SaveChanges();
    // }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AbstractEntity>())
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedTimestamp = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedTimestamp = DateTime.UtcNow;
                    break;
            }

        long? userId = null;
        if (ChangeTracker.Entries<AbstractEntityWithUser>().Any(entry => entry.State == EntityState.Added))
            if (loggedUserService.IsAuthenticated())
                try
                {
                    userId = loggedUserService.GetLoggedUserId();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to get logged user ID: {ex.Message}");
                }

        if (userId.HasValue)
            foreach (var entry in ChangeTracker.Entries<AbstractEntityWithUser>())
                if (entry.State == EntityState.Added)
                    entry.Entity.UserId = userId.Value;
        return await base.SaveChangesAsync(cancellationToken);
    }
}