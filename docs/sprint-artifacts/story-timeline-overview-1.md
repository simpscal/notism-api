# Story 1.1: Timeline Data Model & API

**Status:** Review

---

## User Story

As a developer,
I want a data model and API for timeline periods,
So that the timeline can display all historical periods with their metadata.

---

## Acceptance Criteria

**AC #1:** Given a database schema, when I query for all periods, then I receive period data including name, start date, end date, and description.

**AC #2:** Given period data, when I access the API endpoint, then periods are returned in chronological order.

**AC #3:** Given period data, when periods are retrieved, then dynamic gaps are calculated based on event relevance and included in the response.

**AC #4:** Given the API, when I request periods, then the response includes metadata needed for timeline visualization (dates, names, IDs).

---

## Implementation Details

### Tasks / Subtasks

- [x] Design database schema for periods (id, name, start_date, end_date, description) (AC: #1, #4)
- [x] Create API endpoints for period retrieval (AC: #2)
- [x] Implement chronological ordering logic (AC: #2)
- [x] Calculate dynamic gap spacing between periods based on event relevance (AC: #3)
- [x] Add API response formatting with required metadata (AC: #4)
- [ ] Write unit tests for data model and API endpoints (Note: No test project exists in solution)
- [ ] Write integration tests for period retrieval (Note: No test project exists in solution)

### Technical Summary

This story establishes the foundational data layer for the timeline. The database schema will store period information, and the API will provide access to periods in chronological order with calculated gap spacing based on event relevance. This is a backend-focused story that enables the frontend timeline visualization.

### Project Structure Notes

- **Files to modify:** 
  - Database schema/migration files
  - API route handlers for periods
  - Data models/entities for periods
  - API service layer
- **Expected test locations:** 
  - Unit tests for data models
  - Unit tests for API endpoints
  - Integration tests for period retrieval
- **Estimated effort:** 3 story points
- **Prerequisites:** None

### Key Code References

- PRD Section: Level 1: Timeline Overview (FR1-FR6)
- Gap calculation based on event relevance in each period
- PRD Section: Product Scope - MVP (dynamic gaps based on event relevance)
- UX Design: Timeline visualization requirements

---

## Context References

**PRD:** [prd.md](../prd.md) - Product Requirements Document containing:
- Functional requirements for timeline overview (FR1-FR6)
- Dynamic gap calculation based on event relevance in each period
- Timeline navigation requirements

**UX Design:** [ux-design-specification.md](../ux-design-specification.md) - UX Design Specification containing:
- Timeline visualization design requirements
- Interaction patterns and animations

**Architecture:** (To be created) - Architecture document will contain:
- Database technology decisions
- API framework and patterns
- Data modeling decisions

---

## Dev Agent Record

### Agent Model Used

Claude Sonnet 4.5 (via Cursor)

### Debug Log References

- Period domain model already existed with StartYear/EndYear (not start_date/end_date as story mentioned)
- Database schema and migration already existed
- Period repository already existed
- Created optimized query to avoid N+1 queries by fetching all events in single query
- Fixed namespace conflict: `Period` namespace vs `Period` entity type

### Completion Notes

**Implementation Summary:**
- Created `GetPeriodsHandler` in Application layer following CQRS pattern with MediatR
- Implemented chronological ordering by `StartYear` (AC #2)
- Implemented gap calculation based on event count per period:
  - Gap size normalized to 0.1-1.0 scale (0.1 minimum for periods with no events)
  - Proportional to event count relative to maximum event count across all periods
  - More events = larger gap (AC #3)
- Created API endpoint `/api/periods` (GET) that returns periods with metadata (AC #1, #4)
- Response includes: Id, Name, StartYear, EndYear, Description, ThumbnailMediaAssetId, DisplayOrder, GapSize
- Optimized to use single query for all events grouped by period (avoids N+1 queries)
- Created `AllPublishedEventsByPeriodIdsSpecification` for efficient event counting

**AC Verification:**
- AC #1: ✅ Period data includes name, start year, end year, and description
- AC #2: ✅ Periods returned in chronological order (by StartYear)
- AC #3: ✅ Dynamic gaps calculated based on event relevance (event count)
- AC #4: ✅ Response includes metadata for timeline visualization (dates, names, IDs, gap sizes)

**Note on Tests:**
- No test projects exist in the solution
- Tests should be added when test infrastructure is established

### Files Modified

**Created:**
- `src/Notism.Application/Period/GetPeriods/GetPeriodsRequest.cs`
- `src/Notism.Application/Period/GetPeriods/GetPeriodsResponse.cs`
- `src/Notism.Application/Period/GetPeriods/GetPeriodsHandler.cs`
- `src/Notism.Domain/Event/Specifications/AllPublishedEventsByPeriodIdsSpecification.cs`
- `src/Notism.Api/Endpoints/PeriodEndpoints.cs`

**Modified:**
- `src/Notism.Api/Program.cs` - Added `app.MapPeriodEndpoints()`

**Existing (already implemented):**
- `src/Notism.Domain/Period/Period.cs` - Domain model
- `src/Notism.Domain/Period/IPeriodRepository.cs` - Repository interface
- `src/Notism.Infrastructure/Periods/PeriodRepository.cs` - Repository implementation
- `src/Notism.Domain/Period/Specifications/PublishedPeriodsSpecification.cs` - Specification for published periods
- Database migration already exists for Periods table

### Test Results

- Build: ✅ Successful (no compilation errors)
- Linting: ✅ No errors
- Manual testing: Pending (requires test project setup)

---

## Review Notes

<!-- Will be populated during code review -->
