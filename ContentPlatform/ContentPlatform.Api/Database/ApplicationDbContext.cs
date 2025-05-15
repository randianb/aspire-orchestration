using ContentPlatform.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContentPlatform.Api.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("content");
        // 配置 TagEntity 的 Value/LastValue 关系（JSON 序列化方式）
        modelBuilder.Entity<TagEntity>()
            .Property(e => e.ValueJson)
            .HasColumnName("ValueJson")
            .HasColumnType("jsonb"); // PostgreSQL 专用
    
        modelBuilder.Entity<TagEntity>()
            .Property(e => e.LastValueJson)
            .HasColumnName("LastValueJson")
            .HasColumnType("jsonb"); // PostgreSQL 专用
    
        // 明确忽略 Value 和 LastValue 的导航属性（因为它们已通过 JSON 处理）
        modelBuilder.Entity<TagEntity>()
            .Ignore(t => t.Value)
            .Ignore(t => t.LastValue);
        // 对 ChannelTagEntity 做相同配置
        modelBuilder.Entity<ChannelTagEntity>()
            .Property(e => e.ValueJson)
            .HasColumnName("ValueJson")
            .HasColumnType("jsonb");
    
        modelBuilder.Entity<ChannelTagEntity>()
            .Property(e => e.LastValueJson)
            .HasColumnName("LastValueJson")
            .HasColumnType("jsonb");
    
        modelBuilder.Entity<ChannelTagEntity>()
            .Ignore(t => t.Value)
            .Ignore(t => t.LastValue);
        // 其他实体配置...
        modelBuilder.Entity<DriverEntity>().HasIndex(d => d.DriverCode).IsUnique();
        modelBuilder.Entity<EquipEntity>().HasIndex(e => e.EquipCode).IsUnique();
        modelBuilder.Entity<GroupEntity>().HasIndex(g => new { g.EquipCode, g.GroupCode }).IsUnique();
        modelBuilder.Entity<TagEntity>().HasIndex(t => new { t.EquipCode, t.GroupCode, t.TagCode }).IsUnique();
        modelBuilder.Entity<ChannelEntity>().HasIndex(c => c.ChannelCode).IsUnique();
        modelBuilder.Entity<MachineEntity>().HasIndex(m => m.MachineCode).IsUnique();
        modelBuilder.Entity<ChannelTagEntity>().HasIndex(t => new {  t.TagCode, t.ChannelCode }).IsUnique();
        modelBuilder.Entity<SenderEntity>().HasIndex(s => s.SenderCode).IsUnique();
    }

    public DbSet<Article> Articles { get; set; }
    public DbSet<DriverEntity> Drivers{ get; set; }
    public DbSet<ChannelEntity> Channels{ get; set; }
    public DbSet<TagEntity> Tags{ get; set; }
}
