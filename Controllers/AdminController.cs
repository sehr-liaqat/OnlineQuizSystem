using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineQuizSystem.Data;

namespace OnlineQuizSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalQuizzes = _context.Quizzes.Count();
            ViewBag.TotalStudents = _context.Users.Count(u => u.Role == "Student");
            ViewBag.TotalTeachers = _context.Users.Count(u => u.Role == "Teacher");
            ViewBag.TotalAttempts = _context.QuizAttempts.Count();
            ViewBag.Users = _context.Users.OrderByDescending(u => u.CreatedAt).Take(5).ToList();
            return View();
        }

        public IActionResult ManageUsers()
        {
            var users = _context.Users.OrderByDescending(u => u.CreatedAt).ToList();
            return View(users);
        }

        public IActionResult ToggleUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                _context.SaveChanges();
            }
            return RedirectToAction("ManageUsers");
        }

        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            return RedirectToAction("ManageUsers");
        }

        public IActionResult ManageQuizzes()
        {
            var quizzes = _context.Quizzes
                .OrderByDescending(q => q.CreatedAt)
                .ToList();
            return View(quizzes);
        }

        public IActionResult QuizDetail(int id)
        {
            var quiz = _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
                .Include(q => q.CreatedByUser)
                .FirstOrDefault(q => q.Id == id);

            if (quiz == null) return NotFound();

            ViewBag.TotalAttempts = _context.QuizAttempts.Count(qa => qa.QuizId == id);
            ViewBag.PassedCount = _context.QuizAttempts.Count(qa => qa.QuizId == id && qa.IsPassed);

            return View(quiz);
        }

        public IActionResult ToggleQuiz(int id)
        {
            var quiz = _context.Quizzes.Find(id);
            if (quiz != null)
            {
                quiz.IsActive = !quiz.IsActive;
                _context.SaveChanges();
            }
            return RedirectToAction("ManageQuizzes");
        }
    }
}