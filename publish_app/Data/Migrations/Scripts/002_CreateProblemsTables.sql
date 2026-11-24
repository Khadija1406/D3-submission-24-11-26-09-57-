-- 002_CreateProblemsTables.sql
CREATE TABLE IF NOT EXISTS Problems (
    Id TEXT PRIMARY KEY,
    UserId TEXT NOT NULL,
    Title TEXT NOT NULL,
    Description TEXT NOT NULL,
    Category TEXT NOT NULL DEFAULT 'technical',
    Status TEXT NOT NULL DEFAULT 'active',
    Impact TEXT NOT NULL DEFAULT 'Medium',
    CreatedAt TEXT NOT NULL DEFAULT (date('now')),
    UpdatedAt TEXT NOT NULL DEFAULT (date('now')),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_Problems_UserId ON Problems (UserId);
CREATE INDEX IF NOT EXISTS IX_Problems_Category ON Problems (Category);
CREATE INDEX IF NOT EXISTS IX_Problems_Status ON Problems (Status);

CREATE TABLE IF NOT EXISTS ProblemOccurrences (
    Id TEXT PRIMARY KEY,
    ProblemId TEXT NOT NULL,
    OccurrenceDate TEXT NOT NULL,
    Note TEXT NOT NULL DEFAULT '',
    FOREIGN KEY (ProblemId) REFERENCES Problems (Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_ProblemOccurrences_ProblemId ON ProblemOccurrences (ProblemId);

CREATE TABLE IF NOT EXISTS ProblemTriedSolutions (
    Id TEXT PRIMARY KEY,
    ProblemId TEXT NOT NULL,
    SolutionKey TEXT NOT NULL,
    Status TEXT NOT NULL DEFAULT 'partial',
    Note TEXT NOT NULL DEFAULT '',
    FOREIGN KEY (ProblemId) REFERENCES Problems (Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_ProblemTriedSolutions_ProblemId ON ProblemTriedSolutions (ProblemId);
