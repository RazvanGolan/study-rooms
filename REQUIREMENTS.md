# Requirements

What the application has to do, as agreed in class in week 2. Each rule has a number, so the slides, commits and code comments can point to it (R5, R13, ...).

There are two roles. A student books rooms and joins bookings. An admin manages rooms and can see and change every booking.

## Rooms

- R1. An admin adds, edits and removes rooms. A room has a name, a building, a capacity and its equipment, picked from a fixed list: projector, whiteboard, TV, power outlets.
- R2. Removing a room cancels all its future bookings and notifies everyone in them. The room stays in the database as hidden, so past bookings can still show it.
- R3. An admin may lower a room's capacity below the size of a booking that already exists. That booking keeps its people, but nobody new can join until it fits again.

## Timetable and booking

- R4. Every logged in user can see the timetable: for a chosen day, every room and its bookings between 8:00 and 20:00.
- R5. A student books a room for a free time range and becomes the owner of that booking. Two active bookings of the same room never overlap.
- R6. Start and end times fall on a 30 minute step (10:00, 10:30, 11:00 and so on). A booking lasts from 30 minutes to 3 hours, fits inside 8:00 to 20:00 on one day, does not start in the past, and is at most 7 days ahead. These limits come from configuration.
- R7. For any booking, students can see who is in it and how many places are left. They see display names only. Emails and other personal details of other students are never shown. Admins see everything.

## Joining a booking

- R8. When booking, the owner picks who can join: nobody (private), anyone while there is room (open), or only people the owner approves (on request).
- R9. The number of people in a booking, the owner included, never goes over the room's capacity.
- R10. A participant can leave the booking until it starts, and the owner is notified. The owner can remove a participant until the booking starts, and that student is notified.
- R11. Participants cannot cancel the booking. Only the owner or an admin can.
- R12. A join request puts a student in the booking only once the owner approves it. When a booking fills up or is cancelled, its pending requests are declined automatically and the students who sent them are notified.

## Limits

- R13. A student cannot be in two places at once: the active bookings they own or joined never overlap. Bookings at different times are fine. Pending requests do not count, but the owner cannot approve a request from a student who is busy at that time, and approving a request declines the student's other pending requests that overlap with it.
- R14. The owner can cancel a booking until 2 hours before it starts (this also comes from configuration). After that, only an admin can cancel it.

## Notifications

- R15. Everyone in a booking gets a reminder some time before it starts. How long before is one setting for the whole application.
- R16. When a booking is cancelled, by its owner, by an admin, or because its room was removed, everyone in it is notified.
- R17. The owner is notified when someone asks to join, joins or leaves. A student who asked to join is notified when the request is approved or declined.

Who gets which notification:

| Event | Who is notified |
|---|---|
| A booking is created | the owner, as a confirmation |
| Someone joins an open booking | the owner |
| Someone asks to join | the owner |
| A request is approved or declined | the student who asked |
| A participant leaves | the owner |
| The owner removes a participant | the removed student |
| The booking starts soon | everyone in it |
| The owner or an admin cancels | everyone in it, except whoever cancelled |
| The room is removed | everyone in the room's future bookings |

## Admin and accounts

- R18. An admin sees all bookings and can cancel any of them. An admin can create a booking for a student, which skips the 2 hour and 7 day limits but still follows R5, R9 and R13. An admin can also block a room for a class or for maintenance: a booking with no owner and no participants, only a note such as "Class: PCLP".
- R19. Users create an account with an email and a password, or sign in with GitHub or Google. Each account has a display name, chosen when it is created, and that name is what other students see.
