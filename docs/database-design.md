# Notism Database Design

**Author:** Winston (Architect)  
**Date:** 2025-01-27  
**Version:** 1.1

---

## Executive Summary

This document defines the complete database schema for the Notism application, a visualization-first learning tool for human history. The design supports a three-level navigation structure (Timeline Overview → Period Details → Event Details) and a blog feature for narrative historical content, with comprehensive content management capabilities for administrators.

The database is designed using PostgreSQL with Entity Framework Core, following Domain-Driven Design principles and Clean Architecture patterns.

---

## Design Principles

1. **Aggregate Boundaries**: Each aggregate root (Period, Event, Blog) maintains its own consistency boundary
2. **Soft Deletes**: All entities support soft deletion for audit and recovery
3. **Audit Trail**: Comprehensive tracking of creation, updates, and deletions
4. **Content Versioning**: Support for content history and rollback capabilities
5. **Geographic Data**: Native support for geographic coordinates using PostgreSQL's geography types
6. **Media Management**: Flexible media asset management with metadata
7. **Performance**: Strategic indexing for common query patterns

---

## Entity Relationship Diagram

```
┌─────────────┐
│    Users    │ (Existing)
└─────────────┘
       │
       │ 1:N
       ▼
┌─────────────┐
│  RefreshTokens │ (Existing)
└─────────────┘

┌─────────────┐
│   Periods   │
└─────────────┘
       │
       │ 1:N
       ▼
┌─────────────┐
│   Events    │
└─────────────┘
       │
       │ N:M         N:M
       ▼             ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│ EventMedia  │────▶│MediaAssets  │◀────│  BlogMedia  │
└─────────────┘     └─────────────┘     └─────────────┘
       │                                    │
       │                                    │ N:M
       │                                    ▼
       │                            ┌─────────────┐
       │                            │    Blogs    │
       │                            └─────────────┘
       │                                    │
       │                                    │ N:M
       │                                    ▼
       │                            ┌─────────────┐
       │                            │BlogEventMentions│
       │                            └─────────────┘
       │                                    │
       │                                    │ N:1
       │                                    ▼
       │                            ┌─────────────┐
       │                            │   Events    │
       │                            └─────────────┘
       │
       │ 1:N
       ▼
┌─────────────┐
│ContentVersions│
└─────────────┘

┌─────────────┐
│  AuditLogs  │
└─────────────┘
```

---

## Core Tables

### 1. Periods

Represents historical periods on the timeline (e.g., "Ancient Apes", "Stone Age", "Bronze Age").

**Table Name:** `Periods`

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| `Id` | `uuid` | PRIMARY KEY, NOT NULL | Unique identifier |
| `Name` | `varchar(200)` | NOT NULL | Period name (e.g., "Stone Age") |
| `StartYear` | `integer` | NOT NULL | Start year (BCE/CE) |
| `EndYear` | `integer` | NOT NULL | End year (BCE/CE) |
| `Description` | `text` | NULL | Rich text description (markdown/HTML) |
| `ThumbnailMediaAssetId` | `uuid` | NULL | Thumbnail/avatar image (FK to MediaAssets) |
| `DisplayOrder` | `integer` | NOT NULL, DEFAULT 0 | Order for timeline display |
| `IsPublished` | `boolean` | NOT NULL, DEFAULT false | Publication status |
| `CreatedAt` | `timestamptz` | NOT NULL | Creation timestamp |
| `UpdatedAt` | `timestamptz` | NOT NULL | Last update timestamp |
| `CreatedBy` | `uuid` | NULL | User ID who created (FK to Users) |
| `UpdatedBy` | `uuid` | NULL | User ID who last updated (FK to Users) |
| `IsDeleted` | `boolean` | NOT NULL, DEFAULT false | Soft delete flag |
| `DeletedAt` | `timestamptz` | NULL | Deletion timestamp |

**Indexes:**
- Composite: `IX_Periods_IsPublished_IsDeleted_DisplayOrder` on `(IsPublished, IsDeleted, DisplayOrder)` - For timeline overview queries

**Business Rules:**
- `StartYear` must be less than or equal to `EndYear`
- `Name` must be unique among non-deleted periods
- `DisplayOrder` should be unique among non-deleted periods (enforced at application level)
- `ThumbnailMediaAssetId` references an image media asset (should be of type 'image')

**Foreign Keys:**
- `FK_Periods_MediaAssets_Thumbnail` → `MediaAssets.Id` (SET NULL on delete)

---

### 2. Events

Represents historical events within periods (e.g., "Discovery of Fire", "Invention of Wheel").

**Table Name:** `Events`

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| `Id` | `uuid` | PRIMARY KEY, NOT NULL | Unique identifier |
| `PeriodId` | `uuid` | NOT NULL, FK → Periods.Id | Parent period |
| `Title` | `varchar(300)` | NOT NULL | Event title |
| `ShortDescription` | `varchar(500)` | NULL | Brief description for popups |
| `Description` | `text` | NULL | Full rich text description (markdown/HTML) |
| `EventDate` | `date` | NULL | Specific date of event (null for BCE events or when only year is known) |
| `EventYear` | `integer` | NOT NULL | Year for chronological ordering (negative for BCE, positive for CE) |
| `EventMonth` | `integer` | NULL | Month (1-12) if known |
| `EventDay` | `integer` | NULL | Day (1-31) if known |
| `IsApproximateDate` | `boolean` | NOT NULL, DEFAULT true | Whether date is approximate |
| `Latitude` | `decimal(10,8)` | NULL | Geographic latitude (-90 to 90) |
| `Longitude` | `decimal(11,8)` | NULL | Geographic longitude (-180 to 180) |
| `LocationName` | `varchar(200)` | NULL | Human-readable location name |
| `ThumbnailMediaAssetId` | `uuid` | NULL | Thumbnail/avatar image (FK to MediaAssets) |
| `DisplayOrder` | `integer` | NOT NULL, DEFAULT 0 | Order within period |
| `IsPublished` | `boolean` | NOT NULL, DEFAULT false | Publication status |
| `CreatedAt` | `timestamptz` | NOT NULL | Creation timestamp |
| `UpdatedAt` | `timestamptz` | NOT NULL | Last update timestamp |
| `CreatedBy` | `uuid` | NULL | User ID who created (FK to Users) |
| `UpdatedBy` | `uuid` | NULL | User ID who last updated (FK to Users) |
| `IsDeleted` | `boolean` | NOT NULL, DEFAULT false | Soft delete flag |
| `DeletedAt` | `timestamptz` | NULL | Deletion timestamp |

**Indexes:**
- `IX_Events_PeriodId` on `PeriodId` (FK index - crucial for period event queries)
- Composite: `IX_Events_EventYear_DisplayOrder` on `(EventYear, DisplayOrder)` - For chronological event listing (used after PeriodId filter)
- Composite: `IX_Events_PeriodId_IsPublished_IsDeleted` on `(PeriodId, IsPublished, IsDeleted)` - For published event queries
- Geographic: `IX_Events_Location` on `(Latitude, Longitude)` using GIST index (PostgreSQL) - For map visualization queries

**Business Rules:**
- `EventYear` must be within the period's `StartYear` and `EndYear` range
- `EventYear` can be negative for BCE dates (e.g., -500 for 500 BCE) or positive for CE dates
- `EventDate` is optional - null for BCE events or when only year is known
- If `EventDate` is provided, it should be consistent with `EventYear`, `EventMonth`, and `EventDay`
- For BCE events, `EventDate` is typically null since exact dates are rarely known
- If `Latitude` is provided, `Longitude` must also be provided (and vice versa)
- `EventMonth` must be between 1-12 if provided
- `EventDay` must be valid for the given month if provided
- Events with the same `EventYear` (and optionally `EventDate` if not null) are considered simultaneous (for clustering)
- `ThumbnailMediaAssetId` references an image media asset (should be of type 'image')

**Foreign Keys:**
- `FK_Events_Periods` → `Periods.Id` (CASCADE on delete)
- `FK_Events_MediaAssets_Thumbnail` → `MediaAssets.Id` (SET NULL on delete)

---

### 3. Blogs

Represents historical blog posts that provide narrative context and deeper exploration of historical topics.

**Table Name:** `Blogs`

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| `Id` | `uuid` | PRIMARY KEY, NOT NULL | Unique identifier |
| `Title` | `varchar(300)` | NOT NULL | Blog post title |
| `Slug` | `varchar(350)` | NOT NULL | URL-friendly slug (unique) |
| `ShortDescription` | `varchar(500)` | NULL | Brief description for list views |
| `Content` | `text` | NOT NULL | Full blog content (markdown/HTML) |
| `AuthorId` | `uuid` | NULL | Author user ID (FK to Users) |
| `AuthorName` | `varchar(200)` | NULL | Author display name (for display purposes) |
| `PublishedAt` | `timestamptz` | NULL | Publication date/time |
| `ThumbnailMediaAssetId` | `uuid` | NULL | Thumbnail image (FK to MediaAssets) |
| `IsPublished` | `boolean` | NOT NULL, DEFAULT false | Publication status |
| `ViewCount` | `integer` | NOT NULL, DEFAULT 0 | Number of views |
| `MetaTitle` | `varchar(200)` | NULL | SEO meta title |
| `MetaDescription` | `varchar(500)` | NULL | SEO meta description |
| `Tags` | `varchar[]` | NULL | Array of tags for categorization |
| `CreatedAt` | `timestamptz` | NOT NULL | Creation timestamp |
| `UpdatedAt` | `timestamptz` | NOT NULL | Last update timestamp |
| `CreatedBy` | `uuid` | NULL | User ID who created (FK to Users) |
| `UpdatedBy` | `uuid` | NULL | User ID who last updated (FK to Users) |
| `IsDeleted` | `boolean` | NOT NULL, DEFAULT false | Soft delete flag |
| `DeletedAt` | `timestamptz` | NULL | Deletion timestamp |

**Indexes:**
- `IX_Blogs_Slug` on `Slug` (unique index - crucial for URL lookups)
- `IX_Blogs_AuthorId` on `AuthorId` (FK index)
- Composite: `IX_Blogs_IsPublished_IsDeleted_PublishedAt` on `(IsPublished, IsDeleted, PublishedAt DESC)` - For blog list queries
- Full-text: `IX_Blogs_Title_Content` on `(Title, Content)` using GIN index (PostgreSQL) - For blog search

**Business Rules:**
- `Slug` must be unique among non-deleted blogs
- `Slug` must be URL-friendly (lowercase, hyphens, alphanumeric)
- If `PublishedAt` is set, `IsPublished` should typically be true
- `AuthorName` is optional fallback if `AuthorId` is null or user is deleted
- `ViewCount` is incremented on each view (application-level)

**Foreign Keys:**
- `FK_Blogs_Users_Author` → `Users.Id` (SET NULL on delete)
- `FK_Blogs_Users_CreatedBy` → `Users.Id` (SET NULL on delete)
- `FK_Blogs_MediaAssets_Thumbnail` → `MediaAssets.Id` (SET NULL on delete)

---

### 4. BlogEventMentions

Junction table linking blogs to events mentioned within blog content (many-to-many relationship).

**Table Name:** `BlogEventMentions`

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| `Id` | `uuid` | PRIMARY KEY, NOT NULL | Unique identifier |
| `BlogId` | `uuid` | NOT NULL, FK → Blogs.Id | Blog reference |
| `EventId` | `uuid` | NOT NULL, FK → Events.Id | Event reference |
| `MentionOrder` | `integer` | NOT NULL, DEFAULT 0 | Order of mention in content |
| `Context` | `varchar(500)` | NULL | Surrounding text context for the mention |
| `CreatedAt` | `timestamptz` | NOT NULL | Creation timestamp |

**Indexes:**
- `IX_BlogEventMentions_BlogId` on `BlogId` (FK index - crucial for blog event mentions)
- `IX_BlogEventMentions_EventId` on `EventId` (FK index - crucial for event blog queries)
- Composite: `IX_BlogEventMentions_BlogId_MentionOrder` on `(BlogId, MentionOrder)` - For ordered mention display
- Unique: `UQ_BlogEventMentions_Blog_Event` on `(BlogId, EventId)` (prevent duplicate mentions)

**Business Rules:**
- Same event can be mentioned multiple times in a blog (if needed, use `MentionOrder` to track)
- `MentionOrder` indicates the order of appearance in the blog content
- `Context` provides surrounding text for better UX when displaying mentions

**Foreign Keys:**
- `FK_BlogEventMentions_Blogs` → `Blogs.Id` (CASCADE on delete)
- `FK_BlogEventMentions_Events` → `Events.Id` (CASCADE on delete - if event deleted, remove mentions)

---

### 5. BlogMedia

Junction table linking blogs to media assets (images, videos) used in blog content (many-to-many relationship).

**Table Name:** `BlogMedia`

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| `Id` | `uuid` | PRIMARY KEY, NOT NULL | Unique identifier |
| `BlogId` | `uuid` | NOT NULL, FK → Blogs.Id | Blog reference |
| `MediaAssetId` | `uuid` | NOT NULL, FK → MediaAssets.Id | Media asset reference |
| `DisplayOrder` | `integer` | NOT NULL, DEFAULT 0 | Order for display in content |
| `IsFeatured` | `boolean` | NOT NULL, DEFAULT false | Featured image in blog header |
| `UsageType` | `varchar(20)` | NOT NULL, DEFAULT 'content' | Usage: 'content', 'header', 'thumbnail' |
| `CreatedAt` | `timestamptz` | NOT NULL | Creation timestamp |

**Indexes:**
- `IX_BlogMedia_BlogId` on `BlogId` (FK index - crucial for blog media queries)
- `IX_BlogMedia_MediaAssetId` on `MediaAssetId` (FK index)
- Composite: `IX_BlogMedia_BlogId_DisplayOrder` on `(BlogId, DisplayOrder)` - For ordered media display
- Unique: `UQ_BlogMedia_Blog_Media` on `(BlogId, MediaAssetId)` (prevent duplicates)

**Business Rules:**
- Only one media asset per blog can have `IsFeatured = true`
- `UsageType` must be one of: 'content', 'header', 'thumbnail'
- If `IsFeatured = true`, `UsageType` should typically be 'header'

**Foreign Keys:**
- `FK_BlogMedia_Blogs` → `Blogs.Id` (CASCADE on delete)
- `FK_BlogMedia_MediaAssets` → `MediaAssets.Id` (RESTRICT on delete - prevent deletion if in use)

---

### 6. MediaAssets

Stores metadata for images and animations used in events.

**Table Name:** `MediaAssets`

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| `Id` | `uuid` | PRIMARY KEY, NOT NULL | Unique identifier |
| `FileName` | `varchar(255)` | NOT NULL | Original filename |
| `StoragePath` | `varchar(1000)` | NOT NULL | Path in storage (S3, local, etc.) |
| `ContentType` | `varchar(100)` | NOT NULL | MIME type (image/jpeg, video/mp4, etc.) |
| `MediaType` | `varchar(20)` | NOT NULL | Type: 'image', 'animation', 'video' |
| `FileSize` | `bigint` | NOT NULL | Size in bytes |
| `Width` | `integer` | NULL | Image/video width in pixels |
| `Height` | `integer` | NULL | Image/video height in pixels |
| `Duration` | `integer` | NULL | Animation/video duration in seconds |
| `ThumbnailPath` | `varchar(1000)` | NULL | Path to thumbnail image |
| `AltText` | `varchar(500)` | NULL | Accessibility alt text |
| `Caption` | `varchar(500)` | NULL | Optional caption |
| `Tags` | `varchar[]` | NULL | Array of tags for organization |
| `CreatedAt` | `timestamptz` | NOT NULL | Creation timestamp |
| `UpdatedAt` | `timestamptz` | NOT NULL | Last update timestamp |
| `CreatedBy` | `uuid` | NULL | User ID who created (FK to Users) |
| `IsDeleted` | `boolean` | NOT NULL, DEFAULT false | Soft delete flag |
| `DeletedAt` | `timestamptz` | NULL | Deletion timestamp |

**Indexes:**
- `IX_MediaAssets_CreatedBy` on `CreatedBy` (FK index)
- Full-text: `IX_MediaAssets_Tags` on `Tags` using GIN index (PostgreSQL) - For media search by tags

**Business Rules:**
- `MediaType` must be one of: 'image', 'animation', 'video'
- `ContentType` must match `MediaType` (e.g., image/jpeg for image type)
- `StoragePath` must be unique among non-deleted assets
- If `Width` is provided, `Height` should also be provided

---

### 7. EventMedia

Junction table linking events to media assets (many-to-many relationship).

**Table Name:** `EventMedia`

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| `Id` | `uuid` | PRIMARY KEY, NOT NULL | Unique identifier |
| `EventId` | `uuid` | NOT NULL, FK → Events.Id | Event reference |
| `MediaAssetId` | `uuid` | NOT NULL, FK → MediaAssets.Id | Media asset reference |
| `DisplayOrder` | `integer` | NOT NULL, DEFAULT 0 | Order for display |
| `IsPrimary` | `boolean` | NOT NULL, DEFAULT false | Primary media for event |
| `UsageType` | `varchar(20)` | NOT NULL, DEFAULT 'general' | Usage: 'general', 'timeline', 'map', 'detail' |
| `CreatedAt` | `timestamptz` | NOT NULL | Creation timestamp |

**Indexes:**
- `IX_EventMedia_EventId` on `EventId` (FK index - crucial for event media queries)
- `IX_EventMedia_MediaAssetId` on `MediaAssetId` (FK index)
- Composite: `IX_EventMedia_EventId_DisplayOrder` on `(EventId, DisplayOrder)` - For ordered media display
- Unique: `UQ_EventMedia_Event_Media` on `(EventId, MediaAssetId)` (prevent duplicates)

**Business Rules:**
- Only one media asset per event can have `IsPrimary = true`
- `UsageType` must be one of: 'general', 'timeline', 'map', 'detail'

**Foreign Keys:**
- `FK_EventMedia_Events` → `Events.Id` (CASCADE on delete)
- `FK_EventMedia_MediaAssets` → `MediaAssets.Id` (RESTRICT on delete - prevent deletion if in use)

---

### 8. ContentVersions

Tracks version history for period, event, and blog content (supports rollback and audit).

**Table Name:** `ContentVersions`

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| `Id` | `uuid` | PRIMARY KEY, NOT NULL | Unique identifier |
| `EntityType` | `varchar(50)` | NOT NULL | 'Period', 'Event', or 'Blog' |
| `EntityId` | `uuid` | NOT NULL | ID of the entity |
| `VersionNumber` | `integer` | NOT NULL | Sequential version number |
| `Content` | `jsonb` | NOT NULL | Full entity state as JSON |
| `ChangeDescription` | `varchar(500)` | NULL | Description of changes |
| `CreatedAt` | `timestamptz` | NOT NULL | Version creation timestamp |
| `CreatedBy` | `uuid` | NULL | User ID who created version (FK to Users) |

**Indexes:**
- Composite: `IX_ContentVersions_EntityType_EntityId_VersionNumber` on `(EntityType, EntityId, VersionNumber)` - For version retrieval

**Business Rules:**
- `VersionNumber` must be sequential per entity (1, 2, 3, ...)
- `EntityType` must be one of: 'Period', 'Event', 'Blog'
- `Content` JSON must contain complete entity state

---

### 9. AuditLogs

Tracks all administrative operations for security and compliance.

**Table Name:** `AuditLogs`

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| `Id` | `uuid` | PRIMARY KEY, NOT NULL | Unique identifier |
| `UserId` | `uuid` | NULL | User ID who performed action (FK to Users) |
| `Action` | `varchar(50)` | NOT NULL | Action type (CREATE, UPDATE, DELETE, PUBLISH, etc.) |
| `EntityType` | `varchar(50)` | NOT NULL | Entity type (Period, Event, Blog, MediaAsset, etc.) |
| `EntityId` | `uuid` | NULL | ID of affected entity |
| `Changes` | `jsonb` | NULL | Before/after state or change details |
| `IpAddress` | `varchar(45)` | NULL | IP address of request |
| `UserAgent` | `varchar(500)` | NULL | User agent string |
| `CreatedAt` | `timestamptz` | NOT NULL | Action timestamp |

**Indexes:**
- `IX_AuditLogs_UserId` on `UserId` (FK index)
- Composite: `IX_AuditLogs_EntityType_EntityId` on `(EntityType, EntityId)` - For entity audit history
- Composite: `IX_AuditLogs_UserId_CreatedAt` on `(UserId, CreatedAt)` - For user activity queries

**Business Rules:**
- `Action` should follow standard CRUD operations: CREATE, READ, UPDATE, DELETE, PUBLISH, UNPUBLISH
- `Changes` JSON should contain relevant before/after state for UPDATE actions

---

## Existing Tables (Reference)

### Users
Already implemented. Key fields:
- `Id` (uuid, PK)
- `Email` (varchar(255), unique)
- `PasswordHash` (varchar(255))
- `Role` (varchar(20)) - 'user' or 'admin'
- `FirstName`, `LastName`, `AvatarUrl`
- Soft delete support

### RefreshTokens
Already implemented. Key fields:
- `Id` (uuid, PK)
- `UserId` (uuid, FK → Users)
- `Token` (varchar(255), unique)
- `ExpiresAt` (timestamptz)
- `IsRevoked` (boolean)

### PasswordResetTokens
Already implemented. Key fields:
- `Id` (uuid, PK)
- `UserId` (uuid, FK → Users)
- `Token` (varchar(255), unique)
- `ExpiresAt` (timestamptz)
- `IsUsed` (boolean)

---

## Data Types and Constraints

### Date Handling

- **Years**: Stored as `integer` to support BCE/CE (negative for BCE, e.g., -500 for 500 BCE, positive for CE)
- **Dates**: `date` type for specific dates when known, but **nullable** for BCE events or when only year is available
- **BCE Events**: For BCE events, `EventDate` is typically `NULL` since exact dates are rarely known; only `EventYear` is used
- **CE Events**: For CE events, `EventDate` may be provided if the specific date is known
- **Timestamps**: All use `timestamptz` (timestamp with time zone) for UTC storage

### Geographic Data

- **Coordinates**: `decimal(10,8)` for latitude, `decimal(11,8)` for longitude
- **Indexing**: PostgreSQL GIST index on `(Latitude, Longitude)` for spatial queries
- **Validation**: Latitude must be -90 to 90, Longitude must be -180 to 180

### Text Content

- **Short Text**: `varchar` with appropriate length limits
- **Long Text**: `text` type for descriptions and content
- **Rich Content**: Supports both markdown and HTML (stored as text, rendered by application)

### JSON Storage

- **ContentVersions.Content**: `jsonb` for efficient JSON storage and querying
- **AuditLogs.Changes**: `jsonb` for flexible change tracking
- **MediaAssets.Tags**: `varchar[]` array type for tags

---

## Indexing Strategy

### Performance Indexes

**Crucial Indexes Only:**
1. **Foreign Key Indexes**: All FK columns indexed for JOIN performance
2. **Composite Query Indexes**: Multi-column indexes for common query patterns:
   - Timeline/List queries: `(IsPublished, IsDeleted, DisplayOrder/PublishedAt)`
   - Chronological ordering: `(PeriodId, EventYear, DisplayOrder)`
   - Published content filtering: `(PeriodId, IsPublished, IsDeleted)`
3. **Geographic Queries**: GIST index on `(Latitude, Longitude)` for map visualizations
4. **Full-Text Search**: GIN indexes for search functionality (blog content, media tags)
5. **Unique Constraints**: Slug and junction table uniqueness
6. **Ordered Display**: Composite indexes with DisplayOrder for sorted results

### Unique Constraints

- `Periods.Name` (among non-deleted records - application level)
- `Blogs.Slug` (among non-deleted records - application level)
- `MediaAssets.StoragePath` (among non-deleted records - application level)
- `EventMedia(EventId, MediaAssetId)` (prevent duplicate associations)
- `BlogEventMentions(BlogId, EventId)` (prevent duplicate mentions)
- `BlogMedia(BlogId, MediaAssetId)` (prevent duplicate associations)

---

## Relationships Summary

| Relationship | Type | Cascade Delete | Notes |
|------------|------|----------------|-------|
| Periods → MediaAssets (Thumbnail) | Many-to-One | SET NULL | Preserve period if thumbnail deleted |
| Events → Periods | Many-to-One | CASCADE | Deleting period deletes events |
| Events → MediaAssets (Thumbnail) | Many-to-One | SET NULL | Preserve event if thumbnail deleted |
| EventMedia → Events | Many-to-One | CASCADE | Deleting event removes media associations |
| EventMedia → MediaAssets | Many-to-One | RESTRICT | Prevent deletion of media in use |
| Blogs → Users (Author) | Many-to-One | SET NULL | Preserve blog if author deleted |
| Blogs → Users (CreatedBy) | Many-to-One | SET NULL | Preserve blog if creator deleted |
| Blogs → MediaAssets (Thumbnail) | Many-to-One | SET NULL | Preserve blog if thumbnail deleted |
| BlogEventMentions → Blogs | Many-to-One | CASCADE | Deleting blog removes event mentions |
| BlogEventMentions → Events | Many-to-One | CASCADE | Deleting event removes blog mentions |
| BlogMedia → Blogs | Many-to-One | CASCADE | Deleting blog removes media associations |
| BlogMedia → MediaAssets | Many-to-One | RESTRICT | Prevent deletion of media in use |
| ContentVersions → Users | Many-to-One | SET NULL | Preserve history if user deleted |
| AuditLogs → Users | Many-to-One | SET NULL | Preserve audit trail if user deleted |
| Periods → Users (CreatedBy) | Many-to-One | SET NULL | Preserve period if creator deleted |
| Events → Users (CreatedBy) | Many-to-One | SET NULL | Preserve event if creator deleted |
| Periods → MediaAssets (Thumbnail) | Many-to-One | SET NULL | Preserve period if thumbnail deleted |
| Events → MediaAssets (Thumbnail) | Many-to-One | SET NULL | Preserve event if thumbnail deleted |

---

## Query Patterns

### Common Queries

1. **Timeline Overview**: Get all published periods ordered by `DisplayOrder`
   ```sql
   SELECT * FROM Periods 
   WHERE IsPublished = true AND IsDeleted = false 
   ORDER BY DisplayOrder;
   ```

2. **Period Events**: Get all events for a period, ordered chronologically
   ```sql
   SELECT * FROM Events 
   WHERE PeriodId = @periodId 
     AND IsPublished = true AND IsDeleted = false 
   ORDER BY EventYear, EventDate NULLS LAST, DisplayOrder;
   ```
   Note: `EventDate NULLS LAST` ensures BCE events (with null dates) are ordered by year, then by date when available.

3. **Simultaneous Events**: Get events with same year/date for clustering
   ```sql
   -- Events with same year (for BCE events or when date is unknown)
   SELECT EventYear, COUNT(*) as EventCount 
   FROM Events 
   WHERE PeriodId = @periodId AND IsPublished = true AND IsDeleted = false 
   GROUP BY EventYear 
   HAVING COUNT(*) > 1
   
   UNION ALL
   
   -- Events with same specific date (when EventDate is not null)
   SELECT EventYear, COUNT(*) as EventCount 
   FROM Events 
   WHERE PeriodId = @periodId 
     AND IsPublished = true AND IsDeleted = false 
     AND EventDate IS NOT NULL
   GROUP BY EventDate 
   HAVING COUNT(*) > 1;
   ```
   Note: Clustering considers both year-based grouping (for BCE events) and date-based grouping (for events with specific dates).

4. **Event with Media**: Get event with all associated media
   ```sql
   SELECT e.*, ma.*, em.DisplayOrder, em.IsPrimary, em.UsageType
   FROM Events e
   LEFT JOIN EventMedia em ON e.Id = em.EventId
   LEFT JOIN MediaAssets ma ON em.MediaAssetId = ma.Id
   WHERE e.Id = @eventId AND e.IsDeleted = false;
   ```

5. **Geographic Events**: Get events within a geographic bounding box
   ```sql
   SELECT * FROM Events 
   WHERE PeriodId = @periodId 
     AND Latitude BETWEEN @minLat AND @maxLat
     AND Longitude BETWEEN @minLng AND @maxLng
     AND IsPublished = true AND IsDeleted = false;
   ```

6. **Blogs List**: Get all published blogs ordered by publication date (reverse chronological)
   ```sql
   SELECT * FROM Blogs 
   WHERE IsPublished = true AND IsDeleted = false 
     AND PublishedAt IS NOT NULL
   ORDER BY PublishedAt DESC;
   ```

7. **Blog with Event Mentions**: Get blog with all mentioned events
   ```sql
   SELECT b.*, e.*, bem.MentionOrder, bem.Context
   FROM Blogs b
   LEFT JOIN BlogEventMentions bem ON b.Id = bem.BlogId
   LEFT JOIN Events e ON bem.EventId = e.Id
   WHERE b.Id = @blogId AND b.IsDeleted = false
   ORDER BY bem.MentionOrder;
   ```

8. **Blog with Media**: Get blog with all associated media
   ```sql
   SELECT b.*, ma.*, bm.DisplayOrder, bm.IsFeatured, bm.UsageType
   FROM Blogs b
   LEFT JOIN BlogMedia bm ON b.Id = bm.BlogId
   LEFT JOIN MediaAssets ma ON bm.MediaAssetId = ma.Id
   WHERE b.Id = @blogId AND b.IsDeleted = false
   ORDER BY bm.DisplayOrder;
   ```

9. **Events Mentioned in Blogs**: Get all blogs that mention a specific event
   ```sql
   SELECT b.*, bem.MentionOrder, bem.Context
   FROM Blogs b
   INNER JOIN BlogEventMentions bem ON b.Id = bem.BlogId
   WHERE bem.EventId = @eventId 
     AND b.IsPublished = true AND b.IsDeleted = false
   ORDER BY b.PublishedAt DESC;
   ```

10. **Blog Search**: Full-text search on blog titles and content
    ```sql
    SELECT * FROM Blogs 
    WHERE IsPublished = true AND IsDeleted = false
      AND (
        Title ILIKE '%' || @searchTerm || '%'
        OR Content ILIKE '%' || @searchTerm || '%'
        OR @searchTerm = ANY(Tags)
      )
    ORDER BY PublishedAt DESC;
    ```

---

## Migration Strategy

### Phase 1: Core Entities
1. Create `Periods` table
2. Create `Events` table with foreign key to `Periods`
3. Add indexes for common queries

### Phase 2: Media Management
1. Create `MediaAssets` table
2. Create `EventMedia` junction table
3. Add media-related indexes

### Phase 3: Content Management
1. Create `ContentVersions` table
2. Create `AuditLogs` table
3. Add versioning and audit indexes

### Phase 4: Blog Feature (Post-MVP)
1. Create `Blogs` table
2. Create `BlogEventMentions` junction table
3. Create `BlogMedia` junction table
4. Add blog-related indexes
5. Update `ContentVersions` to support Blog entity type

### Phase 5: Optimization
1. Add geographic indexes (GIST)
2. Add full-text search indexes (GIN)
3. Optimize composite indexes based on query patterns

---

## Data Integrity Rules

### Application-Level Constraints

1. **Period Validation**:
   - `StartYear` ≤ `EndYear`
   - `Name` unique among non-deleted periods
   - `DisplayOrder` unique among non-deleted periods

2. **Event Validation**:
   - `EventYear` within period's `StartYear` to `EndYear` range
   - `EventYear` can be negative for BCE (e.g., -500 for 500 BCE) or positive for CE
   - `EventDate` is optional - typically null for BCE events or when only year is known
   - If `EventDate` is provided, it must be consistent with `EventYear`, `EventMonth`, and `EventDay`
   - If `Latitude` provided, `Longitude` must be provided
   - `EventMonth` in range 1-12 if provided
   - `EventDay` valid for given month if provided

3. **Media Validation**:
   - `MediaType` matches `ContentType` category
   - `StoragePath` unique among non-deleted assets
   - Only one primary media per event

4. **Blog Validation**:
   - `Slug` unique among non-deleted blogs
   - `Slug` must be URL-friendly format
   - If `PublishedAt` is set, `IsPublished` should be true
   - Only one featured media per blog

5. **Versioning**:
   - `VersionNumber` sequential per entity
   - `EntityType` must be 'Period', 'Event', or 'Blog'

---

## Security Considerations

1. **Soft Deletes**: All entities support soft deletion for data recovery
2. **Audit Trail**: All admin operations logged in `AuditLogs`
3. **User Tracking**: `CreatedBy` and `UpdatedBy` track user actions
4. **Content Versioning**: Full history preserved for rollback
5. **Foreign Key Constraints**: Prevent orphaned records
6. **Index Security**: Indexes don't expose sensitive data

---

## Performance Considerations

1. **Indexing**: Strategic indexes for common query patterns
2. **Pagination**: All list queries should support pagination
3. **Soft Delete Filtering**: Indexes include `IsDeleted` for efficient filtering
4. **Geographic Queries**: GIST index enables efficient spatial queries
5. **JSON Queries**: `jsonb` type enables efficient JSON querying
6. **Composite Indexes**: Optimized for multi-column queries

---

## Future Enhancements

### Post-MVP Considerations

1. **Country-Specific Timelines**: Add `CountryId` to `Periods` and `Events`
2. **Event Relationships**: Junction table for related events
3. **User Favorites**: Track user bookmarks/favorites for blogs and events
4. **Blog Categories**: Add category system for blog organization
5. **Blog Comments**: User comments on blog posts (if user accounts added)
6. **Search Indexing**: Full-text search on event/period/blog content
7. **Analytics**: Track user interactions, views, and engagement metrics
8. **Content Translation**: Multi-language support tables
9. **Blog Series**: Group related blog posts into series
10. **Related Blogs**: Automatic or manual related blog suggestions

---

## Appendix: Entity Framework Core Configuration

### Period Configuration Example

```csharp
modelBuilder.Entity<Period>(entity =>
{
    entity.HasKey(p => p.Id);
    entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
    entity.Property(p => p.StartYear).IsRequired();
    entity.Property(p => p.EndYear).IsRequired();
    entity.Property(p => p.DisplayOrder).IsRequired().HasDefaultValue(0);
    entity.Property(p => p.IsPublished).IsRequired().HasDefaultValue(false);
    
    entity.HasIndex(p => p.StartYear);
    entity.HasIndex(p => p.DisplayOrder);
    entity.HasIndex(p => new { p.IsPublished, p.IsDeleted, p.DisplayOrder });
    
    entity.HasOne<User>()
        .WithMany()
        .HasForeignKey(p => p.CreatedBy)
        .OnDelete(DeleteBehavior.SetNull);
    
    entity.HasOne<MediaAsset>()
        .WithMany()
        .HasForeignKey(p => p.ThumbnailMediaAssetId)
        .OnDelete(DeleteBehavior.SetNull);
});
```

### Event Configuration Example

```csharp
modelBuilder.Entity<Event>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Title).HasMaxLength(300).IsRequired();
    entity.Property(e => e.EventDate).IsRequired(false); // Nullable for BCE events
    entity.Property(e => e.EventYear).IsRequired();
    entity.Property(e => e.Latitude).HasPrecision(10, 8);
    entity.Property(e => e.Longitude).HasPrecision(11, 8);
    
    entity.HasIndex(e => e.PeriodId);
    entity.HasIndex(e => e.EventYear);
    entity.HasIndex(e => e.EventDate); // Nullable index
    entity.HasIndex(e => new { e.PeriodId, e.EventYear, e.DisplayOrder });
    
    // Geographic index (PostgreSQL specific)
    entity.HasIndex(e => new { e.Latitude, e.Longitude })
        .HasMethod("gist");
    
    entity.HasOne<Period>()
        .WithMany()
        .HasForeignKey(e => e.PeriodId)
        .OnDelete(DeleteBehavior.Cascade);
    
    entity.HasOne<MediaAsset>()
        .WithMany()
        .HasForeignKey(e => e.ThumbnailMediaAssetId)
        .OnDelete(DeleteBehavior.SetNull);
});
```

### Blog Configuration Example

```csharp
modelBuilder.Entity<Blog>(entity =>
{
    entity.HasKey(b => b.Id);
    entity.Property(b => b.Title).HasMaxLength(300).IsRequired();
    entity.Property(b => b.Slug).HasMaxLength(350).IsRequired();
    entity.Property(b => b.ShortDescription).HasMaxLength(500);
    entity.Property(b => b.Content).IsRequired();
    entity.Property(b => b.AuthorName).HasMaxLength(200);
    entity.Property(b => b.MetaTitle).HasMaxLength(200);
    entity.Property(b => b.MetaDescription).HasMaxLength(500);
    entity.Property(b => b.ViewCount).IsRequired().HasDefaultValue(0);
    entity.Property(b => b.IsPublished).IsRequired().HasDefaultValue(false);
    
    entity.HasIndex(b => b.Slug).IsUnique();
    entity.HasIndex(b => b.PublishedAt);
    entity.HasIndex(b => new { b.IsPublished, b.IsDeleted, b.PublishedAt });
    
    // Full-text search index (PostgreSQL specific)
    entity.HasIndex(b => new { b.Title, b.Content })
        .HasMethod("gin")
        .HasOperators("gin_trgm_ops");
    
    entity.HasOne<User>()
        .WithMany()
        .HasForeignKey(b => b.AuthorId)
        .OnDelete(DeleteBehavior.SetNull);
    
    entity.HasOne<MediaAsset>()
        .WithMany()
        .HasForeignKey(b => b.ThumbnailMediaAssetId)
        .OnDelete(DeleteBehavior.SetNull);
});
```

### BlogEventMention Configuration Example

```csharp
modelBuilder.Entity<BlogEventMention>(entity =>
{
    entity.HasKey(bem => bem.Id);
    
    entity.HasIndex(bem => bem.BlogId);
    entity.HasIndex(bem => bem.EventId);
    entity.HasIndex(bem => new { bem.BlogId, bem.MentionOrder });
    
    entity.HasIndex(bem => new { bem.BlogId, bem.EventId })
        .IsUnique();
    
    entity.HasOne<Blog>()
        .WithMany()
        .HasForeignKey(bem => bem.BlogId)
        .OnDelete(DeleteBehavior.Cascade);
    
    entity.HasOne<Event>()
        .WithMany()
        .HasForeignKey(bem => bem.EventId)
        .OnDelete(DeleteBehavior.Cascade);
});
```

### BlogMedia Configuration Example

```csharp
modelBuilder.Entity<BlogMedia>(entity =>
{
    entity.HasKey(bm => bm.Id);
    
    entity.Property(bm => bm.UsageType)
        .HasMaxLength(20)
        .IsRequired()
        .HasDefaultValue("content");
    
    entity.HasIndex(bm => bm.BlogId);
    entity.HasIndex(bm => bm.MediaAssetId);
    entity.HasIndex(bm => new { bm.BlogId, bm.DisplayOrder });
    
    entity.HasIndex(bm => new { bm.BlogId, bm.MediaAssetId })
        .IsUnique();
    
    entity.HasOne<Blog>()
        .WithMany()
        .HasForeignKey(bm => bm.BlogId)
        .OnDelete(DeleteBehavior.Cascade);
    
    entity.HasOne<MediaAsset>()
        .WithMany()
        .HasForeignKey(bm => bm.MediaAssetId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

---

_This database design provides a solid foundation for the Notism application, supporting both the public-facing timeline visualization, comprehensive blog feature, and the admin content management system._

