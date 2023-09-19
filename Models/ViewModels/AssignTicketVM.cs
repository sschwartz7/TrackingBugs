using Microsoft.AspNetCore.Mvc.Rendering;

namespace TrackingBugs.Models.ViewModels
{
    public class AssignTicketViewModel
    {
        public Ticket? Ticket { get; set; }
        public SelectList? Developers { get; set; }
        public string? DeveloperId { get; set; }
    }
}
