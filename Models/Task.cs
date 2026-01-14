using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementPlatform.Models
{
    public enum TaskStatus
    {
        NotStarted,
        InProgress,
        Completed
    }

    public class Task
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required]
        [ValidationAttributes.DateGreaterThan("StartDate", ErrorMessage = "Deadline must be after the start date.")]
        [ValidationAttributes.FutureDate(ErrorMessage = "Deadline cannot be in the past.")]
        public DateTime EndDate { get; set; }

        [Required]
        public TaskStatus Status { get; set; }

        public string? MediaContent { get; set; }

        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public Project Project { get; set; }

        public string? AssignedToUserId { get; set; }

        [ForeignKey("AssignedToUserId")]
        public ApplicationUser? AssignedTo { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
