using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineQuizSystem.Data;
using OnlineQuizSystem.Models;
using OnlineQuizSystem.Models.ViewModels;
using System.Security.Claims;

namespace OnlineQuizSystem.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeacherController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            ViewBag.MyQuizzes = _context.Quizzes.Count(q => q.CreatedByUserId == userId);
            ViewBag.TotalAttempts = _context.QuizAttempts
                .Where(qa => qa.Quiz!.CreatedByUserId == userId).Count();
            ViewBag.RecentQuizzes = _context.Quizzes
                .Where(q => q.CreatedByUserId == userId)
                .OrderByDescending(q => q.CreatedAt)
                .Take(5).ToList();
            return View();
        }

        public IActionResult MyQuizzes()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var quizzes = _context.Quizzes
                .Where(q => q.CreatedByUserId == userId)
                .OrderByDescending(q => q.CreatedAt)
                .ToList();
            return View(quizzes);
        }

        [HttpGet]
        public IActionResult CreateQuiz()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateQuiz(Quiz model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            model.CreatedByUserId = userId;
            model.CreatedAt = DateTime.Now;
            _context.Quizzes.Add(model);
            _context.SaveChanges();
            TempData["Success"] = "Quiz created successfully! Now add questions.";
            return RedirectToAction("AddQuestions", new { quizId = model.Id });
        }

        [HttpGet]
        public IActionResult AddQuestions(int quizId)
        {
            var quiz = _context.Quizzes.Find(quizId);
            if (quiz == null) return NotFound();

            ViewBag.Quiz = quiz;
            ViewBag.Questions = _context.Questions
                .Where(q => q.QuizId == quizId)
                .Include(q => q.Options)
                .ToList();

            return View();
        }

        [HttpPost]
        public IActionResult AddQuestions(int quizId, string questionText,
            int marks, List<string> optionTexts, int correctOption)
        {
            var question = new Question
            {
                QuizId = quizId,
                QuestionText = questionText,
                Marks = marks
            };
            _context.Questions.Add(question);
            _context.SaveChanges();

            for (int i = 0; i < optionTexts.Count; i++)
            {
                var option = new Option
                {
                    QuestionId = question.Id,
                    OptionText = optionTexts[i],
                    IsCorrect = (i == correctOption)
                };
                _context.Options.Add(option);
            }
            _context.SaveChanges();

            TempData["Success"] = "Question added successfully!";
            return RedirectToAction("AddQuestions", new { quizId });
        }

        public IActionResult QuizResults(int quizId)
        {
            var results = _context.QuizAttempts
                .Where(qa => qa.QuizId == quizId)
                .Select(qa => new
                {
                    qa.User!.FullName,
                    qa.User.Email,
                    qa.ObtainedMarks,
                    qa.IsPassed,
                    qa.AttemptedAt
                }).ToList();

            ViewBag.Results = results;
            ViewBag.Quiz = _context.Quizzes.Find(quizId);
            return View();
        }

        public IActionResult DeleteQuestion(int id, int quizId)
        {
            var question = _context.Questions
                .Include(q => q.Options)
                .FirstOrDefault(q => q.Id == id);

            if (question != null)
            {
                _context.Options.RemoveRange(question.Options);
                _context.Questions.Remove(question);
                _context.SaveChanges();
                TempData["Success"] = "Question deleted successfully!";
            }
            return RedirectToAction("AddQuestions", new { quizId });
        }

        public IActionResult DeleteQuiz(int id)
        {
            var quiz = _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefault(q => q.Id == id);

            if (quiz != null)
            {
                foreach (var question in quiz.Questions)
                    _context.Options.RemoveRange(question.Options);

                _context.Questions.RemoveRange(quiz.Questions);
                _context.Quizzes.Remove(quiz);
                _context.SaveChanges();
                TempData["Success"] = "Quiz deleted successfully!";
            }
            return RedirectToAction("MyQuizzes");
        }
    }
}