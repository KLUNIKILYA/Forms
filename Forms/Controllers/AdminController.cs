using DataBase;
using DataBase.Models;
using DataBase.Repositories;
using Enums.User;
using Forms.Models;
using Forms.Models.Forms;
using Forms.Models.User;
using Forms.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Forms.Controllers
{
    public class AdminController : Controller
    {
        IUserRepository _userRepository;
        AdminService _adminService;
        WebDbContext _webDbContext;
        AuthServices _authServices;

        public AdminController(IUserRepository userRepository, AdminService adminService, WebDbContext webDbContext, AuthServices authServices)
        {
            _userRepository = userRepository;
            _adminService = adminService;
            _webDbContext = webDbContext;
            _authServices = authServices;
        }

        public async Task<IActionResult> Users(int page = 1, int pageSize = 10)
        {
            var query = _userRepository.GetQueryable();

            var totalUsers = await query.CountAsync();
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserInListViewModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    IsBlocked = u.IsBlocked,
                    IsAdmin = u.Role == Role.Admin,
                })
                .ToListAsync();

            var model = new UserManagementViewModel
            {
                Users = users,
                CurrentPage = page,
                PageSize = pageSize,
                TotalUsers = totalUsers
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult BlockUsers(string selectedUserIds, int page)
        {
            var userIds = ParseUserIds(selectedUserIds);
            foreach (var userId in userIds)
            {
                var user = _userRepository.GetById(userId);
                if (user != null)
                {
                    _userRepository.BlockUsers(userId);
                }
            }

            return RedirectToAction("Users", new
            {
                page = page ,
                pageSize = 10
            });
        }

        [HttpPost]
        public IActionResult UnblockUsers(string selectedUserIds, int page)
        {
            var userIds = ParseUserIds(selectedUserIds);
            foreach (var userId in userIds)
            {
                var user = _userRepository.GetById(userId);
                if (user != null)
                {
                    _userRepository.UnblockUsers(userId);
                }
            }

            return RedirectToAction("Users", new
            {
                page = page,
                pageSize = 10
            });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUsers(string selectedUserIds, int page)
        {
            var userIds = ParseUserIds(selectedUserIds);

            foreach (var userId in userIds)
            {
                await _adminService.DeleteUserAndAllRelatedDataAsync(userId);
            }

            return RedirectToAction("Users", new
            {
                page = page,
                pageSize = 10
            });
        }

        [HttpPost]
        public IActionResult MakeAdmin(string selectedUserIds, int page)
        {
            var userIds = ParseUserIds(selectedUserIds);
            foreach (var userId in userIds)
            {
                var user = _userRepository.GetById(userId);
                if (user != null)
                {
                    _userRepository.MakeAdmin(userId);
                }
            }

            return RedirectToAction("Users", new
            {
                sortField = "Name",
                page = page,
                pageSize = 10
            });
        }


        [HttpPost]
        public async Task<IActionResult> RemoveAdmin(string selectedUserIds, int page)
        {
            var currentAdminId = _authServices.GetId();

            if (currentAdminId == null)
            {
                return Unauthorized("Could not identify the current user.");
            }

            var userIds = ParseUserIds(selectedUserIds);
            bool isDemotingSelf = userIds.Contains(currentAdminId.Value);

            foreach (var userId in userIds)
            {
                var user = _userRepository.GetById(userId);
                if (user != null)
                {
                    _userRepository.RemoveAdmin(userId);
                }
            }

            if (isDemotingSelf)
            {
                await HttpContext.SignOutAsync(AuthServices.AUTH_TYPE_KEY);

                TempData["StatusMessage"] = "Ваши права администратора были сняты. Пожалуйста, войдите в систему снова.";
                return RedirectToAction("Login", "Auth");
            }
            else
            {
                TempData["StatusMessage"] = "Права администратора были сняты для выбранных пользователей.";
                return RedirectToAction("Users", new
                {
                    page = page,
                    pageSize = 10
                });
            }
        }


        [HttpGet("Admin/UserProfile/{id}")]
        public async Task<IActionResult> UserProfile(int id)
        {
            if (!_authServices.IsAdmin())
            {
                return Forbid("You do not have permission to view this page.");
            }

            var user = await _webDbContext.Users
                .AsNoTracking()
                .Include(u => u.Templates)
                    .ThenInclude(t => t.Forms)
                .Include(u => u.Forms)
                    .ThenInclude(f => f.Template)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("User not found.");
            }


            var viewModel = new UserProfileViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                IsBlocked = user.IsBlocked,
                IsAdmin = user.Role == Role.Admin,
                Templates = user.Templates.Select(t => new UserTemplateSummaryViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    IsPublic = t.IsPublic,
                    TimesFilled = t.Forms.Count,
                    ImageUrl = t.ImageUrl
                }).ToList(),
                FilledForms = user.Forms.Select(f => new MyFormViewModel
                {
                    FormId = f.Id,
                    TemplateTitle = f.Template.Title,
                    TemplateDescription = f.Template.Description,
                    LastUpdated = f.UpdatedAt ?? f.CreatedAt,
                    TemplateImageUrl = f.Template.ImageUrl
                }).OrderByDescending(f => f.LastUpdated).ToList()
            };

            return View(viewModel);
        }


        private List<int> ParseUserIds(string selectedUserIds)
        {
            if (string.IsNullOrEmpty(selectedUserIds))
                return new List<int>();

            return selectedUserIds
                .Split(',')
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(int.Parse)
                .ToList();
        }
    }
}
