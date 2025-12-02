# notism - Product Requirements Brief

**Author:** BMad  
**Date:** 2025-01-27  
**Version:** 2.0

---

## Overview

notism is a web application that visualizes the complete human development timeline from ancient apes to the present day. It emphasizes **visual storytelling** over text-heavy content, making history engaging through animations, images, and interactive exploration.

---

## Core Concept

**Visualization-first learning** - Users explore history through:
- Rich visual content (animations, images, interactive maps)
- Three-level navigation hierarchy
- Smooth, fluid timeline navigation
- Minimal text with maximum visual impact

---

## Three-Level Navigation Structure

### Level 1: Timeline Overview
**Purpose:** Display all periods from ancient apes to present

**Features:**
- Complete timeline visualization showing all periods
- **Dynamic gaps between periods** (visual spacing between period markers calculated based on event relevance)
- Period markers/indicators on the timeline
- Smooth navigation between periods
- Jump to any period from the timeline

**Interaction:**
- Click/select a period to navigate to Level 2 (Period Details)

---

### Level 2: Period Details
**Purpose:** Showcase all important events within a selected period

**Layout:**
- **Dual view design**: Timeline on the left, interactive map on the right
- **Synchronized views**: Timeline and map are synchronized - selecting an event highlights it in both views

**Features:**
- **Events displayed in linear chronological order** on the timeline (left side)
- **Events visualized as dots on an interactive map** (right side) at their geographic locations
- **Images or animations** displayed for events (when available)
- **Cluster/Group visualization** for simultaneous events:
  - Events occurring at the same time are grouped into cluster markers
  - Cluster markers show the count of simultaneous events (e.g., "3")
  - Clicking/tapping a cluster expands to show all events in that group
  - Expanded events can be displayed inline or in a list format
- Smooth transition from Level 1

**Visualization Methods:**
- **Timeline markers** - Events appear as markers on the timeline (left)
- **Map dots** - Each event appears as a dot on the map (right) at its geographic location
- **Cluster markers** - Simultaneous events grouped with count indicator
- **Images** - Static images providing visual context
- **Animations** - Animated content bringing events to life
- Events can use one or multiple visualization methods

**Simultaneous Events Handling:**
- Events occurring at the same time (same year/date) are grouped into clusters
- Cluster marker appears on timeline showing event count
- Clicking cluster expands to reveal all events in the group
- Expanded events maintain their geographic positions on the map
- Map shows all event dots simultaneously (no clustering on map)

**Interaction:**
- **Hover on map dot** → Shows popup with brief event information (title, date, short description)
- Click/select an event marker on timeline → highlights corresponding map dot
- Click/select a map dot → highlights corresponding timeline marker
- Click/select an event to navigate to Level 3 (Event Details)
- Click cluster marker to expand and view all simultaneous events
- Navigate back to Level 1 (Timeline Overview)

---

### Level 3: Event Details
**Purpose:** Comprehensive information about a specific event

**Features:**
- **Comprehensive event information** (detailed description, context, significance)
- **Images or animations** displayed prominently (when available)
- Related events or connections
- Historical context and background
- Smooth transition from Level 2

**Visualization:**
- High-quality images or animations as primary visual content
- Supporting text and information
- Related content suggestions

**Interaction:**
- Navigate back to Level 2 (Period Details)
- Navigate to related events
- Navigate back to Level 1 (Timeline Overview)

---

## Product Scope

### MVP - Human Development Focus
**Scope:** Focus exclusively on **Human Development** timeline

**Features:**
- Three-level navigation (Timeline → Period → Event)
- Timeline with all periods (dynamic gaps based on event relevance)
- Period details with **dual view layout** (timeline left, map right)
- Linear event display on timeline with cluster/group visualization for simultaneous events
- Map visualization with event dots synchronized with timeline
- **Hover popups on map dots** showing brief event information (title, date, description)
- Image and animation support for events
- Comprehensive event detail views
- Smooth navigation between levels
- **Admin panel** for content management with rich text/markdown editors

**Out of Scope for MVP:**
- Country-specific development timelines
- User accounts or personalization (for end users)
- Content editing or management (admin panel is separate)

---

## Admin Panel

**Purpose:** Content management system for administrators to add, update, and manage timeline data

**Features:**
- **Timeline Management**: Add/update/delete periods on the timeline
- **Period Management**: Add/update/delete periods with details
- **Event Management**: Add/update/delete events within periods
- **Rich Text/Markdown Editors**: 
  - Rich text editor for content writing (WYSIWYG)
  - Markdown editor option for content writing
  - Support for formatting, images, links, and structured content
- **Event Data Management**:
  - Set event dates and periods
  - Add geographic coordinates (latitude/longitude) for map visualization
  - Upload images and animations
  - Configure event visualization methods (image, map, animation)
- **Content Preview**: Preview how content will appear in the application
- **Authentication**: Secure admin access with authentication

**Admin Panel Structure:**
- Dashboard overview
- Timeline editor (manage periods)
- Period editor (manage events within periods)
- Event editor (comprehensive event details)
- Blog editor (manage blog posts with event mentions) (Post-MVP)
- Media library (manage images and animations)
- Settings and configuration

---

### Growth Features (Post-MVP)

**Blogs Feature:** Historical blog posts that provide narrative context and deeper exploration

**Features:**
- **Blogs Tab:** Display a list of historical blog posts with titles, publication dates, brief descriptions, and thumbnails
- **Blog Details View:** Comprehensive blog content with rich text, images, and historical context
- **Event Mentions:** Events can be mentioned within blog content and are clickable, allowing users to navigate directly to event details (Level 3)
- **Blog-Event Integration:** Seamless navigation between blog content and related events, enhancing the learning experience by connecting narrative storytelling with specific historical events

**Admin Panel Support:**
- Blog creation and management with rich text/markdown editors
- Event mention functionality - administrators can select events to mention in blog content
- Content preview before publishing
- Media management for blog images

### Long-Term Vision - Country-Specific Development

**Future Feature:** Add country-specific development timelines

**Structure:**
- **Similar 3-level navigation** as Human Development
- **Level 1:** Timeline showing all periods for the selected country
- **Level 2:** Period details with events displayed linearly (map dots, images, animations)
- **Level 3:** Event details with comprehensive information

**Implementation:**
- Country selection/filtering mechanism
- Separate data structure for country-specific events
- Reuse same visualization patterns and navigation structure
- Maintain consistency with Human Development experience

---

## Functional Requirements

### Level 1: Timeline Overview
- Display all periods from ancient apes to present
- Dynamic visual gaps between periods (calculated based on event relevance)
- Period markers clearly visible
- Smooth scrolling/panning
- Click to navigate to period details

### Level 2: Period Details
- **Dual view layout**: Timeline on left, map on right
- Display all important events within selected period
- Events in linear chronological order on timeline (left)
- Map visualization with event dots (right)
- **Hover popup on map dots**: Brief event information (title, date, description) appears on hover
- **Cluster/Group visualization** for simultaneous events:
  - Simultaneous events grouped into cluster markers on timeline
  - Cluster markers show event count
  - Expandable clusters reveal all events in group
- Synchronized interaction: Clicking timeline marker highlights map dot and vice versa
- Images/animations for events
- Click to navigate to event details

### Level 3: Event Details
- Comprehensive event information
- Images/animations prominently displayed
- Historical context and background
- Related events connections
- Navigation back to period or timeline

### Blogs Feature (Post-MVP)
- Blogs tab in main navigation
- List view of historical blog posts (reverse chronological order)
- Blog details view with comprehensive content
- Clickable event mentions in blog content that navigate to event details
- Visual distinction for event mentions (clearly clickable)
- Navigation back to blogs list from blog details
- Maintains minimal, clean design aesthetic

### User Experience
- Smooth, fluid animations (60fps target)
- Clear navigation hierarchy (breadcrumbs or back buttons)
- Minimal, clean interface design
- Sparse information density (focus on visuals)
- Keyboard navigation support
- WCAG 2.1 Level AA accessibility

---

## Technical Requirements

### Platform
- **Type:** Web application
- **Browsers:** Modern browsers only (Chrome, Firefox, Safari, Edge - latest 2 versions)
- **Responsive:** Desktop-first, tablet support

### Performance
- 60fps smooth animations
- Core timeline visible within 2 seconds
- Progressive content loading (on-demand)
- Optimized images and assets
- Efficient map rendering with multiple event dots
- Hover popups appear/disappear smoothly without performance impact

### Data Structure
- **MVP Data Storage**: Static JSON data files OR database (depending on admin panel implementation)
- Period-based data organization
- Event data includes: date, period, location coordinates (for map dots), images, animations, rich text/markdown content
- Event clustering logic: Events with same date/year grouped for cluster visualization
- Human Development data structure (MVP)
- Country-specific data structure (future)

### Admin Panel Architecture
- **Authentication**: Secure admin access with login/authentication
- **Content Management**: CRUD operations for timeline, periods, and events
- **Editor Support**: 
  - Rich text editor (WYSIWYG) for content creation
  - Markdown editor as alternative option
  - Support for formatting, images, links, structured content
- **Media Management**: Upload, organize, and manage images and animations
- **Data Management**: 
  - Set event dates, periods, geographic coordinates
  - Configure visualization methods
  - Preview content before publishing

### Layout & UI Structure
- **Period Details View (Level 2):**
  - Left panel: Timeline visualization (50-60% width)
  - Right panel: Interactive map (40-50% width)
  - Responsive: Stack vertically on smaller screens (timeline top, map bottom)

---

## Success Criteria

- Users can navigate seamlessly through all three levels
- Visual approach makes history accessible and engaging
- Smooth, fluid interactions throughout
- Map visualization effectively shows event geography
- Hover popups provide quick event information without navigation
- Educational value for students and educators
- Foundation ready for country-specific expansion
- Admin panel enables efficient content management

---

_This brief captures the essential requirements for notism's three-level timeline visualization product, focused on Human Development with future expansion to country-specific timelines._
