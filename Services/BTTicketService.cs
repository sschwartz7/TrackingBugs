using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrackingBugs.Data;
using TrackingBugs.Models;
using TrackingBugs.Enums;
using TrackingBugs.Services.Interfaces;
using Microsoft.CodeAnalysis;
using System.ComponentModel.Design;

namespace TrackingBugs.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTRoleService _roleService;

        public BTTicketService(ApplicationDbContext context, UserManager<BTUser> userManager, IBTRoleService roleService = null)
        {
            _context = context;
            _userManager = userManager;
            _roleService = roleService;
        }

        public async Task AddTicketAsync(Ticket? ticket)
        {

            try
            {
                if (ticket != null)
                {
                    _context.Add(ticket);
                    await _context.SaveChangesAsync();
                    return;
                }
                return;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task AddTicketAttachmentAsync(TicketAttachment? ticketAttachment)
        {
            if (ticketAttachment == null) { return; }
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddTicketCommentAsync(TicketComment? ticketComment)
        {
            if (ticketComment == null) { return; }
            try
            {
                await _context.AddAsync(ticketComment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task ArchiveTicketAsync(Ticket? ticket)
        {
            if (ticket == null) { return; }
            try
            {
                ticket.Archived = true;
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AssignTicketAsync(int? ticketId, string? userId)
        {
            try
            {
                if (ticketId != null && !string.IsNullOrEmpty(userId))
                {
                    BTUser? btUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                    Ticket? ticket = await GetTicketByIdAsync(ticketId, btUser!.CompanyId);
                    if (ticket != null)
                    {
                        ticket!.DeveloperUserId = userId;
                        await UpdateTicketAsync(ticket);
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetActiveTicketsByMemberIdAsync(int? companyId, string? memberId)
        {
            try
            {
                List<Ticket>? tickets = new();
                if (companyId != null)
                {
                    tickets = await _context.Tickets.Where(t => t.Project!.CompanyId == companyId && t.Archived == false)
                        .Include(t => t.Project)
                            .ThenInclude(p => p!.Company)
                        .Include(t => t.TicketStatus)
                        .Include(t => t.TicketPriority)
                        .Include(t => t.Attachments)
                        .Include(t => t.Comments)
                        .Include(t => t.DeveloperUser)
                        .Include(t => t.History)
                        .Include(t => t.SubmitterUser)
                        .Where(t => t.DeveloperUserId == memberId)
                        .ToListAsync();

                }
                return tickets;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int? companyId)
        {
            try
            {
                List<Ticket>? tickets = new();
                if (companyId != null)
                {
                    tickets = await _context.Tickets.Where(t => t.Project!.CompanyId == companyId && t.Archived == false)
                        .Include(t => t.Project)
                            .ThenInclude(p => p!.Company)
                        .Include(t => t.Attachments)
                                                .Include(t => t.TicketStatus)
                        .Include(t => t.TicketPriority)
                        .Include(t => t.Comments)
                        .Include(t => t.DeveloperUser)
                        .Include(t => t.History)
                        .Include(t => t.SubmitterUser)
                        .ToListAsync();

                }
                return tickets!;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetArchivedTicketsByCompanyIdAsync(int? companyId)
        {
            try
            {
                List<Ticket>? tickets = new();
                if (companyId != null)
                {
                    tickets = await _context.Tickets.Where(t => t.Project!.CompanyId == companyId && t.Archived == true)
                        .Include(t => t.Project)
                            .ThenInclude(p => p!.Company)
                        .Include(t => t.Attachments)
                        .Include(t => t.TicketStatus)
                        .Include(t => t.TicketPriority)
                        .Include(t => t.Comments)
                        .Include(t => t.DeveloperUser)
                        .Include(t => t.History)
                        .Include(t => t.SubmitterUser)
                        .ToListAsync();

                }
                return tickets;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket> GetTicketAsNoTrackingAsync(int? ticketId, int? companyId)
        {
            try
            {
                Ticket? ticket = new();
                if (ticketId != null && companyId != null)
                {
                    ticket = await _context.Tickets.Where(t => t.Project!.CompanyId == companyId && t.Archived == false)
                        .Include(t => t.Project)
                            .ThenInclude(p => p!.Company)
                        .Include(t => t.Attachments)
                        .Include(t => t.Comments)
                        .Include(t => t.DeveloperUser)
                        .Include(t => t.History)
                        .Include(t => t.SubmitterUser)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Id == ticketId);

                }
                return ticket!;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<TicketAttachment?> GetTicketAttachmentByIdAsync(int? ticketAttachmentId)
        {
            if (ticketAttachmentId == null) { return new TicketAttachment(); }
            try
            {
                TicketAttachment? ticketAttachment = await _context.TicketAttachments
                                                                  .Include(t => t.BTUser)
                                                                  .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
                return ticketAttachment;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<Ticket> GetTicketByIdAsync(int? ticketId, int? companyId)
        {
            try
            {
                Ticket? ticket = new();
                if (ticketId != null && companyId != null)
                {
                    ticket = await _context.Tickets.Where(t => t.Project!.CompanyId == companyId && t.Archived == false)
                        .Include(t => t.Project)
                            .ThenInclude(p => p!.Company)
                        .Include(t => t.Attachments)
                        .Include(t => t.Comments)
                        .Include(t => t.DeveloperUser)
                        .Include(t => t.History)
                        .Include(t => t.SubmitterUser)
                        .FirstOrDefaultAsync(t => t.Id == ticketId);

                }
                return ticket!;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BTUser?> GetTicketDeveloperAsync(int? ticketId, int? companyId)
        {
            BTUser? developer = new();
            try
            {
                if (ticketId != null && companyId != null)
                {
                    Ticket? ticket = await GetTicketByIdAsync(ticketId, companyId);
                    developer = ticket?.DeveloperUser;
                }
                return developer;

            }
            catch (Exception)
            {

                throw;
            }
        }


        public Task<IEnumerable<TicketPriority>> GetTicketPrioritiesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string? userId, int? companyId)
        {
            try
            {
                List<Ticket>? tickets = new();
                if (companyId != null && !string.IsNullOrEmpty(userId))
                {
                    tickets = await _context.Tickets.Where(t => t.Project!.CompanyId == companyId && t.Project.Members.Any(m => m.Id == userId) && t.Archived == false)
                        .Include(t => t.Project)
                            .ThenInclude(p => p!.Company)
                        .Include(t => t.Attachments)
                        .Include(t => t.Comments)
                        .Include(t => t.DeveloperUser)
                        .Include(t => t.History)
                        .Include(t => t.SubmitterUser)
                        .ToListAsync();

                }
                return tickets!;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<IEnumerable<TicketStatus>> GetTicketStatusesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TicketType>> GetTicketTypesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Ticket>> GetUnassignedTicketsByCompanyIdAsync(int? companyId)
        {
            try
            {
                List<Ticket>? tickets = new();
                if (companyId != null)
                {
                    tickets = await _context.Tickets.Where(t => t.Project!.CompanyId == companyId && t.Archived == false)
                        .Include(t => t.Project)
                            .ThenInclude(p => p!.Company)
                        .Include(t => t.Attachments)
                        .Include(t => t.TicketStatus)
                        .Include(t => t.TicketPriority)
                        .Include(t => t.Comments)
                        .Include(t => t.DeveloperUser)
                        .Include(t => t.History)
                        .Include(t => t.SubmitterUser)
                        .Where(t => t.DeveloperUserId == null)
                        .ToListAsync();

                }
                return tickets;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RestoreTicketAsync(Ticket? ticket)
        {
            if (ticket == null) { return; }
            try
            {
                ticket.Archived = false;
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateTicketAsync(Ticket? ticket)
        {
            try
            {
                if (ticket != null)
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
