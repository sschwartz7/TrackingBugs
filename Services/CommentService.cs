using TrackingBugs.Data;
using TrackingBugs.Models;
using TrackingBugs.Services.Interfaces;

namespace TrackingBugs.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;

        public CommentService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task CreateComment(TicketComment? comment)
        {
            if(comment == null) { return; }
            try
            {
                _context.Add(comment);
                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
