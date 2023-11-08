using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrackingBugs.Data;
using TrackingBugs.Models;
using TrackingBugs.Services;
using TrackingBugs.Services.Interfaces;
using Microsoft.Identity.Client;
using TrackingBugs.Models.ViewModels;
using TrackingBugs.Enums;
using System.Net.Sockets;
using Microsoft.AspNetCore.Authorization;

namespace TrackingBugs.Controllers
{
    [Authorize]
    public class TicketsController : BTBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly ICommentService _commentService;
        private readonly IBTFileService _fileService;
        private readonly IBTTicketService _ticketService;
        private readonly IProjectService _projectService;
        private readonly IBTTicketHistoryService _ticketHistoryService;
        private readonly IBTNotificationService _notificationService;

        public TicketsController(ApplicationDbContext context,
            UserManager<BTUser> userManager,
            ICommentService commentService,
            IBTFileService fileService,
            IBTTicketService ticketService,
            IProjectService projectService,
            IBTTicketHistoryService ticketHistoryService,
            IBTNotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _commentService = commentService;
            _fileService = fileService;
            _ticketService = ticketService;
            _projectService = projectService;
            _ticketHistoryService = ticketHistoryService;
            _notificationService = notificationService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyIdAsync(_companyId);
            return View(tickets);
        }
        public async Task<IActionResult> ArchivedTickets()
        {
            List<Ticket> tickets = await _ticketService.GetArchivedTicketsByCompanyIdAsync(_companyId);
            return View(nameof(Index),tickets);
        }
        public async Task<IActionResult> UnassignedTickets()
        {
            List<Ticket> tickets = await _ticketService.GetUnassignedTicketsByCompanyIdAsync(_companyId);
            return View(nameof(Index), tickets);
        }
        public async Task<IActionResult> ActiveTicketsByMember()
        {
            List<Ticket> tickets = await _ticketService.GetActiveTicketsByMemberIdAsync(_companyId, _userManager.GetUserId(User));
            return View(nameof(Index), tickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id, string? message)
        {
            ViewData["SwalMessage"] = message;
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .Include(t => t.DeveloperUser)
                .Include(t => t.Attachments)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.History)
                    .ThenInclude(h => h.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.Set<TicketStatus>(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Create

        public IActionResult CreateTicket()
        {
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName");
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name");
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>(), "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(_context.Set<TicketStatus>(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTicket([Bind("Description,Title,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId")] Ticket ticket)
        {
            ModelState.Remove("SubmitterUserId");

            string submitterId = _userManager.GetUserId(User)!;

            if (ModelState.IsValid)
            {
                ticket.Created = DateTime.Now;
                ticket.SubmitterUserId = submitterId;
                await _ticketService.AddTicketAsync(ticket);

            Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, _companyId);

            await _ticketHistoryService.AddHistoryAsync(null!, newTicket, _userManager.GetUserId(User));

            await _notificationService.NewTicketNotificationAsync(ticket.Id, _userManager.GetUserId(User));

                return RedirectToAction(nameof(Index));
            }


            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.Set<TicketStatus>(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            if (ticket.DeveloperUserId == null)
            {
                ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName");
            }
            else
            {
                ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.DeveloperUserId);
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.Set<TicketStatus>(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Name", "Id", ticket.TicketTypeId);
            return View( ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Created,Updated,ArchivedByProject,Description,Title,Archived,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, _companyId);

                try
                {
                    ticket.Updated = DateTime.UtcNow;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, _companyId);

                await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, _userManager.GetUserId(User));

                return RedirectToAction(nameof(Index));
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.Set<TicketStatus>(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Id", "Id", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(nameof(Details), ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveToggle(int? id)
        {
            if (id == null || id == 0) return NotFound();

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);
            if (ticket == null) return NotFound();
            if (ticket.Archived == false)
            {
                await _ticketService.ArchiveTicketAsync(ticket);
            }
            else
            {
                await _ticketService.RestoreTicketAsync(ticket);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AddTicketComment([Bind("Created,Comment,UserId,TicketId")] TicketComment ticketComment)
        {
            ticketComment.UserId = _userManager.GetUserId(User);
            ticketComment.Created = DateTime.Now;

            await _commentService.CreateComment(ticketComment);
            return RedirectToAction(nameof(Details), new { id = ticketComment.TicketId });
        }
        private bool TicketExists(int id)
        {
            return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            ModelState.Remove("BTUserId");

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileContentType = ticketAttachment.FormFile.ContentType;

                ticketAttachment.Created = DateTime.Now;
                ticketAttachment.BTUserId = _userManager.GetUserId(User);

                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string fileName = ticketAttachment.FileName;
            byte[] fileData = ticketAttachment.FileData;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }
        public IActionResult AssignDeveloper()
        {
            return View();
        }

        [HttpGet]
         [Authorize(Roles ="Admin,ProjectManager")]
        public async Task<IActionResult> AssignTicket(int? id)
        {
            if (id == null) { return NotFound(); }
            AssignTicketViewModel viewModel = new();

            viewModel.Ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);
            string? devId = viewModel.Ticket.DeveloperUserId;
            viewModel.Developers = new SelectList(await _projectService.GetProjectMembersByRoleAsync(viewModel.Ticket?.ProjectId,
                                                                                                        nameof(BTRoles.Developer), _companyId)
                                                                                                        ,"Id", "FullName",
                                                                                                        devId);
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="Admin,ProjectManager")]
        public async Task<IActionResult> AssignTicket(AssignTicketViewModel viewModel)
        {
            if(viewModel.DeveloperId != null && viewModel.Ticket?.Id != null) 
            {
                Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(viewModel.Ticket.Id, _companyId);

                try
                {
                    await _ticketService.AssignTicketAsync(viewModel.Ticket.Id, viewModel.DeveloperId);
                }
                catch (Exception)
                {

                    throw;
                }

                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(viewModel.Ticket.Id, _companyId);
                await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, _userManager.GetUserId(User));

                //Add Notificaiton
                await _notificationService.NewDeveloperNotificationAsync(viewModel.Ticket?.Id, viewModel.DeveloperId, _userManager.GetUserId(User));
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
