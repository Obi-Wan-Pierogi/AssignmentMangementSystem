using AssignmentManagement.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AssignmentManagement.WebAPI.Dtos
{
    public class AssignmentCreationDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 100 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AssignmentManagement.Core.Enums.Priority? Priority { get; set; }

        public string? Notes { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
