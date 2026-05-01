namespace OnlineQuizSystem.Models
{
    public class QuizAttempt
    {
        public int Id { get; set; }
        public int ObtainedMarks { get; set; }
        public bool IsPassed { get; set; }
        public DateTime AttemptedAt { get; set; } = DateTime.Now;
        public int TimeTakenSeconds { get; set; }

        // Foreign Keys
        public int UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public int QuizId { get; set; }
        public Quiz? Quiz { get; set; }
    }
}