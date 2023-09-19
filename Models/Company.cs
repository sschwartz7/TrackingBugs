using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackingBugs.Models
{
    public class Company
    {
        public int Id { get; set; }//primary key
        [Required]public string? Name { get; set; }
        public string? Description { get; set; }
        [NotMapped]
        public IFormFile? ImageFormFile { get; set; }
        public byte[]? ImageFileData { get; set; }
        public string? ImageFileType { get; set; }
        //Nav Properties
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();
        public virtual ICollection<Invite> Invites { get; set; } = new HashSet<Invite>();
        public virtual ICollection<Project> Projects { get; set; }= new HashSet<Project>();
    }
}
