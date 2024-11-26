using EventSystem.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace EventSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<UserEvent> UsersEvents { get; set; }
        public virtual DbSet<Admin> Admins { get; set; }
        public virtual DbSet<EventInvitation> Invites { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserEvent>()
                .HasQueryFilter(ue => !ue.Event.IsDeleted)
                .HasKey(ue => new { ue.UserId, ue.EventId });

            builder.Entity<Admin>()
                .HasOne(a => a.User)  
                .WithOne() 
                .HasForeignKey<Admin>(a => a.UserId);

            builder.Entity<Event>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<EventInvitation>().HasQueryFilter(e => !e.Event.IsDeleted);
        }
    }
}
