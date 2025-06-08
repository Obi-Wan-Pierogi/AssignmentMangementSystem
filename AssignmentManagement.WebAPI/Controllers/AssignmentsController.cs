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

        // Get all assignments
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

        // Get an assignment by title
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

        // Delete an assignment
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
