using System.ComponentModel.DataAnnotations;

namespace TrackingBugs.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string? PropertyName { get; set; }
        public string? Description { get; set; }
        private DateTime _created;
        public DateTime Created
        {
            get => _created;
            set => _created = value.ToUniversalTime();
        }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }   

        [Required]public string? UserId { get; set; }
        public virtual BTUser? User { get; set; }
        public virtual Ticket? Ticket { get; set; }
    }
}