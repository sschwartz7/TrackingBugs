using TrackingBugs.Models;

namespace TrackingBugs.Services.Interfaces
{
    public interface IProjectService
    {
        public Task<Project> GetProjectAsync(int? id, int? companyId);
        public Task AddProjectAsync(Project project, int companyId);
        public Task<bool> AddMemberToProjectAsync(BTUser? member, int? projectId);
        public Task AddMembersToProjectAsync(IEnumerable<string>? userIds, int? projectId, int? companyId);

        public Task<bool> AddProjectManagerAsync(string? userId, int? projectId);
        public Task ArchiveProjectAsync(int? projectId, int? companyId);
        public Task UnarchiveProjectAsync(int? projectId, int? companyId);

        public Task<List<Project>> GetAllProjectsByCompanyIdAsync(int? companyId);
        public Task<List<Project>> GetActiveProjectsByCompanyIdAsync(int? companyId);

        public Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int? companyId);
        public Task<BTUser> GetProjectManagerAsync(int? projectId);
        public Task<List<BTUser>> GetProjectMembersByRoleAsync(int? projectId, string? roleName, int? companyId);
        public Task<List<Project>> GetActiveProjectsByMemberIdAsync(int? companyId, string? memberId);
        public Task<List<Project>> GetUnassignedProjectsByCompanyIdAsync(int? companyId);

        public Task<IEnumerable<ProjectPriority>> GetProjectPrioritiesAsync();
        public Task<List<Project>?> GetUserProjectsAsync(string? userId);
        public Task RemoveMembersFromProjectAsync(int? projectId, int? companyId);

        public Task RemoveProjectManagerAsync(int? projectId);
        public Task<bool> RemoveMemberFromProjectAsync(BTUser? member, int? projectId);
        public Task RestoreProjectAsync(Project? project, int? companyId);
        public Task UpdateProjectAsync(Project? project, int companyId);

        public Task<Project> GetNewestPriorityProjectByCompanyIdAsync(int? companyId);

        public Task<List<Project>> GetAllProjectsByPriorityAsync(int? companyId, string priority);

	}
}
