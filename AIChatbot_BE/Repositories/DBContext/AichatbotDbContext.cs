//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Repositories.Models;
//using System;
//using System.Collections.Generic;

//namespace Repositories.DBContext;

//public partial class AichatbotDbContext : DbContext
//{
//    public AichatbotDbContext()
//    {
//    }

//    public AichatbotDbContext(DbContextOptions<AichatbotDbContext> options)
//        : base(options)
//    {
//    }

//    public virtual DbSet<Answer> Answers { get; set; }

//    public virtual DbSet<ChatRoom> ChatRooms { get; set; }

//    public virtual DbSet<ChatRoomQuestion> ChatRoomQuestions { get; set; }

//    public virtual DbSet<Question> Questions { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=KININ\\SQLEXPRESS;Uid=sa;Pwd=1234567890;Database=AIChatbotDB;TrustServerCertificate=True");

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        modelBuilder.Entity<Answer>(entity =>
//        {
//            entity.HasKey(e => e.AnswerId).HasName("PK__Answer__33724318380E99B5");

//            entity.ToTable("Answer");

//            entity.Property(e => e.AnswerId)
//                .HasMaxLength(36)
//                .HasColumnName("answer_id");
//            entity.Property(e => e.AnsContent)
//                .HasMaxLength(4000)
//                .HasColumnName("ans_content");
//            entity.Property(e => e.AnsCreateAt)
//                .HasDefaultValueSql("(getdate())")
//                .HasColumnType("datetime")
//                .HasColumnName("ans_create_at");
//            entity.Property(e => e.LegalclauseId)
//                .HasMaxLength(100)
//                .IsUnicode(false)
//                .HasColumnName("legalclause_id");
//            entity.Property(e => e.QuestionId)
//                .HasMaxLength(36)
//                .HasColumnName("question_id");

//            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
//                .HasForeignKey(d => d.QuestionId)
//                .HasConstraintName("FK__Answer__question__4D94879B");
//        });

//        modelBuilder.Entity<ChatRoom>(entity =>
//        {
//            entity.HasKey(e => e.ChatId).HasName("PK__ChatRoom__826385AD3EFFB7A6");

//            entity.ToTable("ChatRoom");

//            entity.Property(e => e.ChatId)
//                .HasMaxLength(36)
//                .HasColumnName("chatId");
//            entity.Property(e => e.CreateAt)
//                .HasColumnType("datetime")
//                .HasColumnName("create_at");
//            entity.Property(e => e.UserId)
//                .HasMaxLength(1)
//                .HasColumnName("userId");
//        });

//        modelBuilder.Entity<ChatRoomQuestion>(entity =>
//        {
//            entity.HasKey(e => e.Id).HasName("PK__ChatRoom__3213E83FFA81D239");

//            entity.ToTable("ChatRoomQuestion");

//            entity.Property(e => e.Id)
//                .HasMaxLength(36)
//                .HasColumnName("id");
//            entity.Property(e => e.ChatId)
//                .HasMaxLength(36)
//                .HasColumnName("chatId");
//            entity.Property(e => e.QuestionId)
//                .HasMaxLength(36)
//                .HasColumnName("question_id");

//            entity.HasOne(d => d.Chat).WithMany(p => p.ChatRoomQuestions)
//                .HasForeignKey(d => d.ChatId)
//                .HasConstraintName("FK__ChatRoomQ__chatI__5812160E");

//            entity.HasOne(d => d.Question).WithMany(p => p.ChatRoomQuestions)
//                .HasForeignKey(d => d.QuestionId)
//                .HasConstraintName("FK__ChatRoomQ__quest__571DF1D5");
//        });

//        modelBuilder.Entity<Question>(entity =>
//        {
//            entity.HasKey(e => e.QuestionId).HasName("PK__Question__2EC21549CBE73D83");

//            entity.ToTable("Question");

//            entity.Property(e => e.QuestionId)
//                .HasMaxLength(36)
//                .HasColumnName("question_id");
//            entity.Property(e => e.QuesCreateAt)
//                .HasDefaultValueSql("(getdate())")
//                .HasColumnType("datetime")
//                .HasColumnName("ques_create_at");
//            entity.Property(e => e.QuestionContent)
//                .HasMaxLength(4000)
//                .HasColumnName("question_content");
//            entity.Property(e => e.UserId)
//                .HasMaxLength(36)
//                .HasColumnName("user_id");
//            entity.Property(e => e.Embedding)
//                .HasColumnName("Embedding");
//        });

//        OnModelCreatingPartial(modelBuilder);
//    }

//    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
//}
