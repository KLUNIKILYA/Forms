using DataBase;
using DataBase.Models;
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
        WebDbContext _webDbContext;
        AuthServices _authServices;

        public FormsController(WebDbContext webDbContext, AuthServices authServices)
        {
            _webDbContext = webDbContext;
            _authServices = authServices;
        }


        [HttpGet("Forms/Open/{templateId}")]
        public async Task<IActionResult> OpenOrStartForm(int templateId)
        {
            var userId = GetCurrentUserId();

            var existingForm = await _webDbContext.Forms
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.UserId == userId && f.TemplateId == templateId);

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

                _webDbContext.Forms.Add(newForm);
                await _webDbContext.SaveChangesAsync();

                return RedirectToAction("Fill", new { formId = newForm.Id });
            }
        }

        [HttpGet("Forms/Fill/{formId}")]
        public IActionResult Fill(int formId)
        {
            return View("Form");
        }

        [HttpGet("Forms/Form/{id}")]
        public IActionResult Form()
        {
            return View();
        }


        [HttpGet("Forms/GetFormForFilling/{formId}")]
        public async Task<IActionResult> GetFormForFilling(int formId)
        {
            try
            {
                var form = await _webDbContext.Forms
                    .Include(f => f.Template)
                        .ThenInclude(t => t.Questions)
                            .ThenInclude(q => q.Options)
                    .Include(f => f.Template.AllowedUsers)
                    .Include(f => f.Answers)
                        .ThenInclude(a => a.Question)
                    .FirstOrDefaultAsync(f => f.Id == formId);

                if (form == null)
                {
                    return NotFound("Форма не найдена");
                }

                var userId = GetCurrentUserId();
                if (form.UserId != userId)
                {
                    if (!form.Template.IsPublic &&
                       !form.Template.AllowedUsers.Any(u => u.UserId == userId) &&
                       form.Template.AuthorId != userId)
                    {
                        return Forbid("У вас нет доступа к этой форме");
                    }
                }

                var existingAnswers = new Dictionary<int, object>();
                foreach (var answer in form.Answers)
                {
                    if (answer.Question.Type == Enums.Question.QuestionType.Checkbox && !string.IsNullOrEmpty(answer.Value))
                    {
                        try
                        {
                            var checkboxValues = JsonSerializer.Deserialize<List<string>>(answer.Value);
                            existingAnswers[answer.QuestionId] = checkboxValues;
                        }
                        catch { existingAnswers[answer.QuestionId] = new List<string>(); }
                    }
                    else
                    {
                        existingAnswers[answer.QuestionId] = answer.Value;
                    }
                }

                var response = new
                {
                    Id = form.Id,
                    Template = new
                    {
                        Title = form.Template.Title,
                        Description = form.Template.Description,
                        Questions = form.Template.Questions.Select(q => new
                        {
                            Id = q.Id,
                            Title = q.Title,
                            Description = q.Description,
                            Type = q.Type.ToString(),
                            IsRequired = q.IsRequired,
                            Options = q.Options.Select(o => o.Value).ToList()
                        }).ToList()
                    },
                    Answers = existingAnswers
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };

                return new JsonResult(response, jsonOptions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Произошла ошибка при загрузке формы");
            }
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

                var form = await _webDbContext.Forms
                    .Include(f => f.Answers)
                    .FirstOrDefaultAsync(f => f.Id == request.FormId);

                if (form == null)
                {
                    return NotFound("Форма не найдена.");
                }

                if (form.UserId != userId)
                {
                    return Forbid("У вас нет прав для отправки этой формы.");
                }

                _webDbContext.Answers.RemoveRange(form.Answers);
                await _webDbContext.SaveChangesAsync();

                var newAnswers = new List<AnswerData>();
                if (request.Answers != null)
                {
                    foreach (var answer in request.Answers)
                    {
                        var questionId = answer.Key;
                        var value = answer.Value;

                        var answerData = new AnswerData
                        {
                            FormId = request.FormId,
                            QuestionId = questionId
                        };

                        if (value is JsonElement element && element.ValueKind == JsonValueKind.Array)
                        {
                            answerData.Value = JsonSerializer.Serialize(element);
                        }
                        else
                        {
                            answerData.Value = value?.ToString();
                        }

                        newAnswers.Add(answerData);
                    }
                }

                if (newAnswers.Any())
                {
                    await _webDbContext.Answers.AddRangeAsync(newAnswers);
                }

                form.UpdatedAt = DateTime.UtcNow;

                await _webDbContext.SaveChangesAsync();

                return Ok(new { message = "Форма успешно сохранена!", formId = form.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Произошла внутренняя ошибка при сохранении формы.");
            }
        }


        [HttpGet]
        public async Task<IActionResult> MyForms()
        {
            // Получаем ID текущего пользователя
            var userId = GetCurrentUserId();

            // 1. Находим все записи FormData, принадлежащие этому пользователю
            var userForms = await _webDbContext.Forms
                .Where(f => f.UserId == userId)
                .Include(f => f.Template) // Обязательно включаем данные шаблона (для заголовка и описания)
                .OrderByDescending(f => f.UpdatedAt ?? f.CreatedAt) // Сортируем: сначала недавно измененные
                .ToListAsync();

            // 2. Преобразуем (мапим) данные из базы в удобную ViewModel
            var viewModel = userForms.Select(form => new MyFormViewModel
            {
                FormId = form.Id,
                TemplateTitle = form.Template.Title,
                TemplateDescription = form.Template.Description,
                // Используем дату обновления, если она есть, иначе - дату создания
                LastUpdated = form.UpdatedAt ?? form.CreatedAt,
                TemplateImageUrl = form.Template.ImageUrl // Получаем URL изображения из шаблона
            }).ToList();

            // 3. Передаем готовую модель в представление
            return View(viewModel);
        }



        private int GetCurrentUserId()
        {
            return _authServices.GetId().Value;
        }
    }
}