using DataBase.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Repositories
{
    public interface IFormRepository
    {
        Task<FormData?> GetByUserIdAndTemplateIdAsync(int userId, int templateId);

        Task<FormData?> GetFormForFillingAsync(int formId);

        Task<List<FormData>> GetUserFilledFormsAsync(int userId);

        Task<bool> IsTemplateViewableAsync(int templateId);

        void Add(FormData form);

        Task AddAnswersAsync(IEnumerable<AnswerData> answers);

        void RemoveAnswers(IEnumerable<AnswerData> answers);

        Task<int> SaveChangesAsync();
    }

    public class FormRepository : IFormRepository
    {
        private WebDbContext _context;

        public FormRepository(WebDbContext context)
        {
            _context = context;
        }

        public void Add(FormData form)
        {
            _context.Forms.Add(form);
        }

        public async Task AddAnswersAsync(IEnumerable<AnswerData> answers)
        {
            await _context.Answers.AddRangeAsync(answers);
        }

        public async Task<FormData?> GetByUserIdAndTemplateIdAsync(int userId, int templateId)
        {
            return await _context.Forms
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.UserId == userId && f.TemplateId == templateId);
        }

        public async Task<FormData?> GetFormForFillingAsync(int formId)
        {
            return await _context.Forms
                .Include(f => f.Template)
                    .ThenInclude(t => t!.Questions)
                        .ThenInclude(q => q.Options)
                .Include(f => f.Template!.AllowedUsers)
                .Include(f => f.Answers)
                    .ThenInclude(a => a.Question)
                .FirstOrDefaultAsync(f => f.Id == formId);
        }

        public async Task<List<FormData>> GetUserFilledFormsAsync(int userId)
        {
            return await _context.Forms
                .Where(f => f.UserId == userId)
                .Include(f => f.Template) 
                .OrderByDescending(f => f.UpdatedAt ?? f.CreatedAt) 
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> IsTemplateViewableAsync(int templateId)
        {
            return await _context.Templates.AnyAsync(t => t.Id == templateId);
        }

        public void RemoveAnswers(IEnumerable<AnswerData> answers)
        {
            _context.Answers.RemoveRange(answers);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
