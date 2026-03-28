# Contributing

Thanks for taking an interest. This project follows a simple workflow intended for a single developer with reviewer-friendly PRs.

Getting started
- Clone the repo and open the solution in Visual Studio or use the `dotnet` CLI.
- Build: `dotnet build LiveChatServer`
- Run locally: `dotnet run --project LiveChatServer`
- Run tests: `dotnet test` (tests are in `tests/` when present)

Branching & workflow
- Work on the `dev` branch. Create short-lived feature branches off `dev`:
  - `feat/<short-desc>` for new features
  - `fix/<short-desc>` for bug fixes
  - `chore/<short-desc>` for infra/docs
- When a sprint or feature is complete, open a PR from your branch into `dev` for review.
- When `dev` reaches a stable sprint state, open a PR from `dev` → `main`.

Pull requests
- Use the provided PR template (`.github/PULL_REQUEST_TEMPLATE.md`).
- Ensure CI passes and at least one reviewer approves before merging into `main`.
- Keep PRs focused and include a short description and verification steps.

Code style & quality
- Follow rules in `LiveChatServer.GeneratedMSBuildEditorConfig.editorconfig`.
- Keep changes small and well-documented. Add or update docs under `Solution Items/` when behavior or APIs change.

Issues & security
- Open issues for bugs and enhancements; tag with appropriate labels.
- For security issues, follow the guidance in `SECURITY.md` (if present) or contact the repo owner privately.

Thank you — contributions and reviews are welcome. If unsure, open an issue or a draft PR to start the discussion.
