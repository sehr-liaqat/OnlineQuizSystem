using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineQuizSystem.Data;

namespace OnlineQuizSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public QuizApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/QuizApi/quizzes
        [HttpGet("quizzes")]
        public IActionResult GetQuizzes()
        {
            var quizzes = _context.Quizzes
                .Where(q => q.IsActive)
                .Select(q => new
                {
                    q.Id,
                    q.Title,
                    q.Subject,
                    q.TimeLimitMinutes,
                    q.TotalMarks,
                    QuestionCount = q.Questions.Count
                }).ToList();

            return Ok(quizzes);
        }

        // GET: api/QuizApi/stats
        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            var stats = new
            {
                TotalQuizzes = _context.Quizzes.Count(),
                TotalUsers = _context.Users.Count(),
                TotalAttempts = _context.QuizAttempts.Count(),
                PassRate = _context.QuizAttempts.Any()
                    ? Math.Round(_context.QuizAttempts
                        .Count(qa => qa.IsPassed) * 100.0
                        / _context.QuizAttempts.Count(), 1)
                    : 0
            };
            return Ok(stats);
        }
    }
}