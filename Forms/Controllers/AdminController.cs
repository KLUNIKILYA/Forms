using DataBase.Repositories;
using Enums.User;
using Forms.Models;
using Forms.Services;
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

        public AdminController(IUserRepository userRepository, AdminService adminService)
        {
            _userRepository = userRepository;
            _adminService = adminService;
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
        public IActionResult RemoveAdmin(string selectedUserIds, int page)
        {
            var userIds = ParseUserIds(selectedUserIds);
            foreach (var userId in userIds)
            {
                var user = _userRepository.GetById(userId);
                if (user != null)
                {
                    _userRepository.RemoveAdmin(userId);
                }
            }

            return RedirectToAction("Users", new
            {
                page = page,
                pageSize = 10
            });
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
