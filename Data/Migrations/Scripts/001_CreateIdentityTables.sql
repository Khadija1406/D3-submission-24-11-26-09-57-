-- 001_CreateIdentityTables.sql
CREATE TABLE IF NOT EXISTS AspNetUsers (
    Id TEXT PRIMARY KEY,
    UserName TEXT NULL,
    NormalizedUserName TEXT NULL,
    Email TEXT NULL,
    NormalizedEmail TEXT NULL,
    EmailConfirmed INTEGER NOT NULL DEFAULT 0,
    PasswordHash TEXT NULL,
    SecurityStamp TEXT NULL,
    ConcurrencyStamp TEXT NULL,
    PhoneNumber TEXT NULL,
    PhoneNumberConfirmed INTEGER NOT NULL DEFAULT 0,
    TwoFactorEnabled INTEGER NOT NULL DEFAULT 0,
    LockoutEnd TEXT NULL,
    LockoutEnabled INTEGER NOT NULL DEFAULT 0,
    AccessFailedCount INTEGER NOT NULL DEFAULT 0,
    FullName TEXT NOT NULL DEFAULT '',
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    AvatarUrl TEXT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_AspNetUsers_NormalizedUserName ON AspNetUsers (NormalizedUserName);
CREATE INDEX IF NOT EXISTS IX_AspNetUsers_NormalizedEmail ON AspNetUsers (NormalizedEmail);

CREATE TABLE IF NOT EXISTS AspNetRoles (
    Id TEXT PRIMARY KEY,
    Name TEXT NULL,
    NormalizedName TEXT NULL,
    ConcurrencyStamp TEXT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_AspNetRoles_NormalizedName ON AspNetRoles (NormalizedName);

CREATE TABLE IF NOT EXISTS AspNetUserRoles (
    UserId TEXT NOT NULL,
    RoleId TEXT NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE
);
