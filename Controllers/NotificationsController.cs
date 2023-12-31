﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrackingBugs.Data;
using TrackingBugs.Models;
using TrackingBugs.Services;
using TrackingBugs.Services.Interfaces;

namespace TrackingBugs.Controllers
{
    [Authorize]
    public class NotificationsController : BTBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTNotificationService _NotificationService;
        private readonly UserManager<BTUser> _userManager;

        public NotificationsController(ApplicationDbContext context, IBTNotificationService notificationService, UserManager<BTUser> userManager)
        {
            _context = context;
            _NotificationService = notificationService;
            _userManager = userManager;
        }

        // GET: Notifications
        public async Task<IActionResult> Index()
        {
           List<Notification> notifications = await _NotificationService.GetNotificationsByUserIdAsync(_userManager.GetUserId(User));
            return View(notifications);
        }

        // GET: Notifications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Notifications == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.NotificationType)
                .Include(n => n.Project)
                .Include(n => n.Recipient)
                .Include(n => n.Sender)
                .Include(n => n.Ticket)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // GET: Notifications/Create
        public IActionResult Create()
        {
            ViewData["NotificationTypeId"] = new SelectList(_context.Set<NotificationType>(), "Id", "Id");
            ViewData["ProjectId"] = new SelectList(_context.Set<Project>(), "Id", "Description");
            ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["TicketId"] = new SelectList(_context.Set<Ticket>(), "Id", "Description");
            return View();
        }

        // POST: Notifications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Message,ProjectId,TicketId,Created,SenderId,RecipientId,NotificationTypeId,HasBeenViewed")] Notification notification)
        {
            if (ModelState.IsValid)
            {
                _context.Add(notification);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["NotificationTypeId"] = new SelectList(_context.Set<NotificationType>(), "Id", "Id", notification.NotificationTypeId);
            ViewData["ProjectId"] = new SelectList(_context.Set<Project>(), "Id", "Description", notification.ProjectId);
            ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "Id", notification.RecipientId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", notification.SenderId);
            ViewData["TicketId"] = new SelectList(_context.Set<Ticket>(), "Id", "Description", notification.TicketId);
            return View(notification);
        }

        // GET: Notifications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Notifications == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            ViewData["NotificationTypeId"] = new SelectList(_context.Set<NotificationType>(), "Id", "Id", notification.NotificationTypeId);
            ViewData["ProjectId"] = new SelectList(_context.Set<Project>(), "Id", "Description", notification.ProjectId);
            ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "Id", notification.RecipientId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", notification.SenderId);
            ViewData["TicketId"] = new SelectList(_context.Set<Ticket>(), "Id", "Description", notification.TicketId);
            return View(notification);
        }

        // POST: Notifications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Message,ProjectId,TicketId,Created,SenderId,RecipientId,NotificationTypeId,HasBeenViewed")] Notification notification)
        {
            if (id != notification.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notification);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotificationExists(notification.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["NotificationTypeId"] = new SelectList(_context.Set<NotificationType>(), "Id", "Id", notification.NotificationTypeId);
            ViewData["ProjectId"] = new SelectList(_context.Set<Project>(), "Id", "Description", notification.ProjectId);
            ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "Id", notification.RecipientId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", notification.SenderId);
            ViewData["TicketId"] = new SelectList(_context.Set<Ticket>(), "Id", "Description", notification.TicketId);
            return View(notification);
        }

        // GET: Notifications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Notifications == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.NotificationType)
                .Include(n => n.Project)
                .Include(n => n.Recipient)
                .Include(n => n.Sender)
                .Include(n => n.Ticket)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // POST: Notifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Notifications == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Notification'  is null.");
            }
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NotificationExists(int id)
        {
          return (_context.Notifications?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        // POST: Notifications/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> View(int id)
        {
            await _NotificationService.ViewNotificationAsync(id, _userManager.GetUserId(User));
            return RedirectToAction(nameof(Index));
        }
    }
}
