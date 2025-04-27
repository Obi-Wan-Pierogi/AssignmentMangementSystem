namespace AssignmentManagement.Core
{
    public class Assignment
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public bool IsCompleted { get; private set; }

        public Assignment(string title, string description)
        {
            Validate(title, nameof(title));
            Validate(description, nameof(description));

            Title = title;
            Description = description;
            IsCompleted = false;
        }

        public void Update(string newTitle, string newDescription)
        {
            Validate(newTitle, nameof(newTitle));
            Validate(newDescription, nameof(newDescription));

            Title = newTitle;
            Description = newDescription;
        }

        public void MarkComplete()
        {
            IsCompleted = true;
        }

        private void Validate(string input, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException($"{fieldName} cannot be blank or whitespace.");
        }
    }
}
