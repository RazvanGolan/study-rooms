---
name: Study Rooms
description: The day as one strict typographic grid. Ink on paper, one signal red for now, yours, selection and the primary action.
colors:
  paper: "#f4f4f2"
  paper-2: "#ebebe8"
  ink: "#141414"
  ink-2: "#2b2b29"
  muted: "#5f5f5b"
  rule: "#d9d9d5"
  rule-2: "#bdbdb8"
  red: "#e2231a"
  red-ink: "#c41c14"
  red-wash: "rgba(226, 35, 26, 0.1)"
  on-red: "#ffffff"
  private: "#c4c4bf"
  on-ink: "#f4f4f2"
  past: "rgba(20, 20, 20, 0.045)"
  paper-dark: "#121212"
  paper-2-dark: "#1c1c1b"
  ink-dark: "#efefec"
  ink-2-dark: "#d6d6d2"
  muted-dark: "#a3a39d"
  rule-dark: "#2c2c2a"
  rule-2-dark: "#454542"
  red-dark: "#ff4a3d"
  red-ink-dark: "#ff6a5f"
  red-wash-dark: "rgba(255, 74, 61, 0.14)"
  on-red-dark: "#121212"
  private-dark: "#4a4a47"
  on-ink-dark: "#121212"
  past-dark: "rgba(255, 255, 255, 0.035)"
typography:
  display:
    fontFamily: "'Archivo Variable', 'Archivo', system-ui, sans-serif"
    fontSize: "clamp(64px, 8.4vw, 112px)"
    fontWeight: 800
    lineHeight: 0.82
    letterSpacing: "-0.035em"
    fontVariation: "'wdth' 75"
    fontFeature: "'tnum'"
  hour-numeral:
    fontFamily: "'Archivo Variable', 'Archivo', system-ui, sans-serif"
    fontSize: "clamp(30px, 3.2vw, 48px)"
    fontWeight: 800
    lineHeight: 1
    letterSpacing: "-0.02em"
    fontVariation: "'wdth' 72"
    fontFeature: "'tnum'"
  headline:
    fontFamily: "'Archivo Variable', 'Archivo', system-ui, sans-serif"
    fontSize: "48px"
    fontWeight: 800
    lineHeight: 0.95
    letterSpacing: "-0.03em"
    fontVariation: "'wdth' 75"
    fontFeature: "'tnum'"
  title:
    fontFamily: "'Archivo Variable', 'Archivo', system-ui, sans-serif"
    fontSize: "26px"
    fontWeight: 800
    lineHeight: 1.05
    letterSpacing: "-0.01em"
  body:
    fontFamily: "'Archivo Variable', 'Archivo', system-ui, sans-serif"
    fontSize: "16px"
    fontWeight: 400
    lineHeight: 1.45
    fontFeature: "'tnum'"
  button:
    fontFamily: "'Archivo Variable', 'Archivo', system-ui, sans-serif"
    fontSize: "15px"
    fontWeight: 650
    lineHeight: 1.45
  label:
    fontFamily: "'Archivo Variable', 'Archivo', system-ui, sans-serif"
    fontSize: "12px"
    fontWeight: 700
    lineHeight: 1.45
    letterSpacing: "0.05em"
rounded:
  none: "0px"
  mark: "50%"
spacing:
  gutter: "clamp(16px, 3vw, 40px)"
  xs: "6px"
  sm: "10px"
  md: "18px"
  lg: "24px"
  xl: "40px"
  plate: "212px"
  plate-phone: "104px"
components:
  button-primary:
    backgroundColor: "{colors.red}"
    textColor: "{colors.on-red}"
    typography: "{typography.button}"
    rounded: "{rounded.none}"
    padding: "0 18px"
    height: "44px"
  button-primary-hover:
    backgroundColor: "{colors.red-ink}"
    textColor: "{colors.on-red}"
  button-outline:
    backgroundColor: "transparent"
    textColor: "{colors.ink}"
    typography: "{typography.button}"
    rounded: "{rounded.none}"
    padding: "0 18px"
    height: "44px"
  button-outline-hover:
    backgroundColor: "{colors.ink}"
    textColor: "{colors.on-ink}"
  button-solid:
    backgroundColor: "{colors.ink}"
    textColor: "{colors.on-ink}"
    rounded: "{rounded.none}"
    padding: "0 18px"
    height: "44px"
  button-solid-hover:
    backgroundColor: "{colors.ink-2}"
  button-danger:
    backgroundColor: "transparent"
    textColor: "{colors.red-ink}"
    rounded: "{rounded.none}"
  button-danger-hover:
    backgroundColor: "{colors.red-ink}"
    textColor: "{colors.on-red}"
  button-small:
    padding: "0 12px"
    height: "34px"
  input:
    backgroundColor: "{colors.paper-2}"
    textColor: "{colors.ink}"
    typography: "{typography.body}"
    rounded: "{rounded.none}"
    padding: "0 12px"
    height: "44px"
  segmented-option:
    backgroundColor: "transparent"
    textColor: "{colors.ink}"
    padding: "0 14px"
    height: "40px"
  segmented-option-checked:
    backgroundColor: "{colors.ink}"
    textColor: "{colors.on-ink}"
  day-button-pressed:
    backgroundColor: "{colors.ink}"
    textColor: "{colors.on-ink}"
    padding: "6px 10px"
  block-open:
    backgroundColor: "{colors.ink}"
    textColor: "{colors.on-ink}"
    padding: "9px 11px 26px"
  block-request:
    backgroundColor: "{colors.paper}"
    textColor: "{colors.ink}"
    padding: "9px 11px 26px"
  block-private:
    backgroundColor: "{colors.private}"
    textColor: "{colors.ink}"
    padding: "9px 11px 26px"
  block-mine:
    backgroundColor: "{colors.red}"
    textColor: "{colors.on-red}"
    padding: "9px 11px 26px"
  block-requested:
    backgroundColor: "{colors.paper}"
    textColor: "{colors.red-ink}"
    padding: "9px 11px 26px"
  selection-range:
    backgroundColor: "{colors.red-wash}"
    textColor: "{colors.red-ink}"
    padding: "8px 10px"
  booking-bar:
    backgroundColor: "{colors.ink}"
    textColor: "{colors.on-ink}"
    padding: "14px clamp(16px, 3vw, 40px)"
  side-panel:
    backgroundColor: "{colors.paper}"
    textColor: "{colors.ink}"
    width: "min(460px, 100vw)"
  toast:
    backgroundColor: "{colors.ink}"
    textColor: "{colors.on-ink}"
    padding: "8px 6px 8px 14px"
  status-tag:
    backgroundColor: "transparent"
    textColor: "{colors.ink}"
    typography: "{typography.label}"
    padding: "1px 7px"
---

# Design System: Study Rooms

## Overview

**Creative North Star: "The Swiss Timetable"**

Study Rooms is a departure board set in ink on paper. The day is one strict typographic grid: hour numerals at poster scale across the top, room name plates fixed down the left, 24 half hour columns between them, and bookings laid in as flat rectangles of ink or red. Structure comes from type and from rules of two weights, not from boxes, cards, radii or shadows. The red line of now cuts through the grid and is the only thing on the screen that moves on its own.

The world is dense and legible at the same time. Big condensed figures (weekday and date, hours, times) carry the reading at a distance, which is what makes the grid work on a classroom projector, while the UI text around them stays at normal width and a calm 15 to 16px. Every booking state is said once, by its fill, and repeated in a legend. One signal red is held back for the things that are about the person looking: now, yours, what you asked for, what you are about to book, and the action that books it.

The dark theme is a straight inversion of paper and ink with a lighter red. Nothing else changes between themes.

Key characteristics:
- Off white paper ground, ink black type and fills, one signal red.
- Archivo variable on its width axis: condensed (70% to 85%) for display and figures, normal width for interface text, tabular figures everywhere.
- Square corners throughout; the only round forms are seat marks and the spinner.
- Flat at rest; a shadow exists only under overlays (side panel, toasts).
- State by fill pattern: solid, outlined, grey, hatched, red, red striped.
- Motion is limited to overlays arriving on one exponential ease out.

## Colors

A near monochrome paper and ink palette with a single saturated red used as a signal, never as decoration.

### Primary
- Signal Red (red): the now rule and its time tag, bookings you are in, the active tab underline, the primary button, the focus ring, text selection, the caret and the red parts of the login poster. Its hover and text weight partner is Deep Signal Red (red-ink), used for red text on paper (the Today tag, the "you asked" block label, cancelled status tags), the error note border and the danger button.
- Red Wash (red-wash): the translucent tint behind the selection range, under the "you asked" stripes, behind error notes and on free cell hover.

### Neutral
- Paper Ground (paper): the page, the top bar, the side panel, sticky name plates and the inside of outlined blocks.
- Shaded Paper (paper-2): input wells, icon button and day button hover, inline admin forms, request rows and the loading sweep.
- Ink (ink): all primary text, open and full booking blocks, 2px structural rules, the booking bar, toasts and pressed or checked controls.
- Soft Ink (ink-2): secondary text that must stay strong (building names, panel subtitles, page intros) and the hover of solid ink surfaces.
- Muted Grey (muted): metadata, uppercase labels, counts, placeholders and cancelled rows.
- Hairline (rule): 1px row dividers and half hour column lines.
- Strong Hairline (rule-2): the on the hour column lines inside lanes, tab hover underline, sample account chips and scrollbars.
- Private Grey (private): the fill of private bookings, always paired with a legend entry and a label in the panel.
- Past Shade (past): the faint tint over half hour cells that are already over.

### Named Rules

**The Red Means You Rule.** Red marks only now, yours, your request, your selection, the one primary action on a surface, focus, the current tab and errors. If a red element is not about the viewer or the present moment, it is wrong.

**The State by Fill Rule.** A booking's state is carried by its fill pattern, not by color alone: solid ink open, ink outline on request, grey private, diagonal hatch blocked, solid red yours, solid red outline with red stripes for a request you made. Full bookings stay solid ink and say "full" in words. Grey is never the only signal; private is always named in the legend and the panel.

**The Straight Inversion Rule.** Dark mode swaps paper and ink, lifts red to the dark red tokens and keeps every rule, fill pattern and layout identical. Text on red becomes dark paper in dark mode.

## Typography

Display Font: Archivo Variable with the wdth axis, self hosted through @fontsource-variable/archivo (with Archivo, system-ui, sans-serif)
Body Font: the same family at normal width
Icons: lucide-react line icons at 16 to 18px

Character: one grotesque in two voices. Condensed heavy figures read like a station timetable; the same face at normal width reads like plain labelling.

### Hierarchy
- Display (800, 75% width, clamp(64px, 8.4vw, 112px), line height 0.82, tracking -0.035em): the weekday and date on the timetable. Page titles use the same voice at clamp(48px, 6.4vw, 88px), and the login poster goes to 70% width at clamp(72px, 11vw, 168px).
- Hour numeral (800, 72% width, clamp(30px, 3.2vw, 48px), line height 1): the 08 to 19 hours across the top of the grid, each set against a 2px ink rule on its left.
- Headline (800, 75% width, 48px, line height 0.95): the time in the side panel; the same voice sets the date figure in list entries (46px), the room and time in the booking bar (clamp(26px, 3vw, 38px) at 78%) and entry times (24px at 85%).
- Title (800, normal width, 22px to 34px): room name plates (26px), the side panel heading (34px), the login heading (34px), group titles and admin form headings (22px).
- Body (400, 16px, line height 1.45): running text and inputs; secondary text drops to 15px or 14px. Block text is 13.5px with 15px weight 750 names.
- Button (650, 15px): button and tab text; small buttons drop to 14px.
- Label (700, 12px, tracking 0.05em, uppercase): field labels (13px, 650), fact terms in the panel, list section headings, table headers, status tags, the admin role tag and the equipment line on plates.

### Named Rules

**The Two Widths Rule.** Condensed widths (70% to 85%) belong to display type and to figures: dates, hours, times, capacities. Interface text, names and labels stay at normal width.

**The Tabular Figures Rule.** Figures are tabular everywhere (set on the root), so times and counts line up down columns and do not jitter as the clock moves.

## Layout

The timetable is a two column grid: a sticky name plate column (212px, 104px on phones) and a lane that divides 8:00 to 20:00 into 24 equal half hour columns. Hour headers sit in 12 columns with a 2px ink rule on the left of each; inside lanes the on the hour lines are 2px strong hairline and the half hour lines 1px hairline. Lanes are 108px tall (96px on phones) and bookings sit 10px in from top and bottom, 2px in from their start line and 4px short of their end.

The hour row reserves a 30px strip under the numerals. The now rule starts at the hours baseline and runs down through every lane; its time tag sits in that reserved strip on the past side of the rule, so it never covers a numeral or a booking.

Pages use one fluid side gutter (clamp(16px, 3vw, 40px)) for the top bar, page, booking bar and toast inset, with 28px above content and 120px below so the bar never hides the last row. Spacing runs on a loose rhythm of 6, 10, 18, 24 and 40px: tight gaps inside controls, 18px between fields and list rows, 40px between groups. Lists (my bookings, admin) are rows divided by hairlines under a 2px ink group rule, not cards.

Responsive behavior: at 1100px the admin side column stacks and list entries collapse to two columns. At 1080px and below the timetable turns on its side: hours run down a 48px gutter as 26px condensed numerals with a 2px ink rule at each hour, every half hour is a 34px row, and each room is a column under a header of room plates that stays at the top while the page scrolls. As many columns fit as the width allows at 96px or more each; when there are more rooms, two arrow buttons in the header corner move the view one room at a time, so the page never scrolls sideways. The now rule runs across all columns with its time tag in the gutter. Above 1080px the horizontal grid has a 860px minimum width and never needs to scroll. At 760px the tabs wrap under the brand, the top bar stops being sticky so the room header can take the top edge, and the day stepper becomes a fixed ten column grid that fits the whole week. Selecting a range scrolls the page so the range sits between the room header and the booking bar, and the page gains 300px of bottom room so the bar never hides the selection. The side panel becomes a bottom sheet up to 88vh tall.

## Elevation & Depth

The system is flat. Depth at rest comes from tone (paper against shaded paper, ink against paper) and from rules, never from shadow. Only surfaces that float over the grid cast a shadow: the side panel and toasts share one soft overlay shadow, and the panel also has a translucent scrim behind it. The booking bar is solid ink pinned to the bottom edge and needs no shadow to separate.

### Shadow Vocabulary
- Overlay (`box-shadow: 0 18px 48px rgba(20, 20, 20, 0.18), 0 2px 6px rgba(20, 20, 20, 0.08)`; dark: `0 18px 48px rgba(0, 0, 0, 0.6), 0 2px 6px rgba(0, 0, 0, 0.4)`): the side panel and toasts only.

### Named Rules

**The Flat Ink Rule.** Nothing on the page lifts. Hover on a booking block is a 2px ink outline offset by 2px, an open block gets a 3px red outline, and neither moves or casts a shadow. Shadows belong to overlays alone.

## Shapes

Every corner is square (0px): buttons, inputs, blocks, tags, panels, toasts and the day stepper. Form comes from rules and fills: 2px ink borders on buttons and segmented controls, 1.5px on small buttons, day buttons and tags, a 2px ink bottom border on inputs, 2.5px inset outlines on outlined blocks. The only circles are the seat marks (9px, 8px inside blocks) and the loading spinner. Hatching is a -45deg repeating stripe of 2px ink every 8px; the request stripe is a vertical red wash band of 6px every 12px. The select arrow is drawn from two gradient triangles, not an icon.

## Components

### Buttons
Square, ruled and blunt. Every button is 44px tall with 0 18px padding, weight 650 at 15px, and a 2px border. The default is an ink outline that fills with ink on hover. Primary is solid red with a darker red on hover and is used once per surface (Book room, Log in, the panel's main action); in the booking bar it grows to 48px with 26px padding. Solid ink is the secondary filled action. Danger is a red ink outline that fills red on hover. Small buttons are 34px with a 1.5px border. Icon buttons are borderless 40px squares with a shaded paper hover. Disabled drops to 45% opacity. Text links are weight 650 and underlined with a 3px offset. Transitions are 0.15s on color only.

### Segmented control
A 2px ink frame with 1.5px dividers and 40px options; the checked option fills with ink. Inside the booking bar the colors invert through two local variables so the same control reads on ink. Used for the join policy and list filters.

### Filter row

Between the day header and the timetable, above a 1px rule: Building (All plus one button per building, one choice, shown only when there are two or more buildings) and Needs (one toggle per piece of equipment, any number). The buttons are square 34px toggles with a 1.5px ink border; a pressed one is filled with ink. Labels are 12px uppercase muted. When a filter is on, "N of M rooms" and a Show all link sit at the right. The choice is kept in the browser.

### Inputs / Fields
A shaded paper well with a 2px ink bottom border and square corners, 44px tall. Focus turns the bottom border red and doubles it with a 1px inset red line; there is no glow. The caret is red. Labels sit above in 13px uppercase. In the booking bar inputs and selects go transparent with a paper colored bottom border.

### Navigation
The top bar is 60px, sticky, on paper with a 2px ink rule under it. The brand is 19px weight 800. Tabs are 15px weight 550 with a 4px underline that sits on the bar rule: strong hairline on hover, red with weight 700 on the current page. The user name is muted, admins get a small uppercase outlined role tag, and log out is an icon button. The day stepper is a row of 1.5px outlined day buttons (weekday over a 17px weight 800 date) with the pressed day filled in ink, flanked by chevron buttons.

### Booking blocks (signature)
Flat rectangles laid into the lane, 13.5px text with the owner's name at 15px weight 750, the time range below and a row of seat marks pinned to the bottom left. Fills follow the State by Fill Rule: solid ink for open and full, a 2.5px inset ink outline on paper for on request (with "ask to join" in the text), private grey, a hatched block with the label set on paper patches for admin blocks, solid red for bookings you are in (your own name reads "You"), and for a request you made a 2.5px inset solid red outline with vertical red wash stripes and red ink text. Hover is a 2px ink outline offset 2px; the block whose panel is open carries a 3px red outline. The legend repeats every fill at 22 by 12px.

### Selection range
Picking free cells draws a 2.5px red dashed outline over a red wash, with "New booking" and the time range in red ink. Dashed red is reserved for this unsaved range and is never used for a stored booking. Free cells show a red wash on hover and a crosshair cursor; past cells are shaded and refuse the pointer.

### Now rule
A 3px red vertical rule positioned at the current time, starting at the hours baseline and running through all lanes. Its time tag is a small red label (12px, weight 750, paper text) sitting in the strip under the hour numerals, aligned to the past side of the rule.

### Seat marks
One 9px circle per place, 1.5px outline in the current color, filled for each person in the booking, red when a room now holds more people than its capacity. Above 16 places the rest are summarised as a count. Labelled with places left outside blocks.

### Booking bar
A full width ink bar fixed to the bottom that rises in over 0.42s when a range is selected. It carries the room and time in large condensed figures, a line with day, duration and capacity, the start and end selects, the inverted policy control, the red Book room button and a close button. Errors appear inside it as a solid red note.

### Side panel
A 460px paper panel sliding in from the right over a scrim, with a 2px ink rule on its left edge and the overlay shadow. Heading at 34px, the time at headline size, a two column list of facts with uppercase terms, people as hairline separated 46px rows, and a footer of equal width buttons above a 2px ink rule. Destructive actions confirm inline in the footer. On phones it is a bottom sheet with the rule on its top edge.

### Notes and tags
Error notes are a 2px red ink box on red wash with a warning icon, a bold title and an inline status code chip in solid red. Toasts are ink strips at the top right with a red check icon and a dismiss button, dropping in over 0.4s and leaving after 4.5s. Status tags are 12px uppercase text in a 1.5px outline of the current color, red ink for cancelled.

### Lists
My bookings and admin lists are rows on hairlines under 2px ink group rules with a muted count. A list entry leads with a big condensed date figure, then the time and room, then people and requests, then actions. Cancelled entries strike through the date and time with a 2px line and mute them.

## Do's and Don'ts

### Do:
- Do keep red for now, yours, your request, your selection, focus, the current tab, errors and the single primary action on a surface.
- Do express every booking state by fill pattern and repeat it in the legend; name private and full in words.
- Do set dates, hours, times and capacities in condensed heavy Archivo (70% to 85% width) with tabular figures, and keep interface text at normal width.
- Do separate with rules: 2px ink for structure (top bar, hour row, group and page titles, panel head and foot), 1px hairline between rows.
- Do use a 2px ink outline offset 2px for hover on blocks and a 3px red outline for the open block.
- Do use the dashed red outline only for an unsaved selection range, and the solid red outline with red stripes for a request you made.
- Do keep the now rule below the hours baseline with its tag on the past side, in the strip under the numerals.
- Do turn the timetable on its side below 1080px, with hours down the page and rooms across, and page through rooms with the header arrows rather than scrolling sideways.
- Do animate only overlays (bar, panel, sheet, toasts) with cubic-bezier(0.16, 1, 0.3, 1) and honor reduced motion.

### Don't:
- Don't round corners; the only circles are seat marks and the spinner.
- Don't put a shadow on anything that is not an overlay, and don't lift or translate elements on hover.
- Don't wrap content in raised cards; group with rules and the shaded paper tone instead.
- Don't add a second accent hue or use red for decoration.
- Don't use grey as the only signal of a state.
- Don't use dashed red for any stored booking.
