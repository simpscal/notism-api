-- Migration: 20260608093555_AddFoodNameDescriptionTrigramIndexes (DOWN)
-- Purpose: Reverse the trigram-index migration by dropping the two GIN trigram
--          indexes on "Foods". The pg_trgm extension is intentionally left in place
--          (dropping a shared extension on rollback is unsafe).
--
-- HOW TO RUN: execute by hand against the target database, e.g.
--   psql "<connection string>" -f AddFoodNameDescriptionTrigramIndexes.down.sql
--
-- This script is IDEMPOTENT and safe to re-run:
--   * DROP INDEX uses IF EXISTS.
--   * The __EFMigrationsHistory row is removed if present.
--
-- IMPORTANT: DROP INDEX CONCURRENTLY must run at the top level (NOT inside a
-- transaction, function, or DO $$ block). Do NOT wrap this script in BEGIN/COMMIT.

DROP INDEX CONCURRENTLY IF EXISTS "IX_Foods_Description";

DROP INDEX CONCURRENTLY IF EXISTS "IX_Foods_Name";

DELETE FROM "__EFMigrationsHistory"
WHERE "MigrationId" = '20260608093555_AddFoodNameDescriptionTrigramIndexes';
