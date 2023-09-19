using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackingBugs.Extensions;

namespace TrackingBugs.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        private DateTime _created;
        public DateTime Created
        {
            get => _created;
            set => _created = value.ToUniversalTime();
        }


        [NotMapped]
        [DisplayName("Select a file")]
        [DataType(DataType.Upload)]
        [MaxFileSize(1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".pdf" })]
        public IFormFile? FormFile { get; set; }
        public byte[]? FileData { get; set; }
        public string? FileContentType { get; set; }
        public string? FileName { get; set; }

        public int TicketId { get; set; }
        [Required]public string? BTUserId { get; set;}
        //Nav 
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? BTUser { get; set; }
    }
}
