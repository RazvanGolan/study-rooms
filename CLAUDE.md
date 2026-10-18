# CLAUDE.md

This repository holds the study room booking application that the class builds in the optional part of the CFLP C# lab at the Politehnica University of Timișoara. The slides for each session live in a separate repository, `RazvanGolan/cflp-lab`, published at https://razvangolan.github.io/cflp-lab/. Those decks link here by tag.

`REQUIREMENTS.md` is the specification. Its rules are numbered (R1 to R19); slides and commit messages may refer to them by number, but the code never does. Comments describe the rule in words. When a rule changes in class, update that file in the same commit as the code.

## Tags

The code at the end of each session is tagged `week-NN` (`week-03`, `week-04`, ...). Tags never move. Students check them out to catch up, and the decks link to `tree/week-NN` and to `compare/week-NN...week-MM`. Work happens on `main`; tag the commit after the lab:

```bash
git tag week-03 && git push origin week-03
```

## Code

- Target `net10.0` with nullable reference types on.
- The API uses controllers (`[ApiController]` classes in `Controllers/`), not minimal APIs. Dependencies arrive through the controller's constructor.
- Use only what students have seen in the lab or in an earlier optional session. Anything new (a `lock`, an attribute, a pattern) is either explained on a slide that week or left out.
- Students run this application on their own computers, not on the lab machines, so NuGet packages (OpenAPI, EF Core, Identity, Aspire) are fine. Keep it runnable with the .NET SDK alone: SQLite, no Docker, no other database. Containers (Mailpit) are allowed only in the Aspire AppHost and must be optional.
- Write the code the way it is shown in class: plain, with few abstractions, no `#region` markers. A student who missed a week should be able to read the diff between two tags.
- Never put a solution to a graded assignment or a test here. Those are shared with other lab groups and reused from year to year.

## Frontend

`frontend/` is a React app made with Vite, deployed to GitHub Pages. It is the interface that shows the API at work, and it is not taught: students do not read its code and the lab does not explain it. The rules in the Code section above are for the API and do not limit the frontend. It has to look good, work on a phone, a laptop and a projector, and run with Node and `npm` alone. Product context for it is in `PRODUCT.md`.

## Writing

Students read the README and `REQUIREMENTS.md`, so follow the writing rules from `cflp-lab`: no em or en dashes in prose, state points directly, no bold labels in lists, plain verbs, everything in English.
