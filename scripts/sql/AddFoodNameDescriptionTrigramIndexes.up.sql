-- Migration: 20260608093555_AddFoodNameDescriptionTrigramIndexes (UP)
-- Purpose: Enable pg_trgm and add GIN trigram indexes on LOWER("Name") / LOWER("Description")
--          of the "Foods" table so the case-insensitive substring food search
--          (LOWER(col) LIKE '%kw%') is index-served instead of sequential-scanned.
--
-- HOW TO RUN: execute by hand against the target database, e.g.
--   psql "<connection string>" -f AddFoodNameDescriptionTrigramIndexes.up.sql
--
-- This script is IDEMPOTENT and safe to re-run:
--   * CREATE EXTENSION / CREATE INDEX use IF NOT EXISTS.
--   * The __EFMigrationsHistory row is only inserted if absent.
--
-- IMPORTANT: The index builds use CREATE INDEX CONCURRENTLY to avoid taking a write
-- lock on the populated "Foods" table. CONCURRENTLY:
--   * MUST run at the top level (NOT inside a transaction, function, or DO $$ block).
--   * Each statement below therefore runs autonomously.
-- Do NOT wrap this script in BEGIN/COMMIT. This is also why the EF-generated
-- --idempotent script could not be used (it wraps statements in DO $$ blocks,
-- which Postgres rejects for CONCURRENTLY with: "cannot be executed from a function").

-- 1. Extension (idempotent, transactional-safe).
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- 2. Trigram indexes on the LOWER() expression matched by the search query.
CREATE INDEX CONCURRENTLY IF NOT EXISTS "IX_Foods_Name"
    ON "Foods" USING gin (LOWER("Name") gin_trgm_ops);

CREATE INDEX CONCURRENTLY IF NOT EXISTS "IX_Foods_Description"
    ON "Foods" USING gin (LOWER("Description") gin_trgm_ops);

-- 3. Record the migration as applied so EF Core does not re-run it.
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20260608093555_AddFoodNameDescriptionTrigramIndexes', '8.0.0'
WHERE NOT EXISTS (
    SELECT 1 FROM "__EFMigrationsHistory"
    WHERE "MigrationId" = '20260608093555_AddFoodNameDescriptionTrigramIndexes'
);
