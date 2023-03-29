using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GrpcServer.Entities;

public partial class TalkzContext : DbContext
{
    public TalkzContext()
    {
    }

    public TalkzContext(DbContextOptions<TalkzContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Friendlist> Friendlists { get; set; }

    public virtual DbSet<Userchat> Userchats { get; set; }

    public virtual DbSet<Usercredential> Usercredentials { get; set; }

    public virtual DbSet<Usermessage> Usermessages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Friendlist>(entity =>
        {
            entity.HasKey(e => new { e.UserId1, e.UserId2 }).HasName("PRIMARY");

            entity.ToTable("friendlist");

            entity.Property(e => e.UserId1)
                .HasColumnType("int(11)")
                .HasColumnName("userId1");
            entity.Property(e => e.UserId2)
                .HasColumnType("int(11)")
                .HasColumnName("userId2");
            entity.Property(e => e.IsFriend)
                .HasDefaultValueSql("'NULL'")
                .HasColumnName("isFriend");
        });

        modelBuilder.Entity<Userchat>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ChatId }).HasName("PRIMARY");

            entity.ToTable("userchats");

            entity.Property(e => e.UserId)
                .HasColumnType("int(11)")
                .HasColumnName("userId");
            entity.Property(e => e.ChatId)
                .HasColumnType("int(11)")
                .HasColumnName("chatId");
        });

        modelBuilder.Entity<Usercredential>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("usercredentials");

            entity.Property(e => e.UserId)
                .HasColumnType("int(11)")
                .HasColumnName("userId");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.UserPicLink)
                .HasMaxLength(255)
                .HasDefaultValueSql("'NULL'")
                .HasColumnName("userPicLink");
            entity.Property(e => e.Username)
                .HasMaxLength(12)
                .HasColumnName("username");
            entity.Property(e => e.UsernameId)
                .HasColumnType("int(11)")
                .HasColumnName("usernameId");
        });

        modelBuilder.Entity<Usermessage>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("usermessages");

            entity.Property(e => e.ChatId)
                .HasColumnType("int(11)")
                .HasColumnName("chatId");
            entity.Property(e => e.IsEdited)
                .HasDefaultValueSql("'NULL'")
                .HasColumnName("isEdited");
            entity.Property(e => e.IsRead)
                .HasDefaultValueSql("'NULL'")
                .HasColumnName("isRead");
            entity.Property(e => e.Message)
                .HasColumnType("text")
                .HasColumnName("message");
            entity.Property(e => e.MessageTimestamp)
                .HasColumnType("bigint(20)")
                .HasColumnName("messageTimestamp");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
