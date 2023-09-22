using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrackingBugs.Data;
using TrackingBugs.Data.Migrations;
using TrackingBugs.Enums;
using TrackingBugs.Extensions;
using TrackingBugs.Models;
using TrackingBugs.Models.ViewModels;
using TrackingBugs.Services;
using TrackingBugs.Services.Interfaces;

namespace TrackingBugs.Controllers
{
    [Authorize]
    public class ProjectsController : BTBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IProjectService _projectService;
        private readonly IBTRoleService _roleService;


        public ProjectsController(ApplicationDbContext context, UserManager<BTUser> userManager, IProjectService projectService, IBTRoleService roleService)
        {
            _context = context;
            _userManager = userManager;
            _projectService = projectService;
            _roleService = roleService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            List<Project>? projects = await _projectService.GetActiveProjectsByCompanyIdAsync(_companyId);
            return View(projects);
        }
        public async Task<IActionResult> ArchivedProjects()
        {
            List<Project> projects = await _projectService.GetArchivedProjectsByCompanyIdAsync(_companyId);
            return View(nameof(Index), projects);
        }
        public async Task<IActionResult> UnassignedProjects()
        {
            List<Project> projects = await _projectService.GetUnassignedProjectsByCompanyIdAsync(_companyId);
            return View(nameof(Index), projects);
        }
        public async Task<IActionResult> ActiveProjectsByMember()
        {
            List<Project> projects = await _projectService.GetActiveProjectsByMemberIdAsync(_companyId, _userManager.GetUserId(User));
            return View(nameof(Index), projects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Project? project = await _projectService.GetProjectAsync(id, _companyId);
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Name", project.ProjectPriorityId);
            
            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Create()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");
            ViewData["ProjectPriorityId"] = new SelectList((await _projectService.GetProjectPrioritiesAsync()), "Id", "Name");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Create([Bind("StartDate,EndDate,ProjectPriorityId,Name,Description")] Project project)
        {
            if (ModelState.IsValid)
            {
                await _projectService.AddProjectAsync(project, _companyId);
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Created,ImageFormFile,ImageFileData,ImageFileType,StartDate,EndDate,ProjectPriorityId,Name,Description,Archived")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _projectService.UpdateProjectAsync(project, _companyId);
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Name", project.ProjectPriorityId);
            return View(nameof(Details), project);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Archive(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Project'  is null.");
            }
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _projectService.ArchiveProjectAsync(id, _companyId);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Unarchive(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Project'  is null.");
            }
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _projectService.UnarchiveProjectAsync(id, _companyId);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool ProjectExists(int id)
        {
            return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }


        // GET: Projects/AssignPM
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> AssignPM(int? id)
        {
            if (id == null) { return NotFound(); }

            Project? project = await _projectService.GetProjectAsync(id, _companyId);

            if (project == null) { return NotFound(); }

            IEnumerable<BTUser> projectManagers = await _roleService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), _companyId);
            BTUser? currentPM = await _projectService.GetProjectManagerAsync(id);

            AssignPMViewModel viewModel = new()
            {
                ProjectId = project.Id,
                PMId = currentPM?.Id,
                ProjectName = project.Name,
                PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id)
            };
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignPM(AssignPMViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel.PMId))
            {
                if (await _projectService.AddProjectManagerAsync(viewModel.PMId, viewModel.ProjectId))
                {
                    return RedirectToAction(nameof(Details), new { id = viewModel.ProjectId });
                }
                else
                {
                    ModelState.AddModelError("PMId", "Error assigning PM.");
                }
                ModelState.AddModelError("PMId", "No Project Manager chosen. Please select a PM.");
            }

            IEnumerable<BTUser> projectManagers = await _roleService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), _companyId);
            BTUser? currentPM = await _projectService.GetProjectManagerAsync(viewModel.ProjectId);

            viewModel.PMList = new SelectList(projectManagers, "Id", "FullName", currentPM.Id);
            return View(viewModel);
        }
        //Get
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> AssignProjectMembers(int? id)
        {
            if (id == null) { return NotFound(); }

            Project? project = await _projectService.GetProjectAsync(id, _companyId);

            if (project == null) { return NotFound(); }
            List<BTUser> delvelopers = await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Developer), _companyId);
            List<BTUser> submitters = await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), _companyId);
            List<BTUser> usersList = submitters.Concat(delvelopers).ToList();

            IEnumerable<string> currentMembers = project.Members.Select(u => u.Id);

            ProjectMembersViewModel viewModel = new()
            {
                Project = project,
                UserList = new MultiSelectList(usersList, "Id", "FullName", currentMembers)
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> AssignProjectMembers(ProjectMembersViewModel viewModel)
        {
            if (viewModel.SelectedMembers != null)
            {
                await _projectService.RemoveMembersFromProjectAsync(viewModel.Project!.Id, _companyId);
                await _projectService.AddMembersToProjectAsync(viewModel.SelectedMembers, viewModel!.Project.Id, _companyId);
                return RedirectToAction(nameof(Details), new { id = viewModel.Project?.Id });
            }

            ModelState.AddModelError("SelectedMembers", "No Users Chosen.");
            viewModel.Project = await _projectService.GetProjectAsync(viewModel.Project?.Id, _companyId);
            List<BTUser> delvelopers = await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Developer), _companyId);
            List<BTUser> submitters = await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), _companyId);
            List<BTUser> usersList = submitters.Concat(delvelopers).ToList();

            IEnumerable<string> currentMembers = viewModel.Project.Members.Select(u => u.Id);
            viewModel.UserList = new MultiSelectList(usersList, "Id", "FullName", currentMembers);

            return View(viewModel);


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> ArchiveToggle(int? id)
        {
            if (id == null || id == 0) return NotFound();

            Project? project = await _projectService.GetProjectAsync(id, _companyId);
            if (project == null) return NotFound();
            if (project.Archived == false)
            {
                await _projectService.ArchiveProjectAsync(id, _companyId);
            }
            else
            {
                await _projectService.UnarchiveProjectAsync(id, _companyId);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
