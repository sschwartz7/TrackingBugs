using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace TrackingBugs.Models
{
    public class Notification
    {
        public int Id { get; set; }
        [Required] public string? Title { get; set; }
        [Required] public string? Message { get; set; }
        public int? ProjectId { get; set; }//FK
        public int? TicketId {  get; set; } //FK
        private DateTime _created;
        public DateTime Created
        {
            get
            {
                return _created;
            }
            set
            {
                _created = value.ToUniversalTime();
            }
        }
        [Required]public string? SenderId { get; set; }//FK
        [Required]public string? RecipientId { get; set;}//FK
        public int NotificationTypeId { get; set; }
        public bool HasBeenViewed {  get; set; }
        //Nav Properties
        public virtual NotificationType? NotificationType { get; set; }
        public virtual Ticket? Ticket { get; set; }
        public virtual Project? Project { get; set; }
        public virtual BTUser? Sender { get; set; }
        public virtual BTUser? Recipient { get; set; }
    }
}
