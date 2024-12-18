using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using QLDB.Models;

namespace QLDB.Models
{
    public partial class QLDBContext : DbContext
    {
        public QLDBContext() { }

        public QLDBContext(DbContextOptions<QLDBContext> options) : base(options) { }

        public virtual DbSet<CAUTHU> CAUTHU { get; set; } = null!;
        public virtual DbSet<CHITIETDOIHINH> CHITIETDOIHINH { get; set; } = null!;
        public virtual DbSet<DOIHINHTHIDAU> DOIHINHTHIDAU { get; set; } = null!;
        public virtual DbSet<TAIKHOAN> TAIKHOAN { get; set; } = null!;
        public virtual DbSet<TINHTRANGSUCKHOE> TINHTRANGSUCKHOE { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer("Server=LAPTOP-6V6QCQ0J;Database=QLDB;User Id=sa;Password=123456;TrustServerCertificate=True;");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CAUTHU>(entity =>
            {
                entity.Property(e => e.HoTen).HasMaxLength(100);
                entity.Property(e => e.MaCauThu)
                    .HasMaxLength(7)
                    .IsUnicode(false)
                    .HasComputedColumnSql("('CT'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
                entity.Property(e => e.NgaySinh).HasColumnType("date");
                entity.Property(e => e.QuocTich).HasMaxLength(200);
                entity.Property(e => e.ViTriThiDau).HasMaxLength(50);

                entity.HasOne(d => d.IDTinhTrangSucKhoeNavigation).WithMany(p => p.CAUTHU)
                    .HasForeignKey(d => d.IDTinhTrangSucKhoe)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CAUTHU_IDTinhTrangSucKhoe");
            });

            modelBuilder.Entity<CHITIETDOIHINH>(entity =>
            {
                entity.HasKey(e => new { e.IDDoiHinh, e.IDCauThu })
                    .HasName("PK_ChiTietDoiHinh");

                entity.HasOne(d => d.IDCauThuNavigation).WithMany(p => p.CHITIETDOIHINH)
                    .HasForeignKey(d => d.IDCauThu)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CHITIETDOIHINH_IDCauThu");

                entity.HasOne(d => d.IDDoiHinhNavigation).WithMany(p => p.CHITIETDOIHINH)
                    .HasForeignKey(d => d.IDDoiHinh)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CHITIETDOIHINH_IDDoiHinh");
            });

            modelBuilder.Entity<DOIHINHTHIDAU>(entity =>
            {
                entity.Property(e => e.ChienThuatThiDau).HasMaxLength(100);
                entity.Property(e => e.MaDoiHinh)
                    .HasMaxLength(7)
                    .IsUnicode(false)
                    .HasComputedColumnSql("('DH'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
                entity.Property(e => e.SoDoThiDau).HasMaxLength(50);
                entity.Property(e => e.TenDoiHinh).HasMaxLength(50);
            });

            modelBuilder.Entity<TAIKHOAN>(entity =>
            {
                entity.HasIndex(e => e.Email, "UQ__TAIKHOAN__A9D105344F420D87")
                    .IsUnique();

                entity.HasIndex(e => e.TenTaiKhoan, "UQ__TAIKHOAN__B106EAF8B48B3551")
                    .IsUnique();

                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Hoten).HasMaxLength(100);
                entity.Property(e => e.MaTaiKhoan)
                    .HasMaxLength(7)
                    .IsUnicode(false)
                    .HasComputedColumnSql("('TK'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
                entity.Property(e => e.MatKhau).HasMaxLength(50);
                entity.Property(e => e.TenTaiKhoan).HasMaxLength(100);
            });

            modelBuilder.Entity<TINHTRANGSUCKHOE>(entity =>
            {
                entity.Property(e => e.MaTinhTrang)
                    .HasMaxLength(8)
                    .IsUnicode(false)
                    .HasComputedColumnSql("('TTSK'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
                entity.Property(e => e.TenTinhTrang).HasMaxLength(100);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}