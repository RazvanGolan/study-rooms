# Study Rooms interface

The web page for the Study Rooms API. It shows the timetable, lets you book and join rooms, and has the admin pages. You do not need to read this code for the lab: it is here so you can see the API at work.

## Running it

You need [Node.js](https://nodejs.org/) 20 or newer, and the API running (see the README one folder up).

```bash
cd frontend
npm install
npm run dev
```

Then open http://localhost:5173. On the login page, click one of the sample accounts to fill in its email and password.

To see two students at once, open the page in a normal window and in a private window, and log in as a different student in each.

The page calls the API at http://localhost:5080. To use another address, copy `.env.example` to `.env.local` and change `VITE_API_URL`.

Open the browser's developer tools on the Network tab to see every request the page sends, and what the API answers.
