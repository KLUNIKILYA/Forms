using DataBase;
using Microsoft.EntityFrameworkCore;

namespace Forms.Services
{
    public class AdminService
    {
        private WebDbContext _webDbContext;

        public AdminService(WebDbContext webDbContext)
        {
            _webDbContext = webDbContext;
        }

        public async Task<bool> DeleteUserAndAllRelatedDataAsync(int userId)
        {
            var userToDelete = await _webDbContext.Users.FindAsync(userId);
            if (userToDelete == null) return false;

            var templatesToDelete = await _webDbContext.Templates
                .Where(t => t.AuthorId == userId)
                .ToListAsync();

            if (templatesToDelete.Any())
            {
                _webDbContext.Templates.RemoveRange(templatesToDelete);
            }

            var formsOnOtherTemplates = await _webDbContext.Forms
                .Where(f => f.UserId == userId && !templatesToDelete.Contains(f.Template))
                .ToListAsync();
            var commentsOnOtherTemplates = await _webDbContext.Comments
                .Where(c => c.UserId == userId && !templatesToDelete.Contains(c.Template))
                .ToListAsync();
            var likesOnOtherTemplates = await _webDbContext.Likes
                .Where(l => l.UserId == userId && !templatesToDelete.Contains(l.Template))
                .ToListAsync();
            var accessesOnOtherTemplates = await _webDbContext.TemplateAccesses
                .Where(ta => ta.UserId == userId && !templatesToDelete.Contains(ta.Template))
                .ToListAsync();

            if (formsOnOtherTemplates.Any())
            {
                _webDbContext.Forms.RemoveRange(formsOnOtherTemplates);
            }
            if (commentsOnOtherTemplates.Any())
            {
                _webDbContext.Comments.RemoveRange(commentsOnOtherTemplates);
            }
            if (likesOnOtherTemplates.Any())
            {
                _webDbContext.Likes.RemoveRange(likesOnOtherTemplates);
            }
            if (accessesOnOtherTemplates.Any())
            {
                _webDbContext.TemplateAccesses.RemoveRange(accessesOnOtherTemplates);
            }

            _webDbContext.Users.Remove(userToDelete);

            await _webDbContext.SaveChangesAsync();

            return true;
        }
    }
}
