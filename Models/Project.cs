using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementPlatform.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }

        public string? AISummary { get; set; }
        public DateTime? AISummaryGeneratedAt { get; set; }

        public string OrganizerId { get; set; }
        
        [ForeignKey("OrganizerId")]
        public ApplicationUser Organizer { get; set; }

        public ICollection<Task> Tasks { get; set; } = new List<Task>();

        public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    }
}
