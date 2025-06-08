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

        //// Post a new assignment
        //[HttpPost]
        //[ProducesResponseType(typeof(Assignment), StatusCodes.Status201Created)]
        //[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        //public IActionResult CreateAssignment([FromBody] AssignmentCreationDto assignmentDto)
        //{
        //    try 
        //    {
        //        var priorityToSet = assignmentDto.Priority ?? AssignmentManagement.Core.Enums.Priority.Medium;
        //        _logger.LogInformation($"CONTROLLER: DTO Priority (enum?): {assignmentDto.Priority}, Resolved priorityToSet: {priorityToSet}"); // Keep/Add this log for now

        //        AssignmentManagement.Core.Enums.Priority priorityToSet;
        //        bool parsedSuccessfully = false;
        //        AssignmentManagement.Core.Enums.Priority tempParsedPriority = default;
        //        if (!string.IsNullOrEmpty(assignmentDto.Priority))
        //        {
        //            // Let's log the exact string being passed to TryParse
        //            string priorityStringFromDto = assignmentDto.Priority;
        //            _logger.LogInformation($"CONTROLLER: Attempting Enum.TryParse for string: '{priorityStringFromDto}'");

        //            parsedSuccessfully = Enum.TryParse<AssignmentManagement.Core.Enums.Priority>(
        //                priorityStringFromDto, // Use the logged variable
        //                true, // ignoreCase
        //                out tempParsedPriority); // Use the temporary variable

        //            if (parsedSuccessfully)
        //            {
        //                priorityToSet = tempParsedPriority;
        //                _logger.LogInformation($"CONTROLLER: Enum.TryParse SUCCEEDED. Parsed Enum Value: {priorityToSet} (int: {(int)priorityToSet})");
        //            }
        //            else
        //            {
        //                priorityToSet = AssignmentManagement.Core.Enums.Priority.Medium;
        //                _logger.LogWarning($"CONTROLLER: Enum.TryParse FAILED for string '{priorityStringFromDto}'. tempParsedPriority was: {tempParsedPriority} (int: {(int)tempParsedPriority}). Defaulting to Medium.");
        //            }

        //        }
        //        else 
        //        {
        //            priorityToSet = AssignmentManagement.Core.Enums.Priority.Medium; // DTO string was null or empty
        //            _logger.LogInformation($"CONTROLLER: DTO.Priority string was null or empty. Defaulting to Medium.");
        //        }

        //        _logger.LogInformation($"CONTROLLER: Final priorityToSet: {priorityToSet}");
        //        var newAssignment = new Assignment(assignmentDto.Title, assignmentDto.Description, priorityToSet);

        //        bool success = _assignmentService.AddAssignment(newAssignment);

        //        if (success)
        //        {
        //            return CreatedAtRoute("GetAssignmentByTitle", new { title = newAssignment.Title }, newAssignment);
        //        }
        //        else
        //        {
        //            return Conflict(new ProblemDetails
        //            {
        //                Title = "Could not create assignment.",
        //                Detail = $"An assignment with the title '{assignmentDto.Title}' may already exist or could not be added.",
        //                Status = StatusCodes.Status409Conflict
        //            });
        //        }
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        _logger.LogError($"CONTROLLER: ArgumentException: {ex.Message} --- StackTrace: {ex.StackTrace}");
        //        return BadRequest(new ProblemDetails
        //        {
        //            Title = "Invalid assignment data provided.",
        //            Detail = ex.Message,
        //            Status = StatusCodes.Status400BadRequest
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"CONTROLLER: An unexpected error occurred: {ex.ToString()}"); // Log full exception details
        //        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        //        {
        //            Title = "An unexpected error occurred.",
        //            Detail = "An internal server error occurred. Check logs.",
        //            Status = StatusCodes.Status500InternalServerError
        //        });
        //    }

        //}
        // POST a new assignment
        [HttpPost]
        [ProducesResponseType(typeof(Assignment), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)] // For automatic model validation
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult CreateAssignment([FromBody] AssignmentCreationDto assignmentDto)
        {
            // ASP.NET Core automatically handles model validation based on attributes in AssignmentCreationDto.
            // If validation fails (e.g., [Required] Title is missing), it will return a 400 Bad Request
            // with a ValidationProblemDetails payload before this method's code is even hit.
            // So, explicit `if (!ModelState.IsValid)` is often not needed here for basic validation.

            _logger.LogInformation($"API: CreateAssignment called. DTO Title: '{assignmentDto.Title}'. DTO Priority from model binding: '{assignmentDto.Priority}'");

            try
            {
                // Determine the priority: use the one from DTO if provided, otherwise default to Medium.
                // This relies on the [JsonConverter] on AssignmentCreationDto.Priority for correct deserialization.
                Priority priorityToSet = assignmentDto.Priority ?? Priority.Medium;

                _logger.LogInformation($"API: Resolved priorityToSet: '{priorityToSet}'");

                // Create the new Assignment domain object
                var newAssignment = new Assignment(
                    assignmentDto.Title,
                    assignmentDto.Description,
                    assignmentDto.DueDate,
                    priorityToSet,
                    assignmentDto.Notes
                );

                // Attempt to add the assignment using the service
                bool success = _assignmentService.AddAssignment(newAssignment);

                if (success)
                {
                    _logger.LogInformation($"API: Assignment '{newAssignment.Title}' created successfully with ID {newAssignment.Id}.");
                    // Return 201 Created with a location header pointing to the new resource
                    // and the created assignment in the body.
                    return CreatedAtRoute("GetAssignmentByTitle", new { title = newAssignment.Title }, newAssignment);
                }
                else
                {
                    // This typically means the title already exists
                    _logger.LogWarning($"API: Could not create assignment. Title '{assignmentDto.Title}' might already exist or another add issue occurred.");
                    return Conflict(new ProblemDetails
                    {
                        Title = "Could not create assignment.",
                        Detail = $"An assignment with the title '{assignmentDto.Title}' may already exist or another issue prevented it from being added.",
                        Status = StatusCodes.Status409Conflict,
                        Instance = HttpContext.Request.Path
                    });
                }
            }
            catch (ArgumentException ex) // Catch specific exceptions from domain/service layer if they indicate bad client data
            {
                _logger.LogError($"API: ArgumentException during assignment creation for title '{assignmentDto.Title}': {ex.Message}");
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid assignment data provided.",
                    Detail = ex.Message, // Pass the specific error message from the exception
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                });
            }
            catch (Exception ex) // Catch-all for unexpected errors
            {
                _logger.LogError($"API: An unexpected error occurred while creating assignment with title '{assignmentDto.Title}': {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "An unexpected error occurred.",
                    Detail = "An internal server error occurred while processing your request. Please try again later.",
                    Status = StatusCodes.Status500InternalServerError,
                    Instance = HttpContext.Request.Path
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
