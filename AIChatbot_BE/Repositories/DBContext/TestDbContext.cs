using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repositories.Models;
using System;
using System.Collections.Generic;

namespace Repositories.DBContext;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<ChatRoom> ChatRooms { get; set; }

    public virtual DbSet<ChatRoomQuestion> ChatRoomQuestions { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public static string GetConnectionString(string connectionStringName)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        string connectionString = config.GetConnectionString(connectionStringName);
        return connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(GetConnectionString("AIChatbotDB")).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("PK__Answer__33724318AC01B085");

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
                .HasConstraintName("FK__Answer__question__4D94879B");
        });

        modelBuilder.Entity<ChatRoom>(entity =>
        {
            entity.HasKey(e => e.ChatId).HasName("PK__ChatRoom__A9FBE7C6530BE01E");

            entity.ToTable("ChatRoom");

            entity.Property(e => e.ChatId).HasMaxLength(36);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasMaxLength(36);
        });

        modelBuilder.Entity<ChatRoomQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChatRoom__3214EC07B6BFFE4C");

            entity.ToTable("ChatRoomQuestion");

            entity.Property(e => e.ChatId).HasMaxLength(36);
            entity.Property(e => e.QuestionId).HasMaxLength(36);

            entity.HasOne(d => d.Chat).WithMany(p => p.ChatRoomQuestions)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChatRoomQ__ChatI__52593CB8");

            entity.HasOne(d => d.Question).WithMany(p => p.ChatRoomQuestions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChatRoomQ__Quest__534D60F1");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__2EC21549F4D976D8");

            entity.ToTable("Question");

            entity.Property(e => e.QuestionId)
                .HasMaxLength(36)
                .HasColumnName("question_id");
            entity.Property(e => e.Embedding).HasColumnName("embedding");
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
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
