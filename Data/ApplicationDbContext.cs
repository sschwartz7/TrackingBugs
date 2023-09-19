using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrackingBugs.Models;

namespace TrackingBugs.Data
{
    public class ApplicationDbContext : IdentityDbContext<BTUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<TrackingBugs.Models.Company> Companies { get; set; } = default!;
        public DbSet<TrackingBugs.Models.Invite> Invites { get; set; } = default!;
        public DbSet<TrackingBugs.Models.Notification> Notifications { get; set; } = default!;
        public DbSet<TrackingBugs.Models.NotificationType> NotificationTypes { get; set; } = default!;
        public DbSet<TrackingBugs.Models.Project> Projects { get; set; } = default!;
        public DbSet<TrackingBugs.Models.ProjectPriority> ProjectPriorities { get; set; } = default!;
        public DbSet<TrackingBugs.Models.Ticket> Tickets { get; set; } = default!;
        public DbSet<TrackingBugs.Models.TicketAttachment> TicketAttachments { get; set; } = default!;
        public DbSet<TrackingBugs.Models.TicketHistory> TicketHistories { get; set; } = default!;
        public DbSet<TrackingBugs.Models.TicketComment> TicketComments { get; set; } = default!;
        public DbSet<TrackingBugs.Models.TicketPriority> TicketPriorities { get; set; } = default!;
        public DbSet<TrackingBugs.Models.TicketStatus> TicketStatuses { get; set; } = default!;
        public DbSet<TrackingBugs.Models.TicketType> TicketTypes { get; set; } = default!;
    }
}