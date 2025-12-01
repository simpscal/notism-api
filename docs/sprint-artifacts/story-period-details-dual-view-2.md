# Story 2.2: Dual View Layout & Timeline Component

**Status:** Draft

---

## User Story

As a user,
I want to see events displayed chronologically on a timeline in a dual view layout,
So that I can understand the sequence of events within a period.

---

## Acceptance Criteria

**AC #1:** Given a selected period, when I view period details, then a dual view layout displays (timeline left, map right).

**AC #2:** Given events in a period, when I view the timeline, then events are displayed in linear chronological order.

**AC #3:** Given the dual view, when I view it on smaller screens, then the layout stacks vertically (timeline above map).

**AC #4:** Given the timeline, when it renders, then all event markers are clearly visible and interactive.

**AC #5:** Given navigation, when I'm in period details, then I can navigate back to Level 1 (Timeline Overview).

---

## Implementation Details

### Tasks / Subtasks

- [ ] Create dual view layout component (timeline left, map right) (AC: #1)
- [ ] Implement timeline component for event display (AC: #2, #4)
- [ ] Add chronological event ordering and rendering (AC: #2)
- [ ] Implement responsive layout (stacks on mobile/tablet) (AC: #3)
- [ ] Add navigation back to Level 1 (AC: #5)
- [ ] Use design system components for consistency (AC: #1, #4)
- [ ] Write component tests for dual view layout
- [ ] Write responsive design tests

### Technical Summary

This story creates the dual view layout that displays events chronologically on a timeline (left side) alongside a map (right side). The layout must be responsive, stacking vertically on smaller screens. This establishes the core structure for Level 2 period details.

### Project Structure Notes

- **Files to modify:** 
  - Dual view layout component
  - Timeline component for events
  - Responsive layout utilities
  - Navigation components
- **Expected test locations:** 
  - Component unit tests
  - Layout tests
  - Responsive design tests
- **Estimated effort:** 5 story points
- **Prerequisites:** Story 2.1 (Event Data Model & API)

### Key Code References

- PRD Section: Level 2: Period Details (FR8, FR9, FR21, FR39)
- UX Design: Dual view layout requirements
- Story 2.1: Event data API

---

## Context References

**PRD:** [prd.md](../prd.md) - Product Requirements Document containing:
- Dual view layout requirement (FR8)
- Timeline event display (FR9)
- Responsive layout requirement (FR39)
- Navigation requirements (FR21)

**UX Design:** [ux-design-specification.md](../ux-design-specification.md) - UX Design Specification containing:
- Dual view layout design
- Responsive design guidelines

**Epic:** [epics.md](../epics.md) - Epic 2: Period Details & Dual View

---

## Dev Agent Record

### Agent Model Used

<!-- Will be populated during dev-story execution -->

### Debug Log References

<!-- Will be populated during dev-story execution -->

### Completion Notes

<!-- Will be populated during dev-story execution -->

### Files Modified

<!-- Will be populated during dev-story execution -->

### Test Results

<!-- Will be populated during dev-story execution -->

---

## Review Notes

<!-- Will be populated during code review -->
