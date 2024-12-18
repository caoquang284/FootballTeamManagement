CREATE DATABASE QLDB
GO

USE QLDB
GO

-- Bảng Tài Khoản để đăng nhập 
CREATE TABLE [TAIKHOAN] (
    [ID] INT IDENTITY(1,1) CONSTRAINT [PK_TAIKHOAN] PRIMARY KEY,
    [MaTaiKhoan] AS ('TK' + RIGHT('00000' + CAST([ID] AS VARCHAR(5)), 5)) PERSISTED,
    [TenTaiKhoan] NVARCHAR(100) CONSTRAINT [UQ_TenTaiKhoan] UNIQUE NOT NULL,
    [MatKhau] VARCHAR(50) NOT NULL,
    [Hoten] NVARCHAR(100) NOT NULL,
    [Email] VARCHAR(100) CONSTRAINT [UQ_Email] UNIQUE NOT NULL
)

-- Bảng Cầu Thủ 
CREATE TABLE [CAUTHU] (
    [ID] INT IDENTITY(1,1) CONSTRAINT [PK_CAUTHU] PRIMARY KEY,
    [MaCauThu] AS ('CT' + RIGHT('00000' + CAST([ID] AS VARCHAR(5)), 5)) PERSISTED,
    [HoTen] NVARCHAR(100) NOT NULL,
    [NgaySinh] DATE NOT NULL,
    [QuocTich] NVARCHAR(200) NOT NULL,
    [ViTriThiDau] NVARCHAR(50) NOT NULL, -- Tiền đạo, Tiền vệ, Hậu vệ, Thủ môn 
    [SoAo] INT NOT NULL,
    CONSTRAINT [CK_SoAo] CHECK ([SoAo] > 0), -- Ràng buộc SoAo
    [IDTinhTrangSucKhoe] INT NOT NULL, -- FK 
    [AnhCauThu] NVARCHAR(200) NOT NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0
)

ALTER TABLE [CAUTHU] 
ALTER COLUMN [AnhCauThu] NVARCHAR(200) NULL

-- Bảng Tình trạng sức khỏe 
CREATE TABLE [TINHTRANGSUCKHOE] (
    [ID] INT IDENTITY(1,1) CONSTRAINT [PK_TinhTrangSucKhoe] PRIMARY KEY,
    [MaTinhTrang] AS ('TTSK' + RIGHT('00000' + CAST([ID] AS VARCHAR(5)), 5)) PERSISTED,
    [TenTinhTrang] NVARCHAR(100) NOT NULL, -- Tốt, Chấn thương, ... 
    [KhaNangRaSan] INT NULL, -- Sử dụng INT để lưu giá trị phần trăm (0-100)
    CONSTRAINT [CK_KhaNangRaSan] CHECK ([KhaNangRaSan] >= 0 AND [KhaNangRaSan] <= 100), -- Ràng buộc KhaNangRaSan 
    [IsDeleted] BIT NOT NULL DEFAULT 0
)

-- Bảng Đội hình (A, B, C) 
CREATE TABLE [DOIHINHTHIDAU] (
    [ID] INT IDENTITY(1,1) CONSTRAINT [PK_DoiHinh] PRIMARY KEY,
    [MaDoiHinh] AS ('DH' + RIGHT('00000' + CAST([ID] AS VARCHAR(5)), 5)) PERSISTED,
    [TenDoiHinh] NVARCHAR(50) NOT NULL, -- Đội hình A, Đội hình B, ... 
    [SoDoThiDau] VARCHAR(50) NULL, -- Ví dụ: 4-3-3, 4-4-2 
    [ChienThuatThiDau] NVARCHAR(100) NULL, -- Ví dụ: Tấn công, Phòng ngự 
    [IsDeleted] BIT NOT NULL DEFAULT 0
)

-- Bảng Chi tiết đội hình (liên kết giữa Cầu thủ và Đội hình)
CREATE TABLE [CHITIETDOIHINH] (
    [IDDoiHinh] INT NOT NULL, -- FK
    [IDCauThu] INT NOT NULL, -- FK
    CONSTRAINT [PK_ChiTietDoiHinh] PRIMARY KEY ([IDDoiHinh], [IDCauThu]),
    [IsDeleted] BIT NOT NULL DEFAULT 0
)


-- Khóa ngoại 
ALTER TABLE [CAUTHU] ADD CONSTRAINT [FK_CAUTHU_IDTinhTrangSucKhoe] FOREIGN KEY ([IDTinhTrangSucKhoe]) REFERENCES [TINHTRANGSUCKHOE]([ID])

ALTER TABLE [CHITIETDOIHINH] ADD CONSTRAINT [FK_CHITIETDOIHINH_IDDoiHinh] FOREIGN KEY ([IDDoiHinh]) REFERENCES [DOIHINHTHIDAU]([ID])

ALTER TABLE [CHITIETDOIHINH] ADD CONSTRAINT [FK_CHITIETDOIHINH_IDCauThu] FOREIGN KEY ([IDCauThu]) REFERENCES [CAUTHU]([ID])

-- Data để chạy chương trình
-- TAIKHOAN
INSERT [TAIKHOAN](TenTaiKhoan, MatKhau, Hoten, Email)
VALUES ('hlv', '123456', N'Huấn luyện viên', 'hlv@gmail.com');