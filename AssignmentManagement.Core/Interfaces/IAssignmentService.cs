using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentManagement.Core.Interfaces
{

    public interface IAssignmentService
    {
        /// <summary>
        /// Adds a new assignment to the collection.
        /// </summary>
        /// <param name="assignment">The assignment to add.</param>
        /// <returns>True if the assignment was added successfully; false otherwise.</returns>
        bool AddAssignment(Assignment assignment);

        /// <summary>
        /// Retrieves all assignments, regardless of their completion status.
        /// </summary>
        /// <returns>A list of all assignments.</returns>
        List<Assignment> ListAll();
        /// <summary>
        /// Retrieves only the assignments that are not yet marked as complete.
        /// </summary>
        /// <returns>A list of incomplete assignments.</returns>
        List<Assignment> ListIncomplete();

        /// <summary>
        /// Marks an existing assignment as complete.
        /// </summary>
        /// <param name="title">The title of the assignment to mark as complete.</param>
        /// <returns>True if the assignment was found and marked as complete; otherwise, false.</returns>
        bool MarkAssignmentComplete(string title);

        /// <summary>
        /// Finds a single assignment by its title.
        /// </summary>
        /// <param name="title">The title of the assignment to find.</param>
        /// <returns>The found <see cref="Assignment"/> object, or null if no assignment with that title exists.</returns>
        Assignment FindAssignmentByTitle(string title);

        /// <summary>
        /// Updates the title and description of an existing assignment.
        /// </summary>
        /// <param name="oldTitle">The current title of the assignment to be updated.</param>
        /// <param name="newTitle">The new title for the assignment.</param>
        /// <param name="newDescription">The new description for the assignment.</param>
        /// <returns>True if the update was successful; false if the original assignment was not found or the new title conflicts with an existing assignment.</returns>
        bool UpdateAssignment(string oldTitle, string newTitle, string newDescription);

        /// <summary>
        /// Deletes an assignment from the system.
        /// </summary>
        /// <param name="title">The title of the assignment to delete.</param>
        /// <returns>True if the assignment was successfully found and removed; otherwise, false.</returns>
        bool DeleteAssignment(string title);
    }
}
