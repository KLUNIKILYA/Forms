using DataBase.Models;
using Microsoft.EntityFrameworkCore;

namespace DataBase
{
    public class WebDbContext : DbContext
    {
        public const string CONNECTION_STRING = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=\"FormsDatabase\";Integrated Security=True;";
        public DbSet<AnswerData> Answers { get; set; }
        public DbSet<CommentData> Comments { get; set; }
        public DbSet<FormData> Forms { get; set; }
        public DbSet<LikeData> Likes { get; set; }
        public DbSet<QuestionData> Questions { get; set; }
        public DbSet<QuestionOptionData> QuestionOptions { get; set; }
        public DbSet<TagData> Tags { get; set; }
        public DbSet<TemplateAccess> TemplateAccesses { get; set; }
        public DbSet<TemplateData> Templates { get; set; }
        public DbSet<UserData> Users { get; set; }
        
        public WebDbContext() { }

        public WebDbContext(DbContextOptions<WebDbContext> contextOptions)
            : base(contextOptions) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(CONNECTION_STRING);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserData>(entity =>
            {
                entity.HasIndex(u => u.Email, "IX_Users_Email").IsUnique();

                entity.HasMany(u => u.Templates)
                      .WithOne(t => t.Author)
                      .HasForeignKey(t => t.AuthorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TemplateData>(entity =>
            {
                entity.HasIndex(t => t.IsPublic, "IX_Templates_IsPublic");

                entity.HasMany(t => t.Forms)
                    .WithOne(f => f.Template)
                    .HasForeignKey(f => f.TemplateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(t => t.Questions).
                    WithOne(q => q.Template)
                    .HasForeignKey(q => q.TemplateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(t => t.Comments)
                    .WithOne(c => c.Template)
                    .HasForeignKey(c => c.TemplateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(t => t.Likes)
                    .WithOne(l => l.Template)
                    .HasForeignKey(l => l.TemplateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(t => t.AllowedUsers)
                    .WithOne(a => a.Template)
                    .HasForeignKey(a => a.TemplateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(t => t.Tags)
                      .WithMany(t => t.TemplateTags)
                      .UsingEntity<Dictionary<string, object>>(
                          "TemplateTag",
                          j => j.HasOne<TagData>().WithMany().HasForeignKey("TagId"),
                          j => j.HasOne<TemplateData>().WithMany().HasForeignKey("TemplateId")
                      );
            });



            modelBuilder.Entity<FormData>(entity =>
            {
                entity.HasOne(f => f.User)
                    .WithMany(u => u.Forms)
                    .HasForeignKey(f => f.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(f => f.TemplateId, "IX_Forms_TemplateId");
                entity.HasIndex(f => f.UserId, "IX_Forms_UserId");
            });

            modelBuilder.Entity<CommentData>(entity =>
            {
                entity.HasOne(c => c.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<LikeData>(entity =>
            {
                entity.HasIndex(l => new { l.TemplateId, l.UserId }, "IX_Likes_TemplateId_UserId").IsUnique();
                entity.HasOne(l => l.User)
                    .WithMany(u => u.Likes)
                    .HasForeignKey(l => l.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TemplateAccess>(entity =>
            {
                entity.HasKey(ta => new { ta.TemplateId, ta.UserId });
                entity.HasOne(ta => ta.User)
                    .WithMany(u => u.AccessTemplates)
                    .HasForeignKey(ta => ta.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AnswerData>(entity =>
            {
                entity.HasOne(a => a.Form)
                    .WithMany(f => f.Answers)
                    .HasForeignKey(a => a.FormId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Question)
                    .WithMany()
                    .HasForeignKey(a => a.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<QuestionData>(entity =>
            {
                entity.Property(q => q.Type).HasConversion<string>();
            });

            modelBuilder.Entity<QuestionOptionData>(entity =>
            {
                entity.HasOne(o => o.Question)
                    .WithMany(q => q.Options)
                    .HasForeignKey(o => o.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
