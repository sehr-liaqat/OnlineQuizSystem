namespace OnlineQuizSystem.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int Marks { get; set; } = 1;
        public int QuizId { get; set; }
        public Quiz? Quiz { get; set; }

        // Navigation
        public ICollection<Option> Options { get; set; } = new List<Option>();
    }
}