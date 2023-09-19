using System.ComponentModel.DataAnnotations;

namespace TrackingBugs.Models
{
    public class TicketComment
    {
        public int Id { get; set; }
        private DateTime _created;
        public DateTime Created
        {
            get => _created;
            set => _created = value.ToUniversalTime();
        }

        [Required]
        [StringLength(5000, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]

        public string? Comment { get; set; }

        //Forgien Keys
        public string? UserId { get; set; }
        public int TicketId { get; set; }
        public virtual BTUser? User { get; set; }

        public virtual Ticket? Ticket { get; set; }
    }
    }
