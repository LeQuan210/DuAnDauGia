using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDauGiaDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebDauGiaInfrasData
{
    public class AuctionDbContext : DbContext
    {
        public AuctionDbContext(DbContextOptions<AuctionDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<AuctionSession> AuctionSessions { get; set; }
        public DbSet<FavoriteProduct> FavoriteProducts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình tên bảng cho rõ ràng
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<AuctionSession>().ToTable("AuctionSessions");
            modelBuilder.Entity<User>().Property(u => u.WalletBalance).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<AuctionSession>().Property(a => a.StartingPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<AuctionSession>().Property(a => a.StepPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<AuctionSession>().Property(a => a.CurrentHighestPrice).HasColumnType("decimal(18,2)");
        }
    }
}

    

