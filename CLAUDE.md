# CLAUDE.md

This repository holds the study room booking application that the class builds in the optional part of the CFLP C# lab at the Politehnica University of Timișoara. The slides for each session live in a separate repository, `RazvanGolan/cflp-lab`, published at https://razvangolan.github.io/cflp-lab/. Those decks link here by tag.

`REQUIREMENTS.md` is the specification. Its rules are numbered (R1 to R19) and code comments, commits and slides refer to them by number. When a rule changes in class, update that file in the same commit as the code.

## Tags

The code at the end of each session is tagged `week-NN` (`week-03`, `week-04`, ...). Tags never move. Students check them out to catch up, and the decks link to `tree/week-NN` and to `compare/week-NN...week-MM`. Work happens on `main`; tag the commit after the lab:

```bash
git tag week-03 && git push origin week-03
```

## Code

- Target `net10.0` with nullable reference types on.
- Unlike the demo code in `cflp-lab`, this application uses NuGet packages (EF Core, Identity, Aspire). Everything up to week 9 must still run on a Linux lab machine with only the .NET SDK and NuGet: SQLite, no Docker, no other database. Containers (Mailpit) are allowed only in the Aspire AppHost and must be optional.
- Write the code the way it is shown in class: plain, with few abstractions, no `#region` markers. A student who missed a week should be able to read the diff between two tags.
- Never put a solution to a graded assignment or a test here. Those are shared with other lab groups and reused from year to year.

## Writing

Students read the README and `REQUIREMENTS.md`, so follow the writing rules from `cflp-lab`: no em or en dashes in prose, state points directly, no bold labels in lists, plain verbs, everything in English.
