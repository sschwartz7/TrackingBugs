using Microsoft.AspNetCore.Mvc.Rendering;

namespace TrackingBugs.Models.ViewModels
{
    public class ProjectMembersViewModel
    {
        public Project? Project { get; set; }
        public MultiSelectList? UserList { get; set; } 
        public IEnumerable<string>? SelectedMembers { get; set; }
    }
}
