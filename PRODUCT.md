# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Stack

The backend is an ASP.NET Core API on .NET 10 (controllers, EF Core, SQLite) in `src/StudyRooms.Api`. The frontend is a separate React app made with Vite in `frontend/`. From week 11, once the API is online, it is published to GitHub Pages at https://razvangolan.github.io/study-rooms/; until then it runs locally with `npm run dev`. Its API address comes from `VITE_API_URL`. Login uses bearer tokens kept in `localStorage`, because the frontend and the API are on different sites.

Styling: delegated. The instructor asked for an interface that looks great and is responsive, and left the styling approach to the build. Chosen: TypeScript, plain CSS with custom properties in one stylesheet, the Archivo font self-hosted through `@fontsource-variable/archivo`, icons from `lucide-react`, and no UI or routing library, so the app runs with Node and `npm` alone and builds in the GitHub Pages Action.

## Users

Students at the Politehnica University of Timișoara who need a room to study in, alone or in a group. They open the timetable for a day, find a free time in a room, book it, and decide who can join. Other students find an open booking and join it or ask to join. They use it on their own laptops and on their phones.

Admins manage the rooms, see and change every booking, book for a student, and block a room for a class or for maintenance. Who holds the admin role once the app is used for real is not decided yet.

A second audience is the CFLP lab class itself. The app is built across the semester in the optional part of the lab, and the frontend is how the class sees the API work: every week ends with a showcase in the browser, shown on one laptop through a projector, and in week 11 the class opens the deployed app on their phones and books rooms on a shared timetable.

## Product Purpose

Study Rooms lets students book faculty study rooms for a time range, share a booking with others, and see who else is in it, while the API enforces the rules agreed in class (REQUIREMENTS.md, R1 to R19). It is meant to work as a real booking tool, not only as a classroom demo.

The frontend's job is to make the API visible. It is the interface students use to see their endpoints, rules and error responses in action. It is not teaching material: students do not read the frontend code, and the lab does not explain it. Success means a student can do every action the API supports from the browser, on a phone or a laptop, and the rules (overlaps, capacity, cutoffs, join policies) show up as clear, understandable feedback.

## Positioning

The interface is a faithful window onto an API the class designed and wrote themselves. Every screen maps to endpoints they know (`/timetable`, `/bookings/{id}/join`, `/admin/rooms`, ...), and every refusal they see is a rule from REQUIREMENTS.md that they implemented.

## Operating Context

- Timetable for one day: every room as a row, 8:00 to 20:00 in 24 half-hour columns. Bookings span their columns. The 30 minute step keeps it a grid.
- Booking: pick a start and an end in a free row. Times fall on a 30 minute step, last 30 minutes to 3 hours, do not start in the past, and are at most 7 days ahead. The owner picks a join policy: private, open, or on request.
- A booking's details show display names and places left, and let a student join or request to join.
- My bookings: cancel (until 2 hours before the start), leave (until the start), remove a participant, approve or decline requests.
- Admin page: add, edit and remove rooms; create bookings for students; add blocks such as "Class: PCLP".
- Sign up with email, password and a display name; log in. From week 11, sign in with GitHub or Google, and choose a display name on first sign-in.
- From week 9, an inbox of notifications (`GET /me/notifications`): confirmations, joins, requests, reminders, cancellations.
- Classroom showcases: two browser windows (one normal, one private) as two students on the projector; the class on their phones in week 11.
- Until week 11 the frontend runs only on each student's computer, next to their own API. In week 11 the API moves to an Oracle Cloud VM behind HTTPS and the frontend goes to GitHub Pages.

## Capabilities and Constraints

- The API is the source of truth for every rule. The frontend shows what the API returns and does not invent rules of its own.
- Students see other people's display names only. Emails and other personal details of other students never appear in the student interface. Admins see everything (R7).
- Rooms have a name, a building, a capacity and equipment from a fixed list: projector, whiteboard, TV, power outlets.
- Booking states and participant states come from the API (active, cancelled; pending, approved, declined). A room removed by an admin is hidden, and its past bookings still show it.
- Error responses from the API (409 overlap, 403, 401, validation errors) need readable messages in the interface.
- The access token lasts one hour; the frontend refreshes it through `/account/refresh`.
- It must work well at phone width as well as on a laptop and on a projector.
- All text in the interface is in English.
- The code style rules in CLAUDE.md (only concepts shown in class, plain code) are written for the API. The instructor has said the frontend is not explained to students, so those limits do not bind it.

## Brand Commitments

The name is Study Rooms. Text follows the course writing rules: plain verbs, no em or en dashes, no bold labels, English only.

## Evidence on Hand

- REQUIREMENTS.md with rules R1 to R19 and the notification table.
- The running API with sample data from the first run: three rooms, one admin and four student accounts (README.md), all with the password `Password1!`. These are development data only.
- No real users, usage numbers, testimonials or room photos exist. Do not invent any.

## Product Principles

1. Every API capability has a place in the interface, and every rule the API enforces is visible when it applies.
2. Built to be used for real: a student booking a room between classes on a phone should find it fast and clear.
3. Privacy is a rule, not a setting: other students appear by display name only.
4. It has to look good on a classroom projector, because that is where the class first sees their work.
