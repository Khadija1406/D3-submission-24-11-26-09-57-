-- ====================================================================
-- Pulse SQLite Seed Data Script
-- Tables: AspNetUsers, Problems, ProblemOccurrences, ProblemTriedSolutions
-- ====================================================================

-- 1. Insert Customized Identity User: Alex Rivera (alex@pulse.app / Password123!)
INSERT OR IGNORE INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
    PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed,
    TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FullName, CreatedAt
) VALUES (
    'user-alex-rivera-001',
    'alex@pulse.app',
    'ALEX@PULSE.APP',
    'alex@pulse.app',
    'ALEX@PULSE.APP',
    1,
    'AQAAAAIAAYagAAAAEG3c2Z9p7U1O6Q1qZc0x7L5...',
    'sec-stamp-alex-001',
    'conc-stamp-alex-001',
    0, 0, 0, 0,
    'Alex Rivera',
    datetime('now')
);

-- 2. Insert The 5 Core Problems
INSERT OR REPLACE INTO Problems (Id, UserId, Title, Description, Category, Status, Impact, CreatedAt, UpdatedAt) VALUES
('p-wifi', 'user-alex-rivera-001', 'Wi-Fi keeps dropping in the evening', 'The connection disconnects around 8–10pm, mostly during video calls. Restarting the router helps for a while.', 'technical', 'active', 'High', date('now', '-20 days'), date('now')),
('p-academic', 'user-alex-rivera-001', 'Missing assignment deadlines', 'I keep losing track of due dates when several courses have work due in the same week.', 'academic', 'progress', 'Medium', date('now', '-15 days'), date('now', '-3 days')),
('p-household', 'user-alex-rivera-001', 'Kitchen tap is leaking', 'Slow drip from the kitchen tap, gets worse at night.', 'household', 'resolved', 'Low', date('now', '-25 days'), date('now', '-18 days')),
('p-lifestyle', 'user-alex-rivera-001', 'Sleeping too late on weekdays', 'Going to bed after 2am, which makes mornings hard and lowers my focus.', 'lifestyle', 'active', 'Medium', date('now', '-10 days'), date('now', '-1 days')),
('p-managerial', 'user-alex-rivera-001', 'Team tasks overlapping', 'Two people end up working on the same task because nobody updates the board.', 'managerial', 'active', 'Medium', date('now', '-4 days'), date('now', '-4 days'));

-- 3. Insert Occurrences History
INSERT OR REPLACE INTO ProblemOccurrences (Id, ProblemId, OccurrenceDate, Note) VALUES
('occ-w1', 'p-wifi', date('now', '-20 days'), 'First noticed during a class call.'),
('occ-w2', 'p-wifi', date('now', '-12 days'), 'Dropped twice in one hour.'),
('occ-w3', 'p-wifi', date('now', '-6 days'), 'Restarted router, back after 5 min.'),
('occ-w4', 'p-wifi', date('now', '-2 days'), 'Dropped during a meeting again.'),
('occ-w5', 'p-wifi', date('now'), 'Slow and disconnecting.'),
('occ-a1', 'p-academic', date('now', '-15 days'), 'Late on the database report.'),
('occ-a2', 'p-academic', date('now', '-8 days'), 'Nearly missed the quiz.'),
('occ-a3', 'p-academic', date('now', '-3 days'), 'Submitted lab 2 hours late.'),
('occ-h1', 'p-household', date('now', '-25 days'), 'Noticed the drip.'),
('occ-h2', 'p-household', date('now', '-18 days'), 'Replaced washer — fixed.'),
('occ-l1', 'p-lifestyle', date('now', '-10 days'), ''),
('occ-l2', 'p-lifestyle', date('now', '-5 days'), 'Phone scrolling until late.'),
('occ-l3', 'p-lifestyle', date('now', '-1 days'), ''),
('occ-m1', 'p-managerial', date('now', '-4 days'), 'Duplicate slides made.');
