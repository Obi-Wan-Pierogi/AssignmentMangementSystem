namespace AssignmentManagement.Tests
{
    using AssignmentManagement.Core;
    public class AssignmentTests
    {
        [Fact]
        public void Constructor_ValidInput_ShouldCreateAssignment()
        {
            var assignment = new Assignment("Read Chapter 2", "Summarize key points");
            Assert.Equal("Read Chapter 2", assignment.Title);
            Assert.Equal("Summarize key points", assignment.Description);
            Assert.False(assignment.IsCompleted);
        }

        [Fact]
        public void Constructor_BlankTitle_ShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() => new Assignment("", "Valid description"));
        }

        [Fact]
        public void Update_BlankDescription_ShouldThrowException()
        {
            var assignment = new Assignment("Read Chapter 2", "Summarize key points");
            Assert.Throws<ArgumentException>(() => assignment.Update("Valid title", ""));
        }

        [Fact]
        public void MarkComplete_SetsIsCompletedToTrue()
        {
            var assignment = new Assignment("Task", "Complete the lab");
            assignment.MarkComplete();
            Assert.True(assignment.IsCompleted);
        }
    }
}
