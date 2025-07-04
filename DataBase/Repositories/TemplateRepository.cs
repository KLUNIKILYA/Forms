using DataBase.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Repositories
{
    public interface ITemplateRepository
    {
        Task<TemplateData?> GetTemplateForEditingAsync(int id);

        Task<TemplateData?> GetPublicTemplateWithDetailsAsync(int templateId);

        Task<List<TemplateData>> GetRecentUserTemplatesAsync(int userId, int count);

        Task<List<TemplateData>> GetPopularTemplatesAsync(int count);

        Task<List<TemplateData>> GetUserTemplatesWithFillCountAsync(int userId);

        Task<bool> HasAnswersForQuestionAsync(int questionId);

        Task<TagData?> GetTagByNameAsync(string name);

        void Add(TemplateData template);

        void AddTag(TagData tag);
        void Remove(TemplateData template);
        void RemoveQuestions(IEnumerable<QuestionData> questions);

        Task<int> SaveChangesAsync();
    }
    public class TemplateRepository : ITemplateRepository
    {
        private WebDbContext _webDbContext;

        public TemplateRepository(WebDbContext context)
        {
            _webDbContext = context;
        }

        public void Add(TemplateData template)
        {
            _webDbContext.Templates.Add(template);
        }

        public void Remove(TemplateData template)
        {
            _webDbContext.Templates.Remove(template);
        }

        public void AddTag(TagData tag)
        {
            _webDbContext.Tags.Add(tag);
        }

        public async Task<List<TemplateData>> GetPopularTemplatesAsync(int count)
        {
            var popularTemplateIds = await _webDbContext.Forms
                .GroupBy(form => form.TemplateId)
                .Select(group => new
                {
                    TemplateId = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(count)
                .Select(x => x.TemplateId)
                .ToListAsync();

            return await _webDbContext.Templates
                .Where(t => popularTemplateIds.Contains(t.Id))
                .ToListAsync();
        }

        public async Task<List<TemplateData>> GetRecentUserTemplatesAsync(int userId, int count)
        {
            return await _webDbContext.Templates
                .Where(t => t.AuthorId == userId)
                .OrderByDescending(t => t.CreateAt)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<TagData?> GetTagByNameAsync(string name)
        {
            return await _webDbContext.Tags.FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task<TemplateData?> GetTemplateForEditingAsync(int id)
        {
            return await _webDbContext.Templates
                .Include(t => t.Tags)
                .Include(t => t.AllowedUsers)
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<TemplateData>> GetUserTemplatesWithFillCountAsync(int userId)
        {
            return await _webDbContext.Templates
                .Where(t => t.AuthorId == userId)
                .Include(t => t.Forms)
                .OrderBy(t => t.Title)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> HasAnswersForQuestionAsync(int questionId)
        {
            return await _webDbContext.Answers.AnyAsync(a => a.QuestionId == questionId);
        }

        public void RemoveQuestions(IEnumerable<QuestionData> questions)
        {
            _webDbContext.Questions.RemoveRange(questions);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _webDbContext.SaveChangesAsync();
        }

        public async Task<TemplateData?> GetPublicTemplateWithDetailsAsync(int templateId)
        {
            return await _webDbContext.Templates
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == templateId);
        }
    }
}
