using AssignmentManagement.Core;
using Microsoft.AspNetCore.Mvc;
using AssignmentManagement.WebAPI.Dtos;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http;
using AssignmentManagement.Core.Interfaces;
using AssignmentManagement.Core.Enums;

namespace AssignmentManagement.WebAPI.Controllers
{
    [Route("api/assignment")]
    [ApiController]
    public class AssignmentsController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;
        private readonly IAppLogger _logger;

        public AssignmentsController(IAssignmentService assignmentService, IAppLogger logger)
        {
            _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all assignments in the system.
        /// </summary>
        /// <returns>A list of all assignments.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<Assignment>), StatusCodes.Status200OK)]
        public IActionResult GetAllAssignments()
        {
            _logger.LogInformation("API: GetAllAssignments called.");
            var assignments = _assignmentService.ListAll();
            return Ok(assignments);
        }

        /// <summary>
        /// Creates a new assignment based on the provided data.
        /// </summary>
        /// <param name="assignmentDto">The data transfer object containing the details for the new assignment.</param>
        /// <returns>
        /// A 201 Created response with the new assignment if successful,
        /// a 400 Bad Request if the input data is invalid,
        /// a 409 Conflict if an assignment with the same title already exists,
        /// or a 500 Internal Server Error for unexpected issues.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(typeof(Assignment), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult CreateAssignment([FromBody] AssignmentCreationDto assignmentDto)
        {
            _logger.LogInformation($"API: CreateAssignment called. DTO Title: '{assignmentDto.Title}'.");

            try
            {
                Priority priorityToSet = assignmentDto.Priority ?? Priority.Medium;

                var newAssignment = new Assignment(
                    assignmentDto.Title,
                    assignmentDto.Description,
                    assignmentDto.DueDate,
                    priorityToSet,
                    assignmentDto.Notes
                );

                bool success = _assignmentService.AddAssignment(newAssignment);

                if (success)
                {
                    _logger.LogInformation($"API: Assignment '{newAssignment.Title}' created successfully with ID {newAssignment.Id}.");
                    return CreatedAtRoute("GetAssignmentByTitle", new { title = newAssignment.Title }, newAssignment);
                }
                else
                {
                    _logger.LogWarning($"API: Could not create assignment. Title '{assignmentDto.Title}' might already exist.");
                    return Conflict(new ProblemDetails
                    {
                        Title = "Could not create assignment.",
                        Detail = $"An assignment with the title '{assignmentDto.Title}' may already exist.",
                        Status = StatusCodes.Status409Conflict
                    });
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogError($"API: ArgumentException during assignment creation: {ex.Message}");
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid assignment data provided.",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"API: An unexpected error occurred while creating assignment: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "An unexpected error occurred.",
                    Detail = "An internal server error occurred while processing your request.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Gets a single assignment by its unique title.
        /// </summary>
        /// <param name="title">The title of the assignment to retrieve.</param>
        /// <returns>The found assignment, or a 404 Not Found response.</returns>
        [HttpGet("{title}", Name = "GetAssignmentByTitle")]
        [ProducesResponseType(typeof(Assignment), 200)]
        [ProducesResponseType(404)] // Not Found
        public IActionResult GetAssignmentByTitle(string title)
        {
            _logger.LogInformation($"API: GetAssignmentByTitle called for title: '{title}'.");
            var assignment = _assignmentService.FindAssignmentByTitle(title);
            if (assignment == null)
            {
                _logger.LogWarning($"API: Assignment with title '{title}' not found.");
                return NotFound(new ProblemDetails
                {
                    Title = "Assignment not found.",
                    Detail = $"No assignment with the title '{title}' could be found.",
                    Status = StatusCodes.Status404NotFound,
                    Instance = HttpContext.Request.Path
                });
            }
            return Ok(assignment);
        }

        /// <summary>
        /// Updates an existing assignment's title and description.
        /// </summary>
        /// <param name="title">The original title of the assignment to update.</param>
        /// <param name="assignmentDto">The DTO containing the new title and description for the assignment.</param>
        /// <returns>
        /// A 204 No Content response if the update is successful.
        /// A 400 Bad Request if the request body is invalid.
        /// A 404 Not Found if no assignment with the original title exists.
        /// A 409 Conflict if the new title already belongs to another assignment.
        /// </returns>
        [HttpPut("{title}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public IActionResult UpdateAssignment(string title, [FromBody] AssignmentUpdateDto assignmentDto)
        {
            _logger.LogInformation($"API: UpdateAssignment called for original title: '{title}'.");

            // The [ApiController] attribute automatically handles model validation
            // and will return a 400 Bad Request if the DTO is invalid.

            // Call the existing service method.
            // The 'title' from the URL is the oldTitle.
            // The Title and Description from the DTO are the new values.
            bool success = _assignmentService.UpdateAssignment(title, assignmentDto.Title, assignmentDto.Description);

            if (success)
            {
                // HTTP 204 No Content is the standard response for a successful PUT request
                // that doesn't need to return any data.
                _logger.LogInformation($"API: Successfully updated assignment originally titled '{title}'.");
                return NoContent();
            }
            else
            {
                // The service layer returns false if the original title isn't found
                // or if the new title creates a conflict.
                // We can check if the new title exists to return a more specific error.
                if (_assignmentService.FindAssignmentByTitle(assignmentDto.Title) != null && !title.Equals(assignmentDto.Title, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning($"API: Failed to update assignment. New title '{assignmentDto.Title}' already exists.");
                    return Conflict(new ProblemDetails
                    {
                        Title = "Update failed due to title conflict.",
                        Detail = $"An assignment with the title '{assignmentDto.Title}' already exists.",
                        Status = StatusCodes.Status409Conflict
                    });
                }

                // If it wasn't a conflict, the original assignment likely wasn't found.
                _logger.LogWarning($"API: Failed to update assignment. Original title '{title}' not found.");
                return NotFound(new ProblemDetails
                {
                    Title = "Assignment not found.",
                    Detail = $"No assignment with the title '{title}' could be found to update.",
                    Status = StatusCodes.Status404NotFound
                });
            }
        }

        /// <summary>
        /// Deletes an assignment by its title.
        /// </summary>
        /// <param name="title">The title of the assignment to delete.</param>
        /// <returns>A 204 No Content response on success, or a 404 Not Found response.</returns>
        [HttpDelete("{title}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public IActionResult DeleteAssignment(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest(new ProblemDetails { Title = "Assignment title cannot be empty.", Status = StatusCodes.Status400BadRequest });
            }

            bool success = _assignmentService.DeleteAssignment(title);

            if (success)
            {
                return NoContent();
            }
            else
            {
                return NotFound(new ProblemDetails { Title = "Assignment not found.", Detail = $"Assignment with title '{title}' not found or could not be deleted.", Status = StatusCodes.Status404NotFound });
            }
        }

    }
}
