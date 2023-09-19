using TrackingBugs.Models;

namespace TrackingBugs.Services.Interfaces

{
    public interface ICommentService
    {
       public Task CreateComment(TicketComment comment);
    }
}
