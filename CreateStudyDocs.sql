-- Create DB
IF DB_ID('StudyDocs') IS NULL
CREATE DATABASE StudyDocs;
GO
USE StudyDocs;
GO


-- Subject table
IF OBJECT_ID('dbo.Subject', 'U') IS NULL
BEGIN
CREATE TABLE dbo.[Subject](
SubjectId INT IDENTITY PRIMARY KEY,
[Name] NVARCHAR(100) NOT NULL UNIQUE
);
END
GO


-- Document table
IF OBJECT_ID('dbo.Document', 'U') IS NULL
BEGIN
CREATE TABLE dbo.[Document](
DocumentId INT IDENTITY PRIMARY KEY,
Title NVARCHAR(200) NOT NULL,
SubjectId INT NULL REFERENCES dbo.[Subject](SubjectId),
[Type] NVARCHAR(20) NOT NULL, -- pdf/docx/ppt/link/text
FilePath NVARCHAR(400) NULL,
Notes NVARCHAR(500) NULL,
[Status] BIT NOT NULL DEFAULT 0, -- 0: Chưa học, 1: Đã học
LastOpened DATETIME NULL,
CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
END
GO


-- Seed some subjects
IF NOT EXISTS (SELECT 1 FROM dbo.[Subject])
BEGIN
INSERT dbo.[Subject]([Name]) VALUES (N'Toán'),(N'Lý'),(N'Hóa'),(N'Anh'),(N'Lập trình');
END
GO