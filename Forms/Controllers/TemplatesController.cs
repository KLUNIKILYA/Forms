using DataBase;
using DataBase.Models;
using DataBase.Repositories;
using Enums.Question;
using Forms.Models;
using Forms.Models.Templates;
using Forms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Forms.Controllers
{
    public class TemplatesController : Controller
    {
        private readonly AuthServices _authServices;
        private readonly ITemplateRepository _templateRepository;
        private readonly TemplateService _templateService;

        public TemplatesController(AuthServices authServices, ITemplateRepository templateRepository, TemplateService templateService)
        {
            _authServices = authServices;
            _templateRepository = templateRepository;
            _templateService = templateService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();
            viewModel.IsAuthenticated = _authServices.IsAuthenticated();

            var popularTemplatesData = await _templateRepository.GetPopularTemplatesAsync(5);
            viewModel.PopularTemplates = popularTemplatesData.Select(t => new TemplateIndexViewModel
            {
                Id = t.Id,
                Title = t.Title
            }).ToList();

            if (viewModel.IsAuthenticated)
            {
                viewModel.UserId = _authServices.GetId();
                viewModel.UserName = _authServices.GetName();
                viewModel.IsAdmin = _authServices.IsAdmin();

                var recentTemplatesData = await _templateRepository.GetRecentUserTemplatesAsync(viewModel.UserId.Value, 5);
                viewModel.RecentUserTemplates = recentTemplatesData.Select(t => new TemplateIndexViewModel
                {
                    Id = t.Id,
                    Title = t.Title
                }).ToList();
            }

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult CreateTemplate()
        {
            if (!_authServices.IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }
            return View("CreateTemplate");
        }

        [HttpPost]
        public async Task<IActionResult> CreateTemplate([FromBody] TemplateViewModel templateModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _authServices.GetId();
            if (currentUserId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            var (template, errorMessage) = await _templateService.CreateTemplateAsync(templateModel, currentUserId.Value);

            if (template == null)
            {
                return StatusCode(500, new { message = errorMessage ?? "An unexpected error occurred." });
            }

            var resultDto = new
            {
                Id = template.Id,
                Title = template.Title,
            };

            return CreatedAtAction(nameof(GetTemplateData), new { id = template.Id }, resultDto);
        }

        [HttpGet("Templates/Data/{id}")]
        public async Task<IActionResult> GetTemplateData(int id)
        {
            var template = await _templateRepository.GetTemplateForEditingAsync(id);

            if (template == null)
            {
                return NotFound();
            }

            return Ok(template);
        }



        [HttpGet]
        public async Task<IActionResult> MyTemplates()
        {
            var userId = _authServices.GetId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var userTemplatesData = await _templateRepository.GetUserTemplatesWithFillCountAsync(userId.Value);

            var viewModel = userTemplatesData.Select(template => new UserTemplateViewModel
            {
                Id = template.Id,
                Title = template.Title,
                Description = template.Description,
                IsPublic = template.IsPublic,
                ImageUrl = template.ImageUrl,
                TimesFilled = template.Forms.Count
            }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!_authServices.IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }

            var currentUserId = _authServices.GetId().Value;

            var template = await _templateRepository.GetTemplateForEditingAsync(id);

            if (template == null)
            {
                return NotFound();
            }

            if (template.AuthorId != currentUserId && !_authServices.IsAdmin())
            {
                return Forbid("You do not have permission to edit this template.");
            }

            var viewModel = new TemplateEditViewModel
            {
                Id = template.Id,
                Title = template.Title,
                Description = template.Description,
                Topic = template.Topic,
                IsPublic = template.IsPublic,
                Tags = template.Tags.Select(t => t.Name).ToList(),
                AllowedUserIds = template.AllowedUsers.Select(au => au.UserId).ToList(),
                Questions = template.Questions.OrderBy(q => q.Id).Select(q => new QuestionViewModel
                {
                    Id = q.Id,
                    Text = q.Title,
                    Description = q.Description,
                    Type = q.Type,
                    IsRequired = q.IsRequired,
                    ShowInTable = q.ShowInTable,
                    Options = q.Options.Select(o => o.Value).ToList()
                }).ToList()
            };

            return View("Edit", viewModel);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTemplate(int id, [FromBody] TemplateViewModel templateModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _authServices.GetId();
            if (currentUserId == null) return Unauthorized();

            var (success, errorMessage) = await _templateService.UpdateTemplateAsync(id, templateModel, currentUserId.Value, _authServices.IsAdmin());

            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }

            return Ok(new { message = "Template updated successfully.", id = id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = _authServices.GetId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var template = await _templateRepository.GetTemplateForEditingAsync(id);

            if (template == null)
            {
                return NotFound();
            }

            if (template.AuthorId != currentUserId.Value && !_authServices.IsAdmin())
            {
                return Forbid("You do not have permission to delete this template.");
            }

            _templateRepository.Remove(template);
            await _templateRepository.SaveChangesAsync();

            return RedirectToAction(nameof(MyTemplates));
        }
    }
}
