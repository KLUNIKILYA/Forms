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
        private AuthServices _authServices;
        private WebDbContext _webDbContext;
        private readonly IUserRepository _userRepository;

        public TemplatesController(WebDbContext webDbContext, IUserRepository userRepository, AuthServices authServices)
        {
            _webDbContext = webDbContext;
            _userRepository = userRepository;
            _authServices = authServices;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();

            viewModel.IsAuthenticated = _authServices.IsAuthenticated();
            if (viewModel.IsAuthenticated)
            {
                viewModel.UserId = _authServices.GetId();
                viewModel.UserName = _authServices.GetName();
                viewModel.IsAdmin = _authServices.IsAdmin();
            }

            var popularTemplatesQuery = _webDbContext.Forms
                .GroupBy(form => form.TemplateId)
                .Select(group => new
                {
                    TemplateId = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(x => x.Count).Take(5).Join(
                    _webDbContext.Templates,
                    popular => popular.TemplateId,
                    template => template.Id,
                    (popular, template) => new TemplateIndexViewModel
                    {
                        Id = template.Id,
                        Title = template.Title
                    }
                );

            viewModel.PopularTemplates = await popularTemplatesQuery.ToListAsync();

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult CreateTemplate()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateTemplate([FromBody] TemplateViewModel templateModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);

            }

            var currentUserId = _authServices.GetId();

            var currentUser =  _webDbContext.Users.Find(currentUserId.Value);

            if (currentUser == null || currentUser.IsBlocked)
            {
                return Unauthorized("User not found or is blocked.");
            }

            var template = new TemplateData
            {
                Title = templateModel.Title,
                Description = templateModel.Description,
                Topic = templateModel.Topic,
                IsPublic = templateModel.IsPublic,
                AuthorId = currentUserId.Value,
            };

            if (templateModel.Tags != null && templateModel.Tags.Any())
            {
                foreach (var tagName in templateModel.Tags)
                {
                    var existingTag = _webDbContext.Tags.FirstOrDefault(t => t.Name == tagName);
                    if (existingTag != null)
                    {
                        template.Tags.Add(existingTag);
                    }
                    else
                    {
                        var newTag = new TagData { Name = tagName };
                        _webDbContext.Tags.Add(newTag);

                        template.Tags.Add(newTag);
                    }
                }
            }

            if (!template.IsPublic && templateModel.AllowedUserIds != null)
            {

            }

            if (templateModel.Questions != null)
            {
                foreach (var questionDto in templateModel.Questions)
                {
                    var question = new QuestionData
                    {
                        Title = questionDto.Text,
                        Description = questionDto.Description,
                        Type = questionDto.Type,
                        IsRequired = questionDto.IsRequired,
                        ShowInTable = questionDto.ShowInTable,
                        Template = template
                    };

                    if ((questionDto.Type == QuestionType.Checkbox || questionDto.Type == QuestionType.Dropdown)
                        && questionDto.Options != null)
                    {
                        foreach (var optionText in questionDto.Options)
                        {
                            question.Options.Add(new QuestionOptionData
                            {
                                Value = optionText,
                                Question = question
                            });
                        }
                    }
                    template.Questions.Add(question);
                }
            }

            _webDbContext.Templates.Add(template);

            _webDbContext.SaveChangesAsync();

            var resultDto = new
            {
                Id = template.Id,
                Title = template.Title,

            };

            return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, resultDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTemplate(int id)
        {
            var template = await _webDbContext.Templates
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
            {
                return NotFound();
            }

            return Ok(template);
        }

        [HttpGet]
        public async Task<IActionResult> MyTemplates()
        {
            var userId = _authServices.GetId()!.Value;

            var userTemplates = await _webDbContext.Templates
                .Where(t => t.AuthorId == userId)
                .Include(t => t.Forms)
                .OrderBy(t => t.Title)
                .ToListAsync();

            var viewModel = userTemplates.Select(template => new UserTemplateViewModel
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
    }
}
