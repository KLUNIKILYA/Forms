using DataBase.Models;
using DataBase.Repositories;
using Forms.Models.Forms;
using System.Text.Json;

namespace Forms.Services
{
    public class FormService
    {
        private readonly IFormRepository _formRepository;
        private readonly AuthServices _authServices;

        public FormService(IFormRepository formRepository, AuthServices authServices)
        {
            _formRepository = formRepository;
            _authServices = authServices;
        }

        public async Task<(bool Success, string ErrorMessage, int? FormId)> SubmitFormAsync(SubmitFormRequest request, int currentUserId)
        {
            var form = await _formRepository.GetFormForFillingAsync(request.FormId);

            if (form == null)
            {
                return (false, "Форма не найдена.", null);
            }

            if (form.UserId != currentUserId && !_authServices.IsAdmin())
            {
                return (false, "У вас нет прав для отправки этой формы.", null);
            }

            if (form.Answers.Any())
            {
                _formRepository.RemoveAnswers(form.Answers);
            }

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
                        QuestionId = questionId,
                        Value = (value is JsonElement element && element.ValueKind == JsonValueKind.Array)
                                ? JsonSerializer.Serialize(element)
                                : value?.ToString()
                    };
                    newAnswers.Add(answerData);
                }
            }

            if (newAnswers.Any())
            {
                await _formRepository.AddAnswersAsync(newAnswers);
            }

            form.UpdatedAt = DateTime.UtcNow;

            await _formRepository.SaveChangesAsync();

            return (true, "Форма успешно сохранена!", form.Id);
        }
    }
}
