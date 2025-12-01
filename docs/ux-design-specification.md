# notism - UX Design Specification

**Author:** BMad
**Date:** 2025-01-27
**Version:** 2.0

---

## Project Vision

notism is a web application that visualizes the complete human development timeline from ancient apes to the present day. It serves as an engaging learning tool for students, educators, and general learners who want to understand history and how the world has been shaped, emphasizing visual storytelling over text-heavy content.

---

## Core Experience

**Primary User Action:** Explore the timeline chronologically

**Critical to Get Right:** The initial timeline view - this is the first impression and primary navigation mechanism

**What Should Feel Effortless:** Understanding historical connections - users should easily see how events relate and flow together

**Platform:** Web application (modern browsers)

---

## Desired Emotional Response

**Target Feeling:** Inspired and amazed

Users should experience wonder and inspiration as they explore human history. The visual-first approach should create moments of discovery and amazement, making history feel alive and engaging rather than academic or dry.

---

## Defining Experience

**The Core Experience:** "Explore the human development timeline chronologically through rich visual content"

When someone describes notism, they would say: "It's the app where you visually explore human history from ancient apes to today - everything comes alive through animations and images."

This is the ONE experience that defines the app - smooth, chronological exploration of history through visual storytelling.

**Experience Type:** Timeline visualization with rich media - has established patterns but we're innovating with the visual-first, sparse information density approach.

---

## Design System Recommendation

For a web application focused on visualization and inspiration, I recommend:

**shadcn/ui** (Modern, Tailwind-based, customizable)
- **Why:** Perfect for minimal, clean interfaces with full customization
- **Strengths:** 
  - Highly customizable (copy components, modify as needed)
  - Built on Radix UI primitives (excellent accessibility)
  - Tailwind CSS based (great for custom visualizations)
  - Modern, clean aesthetic that matches "minimal and clean" requirement
  - No design system "look" - you control the visual style
- **Best for:** Projects needing custom visualizations with accessible components

**Alternative:** Radix UI (if you want unstyled primitives with full control)

**Rationale:** Since notism's core is a custom timeline visualization, we need a system that provides accessible primitives without imposing a visual style. shadcn/ui gives us the best of both worlds - accessible components we can fully customize for the timeline experience.

---

## Visual Foundation

### Color Palette

**Selected Theme:** Modern Discovery (Theme 4)

**Primary Colors:**
- **Primary:** `#2563EB` (Vibrant blue) - Main actions, key timeline elements, primary navigation
- **Secondary:** `#7C3AED` (Rich purple) - Supporting actions, secondary elements
- **Accent:** `#F59E0B` (Amber) - Highlights, call-to-action elements, important markers

**Semantic Colors:**
- **Success:** `#10B981` (Green) - Positive feedback, completed states
- **Warning:** `#F59E0B` (Amber) - Warnings, attention needed
- **Error:** `#EF4444` (Red) - Errors, destructive actions

**Neutral Colors:**
- **Background:** `#FFFFFF` (White) - Main background
- **Surface:** `#F9FAFB` (Light gray) - Cards, elevated surfaces
- **Text Primary:** `#111827` (Near black) - Main text, headings
- **Text Secondary:** `#6B7280` (Medium gray) - Secondary text, descriptions
- **Border:** `#E5E7EB` (Light gray) - Borders, dividers

**Theme Personality:** Vibrant, contemporary, energetic - supports the "inspired and amazed" emotional goal while maintaining clean minimalism. The vibrant blue creates energy and discovery, while purple adds depth and sophistication. Amber provides warmth and highlights key moments.

### Typography

**Font Families:**
- **Headings:** System font stack (San Francisco on macOS, Segoe UI on Windows, Roboto on Android)
  - `-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif`
- **Body:** Same system font stack for consistency
- **Monospace:** `'SF Mono', Monaco, 'Cascadia Code', 'Roboto Mono', Consolas, monospace` (for dates, technical content if needed)

**Type Scale:**
- **H1:** 2.5rem (40px) / 1.2 line-height - Main page titles
- **H2:** 2rem (32px) / 1.3 line-height - Section headings
- **H3:** 1.5rem (24px) / 1.4 line-height - Subsection headings
- **H4:** 1.25rem (20px) / 1.4 line-height - Card titles, event names
- **H5:** 1.125rem (18px) / 1.5 line-height - Small headings
- **H6:** 1rem (16px) / 1.5 line-height - Smallest headings
- **Body:** 1rem (16px) / 1.6 line-height - Main content
- **Small:** 0.875rem (14px) / 1.5 line-height - Secondary text, captions
- **Tiny:** 0.75rem (12px) / 1.4 line-height - Labels, metadata

**Font Weights:**
- **Regular (400):** Body text, descriptions
- **Medium (500):** Buttons, emphasis
- **Semibold (600):** Headings, important labels
- **Bold (700):** Strong emphasis, key highlights

### Spacing & Layout

**Base Unit:** 4px (all spacing multiples of 4)

**Spacing Scale:**
- **xs:** 4px (0.25rem)
- **sm:** 8px (0.5rem)
- **md:** 16px (1rem)
- **lg:** 24px (1.5rem)
- **xl:** 32px (2rem)
- **2xl:** 48px (3rem)
- **3xl:** 64px (4rem)
- **4xl:** 96px (6rem)

**Layout Grid:**
- **Desktop:** 12-column grid with 24px gutters
- **Tablet:** 8-column grid with 16px gutters
- **Mobile:** 4-column grid with 16px gutters

**Container Widths:**
- **Desktop:** Max-width 1280px, centered
- **Tablet:** Max-width 1024px
- **Mobile:** Full width with 16px side padding

**Rationale:** 4px base unit provides fine-grained control for precise spacing. The spacing scale supports the "sparse with focus" principle - generous whitespace around content. Grid system ensures consistent alignment while maintaining flexibility for the custom timeline visualization.

---

## Design Direction

**Selected Direction:** Full-Width Immersive (Direction 1)

**Layout Approach:** Full-width timeline, no sidebar - timeline takes center stage

**Visual Hierarchy:** Spacious layout with maximum breathing room, content-focused

**Interaction Pattern:** Timeline as primary navigation mechanism, immersive exploration experience

**Visual Weight:** Minimal, content-focused - interface doesn't compete with historical content

**Rationale:** This direction maximizes visual impact and supports the "inspired and amazed" emotional goal. The full-width approach gives the timeline maximum space to shine, creating an immersive exploration experience. The spacious layout aligns with "sparse with focus" while the content-first approach ensures historical visuals are the star.

**Key Characteristics:**
- Timeline dominates the screen, creating maximum visual impact
- Spacious layout with generous whitespace
- Content-focused design - UI elements support, not compete
- Immersive experience that draws users into exploration

---

## Critical User Journeys

### Journey 1: Initial Timeline View (Most Critical)

**User Goal:** See the complete human development timeline and understand the scope

**Entry Point:** Landing on the homepage

**Flow:**
1. **Entry Screen:**
   - User sees: Full-width timeline visualization spanning from ancient apes to present
   - Visual: Timeline line with event markers, smooth gradient background
   - User does: Observes the timeline, understands the scope
   - System responds: Timeline is immediately visible, no loading delay

2. **Initial Interaction:**
   - User sees: Visual indicators (markers, nodes) for major events
   - User does: Scrolls or pans to explore different periods
   - System responds: Smooth, fluid animation as timeline moves

3. **Success State:**
   - User understands: The complete scope of human history is available
   - Next action: User begins exploring chronologically or jumps to a period

**Design Approach:** Single-screen, immediate value - timeline is visible instantly with no barriers

---

### Journey 2: Chronological Exploration

**User Goal:** Explore the timeline chronologically to understand historical connections

**Entry Point:** From initial timeline view

**Flow:**
1. **Start Exploration:**
   - User sees: Timeline with current position indicator
   - User does: Scrolls/scrolls horizontally or vertically (depending on timeline orientation)
   - System responds: Smooth scrolling with visual feedback showing position

2. **Period Discovery:**
   - User sees: Event markers become more detailed as they approach
   - User does: Continues scrolling through timeline
   - System responds: Visual content (images, animations) loads progressively

3. **Event Exploration:**
   - User sees: Event marker highlights on hover/approach
   - User does: Clicks/taps on event marker
   - System responds: Smooth transition to event detail view

4. **Event Detail View:**
   - User sees: Visual content (animation, images) with sparse descriptive text
   - User does: Views content, understands the event
   - System responds: Content displays with smooth animations

5. **Return to Timeline:**
   - User sees: Option to return to timeline
   - User does: Closes detail view or continues scrolling
   - System responds: Smooth transition back to timeline, maintaining scroll position

**Design Approach:** Natural, linear flow - scrolling feels effortless, content appears progressively

---

### Journey 3: Jump to Specific Period

**User Goal:** Quickly navigate to a specific historical period or event

**Entry Point:** From timeline view or initial load

**Flow:**
1. **Trigger Jump Navigation:**
   - User sees: Period selector or search/jump control (minimal, non-intrusive)
   - User does: Opens period selector or enters period name
   - System responds: Period list or search results appear

2. **Select Period:**
   - User sees: List of major periods (Ancient, Classical, Medieval, Modern, etc.)
   - User does: Selects desired period
   - System responds: Smooth transition/zoom to selected period on timeline

3. **Period View:**
   - User sees: Timeline focused on selected period with relevant events
   - User does: Explores events in that period
   - System responds: Period context maintained, events load progressively

**Design Approach:** Quick access, minimal friction - jump navigation doesn't disrupt the immersive experience

---

### Journey 4: Explore Period Details (Level 2 - Dual View)

**User Goal:** View all important events within a selected period using the dual view layout

**Entry Point:** From Level 1 Timeline Overview (clicking a period)

**Flow:**
1. **Period Selection:**
   - User sees: Period marker on Level 1 timeline
   - User does: Clicks/taps period marker
   - System responds: Smooth transition to Level 2 Period Details view

2. **Dual View Display:**
   - User sees: 
     - Left panel: Timeline showing events in linear chronological order
     - Right panel: Interactive map showing event dots at geographic locations
     - Both views synchronized and visible simultaneously
     - Smooth entrance animation revealing dual view
   - User does: Observes the dual view layout, understands event distribution
   - System responds: Events load progressively, map renders with all event dots

3. **Timeline Interaction:**
   - User sees: Events displayed linearly on timeline (left), some with cluster markers for simultaneous events
   - User does: Scrolls timeline, hovers over event markers, clicks cluster markers
   - System responds: 
     - Hover highlights event marker
     - Clicking cluster marker expands to show all simultaneous events
     - Corresponding map dot highlights when timeline event is selected
     - Smooth animations for all interactions

4. **Map Interaction:**
   - User sees: Event dots on map (right panel) at their geographic locations
   - User does: Hovers over map dots, clicks map dots
   - System responds:
     - Hover popup appears with brief event information (title, date, short description)
     - Popup appears smoothly without blocking map interaction
     - Clicking map dot highlights corresponding timeline marker
     - Synchronized highlighting between timeline and map

5. **Cluster Expansion:**
   - User sees: Cluster marker on timeline showing count (e.g., "3")
   - User does: Clicks/taps cluster marker
   - System responds: 
     - Cluster expands smoothly to reveal all simultaneous events inline
     - Expanded events maintain their geographic positions on map
     - All event dots remain visible on map (no clustering on map)
     - Smooth animation for expansion/collapse

6. **Event Selection:**
   - User sees: Event marker on timeline or map dot
   - User does: Clicks/taps event to view details
   - System responds: Smooth transition to Level 3 Event Details

7. **Navigation:**
   - User sees: Back button or breadcrumb to return to Level 1
   - User does: Returns to timeline overview
   - System responds: Smooth transition maintaining context

**Design Approach:** Dual view maximizes context - users see both temporal (timeline) and spatial (map) relationships simultaneously. Synchronized interaction creates seamless exploration experience.

---

### Journey 5: View Event with Visual Content

**User Goal:** Experience a historical event through rich visual content

**Entry Point:** From Level 2 Period Details (clicking event marker or map dot)

**Flow:**
1. **Event Selection:**
   - User sees: Event marker on timeline or map dot
   - User does: Clicks/taps event marker or map dot
   - System responds: Smooth transition to event view

2. **Event Content Display:**
   - User sees: 
     - Animation or static image (primary visual)
     - Event title and date
     - Sparse descriptive content (maintaining "sparse with focus")
     - Smooth entrance animation
   - User does: Views and absorbs the visual content
   - System responds: Content displays with smooth animations

3. **Content Interaction:**
   - User sees: Interactive elements (if applicable) - hover states, clickable areas
   - User does: Interacts with visual content
   - System responds: Smooth feedback, additional information reveals progressively

4. **Navigation:**
   - User sees: Options to return to period details, view next/previous event, return to timeline overview
   - User does: Chooses navigation action
   - System responds: Smooth transition maintaining context

**Design Approach:** Focused, immersive - one event at a time, maximum visual impact, minimal text

---

### Journey 6: Understanding Historical Connections

**User Goal:** See how events relate and flow together (what should feel effortless)

**Entry Point:** While exploring timeline

**Flow:**
1. **Visual Connection Indicators:**
   - User sees: Visual cues showing relationships between events (lines, color coding, grouping)
   - User does: Observes connections while scrolling
   - System responds: Connections become visible as user explores

2. **Contextual Information:**
   - User sees: Related events highlighted when viewing an event
   - User does: Views event and notices related events
   - System responds: Related events subtly highlighted or suggested

3. **Timeline Flow:**
   - User sees: Chronological progression makes connections obvious
   - User does: Scrolls through timeline naturally
   - System responds: Smooth flow from one period to next, showing progression

**Design Approach:** Visual-first connections - relationships shown through design, not text explanations

---

## Core Experience Principles

**Speed:** Immediate - timeline visible instantly, no loading barriers. Interactions feel instant and responsive.

**Guidance:** Minimal - timeline is self-explanatory. Visual cues guide without text instructions.

**Flexibility:** Balanced - users can explore chronologically (primary) or jump to periods (secondary). Both feel natural.

**Feedback:** Smooth and fluid - all transitions animated, visual feedback on all interactions. Celebratory moments when discovering significant events.

---

## Component Library

### Design System Components (from shadcn/ui)

**Base Components Available:**
- Buttons (primary, secondary, outline, ghost variants)
- Cards (for event content display)
- Inputs (for search/jump functionality if needed)
- Dialogs/Modals (for event detail views)
- Tooltips (for event markers on hover)
- Icons (from Lucide icon library)

**Customization:** All components can be fully customized to match the Modern Discovery color theme and minimal aesthetic.

### Custom Components Required

#### 1. Timeline Component

**Purpose:** The core visualization component that displays the human development timeline

**Anatomy:**
- Timeline line/axis (visual representation of time)
- Event markers (visual indicators for major events)
- Period sections (grouped areas for periods with 250-year gaps between them)
- Navigation controls (zoom, scroll indicators - minimal)
- Current position indicator (shows where user is on timeline)

**Period Structure:**
- Timeline shows 250-year visual gaps between periods (spacing between period markers)
- Periods are visually distinct sections on the timeline
- Events within each period are displayed in linear chronological order

**States:**
- **Default:** Timeline visible with event markers
- **Hover:** Event markers highlight, show tooltip with event name
- **Active/Selected:** Selected event marker emphasized, detail view opens
- **Loading:** Timeline skeleton with shimmer effect (if needed for initial load)
- **Zoomed In:** More detailed view, additional events visible
- **Zoomed Out:** Overview mode, major periods visible

**Variants:**
- **Horizontal Timeline:** Time flows left to right
- **Vertical Timeline:** Time flows top to bottom (alternative for mobile)

**Behavior:**
- Smooth scrolling/panning with momentum
- Zoom in/out with pinch or controls
- Click event marker → smooth transition to event detail
- Hover event marker → tooltip appears with event name and date

**Visual Style:**
- Timeline line: 2-4px stroke, gradient from primary to secondary color
- Event markers: Circular, 8-12px diameter, accent color with white border
- Spacing: Generous spacing between events, sparse information density

---

#### 2. Event Marker Component

**Purpose:** Visual indicator for historical events on the timeline

**Anatomy:**
- Marker circle/point (visual indicator)
- Optional label (event name, can be shown on hover)
- Connection line (if showing relationships)

**States:**
- **Default:** Small marker, subtle appearance
- **Hover:** Marker enlarges slightly, label appears, color intensifies
- **Active:** Marker emphasized, detail view opens
- **Related:** Subtle highlight when viewing related event

**Variants:**
- **Major Event:** Larger marker, more prominent
- **Minor Event:** Smaller marker, subtle
- **Period Marker:** Different style to indicate period boundaries

**Behavior:**
- Hover → tooltip with event name and date
- Click → smooth transition to event detail view
- Visual feedback on all interactions

---

#### 3. Event Detail View Component

**Purpose:** Displays rich visual content for a historical event

**Anatomy:**
- Visual content area (animation or image - primary focus)
- Event title and date
- Descriptive content (sparse, minimal text)
- Navigation controls (close, next/previous event)
- Related events indicator (subtle)

**States:**
- **Loading:** Skeleton loader for visual content
- **Loaded:** Content visible with smooth entrance animation
- **Error:** Graceful error state with retry option

**Variants:**
- **Animation View:** Full-screen or large animation
- **Image View:** High-quality image with overlay text
- **Interactive View:** Interactive elements within visual content
- **Map View:** Interactive map showing event location with geographic context

**Behavior:**
- Smooth entrance animation when opening
- Smooth exit animation when closing
- Progressive content loading (image/animation loads first, then text)
- Keyboard navigation support (ESC to close, arrow keys for next/previous)

**Visual Style:**
- Maximum space for visual content
- Text overlay minimal and non-intrusive
- Close button subtle, top-right corner
- Navigation arrows subtle, sides of content

---

#### 4. Period Selector Component

**Purpose:** Allows users to jump to specific historical periods

**Anatomy:**
- Period list (Ancient, Classical, Medieval, Renaissance, Modern, etc.)
- Search/filter (optional, for many periods)
- Current period indicator

**States:**
- **Collapsed:** Minimal trigger button or icon
- **Expanded:** Period list visible, current period highlighted
- **Selected:** Smooth transition to selected period on timeline

**Variants:**
- **Dropdown:** Traditional dropdown menu
- **Modal:** Full-screen or overlay modal
- **Sidebar:** Persistent sidebar (if not using full-width immersive)

**Behavior:**
- Click to expand/collapse
- Select period → smooth zoom/transition to period on timeline
- Keyboard navigation support
- Search/filter if many periods

**Visual Style:**
- Minimal, non-intrusive when collapsed
- Clean list design when expanded
- Current period clearly indicated

---

#### 5. Period Details Dual View Component

**Purpose:** The core Level 2 view displaying timeline and map side-by-side

**Anatomy:**
- Left panel: Timeline visualization (50-60% width)
- Right panel: Interactive map (40-50% width)
- Divider/resizer (optional, for adjusting panel widths)
- Synchronization indicator (subtle visual feedback when views are linked)

**Layout:**
- **Desktop:** Side-by-side layout, timeline left, map right
- **Tablet:** Side-by-side maintained, touch-optimized
- **Mobile:** Stacked vertically (timeline top, map bottom)

**States:**
- **Default:** Both panels visible, synchronized
- **Timeline Focus:** Timeline panel expanded, map minimized
- **Map Focus:** Map panel expanded, timeline minimized
- **Loading:** Skeleton loaders for both panels
- **Error:** Graceful error state with retry option

**Behavior:**
- Synchronized selection: Clicking timeline marker highlights map dot and vice versa
- Independent scrolling: Timeline scrolls independently, map pans/zooms independently
- Responsive breakpoint: Stacks vertically on smaller screens
- Smooth transitions when switching between focus states

**Visual Style:**
- Clean divider between panels (subtle border or shadow)
- Both panels maintain consistent spacing and padding
- Map uses neutral base colors to not compete with event dots
- Timeline uses same styling as Level 1 timeline component

---

#### 6. Cluster Marker Component

**Purpose:** Groups simultaneous events (same date/year) on the timeline

**Anatomy:**
- Cluster marker circle/indicator (larger than single event marker)
- Event count indicator (number or visual indicator)
- Expansion area (reveals all events when clicked)

**States:**
- **Collapsed:** Cluster marker visible with count (e.g., "3")
- **Expanded:** All simultaneous events displayed inline or in list
- **Hover:** Cluster marker highlights, shows preview of events
- **Active:** Expansion animation in progress

**Variants:**
- **Small Cluster (2-3 events):** Compact marker, inline expansion
- **Medium Cluster (4-7 events):** Larger marker, list expansion
- **Large Cluster (8+ events):** Prominent marker, scrollable list expansion

**Behavior:**
- Click/tap to expand/collapse
- Smooth expansion animation (200-300ms)
- Expanded events maintain chronological order
- All event dots remain visible on map (no clustering on map)
- Clicking expanded event navigates to Level 3

**Visual Style:**
- Cluster marker: 12-16px diameter, accent color with white border
- Count indicator: Bold, readable number or icon
- Expanded events: Same styling as regular event markers, slightly indented
- Smooth animation for expansion/collapse

---

#### 7. Interactive Map Component

**Purpose:** Displays event dots at their geographic locations with hover popups

**Anatomy:**
- Map canvas (interactive map rendering)
- Event dots (visual indicators at geographic coordinates)
- Hover popup (brief event information)
- Map controls (zoom, pan, optional layer toggles)

**States:**
- **Default:** Map visible with all event dots
- **Hover:** Popup appears over map dot
- **Selected:** Map dot highlighted, corresponding timeline marker highlighted
- **Loading:** Map skeleton with shimmer effect
- **Error:** Graceful error state with retry option

**Event Dot States:**
- **Default:** Small dot (6-8px), primary color
- **Hover:** Slightly larger (8-10px), color intensifies, popup appears
- **Selected:** Larger (10-12px), accent color, highlighted border
- **Related:** Subtle highlight when viewing related event

**Hover Popup:**
- **Content:** Event title, date, short description (1-2 sentences)
- **Position:** Appears above or beside map dot, doesn't block other dots
- **Appearance:** Smooth fade-in (150ms), clean card design
- **Dismissal:** Disappears smoothly when mouse moves away (150ms)
- **Non-intrusive:** Doesn't block map interaction, can hover other dots

**Behavior:**
- Hover over map dot → popup appears with event info
- Click map dot → highlights corresponding timeline marker, can navigate to Level 3
- Map dots remain visible (no clustering on map - all dots shown)
- Synchronized with timeline: Selecting timeline marker highlights map dot
- Smooth pan/zoom interactions
- Touch-friendly for tablet users

**Visual Style:**
- Map: Neutral base (light gray or muted colors) to not compete with event dots
- Event dots: Primary color, clear and visible
- Hover popup: White background, subtle shadow, matches design system
- Selected state: Accent color for clear indication

---

#### 8. Timeline Navigation Controls

**Purpose:** Provides zoom and navigation controls (minimal, non-intrusive)

**Anatomy:**
- Zoom in button
- Zoom out button
- Optional: Zoom level indicator
- Optional: Scroll position indicator

**States:**
- **Default:** Subtle, semi-transparent, positioned unobtrusively
- **Hover:** Slightly more visible
- **Active:** Button pressed state
- **Disabled:** When at min/max zoom level

**Variants:**
- **Floating:** Floating action buttons, corner position
- **Inline:** Integrated into timeline area
- **Hidden:** Auto-hide when not needed, show on hover

**Behavior:**
- Click to zoom in/out
- Smooth zoom animation
- Visual feedback on interaction
- Keyboard shortcuts support (Ctrl/Cmd +, Ctrl/Cmd -)

**Visual Style:**
- Minimal, icon-only buttons
- Small size, doesn't compete with timeline
- Subtle background, clear icons

---

### Component Patterns

#### Progressive Content Loading Pattern

**Purpose:** Load visual content progressively as users explore

**Implementation:**
- Initial load: Timeline structure and major event markers
- On scroll: Load event details for visible/nearby events
- On event click: Load full visual content (animation/images)
- Lazy loading: Images and animations load on-demand

**Visual Feedback:**
- Skeleton loaders for content areas
- Smooth fade-in when content loads
- No jarring layout shifts

---

#### Smooth Transition Pattern

**Purpose:** All state changes use smooth animations

**Implementation:**
- Timeline scrolling: Smooth momentum scrolling
- Event detail opening: Smooth fade + scale animation
- Period jumping: Smooth zoom + pan animation
- All transitions: 200-300ms duration, ease-out timing

**Visual Feedback:**
- No abrupt changes
- Context maintained during transitions
- Loading states during transitions if needed

---

#### Focus Management Pattern

**Purpose:** Maintain "sparse with focus" - one thing at a time

**Implementation:**
- Event detail view: Full focus, dims timeline background
- Timeline exploration: Full focus, no distractions
- Navigation: Minimal UI, appears when needed

**Visual Feedback:**
- Clear focus indicators
- Dimmed backgrounds when detail view open
- Smooth focus transitions

---

## Responsive Design Considerations

### Desktop (Primary)

**Layout:** Full-width immersive timeline, maximum visual impact

**Interactions:**
- Mouse hover for event markers
- Click for event selection
- Scroll wheel for timeline navigation
- Keyboard shortcuts for power users

**Optimization:**
- High-resolution images and animations
- Smooth 60fps animations
- Rich visual content

### Tablet

**Layout:** Full-width timeline maintained, touch-optimized

**Interactions:**
- Touch gestures for scrolling
- Tap for event selection
- Pinch to zoom
- Swipe for navigation

**Optimization:**
- Touch-friendly target sizes (minimum 44x44px)
- Optimized image sizes for tablet screens
- Smooth touch interactions

### Mobile (Future Consideration)

**Layout:** May require vertical timeline orientation or adapted layout

**Interactions:**
- Touch gestures primary
- Simplified navigation
- Optimized for smaller screens

**Note:** Mobile optimization can be addressed post-MVP if needed.

---

## Admin Panel UX Design

### Overview

The Admin Panel is a comprehensive content management system that enables administrators to manage all timeline content. The UX design maintains consistency with the public-facing application while providing efficient, intuitive content management workflows.

### Design Principles

**Consistency:** Admin panel uses the same design system (shadcn/ui), color theme, and typography as the public-facing application to maintain brand consistency.

**Efficiency:** Optimized for content creation and management tasks - common actions are easily accessible, workflows are streamlined.

**Clarity:** Clear visual hierarchy, intuitive navigation, and helpful feedback for all operations.

**Safety:** Confirmation dialogs for destructive actions, undo/redo support, and clear error messages.

### Layout Structure

**Navigation:**
- Sidebar navigation (desktop) or hamburger menu (tablet)
- Main sections: Dashboard, Timeline, Periods, Events, Media Library, Settings
- Current section highlighted, breadcrumbs for deep navigation

**Content Area:**
- Main workspace for editing and management
- Responsive grid layouts for lists and cards
- Consistent spacing and padding throughout

**Header:**
- Admin user info, logout, notifications
- Quick actions (create new period, create new event)
- Search functionality (if applicable)

### Key Admin Workflows

#### Dashboard Overview
- **Purpose:** Quick overview of timeline statistics and recent activity
- **Layout:** Card-based grid showing:
  - Total periods count
  - Total events count
  - Recent changes/activity feed
  - Quick action buttons (create period, create event, manage media)
- **UX Pattern:** Information-dense but scannable, clear call-to-action buttons

#### Timeline Management
- **Purpose:** View and manage the complete timeline structure
- **Layout:** Timeline visualization similar to public-facing Level 1, with edit controls
- **Interactions:**
  - Click period to edit
  - Drag to reorder (if supported)
  - Add/delete buttons clearly visible
  - 250-year gap visualization maintained
- **UX Pattern:** Visual timeline editor with inline editing capabilities

#### Period Management
- **Purpose:** Create, edit, and manage periods
- **Layout:** List view or timeline view with period cards
- **Interactions:**
  - Create new period button (prominent)
  - Edit period details (name, dates, description)
  - Delete with confirmation dialog
  - Access events within period
- **UX Pattern:** Standard CRUD interface with clear actions

#### Event Management
- **Purpose:** Create, edit, and manage events within periods
- **Layout:** List view of events with filters and search
- **Interactions:**
  - Create new event button
  - Edit event details (comprehensive form)
  - Delete with confirmation
  - Reorder events chronologically
  - Set geographic coordinates (latitude/longitude)
  - Configure visualization methods
- **UX Pattern:** Form-based editing with rich text/markdown editors

#### Rich Text & Markdown Editors
- **Purpose:** Create and edit event content
- **Layout:** Full-width editor with toolbar
- **Features:**
  - WYSIWYG rich text editor with formatting toolbar
  - Markdown editor with syntax highlighting and live preview
  - Toggle between editor modes
  - Support for images, links, structured content
  - Draft saving capability
- **UX Pattern:** Standard editor interface with mode switching

#### Media Library
- **Purpose:** Upload, organize, and manage images and animations
- **Layout:** Grid view of media assets with thumbnails
- **Interactions:**
  - Upload button (drag-and-drop or file picker)
  - Thumbnail grid with metadata
  - Search and filter capabilities
  - Delete unused assets (with usage checks)
  - View usage information (which events use which assets)
- **UX Pattern:** Media library with grid layout, similar to modern CMS interfaces

#### Content Preview
- **Purpose:** Preview how content will appear in the public-facing application
- **Layout:** Preview pane showing Level 1, 2, or 3 views
- **Interactions:**
  - Toggle preview on/off
  - Real-time preview updates as content is edited
  - Switch between preview levels (1, 2, 3)
- **UX Pattern:** Side-by-side or split view with editor and preview

### Admin Panel Components

**Form Components:**
- Input fields with clear labels
- Date pickers for event dates
- Coordinate inputs (latitude/longitude) with map preview
- File upload with drag-and-drop
- Rich text editor toolbar
- Markdown editor with preview

**Feedback Components:**
- Success messages for completed actions
- Error messages with clear guidance
- Confirmation dialogs for destructive actions
- Loading states for async operations
- Toast notifications for quick feedback

**Navigation Components:**
- Sidebar navigation with icons
- Breadcrumbs for deep navigation
- Tab navigation for related content
- Pagination for large lists

### Responsive Design

**Desktop (Primary):**
- Sidebar navigation, full feature set
- Multi-column layouts for efficiency
- Hover states for interactive elements

**Tablet:**
- Collapsible sidebar or hamburger menu
- Touch-optimized interactions
- Simplified layouts where appropriate

**Mobile (Future Consideration):**
- Bottom navigation or hamburger menu
- Simplified workflows
- Essential features only

### Accessibility

- Same WCAG 2.1 Level AA compliance as public-facing application
- Keyboard navigation for all admin functions
- Screen reader support for all content
- Clear focus indicators
- Accessible form labels and error messages

---

## Accessibility Requirements

### Keyboard Navigation

- All interactive elements keyboard accessible
- Timeline navigation via arrow keys
- Event selection via Tab and Enter
- Zoom controls via keyboard shortcuts
- Escape to close modals/detail views
- Map navigation via arrow keys (pan) and +/- (zoom)

### Screen Reader Support

- Semantic HTML structure
- ARIA labels for timeline and events
- ARIA labels for map and map dots
- Alt text for all images and visual content
- Descriptive labels for interactive elements
- Landmark regions for navigation
- Live regions for dynamic content updates (cluster expansion, popup appearance)

### Visual Accessibility

- Color contrast meets WCAG 2.1 Level AA
- Focus indicators clearly visible
- Text readable at all zoom levels
- Visual content doesn't rely solely on color
- Map dots distinguishable by shape/size, not just color
- Cluster markers clearly distinguishable from single event markers

### Motion Preferences

- Respect `prefers-reduced-motion` media query
- Provide option to disable animations
- Essential information not conveyed only through motion
- Reduced motion alternatives for timeline scrolling and transitions

---

## Design Rationale Summary

**Why Full-Width Immersive:**
- Maximizes visual impact for "inspired and amazed" goal
- Timeline is the star, not competing with navigation
- Supports "sparse with focus" principle
- Creates immersive exploration experience

**Why Modern Discovery Colors:**
- Vibrant blue creates energy and discovery feeling
- Purple adds sophistication and depth
- Amber provides warmth and highlights key moments
- Maintains minimal, clean aesthetic while being inspiring

**Why shadcn/ui:**
- Provides accessible primitives without visual constraints
- Full customization for unique timeline visualization
- Modern, clean components that can be styled to match
- Excellent accessibility built-in

**Why Smooth, Fluid Interactions:**
- Supports "smooth and fluid" interaction style requirement
- Creates delightful user experience
- Maintains focus during exploration
- Feels effortless and natural

---

## UX Pattern Decisions

### Dual View Synchronization Pattern

**Pattern:** Timeline and map views are synchronized - selecting an event in one view highlights it in the other view.

**Implementation:**
- Click timeline marker → corresponding map dot highlights
- Click map dot → corresponding timeline marker highlights
- Hover timeline marker → subtle highlight on map dot (optional)
- Hover map dot → popup appears, timeline marker subtly highlights (optional)

**Visual Feedback:**
- Selected state: Accent color highlight, slightly larger size
- Synchronized highlight: Smooth transition (200ms), clear visual indication
- Hover state: Subtle highlight, tooltip/popup appears

**Rationale:** Synchronized interaction helps users understand the relationship between temporal (timeline) and spatial (map) information, creating a cohesive exploration experience.

---

### Cluster Marker Expansion Pattern

**Pattern:** Simultaneous events are grouped into cluster markers that expand on interaction.

**Implementation:**
- Cluster marker shows event count (e.g., "3")
- Click/tap cluster marker → smooth expansion animation
- Expanded events displayed inline or in list format
- All event dots remain visible on map (no clustering on map)
- Click expanded event → navigate to Level 3 Event Details

**Visual Feedback:**
- Cluster marker: Larger than single event marker, accent color
- Expansion animation: 200-300ms smooth reveal
- Expanded events: Same styling as regular events, slightly indented
- Collapse: Smooth animation back to cluster state

**Rationale:** Cluster markers prevent timeline clutter while maintaining all event information accessible. Map shows all dots simultaneously to preserve geographic context.

---

### Map Hover Popup Pattern

**Pattern:** Hovering over map dots shows brief event information in a non-intrusive popup.

**Implementation:**
- Hover over map dot → popup appears with event title, date, short description
- Popup appears above or beside dot (doesn't block other dots)
- Smooth fade-in animation (150ms)
- Popup disappears when mouse moves away (150ms)
- Clicking map dot navigates to Level 3 (popup doesn't interfere)

**Visual Feedback:**
- Popup: White background, subtle shadow, matches design system
- Map dot: Slightly enlarges on hover, color intensifies
- Smooth animations for appearance/disappearance

**Rationale:** Hover popups provide quick information without requiring navigation, maintaining the "sparse with focus" principle while offering contextual information on demand.

---

### Responsive Dual View Pattern

**Pattern:** Dual view layout adapts to screen size - side-by-side on desktop/tablet, stacked on mobile.

**Implementation:**
- **Desktop:** Timeline left (50-60% width), Map right (40-50% width)
- **Tablet:** Side-by-side maintained, touch-optimized interactions
- **Mobile:** Stacked vertically (timeline top, map bottom)

**Breakpoints:**
- Desktop: > 1024px (side-by-side)
- Tablet: 768px - 1024px (side-by-side, touch-optimized)
- Mobile: < 768px (stacked)

**Visual Feedback:**
- Smooth transition when breakpoint changes
- Both views remain fully functional in all layouts
- Navigation between views maintained

**Rationale:** Responsive adaptation ensures the dual view experience works across all devices while maintaining usability and visual clarity.

---

## Next Steps

This UX design specification provides the foundation for:

1. **Architecture Design:** Technical decisions to support the UX patterns, especially dual view synchronization and map integration
2. **Epic & Story Creation:** Break down UX requirements into implementable stories, including Level 2 dual view, cluster markers, and admin panel
3. **Implementation:** Build the timeline visualization, dual view layout, interactive map, cluster markers, and admin panel components

The design emphasizes:
- **Visual-first approach** - Content is king
- **Minimal, clean interface** - UI supports, doesn't compete
- **Smooth, fluid interactions** - Effortless exploration
- **Sparse information density** - Focus on one thing at a time
- **Inspiring experience** - Users feel amazed and engaged
- **Dual view context** - Temporal and spatial information together
- **Efficient content management** - Admin panel enables easy content creation

---

_Created through collaborative UX design process between BMad and AI facilitator._

_This specification guides the technical architecture and implementation of notism's user experience._

---

