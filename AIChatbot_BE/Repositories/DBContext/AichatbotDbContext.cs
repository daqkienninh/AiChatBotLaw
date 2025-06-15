using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repositories.Models;
using System;
using System.Collections.Generic;

namespace Repositories.DBContext;

public partial class AichatbotDbContext : DbContext
{
    public AichatbotDbContext()
    {
    }

    public AichatbotDbContext(DbContextOptions<AichatbotDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<RegisteredUser> RegisteredUsers { get; set; }

    private string GetConnectionString()
    {
        IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true).Build();
        return configuration["ConnectionStrings:AIChatbotDB"];
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(GetConnectionString());
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("PK__Answer__33724318783AAB78");

            entity.ToTable("Answer");

            entity.Property(e => e.AnswerId)
                .HasMaxLength(36)
                .HasColumnName("answer_id");
            entity.Property(e => e.AnsContent)
                .HasMaxLength(4000)
                .HasColumnName("ans_content");
            entity.Property(e => e.AnsCreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ans_create_at");
            entity.Property(e => e.LegalclauseId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("legalclause_id");
            entity.Property(e => e.QuestionId)
                .HasMaxLength(36)
                .HasColumnName("question_id");

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK__Answer__question__52593CB8");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__2EC21549E2006468");

            entity.ToTable("Question");

            entity.Property(e => e.QuestionId)
                .HasMaxLength(36)
                .HasColumnName("question_id");
            entity.Property(e => e.QuesCreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ques_create_at");
            entity.Property(e => e.QuestionContent)
                .HasMaxLength(4000)
                .HasColumnName("question_content");
            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Questions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Question__user_i__4E88ABD4");
        });

        modelBuilder.Entity<RegisteredUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Register__B9BE370F009903D9");

            entity.ToTable("Registered_user");

            entity.HasIndex(e => e.UserEmail, "UQ__Register__B0FBA2122283A44E").IsUnique();

            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("role");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("user_email");
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .HasColumnName("user_name");
            entity.Property(e => e.UserStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("user_status");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.image)
                .HasMaxLength(255)
                .HasColumnName("image");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
