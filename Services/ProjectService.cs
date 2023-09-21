using Microsoft.EntityFrameworkCore;
using TrackingBugs.Services.Interfaces;
using TrackingBugs.Models;
using TrackingBugs.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using TrackingBugs.Enums;
using System.ComponentModel.Design;

namespace TrackingBugs.Services
{
    public class ProjectService : IProjectService

    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRoleService _roleService;
        private readonly IBTFileService _fileService;

        public ProjectService(ApplicationDbContext context, IBTRoleService roleService, IBTFileService fileService)
        {
            _context = context;
            _roleService = roleService;
            _fileService = fileService;
        }

        public async Task AddMembersToProjectAsync(IEnumerable<string>? userIds, int? projectId, int? companyId)
        {
            try
            {
                if (userIds != null && projectId != null)
                {
                    Project? project = await GetProjectAsync(projectId, companyId);
                    if (project != null)
                    {
                        foreach (string userId in userIds)
                        {
                            bool IsOnProject = project.Members.Any(m => m.Id == userId);
                            BTUser member = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                            if (!IsOnProject)
                            {
                                project.Members.Add(member);

                            }

                        }
                        await _context.SaveChangesAsync();
                        return;
                    }
                }
                return;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddMemberToProjectAsync(BTUser? member, int? projectId)
        {
            try
            {
                if (member != null && projectId != null)
                {
                    Project? project = await GetProjectAsync(projectId, member.CompanyId);
                    if (project != null)
                    {
                        bool IsOnProject = project.Members.Any(m => m.Id == member.Id);
                        if (!IsOnProject)
                        {
                            project.Members.Add(member);
                            await _context.SaveChangesAsync();
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddProjectAsync(Project project, int companyId)
        {
            if (project == null) { return; }
            try
            {
                project.Created = DateTime.Now;
                project.CompanyId = companyId;
                if (project.ImageFormFile != null)
                {
                    //Convert file to byte array and assign it to ImageData
                    project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    //Assign the imagetype based on chosen file
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }
                _context.Add(project);
                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<bool> AddProjectManagerAsync(string? userId, int? projectId)
        {
            try
            {
                if (userId != null && projectId != null)
                {
                    BTUser? currentPM = await GetProjectManagerAsync(projectId);
                    BTUser? selectedPM = await _context.Users.FindAsync(userId);
                    //remove current PM
                    if (currentPM != null) { await RemoveProjectManagerAsync(projectId); }
                    //add new one
                    try
                    {
                        await AddMemberToProjectAsync(selectedPM!, projectId);
                        return true;
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task ArchiveProjectAsync(int? projectId, int? companyId)
        {

            if (projectId == null || companyId == null) { return; }

            try
            {
                Project project = await GetProjectAsync(projectId, companyId);

                project.Archived = true;
                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = true;
                    await _context.SaveChangesAsync();
                }
                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int? companyId)
        {
            List<Project>? projects = new List<Project>();
            if (companyId == null) { return new(); }
            try
            {
                projects = await _context.Projects
                                .Include(p => p.Company)
                                .Include(p => p.Members)
                                .Include(p => p.ProjectPriority)
                                .Include(p => p.Tickets)
                                    .ThenInclude(t => t.Attachments)
                                    .Include(p => p.Tickets)
                                .ThenInclude(t => t.Comments)
                                    .ThenInclude(c => c.User)
                                .Include(p => p.Tickets)
                                    .ThenInclude(t => t.TicketStatus)
                                .Include(p => p.Tickets)
                                    .ThenInclude(t => t.History)
                                .Where(p => p.Company!.Id == companyId)
                                .ToListAsync();

                return projects!;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<List<Project>> GetAllProjectsByPriorityAsync(int? companyId, string priority)
        {
            List<Project>? projects = new List<Project>();
            if (companyId == null) { return new(); }
            try
            {
                projects = await _context.Projects
                                .Include(p => p.Company)
                                .Include(p => p.Members)
                                .Include(p => p.ProjectPriority)
                                .Include(p => p.Tickets)
                                    .ThenInclude(t => t.Attachments)
                                    .Include(p => p.Tickets)
                                .ThenInclude(t => t.Comments)
                                    .ThenInclude(c => c.User)
                                .Include(p => p.Tickets)
                                    .ThenInclude(t => t.History)
                                .Where(p => p.Company!.Id == companyId)
                                .Where(p => p.ProjectPriority.Name == priority)
								.OrderByDescending(b => b.ProjectPriorityId)
								.ToListAsync();

                return projects!;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<List<Project>> GetActiveProjectsByCompanyIdAsync(int? companyId)
        {
            List<Project>? projects = new List<Project>();
            if (companyId == null) { return new(); }
            try
            {
                projects = await _context.Projects
                                .Include(p => p.Company)
                                .Include(p => p.Members)
                                .Include(p => p.ProjectPriority)
                                .Include(p => p.Tickets)
                                    .ThenInclude(t => t.Attachments)
                                    .Include(p => p.Tickets)
                                .ThenInclude(t => t.Comments)
                                    .ThenInclude(c => c.User)
                                .Include(p => p.Tickets)
                                    .ThenInclude(t => t.History)
                                .Include(p => p.Tickets)
                                    .ThenInclude(t => t.TicketStatus)
                                    .Include(t => t.Tickets)
                        .ThenInclude(t => t.TicketPriority)
                                .Where(p => p.Company!.Id == companyId)
                                .Where(p => p.Archived == false)
								.OrderByDescending(b => b.ProjectPriorityId)
								.ToListAsync();

                return projects!;
            }
            catch (Exception)
            {

                throw;
            }
        }
		public async Task<Project> GetNewestPriorityProjectByCompanyIdAsync(int? companyId)
		{
			Project? project = new Project();
			if (companyId == null) { return new(); }
			try
			{
				project = await _context.Projects
								.Include(p => p.Company)
								.Include(p => p.Members)
								.Include(p => p.ProjectPriority)
								.Include(p => p.Tickets)
									.ThenInclude(t => t.Attachments)
									.Include(p => p.Tickets)
								.ThenInclude(t => t.Comments)
									.ThenInclude(c => c.User)
								.Include(p => p.Tickets)
									.ThenInclude(t => t.History)
								.Include(p => p.Tickets)
									.ThenInclude(t => t.TicketStatus)
									.Include(t => t.Tickets)
						.ThenInclude(t => t.TicketPriority)
								.Where(p => p.Company!.Id == companyId)
								.Where(p => p.Archived == false)
								.OrderByDescending(b => b.Created)
								.FirstOrDefaultAsync();

				return project!;
			}
			catch (Exception)
			{

				throw;
			}
		}
		public async Task<List<Project>> GetActiveProjectsByMemberIdAsync(int? companyId, string? memberId)
        {
            List<Project>? projects = new List<Project>();
            if (companyId == null) { return new(); }
            try
            {
                projects = await _context.Projects
                                .Include(p => p.Company)
                                .Include(p => p.Members)
                                .Include(p => p.ProjectPriority)
                                .Include(p => p.Tickets)
                                    .ThenInclude(t => t.Attachments)
                                    .Include(p => p.Tickets)
                                .ThenInclude(t => t.Comments)
                                    .ThenInclude(c => c.User)
                                .Include(p => p.Tickets)
                                    .ThenInclude(t => t.History)
                                .Where(p => p.Company!.Id == companyId)
                                .Where(p => p.Members.Any(m => m.Id == memberId))
                                .Where(p => p.Archived == true)
                                .ToListAsync();

                return projects!;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<List<Project>> GetUnassignedProjectsByCompanyIdAsync(int? companyId)
        {
            List<Project>? projects = new List<Project>();
            if (companyId == null) { return new(); }
            try
            {
                projects = await _context.Projects
                                .Include(p => p.Company)
                                .Include(p => p.Members)
                                .Include(p => p.ProjectPriority)
                                .Include(p => p.Tickets)
                                    .ThenInclude(t => t.Attachments)
                                    .Include(p => p.Tickets)
                                .ThenInclude(t => t.Comments)
                                    .ThenInclude(c => c.User)
                                .Include(p => p.Tickets)
                                    .ThenInclude(t => t.History)
                                .Where(p => p.Company!.Id == companyId)
                                .Where(p => p.Members.Count == 0)
                                .Where(p => p.Archived == false)
                                .ToListAsync();

                return projects!;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int? companyId)
        {

            List<Project>? projects = new List<Project>();
            if (companyId == null) { return new(); }
            try
            {
                projects = await _context.Projects
                                .Include(p => p.Company)
                                .Include(p => p.Members)
                                .Include(p => p.ProjectPriority)
                                .Include(p => p.Tickets)
                                    .ThenInclude(t => t.Attachments)
                                    .Include(p => p.Tickets)
                                .ThenInclude(t => t.Comments)
                                    .ThenInclude(c => c.User)
                                .Include(p => p.Tickets)
                                    .ThenInclude(t => t.History)
                                .Where(p => p.Company!.Id == companyId)
                                .Where(p => p.Archived == true)
                                .ToListAsync();

                return projects!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Project> GetProjectAsync(int? id, int? companyId)
        {
            if (id == null && companyId == null) { return new Project(); }
            try
            {

                Project? project = await _context.Projects
                      .Include(p => p.Company)
                      .Include(p => p.Members)
                      .Include(p => p.ProjectPriority)
                      .Include(p => p.Tickets)
                           .ThenInclude(t => t.TicketStatus)
                      .Include(p => p.Tickets)
                                    .ThenInclude(t => t.TicketPriority)
                      .Include(p => p.Tickets)
                          .ThenInclude(t => t.Attachments)
                      .Include(p => p.Tickets)
                          .ThenInclude(t => t.Comments)
                            .ThenInclude(c => c.User)
                      .Include(p => p.Tickets)
                          .ThenInclude(t => t.History)
                            .ThenInclude(h => h.User)
                      .Where(p => p.Company!.Id == companyId)
                      .FirstOrDefaultAsync(m => m.Id == id);

                return project!;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<BTUser> GetProjectManagerAsync(int? projectId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
                if (project != null)
                {
                    foreach (BTUser member in project.Members)
                    {
                        if (await _roleService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                        {
                            return member;
                        }
                    }
                }
                return null!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int? projectId, string? roleName, int? companyId)
        {
            List<BTUser> members = new List<BTUser>();
            try
            {
                if (projectId != null && companyId != null && !string.IsNullOrEmpty(roleName))
                {
                    Project? project = await GetProjectAsync(projectId, companyId);
                    if (project != null)
                    {
                        foreach (BTUser member in project.Members)
                        {
                            if (await _roleService.IsUserInRoleAsync(member, roleName))
                            {
                                members.Add(member);
                            }
                        }
                        //Testing purposes. does the same code as the foreach
                        //List<string> developers = (await _roleService.GetUsersInRoleAsync(roleName, companyId))?.Select(u => u.Id).ToList()!;
                        //List<BTUser> users = project.Members.Where(m => developers.Contains(m.Id)).ToList();
                    }

                }
                return members;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            try
            {
                IEnumerable<ProjectPriority> priorities = await _context.ProjectPriorities.ToListAsync();
                return priorities;
            }
            catch (Exception)
            {

                throw;
            }
            throw new NotImplementedException();
        }

        public async Task<List<Project>?> GetUserProjectsAsync(string? userId)
        {
            try
            {
                List<Project>? projects = new List<Project>();
                if (userId == null) { return new(); }
                try
                {
                    projects = await _context.Projects
                                    .Include(p => p.Company)
                                    .Include(p => p.Members)
                                    .Include(p => p.ProjectPriority)
                                    .Include(p => p.Tickets)
                                        .ThenInclude(t => t.Attachments)
                                    .Include(p => p.Tickets)
                                        .ThenInclude(t => t.Comments)
                                            .ThenInclude(c => c.User)
                                    .Include(p => p.Tickets)
                                        .ThenInclude(t => t.History)
                                    .Where(p => p.Members!.Any(m => m.Id == userId))
                                    .ToListAsync();

                    return projects!;
                }
                catch (Exception)
                {

                    throw;
                }
            }
            catch (Exception)
            {

                throw;
            }
            throw new NotImplementedException();
        }

        public async Task<bool> RemoveMemberFromProjectAsync(BTUser? member, int? projectId)
        {
            try
            {
                if (member != null && projectId != null)
                {
                    Project? project = await GetProjectAsync(projectId, member.CompanyId);
                    if (project != null)
                    {
                        bool IsOnProject = project.Members.Any(m => m.Id == member.Id);

                        if (IsOnProject)
                        {
                            project.Members.Remove(member);
                            await _context.SaveChangesAsync();
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveMembersFromProjectAsync(int? projectId, int? companyId)
        {
            try
            {
                if (projectId != null)
                {
                    Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
                    if (project != null)
                    {
                        foreach (BTUser member in project.Members)
                        {
                            if (!await _roleService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                            {
                                await RemoveMemberFromProjectAsync(member, projectId);
                            }
                        }
                    }
                    return;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int? projectId)
        {
            try
            {
                if (projectId != null)
                {
                    Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
                    if (project != null)
                    {
                        foreach (BTUser member in project.Members)
                        {
                            if (await _roleService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                            {
                                await RemoveMemberFromProjectAsync(member, projectId);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RestoreProjectAsync(Project? project, int? companyId)
        {
            if (project == null || companyId == null || project.CompanyId != companyId) { return; }
            try
            {
                project.Archived = false;
                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = false;
                    await _context.SaveChangesAsync();
                }
                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateProjectAsync(Project? project, int companyId)
        {
            if (project == null) { return; }
            try
            {
                project.CompanyId = companyId;
                if (project.ImageFormFile != null)
                {
                    //Convert file to byte array and assign it to ImageData
                    project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    //Assign the imagetype based on chosen file
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UnarchiveProjectAsync(int? projectId, int? companyId)
        {
            if (projectId == null || companyId == null) { return; }

            try
            {
                Project project = await GetProjectAsync(projectId, companyId);

                project.Archived = true;
                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = false;
                    await _context.SaveChangesAsync();
                }
                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {

                throw;
            }
        }
    }

    }

