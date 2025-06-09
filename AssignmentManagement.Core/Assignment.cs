using AssignmentManagement.Core.Enums;
using AssignmentManagement.Core.Interfaces;
using System;
using System.Threading;
using System.Text.Json.Serialization;

namespace AssignmentManagement.Core
{
    public class Assignment
    {
        private static int _nextId = 0;

        public int Id { get; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public DateTime? DueDate { get; private set; }
        public bool IsCompleted { get; private set; }
        public Priority Priority { get; }
        public string Notes { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Assignment"/> class for JSON deserialization.
        /// This constructor is intended for use by serialization libraries.
        /// </summary>
        [JsonConstructor]
        public Assignment(int id, string title, string description, DateTime? dueDate, bool isCompleted, Priority priority = Priority.Medium, string notes = null)
        {
            if (string.IsNullOrWhiteSpace(title)) // Keep validation if needed during deserialization
                throw new ArgumentException("Title cannot be empty.", nameof(title));

            Id = id;
            Title = title;
            Description = description;
            DueDate = dueDate;
            IsCompleted = isCompleted;
            Priority = priority;
            Notes = notes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Assignment"/> class for creating a new assignment.
        /// A unique ID will be generated automatically.
        /// </summary>
        public Assignment(string title, string description, DateTime? dueDate, Priority priority = Priority.Medium, string notes = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty.", nameof(title));

            Id = Interlocked.Increment(ref _nextId); // Assign the provided ID
            Title = title;
            Description = description;
            DueDate = dueDate;
            IsCompleted = false;
            Priority = priority;
            Notes = notes;
        }

        /// <summary>
        /// Updates the title and description of the assignment.
        /// </summary>
        /// <param name="newTitle">The new title for the assignment.</param>
        /// <param name="newDescription">The new description for the assignment.</param>
        public void Update(string newTitle, string newDescription)
        {
            Validate(newTitle, nameof(newTitle));
            Validate(newDescription, nameof(newDescription));

            Title = newTitle;
            Description = newDescription;
        }

        /// <summary>
        /// Marks the assignment as complete.
        /// </summary>
        public void MarkComplete()
        {
            IsCompleted = true;
        }

        /// <summary>
        /// Determines if the assignment is overdue.
        /// An assignment is considered overdue if it is not complete and its due date is in the past.
        /// </summary>
        /// <param name="logger">The logger instance for recording evaluation details.</param>
        /// <returns>True if the assignment is overdue; otherwise, false.</returns>
        public bool IsOverdue(IAppLogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger)); // Basic null check

            logger.LogInformation($"Evaluating overdue status for Assignment ID: {Id}, Title: '{Title}' (Due: {DueDate?.ToShortDateString() ?? "N/A"}, Completed: {IsCompleted})");

            if (IsCompleted)
            {
                logger.LogInformation($"Assignment ID: {Id} ('{Title}') is completed, therefore not overdue.");
                return false; // Completed assignments are not overdue
            }
            if (!DueDate.HasValue)
            {
                logger.LogInformation($"Assignment ID: {Id} ('{Title}') has no due date, therefore not overdue.");
                return false; // Assignments with no due date are not overdue
            }

            bool isCurrentlyOverdue = DueDate.Value < DateTime.Now;
            logger.LogInformation($"Assignment ID: {Id} ('{Title}') overdue status: {isCurrentlyOverdue}.");
            return isCurrentlyOverdue;
        }


        public override string ToString()
        {
            // FIX: Notes included in output, and improved formatting for clarity
            var dueDateString = DueDate?.ToShortDateString() ?? "N/A";
            var result = $"- {Title} ({Priority}) due {dueDateString}\n  Description: {Description}";

            if (!string.IsNullOrWhiteSpace(Notes))
            {
                result += $"\n  Notes: {Notes}";
            }
            return result;
        }

        private void Validate(string input, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException($"{fieldName} cannot be blank or whitespace.");
        }

        // Static method to reset the ID counter for testing
        public static void ResetIdCounterForTesting()
        {
            _nextId = 0;
        }
    }
}
