# UI Generation Prompt for notism

Use this prompt to generate UI components, layouts, and designs based on the notism UX Design Specification.

---

## Project Context

**Project:** notism - Human Development Timeline Visualization  
**Goal:** Create a web application that visualizes the complete human development timeline from ancient apes to the present day through rich visual storytelling.

**Core Experience:** Explore the timeline chronologically through visual-first content  
**Target Emotion:** Users should feel inspired and amazed  
**Design Philosophy:** Visual-first, minimal interface, sparse information density, smooth fluid interactions

---

## Design System Foundation

### Color Palette (Modern Discovery Theme)

**Primary Colors:**
- Primary: `#2563EB` (Vibrant blue) - Main actions, key timeline elements, primary navigation
- Secondary: `#7C3AED` (Rich purple) - Supporting actions, secondary elements
- Accent: `#F59E0B` (Amber) - Highlights, call-to-action elements, important markers

**Semantic Colors:**
- Success: `#10B981` (Green) - Positive feedback, completed states
- Warning: `#F59E0B` (Amber) - Warnings, attention needed
- Error: `#EF4444` (Red) - Errors, destructive actions

**Neutral Colors:**
- Background: `#FFFFFF` (White) - Main background
- Surface: `#F9FAFB` (Light gray) - Cards, elevated surfaces
- Text Primary: `#111827` (Near black) - Main text, headings
- Text Secondary: `#6B7280` (Medium gray) - Secondary text, descriptions
- Border: `#E5E7EB` (Light gray) - Borders, dividers

**Theme Personality:** Vibrant, contemporary, energetic - supports "inspired and amazed" while maintaining clean minimalism.

### Typography

**Font Stack:**
- System font: `-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif`
- Monospace: `'SF Mono', Monaco, 'Cascadia Code', 'Roboto Mono', Consolas, monospace` (for dates)

**Type Scale:**
- H1: 2.5rem (40px) / 1.2 line-height
- H2: 2rem (32px) / 1.3 line-height
- H3: 1.5rem (24px) / 1.4 line-height
- H4: 1.25rem (20px) / 1.4 line-height
- H5: 1.125rem (18px) / 1.5 line-height
- H6: 1rem (16px) / 1.5 line-height
- Body: 1rem (16px) / 1.6 line-height
- Small: 0.875rem (14px) / 1.5 line-height
- Tiny: 0.75rem (12px) / 1.4 line-height

**Font Weights:**
- Regular (400): Body text
- Medium (500): Buttons, emphasis
- Semibold (600): Headings
- Bold (700): Strong emphasis

### Spacing System

**Base Unit:** 4px (all spacing multiples of 4)

**Spacing Scale:**
- xs: 4px (0.25rem)
- sm: 8px (0.5rem)
- md: 16px (1rem)
- lg: 24px (1.5rem)
- xl: 32px (2rem)
- 2xl: 48px (3rem)
- 3xl: 64px (4rem)
- 4xl: 96px (6rem)

**Layout Grid:**
- Desktop: 12-column grid, 24px gutters
- Tablet: 8-column grid, 16px gutters
- Mobile: 4-column grid, 16px gutters

**Container Widths:**
- Desktop: Max-width 1280px, centered
- Tablet: Max-width 1024px
- Mobile: Full width, 16px side padding

---

## Design Direction

**Selected Direction:** Full-Width Immersive

**Key Characteristics:**
- Timeline dominates the screen, maximum visual impact
- Spacious layout with generous whitespace
- Content-focused design - UI elements support, not compete
- Immersive experience that draws users into exploration
- No sidebar - timeline takes center stage

---

## Component Specifications

### 1. Timeline Component (Level 1 - Overview)

**Purpose:** Core visualization displaying the complete human development timeline

**Visual Structure:**
- Timeline line/axis: 2-4px stroke, gradient from primary (#2563EB) to secondary (#7C3AED) color
- Event markers: Circular, 8-12px diameter, accent color (#F59E0B) with white border
- Period sections: Visually distinct sections with dynamic gaps based on event relevance
- Current position indicator: Shows user's position on timeline
- Navigation controls: Minimal zoom controls (floating, subtle)

**Layout:**
- Full-width, no sidebar
- Generous spacing between events (sparse information density)
- Smooth gradient background or clean white background

**Interactions:**
- Smooth scrolling/panning with momentum
- Hover event marker → tooltip with event name and date
- Click event marker → smooth transition to event detail view
- Zoom in/out with pinch or controls
- All transitions: 200-300ms duration, ease-out timing

**States:**
- Default: Timeline visible with event markers
- Hover: Event markers highlight, show tooltip
- Active/Selected: Selected event marker emphasized
- Loading: Skeleton with shimmer effect

---

### 2. Period Details Dual View Component (Level 2)

**Purpose:** Display timeline and map side-by-side showing all events in a period

**Layout:**
- **Desktop:** Side-by-side (Timeline left 50-60%, Map right 40-50%)
- **Tablet:** Side-by-side maintained, touch-optimized
- **Mobile:** Stacked vertically (Timeline top, Map bottom)

**Left Panel - Timeline:**
- Linear chronological display of events
- Cluster markers for simultaneous events (showing count, e.g., "3")
- Event markers: Same styling as Level 1
- Smooth scrolling, independent from map

**Right Panel - Interactive Map:**
- Map with neutral base colors (light gray/muted) to not compete with event dots
- Event dots: 6-8px diameter, primary color (#2563EB)
- All event dots visible (no clustering on map)
- Hover popup: White background, subtle shadow, shows event title, date, short description
- Popup appears above/beside dot, doesn't block other dots
- Smooth fade-in/out (150ms)

**Synchronization:**
- Click timeline marker → corresponding map dot highlights (accent color, larger size)
- Click map dot → corresponding timeline marker highlights
- Hover states can optionally sync (subtle highlight)

**Cluster Expansion:**
- Cluster marker shows count (e.g., "3")
- Click cluster → smooth expansion (200-300ms) revealing all simultaneous events inline
- Expanded events maintain chronological order
- All event dots remain visible on map during expansion

**Visual Style:**
- Clean divider between panels (subtle border or shadow)
- Consistent spacing and padding
- Smooth transitions for all interactions

---

### 3. Event Detail View Component (Level 3)

**Purpose:** Display rich visual content for a single historical event

**Layout:**
- Maximum space for visual content (animation or image)
- Text overlay minimal and non-intrusive
- Event title and date prominently displayed
- Sparse descriptive content (maintaining "sparse with focus")

**Content Structure:**
- Visual content area: Primary focus, large display
- Event title: H3 or H4, primary text color
- Date: Small text, secondary color, monospace font
- Description: Body text, secondary color, minimal length
- Navigation: Close button (subtle, top-right), next/previous arrows (subtle, sides)

**Interactions:**
- Smooth entrance animation when opening (fade + scale)
- Smooth exit animation when closing
- Progressive content loading (image/animation first, then text)
- Keyboard navigation: ESC to close, arrow keys for next/previous

**States:**
- Loading: Skeleton loader for visual content
- Loaded: Content visible with smooth entrance animation
- Error: Graceful error state with retry option

---

### 4. Event Marker Component

**Visual Design:**
- Default: Small circular marker (8-12px), accent color (#F59E0B), white border
- Hover: Slightly larger, color intensifies, label appears
- Active: Emphasized, larger size, accent color
- Related: Subtle highlight when viewing related event

**Variants:**
- Major Event: Larger marker (12-16px), more prominent
- Minor Event: Smaller marker (6-8px), subtle
- Period Marker: Different style to indicate period boundaries
- Cluster Marker: Larger (12-16px), shows count, accent color

**Interactions:**
- Hover → tooltip with event name and date
- Click → smooth transition to event detail view
- Visual feedback on all interactions

---

### 5. Cluster Marker Component

**Purpose:** Groups simultaneous events (same date/year) on timeline

**Visual Design:**
- Cluster marker: 12-16px diameter, accent color (#F59E0B), white border
- Count indicator: Bold, readable number or icon
- Expanded events: Same styling as regular event markers, slightly indented

**States:**
- Collapsed: Cluster marker visible with count (e.g., "3")
- Expanded: All simultaneous events displayed inline or in list
- Hover: Cluster marker highlights, shows preview
- Active: Expansion animation in progress

**Interactions:**
- Click/tap to expand/collapse
- Smooth expansion animation (200-300ms)
- Expanded events maintain chronological order
- Clicking expanded event navigates to Level 3

---

### 6. Interactive Map Component

**Purpose:** Display event dots at geographic locations with hover popups

**Visual Design:**
- Map: Neutral base (light gray or muted colors) to not compete with event dots
- Event dots:
  - Default: 6-8px, primary color (#2563EB)
  - Hover: 8-10px, color intensifies
  - Selected: 10-12px, accent color (#F59E0B), highlighted border

**Hover Popup:**
- Content: Event title, date, short description (1-2 sentences)
- Position: Above or beside map dot, doesn't block other dots
- Appearance: White background, subtle shadow, matches design system
- Animation: Smooth fade-in (150ms), fade-out (150ms)
- Non-intrusive: Doesn't block map interaction

**Interactions:**
- Hover over map dot → popup appears with event info
- Click map dot → highlights corresponding timeline marker, can navigate to Level 3
- Map dots remain visible (no clustering on map)
- Synchronized with timeline selection
- Smooth pan/zoom interactions
- Touch-friendly for tablet users

---

### 7. Period Selector Component

**Purpose:** Allow users to jump to specific historical periods

**Visual Design:**
- Collapsed: Minimal trigger button or icon (subtle, non-intrusive)
- Expanded: Clean list design with period names
- Current period: Clearly indicated (highlighted or different color)

**Interactions:**
- Click to expand/collapse
- Select period → smooth zoom/transition to period on timeline
- Keyboard navigation support
- Search/filter if many periods

**Variants:**
- Dropdown: Traditional dropdown menu
- Modal: Full-screen or overlay modal
- Floating: Floating action button with period list

---

### 8. Navigation Controls

**Purpose:** Zoom and navigation controls (minimal, non-intrusive)

**Visual Design:**
- Minimal, icon-only buttons
- Small size, doesn't compete with timeline
- Subtle background, clear icons
- Floating position (corner) or inline

**Interactions:**
- Click to zoom in/out
- Smooth zoom animation
- Visual feedback on interaction
- Keyboard shortcuts support (Ctrl/Cmd +, Ctrl/Cmd -)

**States:**
- Default: Subtle, semi-transparent
- Hover: Slightly more visible
- Disabled: When at min/max zoom level

---

## Interaction Patterns

### Smooth Transitions
- All state changes use smooth animations
- Timeline scrolling: Smooth momentum scrolling
- Event detail opening: Smooth fade + scale animation (200-300ms)
- Period jumping: Smooth zoom + pan animation
- All transitions: 200-300ms duration, ease-out timing

### Progressive Content Loading
- Initial load: Timeline structure and major event markers
- On scroll: Load event details for visible/nearby events
- On event click: Load full visual content (animation/images)
- Lazy loading: Images and animations load on-demand
- Visual feedback: Skeleton loaders, smooth fade-in, no layout shifts

### Focus Management
- Event detail view: Full focus, dims timeline background
- Timeline exploration: Full focus, no distractions
- Navigation: Minimal UI, appears when needed
- Clear focus indicators, smooth focus transitions

### Synchronized Views (Level 2)
- Timeline and map views synchronized
- Selecting event in one view highlights it in the other
- Smooth transition (200ms) for synchronized highlights
- Clear visual indication of selected state

---

## Responsive Design

### Desktop (Primary)
- Full-width immersive timeline
- Side-by-side dual view (Level 2)
- Mouse hover interactions
- High-resolution images and animations
- Smooth 60fps animations

### Tablet
- Full-width timeline maintained
- Side-by-side dual view (touch-optimized)
- Touch gestures for scrolling
- Touch-friendly target sizes (minimum 44x44px)
- Optimized image sizes

### Mobile
- Stacked dual view (timeline top, map bottom)
- Touch gestures primary
- Simplified navigation
- Vertical timeline orientation (if needed)

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
- ARIA labels for timeline, events, map, and map dots
- Alt text for all images and visual content
- Descriptive labels for interactive elements
- Landmark regions for navigation
- Live regions for dynamic content updates

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

## Design Principles to Follow

1. **Visual-First Approach:** Content is king - visual content takes priority over text
2. **Minimal, Clean Interface:** UI supports, doesn't compete with historical content
3. **Smooth, Fluid Interactions:** All interactions feel effortless and natural
4. **Sparse Information Density:** Focus on one thing at a time, generous whitespace
5. **Inspiring Experience:** Users should feel amazed and engaged
6. **Dual View Context:** Temporal (timeline) and spatial (map) information together
7. **Progressive Disclosure:** Information revealed as needed, not all at once

---

## Technical Stack Recommendations

**Design System:** shadcn/ui (Tailwind-based, customizable)  
**Why:** Provides accessible primitives without visual constraints, full customization for unique timeline visualization

**Base Components Available:**
- Buttons (primary, secondary, outline, ghost variants)
- Cards (for event content display)
- Inputs (for search/jump functionality)
- Dialogs/Modals (for event detail views)
- Tooltips (for event markers on hover)
- Icons (from Lucide icon library)

**Customization:** All components can be fully customized to match the Modern Discovery color theme and minimal aesthetic.

---

## Usage Instructions

When generating UI components or layouts:

1. **Reference the color palette** - Use exact hex codes provided
2. **Follow spacing system** - Use 4px base unit multiples
3. **Apply typography scale** - Use specified font sizes and weights
4. **Maintain design direction** - Full-width immersive, content-focused
5. **Implement interactions** - Smooth transitions, progressive loading
6. **Ensure accessibility** - Keyboard navigation, screen reader support, proper contrast
7. **Consider responsive** - Desktop-first, adapt for tablet and mobile
8. **Follow principles** - Visual-first, minimal, sparse, inspiring

**For specific components:**
- Reference the detailed component specifications above
- Include all states (default, hover, active, loading, error)
- Implement smooth transitions and animations
- Ensure proper accessibility attributes
- Maintain visual consistency with design system

---

## Example Generation Requests

**Timeline Component:**
"Generate a full-width timeline component with gradient line, event markers, and smooth scrolling. Use Modern Discovery color theme, sparse spacing, and include hover/click interactions."

**Dual View Layout:**
"Create a side-by-side layout with timeline on left (60%) and interactive map on right (40%). Include synchronized selection, cluster markers on timeline, and hover popups on map dots."

**Event Detail View:**
"Design an event detail view with maximum space for visual content, minimal text overlay, and smooth entrance/exit animations. Include navigation controls and keyboard support."

---

_This prompt is based on the notism UX Design Specification v2.0 and should be used to generate UI components that align with the specified design system, interactions, and user experience goals._

