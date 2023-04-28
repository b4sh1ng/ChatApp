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

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<Friendlist> Friendlists { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Usercredential> Usercredentials { get; set; }

    public virtual DbSet<Usersession> Usersessions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ChatId }).HasName("PRIMARY");

            entity.ToTable("chats");

            entity.Property(e => e.UserId)
                .HasColumnType("int(11)")
                .HasColumnName("userId");
            entity.Property(e => e.ChatId)
                .HasColumnType("int(11)")
                .HasColumnName("chatId");
            entity.Property(e => e.IsListed).HasColumnName("isListed");
        });

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
                .HasDefaultValueSql("'0'")
                .HasColumnName("isFriend");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PRIMARY");

            entity.ToTable("messages");

            entity.Property(e => e.MessageId)
                .HasColumnType("int(11)")
                .HasColumnName("messageId");
            entity.Property(e => e.ChatId)
                .HasColumnType("int(11)")
                .HasColumnName("chatId");
            entity.Property(e => e.FromId)
                .HasColumnType("int(11)")
                .HasColumnName("fromId");
            entity.Property(e => e.IsEdited)
                .HasDefaultValueSql("'0'")
                .HasColumnName("isEdited");
            entity.Property(e => e.IsRead)
                .HasDefaultValueSql("'0'")
                .HasColumnName("isRead");
            entity.Property(e => e.Message1).HasColumnName("message");
            entity.Property(e => e.MessageTimestamp)
                .HasColumnType("bigint(20)")
                .HasColumnName("messageTimestamp");
        });

        modelBuilder.Entity<Usercredential>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("usercredentials");

            entity.Property(e => e.UserId)
                .HasColumnType("int(11)")
                .HasColumnName("userId");
            entity.Property(e => e.CurrentStatus)
                .HasColumnType("int(11)")
                .HasColumnName("currentStatus");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.LastStatus)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int(11)")
                .HasColumnName("lastStatus");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.ProfileImgB64)
                .HasDefaultValueSql("iVBORw0KGgoAAAANSUhEUgAAAMMAAAC9CAIAAACBNV5MAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAPcSURBVHhe7dtBdqNYDAXQ7H8HPe1d9Q769Coayj+J/ys78EFgkrr3vCFIstDUbwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/B3/8+DqwVp/Mw8JU4l8VAihMZCjRxGRsCeRN3+euf/yLxQBcOEVueck0x5HvigCLx8GcoE5t9mEuJ2ZZu6D7x4hwKxE4XcwUx0sgZ3RKvz2GX2ObKvFYMM35Gt0SROWwUexzKC8UkLunFYonviXVPiQc+8xIxw9YzuiVKzWFMrO9XYsuReLjlfP0AMeSGRMHWhbVifes+Sbwy52TR/YhLmsJasbiR7xEvzjlT3zpm25wo23qxrF9crHUx8XqreY6+dQy2OVG29WJBbG3/JU05Td83BtucKNt6saDfWux0ZaJIq3y0aFp3SVOicuvIV/qVxUJXJoq0ykfrm8ZIOxPFW0eein3t+B5Rp9U/VN8x5tmZKN468lS/r9jmUKJUq3+ovmPMszNRvHXkqX5fsc2hRKlW/1B9x5hnZ6J468hT/b5im0OJUq3+ofqOMc/ORPHWkaf6fcU2hxKlWv1D9R1jnp2J4q0jT/X7im0OJUq1+ofqO8Y8OxPFW0ee6vcV2xxKlGr1D9V3jHl2Joq3jjwV+9rxPaJOq3+o6Fh6TFG5deQr/cpioSsTRVrlE/R9Y6rNibKtFwv6rcVOVyaKtMon6PvGVJsTZVsvFsTWxr9HvD7nNH3fGGxzomzrxbJ+cbHWxcTrreY5onXFMUXBOawVixv5HvHinJP13WO8DYmCrQtrxfrWfZJ4peVk0X3fMUWpOYyJ9f1KbDkSD7e8RMyw45iiTqvPmFjie2LXU+KBz7xKjLH1kqLIHDaKPQ7ltWKY8WOK1+ewS2xzZa4gRho5pnixhb1ioYu5iJjqPXE0kXj4M5SJzT7M1cR4d4kDmhIPdOEQseUpVxajbgg0cRlDgRQnshh4Km7li8AqcTcfAQAAAAAAAAAAAAAAAAAAKOFfzBT4OCPHxC4uiRouiRouiRouiRouiRouiRouiRouiRouiRouiRouiRouiRouiRouiRouiRouiRouiRouiRouiRouiTIuiRr3l+SY2M4lUcMlUcMlUcMlUcMlUSMuaQps5JKo4ZK2sbR0v5Fb+F2s6Pcws5QQC1kTZn/4UuLnbwuzWMqUHyx+aUn4FKuZ8jPEjyoMj8WabvleYvjaMCB2d8tlxZy1Ya9Y6EdeK4YpD/VixZGjRbuDwnli9c+yTRQ5OrxYfI/vEq4oPtLVwvcTn/D88KPE1z0o/HHiAtYHAAAAAAAAAAAAAAAAAAAAAICreXv7H+M2Dc0qcX9UAAAAAElFTkSuQmCC")
                .HasColumnName("profileImgB64");
            entity.Property(e => e.Username)
                .HasMaxLength(12)
                .HasColumnName("username");
            entity.Property(e => e.UsernameId)
                .HasColumnType("int(11)")
                .HasColumnName("usernameId");
        });

        modelBuilder.Entity<Usersession>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.SessionId }).HasName("PRIMARY");

            entity.ToTable("usersessions");

            entity.Property(e => e.UserId)
                .HasColumnType("int(11)")
                .HasColumnName("userId");
            entity.Property(e => e.SessionId).HasColumnName("sessionId");
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("'current_timestamp()'")
                .HasColumnType("timestamp")
                .HasColumnName("creationDate");
            entity.Property(e => e.IsExpired).HasColumnName("isExpired");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

