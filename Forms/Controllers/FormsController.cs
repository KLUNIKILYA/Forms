using DataBase;
using DataBase.Models;
using DataBase.Repositories;
using Forms.Models.Forms;
using Forms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Forms.Controllers
{
    public class FormsController : Controller
    {
        private readonly IFormRepository _formRepository;
        private readonly AuthServices _authServices;
        private readonly FormService _formService;

        public FormsController(IFormRepository formRepository, AuthServices authServices, FormService formService)
        {
            _formRepository = formRepository;
            _authServices = authServices;
            _formService = formService;
        }

        [HttpGet("Forms/Open/{templateId}")]
        public async Task<IActionResult> OpenOrStartForm(int templateId)
        {
            if (!_authServices.IsAuthenticated())
            {
                return RedirectToAction("FormReadingMode", new { templateId = templateId });
            }

            var userId = GetCurrentUserId();

            var existingForm = await _formRepository.GetByUserIdAndTemplateIdAsync(userId, templateId);

            if (existingForm != null)
            {
                return RedirectToAction("Fill", new { formId = existingForm.Id });
            }
            else
            {
                var newForm = new FormData
                {
                    TemplateId = templateId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _formRepository.Add(newForm);
                await _formRepository.SaveChangesAsync();

                return RedirectToAction("Fill", new { formId = newForm.Id });
            }
        }

        [HttpGet("Forms/Fill/{formId}")]
        public IActionResult Fill(int formId)
        {
            return View("Form");
        }

        [HttpGet("Forms/View/{templateId}")]
        public async Task<IActionResult> FormReadingMode(int templateId)
        {
            var isViewable = await _formRepository.IsTemplateViewableAsync(templateId);

            if (!isViewable)
            {
                return NotFound("The form template does not exist.");
            }

            return View("FormReadingMode");
        }

        [HttpGet("api/Forms/PublicTemplate/{templateId}")]
        public async Task<IActionResult> GetPublicFormTemplate(int templateId)
        {
            var formTemplate = (await _formRepository.IsTemplateViewableAsync(templateId))
                ? (await new DataBase.WebDbContext().Templates 
                    .Include(t => t.Questions).ThenInclude(q => q.Options).AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == templateId))
                : null;


            if (formTemplate == null)
            {
                return NotFound("Form template not found or is not public.");
            }

            var response = new
            {
                formTemplate.Id,
                Template = new
                {
                    formTemplate.Title,
                    formTemplate.Description,
                    Questions = formTemplate.Questions.Select(q => new
                    {
                        q.Id,
                        q.Title,
                        q.Description,
                        Type = q.Type.ToString(),
                        q.IsRequired,
                        Options = q.Options.Select(o => o.Value).ToList()
                    }).ToList()
                }
            };

            return new JsonResult(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }


        [HttpGet("Forms/GetFormForFilling/{formId}")]
        public async Task<IActionResult> GetFormForFilling(int formId)
        {
            var form = await _formRepository.GetFormForFillingAsync(formId);

            if (form == null)
            {
                return NotFound("Форма не найдена");
            }

            var currentUserId = GetCurrentUserId();
            var isAdmin = _authServices.IsAdmin();

            bool hasAccess = form.UserId == currentUserId || isAdmin;
            if (!hasAccess && !form.Template.IsPublic && !form.Template.AllowedUsers.Any(u => u.UserId == currentUserId))
            {
                return Forbid("У вас нет доступа к этой форме");
            }

            var existingAnswers = form.Answers.ToDictionary(
                a => a.QuestionId,
                a =>
                {
                    if (a.Question.Type == Enums.Question.QuestionType.Checkbox && !string.IsNullOrEmpty(a.Value))
                    {
                        try { return (object)JsonSerializer.Deserialize<List<string>>(a.Value)!; }
                        catch { return new List<string>(); }
                    }
                    return (object)a.Value!;
                });

            var response = new
            {
                Id = form.Id,
                Template = new
                {
                    Title = form.Template.Title,
                    Description = form.Template.Description,
                    Questions = form.Template.Questions.Select(q => new
                    {
                        q.Id,
                        q.Title,
                        q.Description,
                        Type = q.Type.ToString(),
                        q.IsRequired,
                        Options = q.Options.Select(o => o.Value).ToList()
                    }).ToList()
                },
                Answers = existingAnswers
            };

            return new JsonResult(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        [HttpPost]
        public async Task<IActionResult> SubmitForm([FromBody] SubmitFormRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetCurrentUserId();
                var (success, message, formId) = await _formService.SubmitFormAsync(request, userId);

                if (!success)
                {
                    if (message.Contains("прав")) return Forbid(message);
                    return BadRequest(new { message });
                }

                return Ok(new { message, formId });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, "Произошла внутренняя ошибка при сохранении формы.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyForms()
        {
            var userId = GetCurrentUserId();
            var userFormsData = await _formRepository.GetUserFilledFormsAsync(userId);

            var viewModel = userFormsData.Select(form => new MyFormViewModel
            {
                FormId = form.Id,
                TemplateTitle = form.Template.Title,
                TemplateDescription = form.Template.Description,
                LastUpdated = form.UpdatedAt ?? form.CreatedAt,
                TemplateImageUrl = form.Template.ImageUrl
            }).ToList();

            return View(viewModel);
        }

        private int GetCurrentUserId()
        {
            return _authServices.GetId()!.Value;
        }
    }
}