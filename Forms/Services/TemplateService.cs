using DataBase.Models;
using DataBase.Repositories;
using Enums.Question;
using Forms.Models.Templates;

namespace Forms.Services
{
    public class TemplateService
    {
        private readonly ITemplateRepository _templateRepository;

        public TemplateService(ITemplateRepository templateRepository)
        {
            _templateRepository = templateRepository;
        }

        public async Task<(TemplateData? Template, string? ErrorMessage)> CreateTemplateAsync(TemplateViewModel model, int authorId)
        {
            var template = new TemplateData
            {
                Title = model.Title,
                Description = model.Description,
                Topic = model.Topic,
                IsPublic = model.IsPublic,
                AuthorId = authorId,
                CreateAt = DateTime.UtcNow,
            };

            if (model.Tags != null && model.Tags.Any())
            {
                foreach (var tagName in model.Tags)
                {
                    var existingTag = await _templateRepository.GetTagByNameAsync(tagName);
                    if (existingTag != null)
                    {
                        template.Tags.Add(existingTag);
                    }
                    else
                    {
                        var newTag = new TagData { Name = tagName };
                        _templateRepository.AddTag(newTag);
                        template.Tags.Add(newTag);
                    }
                }
            }


            if (model.Questions != null)
            {
                foreach (var questionDto in model.Questions)
                {
                    var question = new QuestionData
                    {
                        Title = questionDto.Text,
                        Description = questionDto.Description,
                        Type = questionDto.Type,
                        IsRequired = questionDto.IsRequired,
                        ShowInTable = questionDto.ShowInTable
                    };

                    if ((questionDto.Type == QuestionType.Checkbox || questionDto.Type == QuestionType.Dropdown) && questionDto.Options != null)
                    {
                        foreach (var optionText in questionDto.Options)
                        {
                            question.Options.Add(new QuestionOptionData { Value = optionText });
                        }
                    }
                    template.Questions.Add(question);
                }
            }

            _templateRepository.Add(template);
            await _templateRepository.SaveChangesAsync();

            return (template, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateTemplateAsync(int templateId, TemplateViewModel model, int currentUserId, bool isAdmin)
        {
            var templateToUpdate = await _templateRepository.GetTemplateForEditingAsync(templateId);

            if (templateToUpdate == null)
            {
                return (false, "Template not found.");
            }

            if (templateToUpdate.AuthorId != currentUserId && !isAdmin)
            {
                return (false, "You do not have permission to update this template.");
            }

            templateToUpdate.Title = model.Title;
            templateToUpdate.Description = model.Description;
            templateToUpdate.Topic = model.Topic;
            templateToUpdate.IsPublic = model.IsPublic;

            templateToUpdate.Tags.Clear();
            if (model.Tags != null && model.Tags.Any())
            {
                foreach (var tagName in model.Tags)
                {
                    var existingTag = await _templateRepository.GetTagByNameAsync(tagName);
                    templateToUpdate.Tags.Add(existingTag ?? new TagData { Name = tagName });
                }
            }

            var incomingQuestionIds = model.Questions.Select(q => q.Id).Where(qId => qId > 0).ToHashSet();

            var questionsToDelete = new List<QuestionData>();
            foreach (var existingQuestion in templateToUpdate.Questions.ToList())
            {
                if (!incomingQuestionIds.Contains(existingQuestion.Id))
                {
                    if (await _templateRepository.HasAnswersForQuestionAsync(existingQuestion.Id))
                    {
                        return (false, $"Cannot delete question '{existingQuestion.Title}' because it already has answers. Please create a new template for breaking changes.");
                    }
                    questionsToDelete.Add(existingQuestion);
                }
            }
            if (questionsToDelete.Any())
            {
                _templateRepository.RemoveQuestions(questionsToDelete);
            }

            foreach (var questionDto in model.Questions)
            {
                QuestionData? question;
                if (questionDto.Id > 0) 
                {
                    question = templateToUpdate.Questions.FirstOrDefault(q => q.Id == questionDto.Id);
                    if (question == null) continue;
                }
                else 
                {
                    question = new QuestionData();
                    templateToUpdate.Questions.Add(question);
                }

                question.Title = questionDto.Text;
                question.Description = questionDto.Description;
                question.Type = questionDto.Type;
                question.IsRequired = questionDto.IsRequired;
                question.ShowInTable = questionDto.ShowInTable;

                question.Options.Clear();
                if ((question.Type == QuestionType.Checkbox || question.Type == QuestionType.Dropdown) && questionDto.Options != null)
                {
                    foreach (var optionText in questionDto.Options)
                    {
                        question.Options.Add(new QuestionOptionData { Value = optionText });
                    }
                }
            }

            await _templateRepository.SaveChangesAsync();
            return (true, null);
        }
    }
}
