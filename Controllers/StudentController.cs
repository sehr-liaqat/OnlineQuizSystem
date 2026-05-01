using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineQuizSystem.Data;
using OnlineQuizSystem.Models;
using OnlineQuizSystem.Models.ViewModels;
using System.Security.Claims;

namespace OnlineQuizSystem.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            ViewBag.AvailableQuizzes = _context.Quizzes.Count(q => q.IsActive);
            ViewBag.MyAttempts = _context.QuizAttempts.Count(qa => qa.UserId == userId);
            ViewBag.PassedQuizzes = _context.QuizAttempts
                .Count(qa => qa.UserId == userId && qa.IsPassed);
            ViewBag.RecentAttempts = _context.QuizAttempts
                .Where(qa => qa.UserId == userId)
                .Include(qa => qa.Quiz)
                .OrderByDescending(qa => qa.AttemptedAt)
                .Take(5).ToList();
            return View();
        }

        public IActionResult AvailableQuizzes()
        {
            var quizzes = _context.Quizzes
                .Where(q => q.IsActive)
                .OrderByDescending(q => q.CreatedAt)
                .ToList();
            return View(quizzes);
        }

        public IActionResult StartQuiz(int quizId)
        {
            var quiz = _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefault(q => q.Id == quizId);

            if (quiz == null) return NotFound();

            var viewModel = new QuizViewModel
            {
                QuizId = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                Subject = quiz.Subject,
                TimeLimitMinutes = quiz.TimeLimitMinutes,
                TotalMarks = quiz.TotalMarks,
                PassingMarks = quiz.PassingMarks,
                Questions = quiz.Questions.Select(q => new QuestionViewModel
                {
                    QuestionId = q.Id,
                    QuestionText = q.QuestionText,
                    Marks = q.Marks,
                    Options = q.Options.Select(o => new OptionViewModel
                    {
                        OptionId = o.Id,
                        OptionText = o.OptionText
                    }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SubmitQuiz(QuizViewModel model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var quiz = _context.Quizzes.Find(model.QuizId);
            int obtained = 0;

            foreach (var q in model.Questions)
            {
                var correct = _context.Options
                    .FirstOrDefault(o => o.QuestionId == q.QuestionId && o.IsCorrect);
                if (correct != null && correct.Id == q.SelectedOptionId)
                    obtained += q.Marks;
            }

            var attempt = new QuizAttempt
            {
                UserId = userId,
                QuizId = model.QuizId,
                ObtainedMarks = obtained,
                IsPassed = obtained >= quiz!.PassingMarks,
                AttemptedAt = DateTime.Now
            };

            _context.QuizAttempts.Add(attempt);
            _context.SaveChanges();

            return RedirectToAction("Result", new { attemptId = attempt.Id });
        }

        public IActionResult Result(int attemptId)
        {
            var attempt = _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .FirstOrDefault(qa => qa.Id == attemptId);

            if (attempt == null) return NotFound();
            return View(attempt);
        }

        public IActionResult MyResults()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var results = _context.QuizAttempts
                .Where(qa => qa.UserId == userId)
                .Include(qa => qa.Quiz)
                .OrderByDescending(qa => qa.AttemptedAt)
                .ToList();
            return View(results);
        }
    }
}