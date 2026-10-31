# Contributing to Mordex Components

Thank you for wanting to help. Mordex Components is a set of WinForms controls for .NET 10, with out-of-process designer support for Visual Studio. It gets better when people report problems and send patches. You do not need to be an expert. A clear bug report is a real contribution.

By taking part, you agree to follow the [Code of Conduct](CODE_OF_CONDUCT.md).

## Contents

- [Ways to contribute](#ways-to-contribute)
- [Project principles](#project-principles)
- [Workflow](#workflow)
- [Project map](#project-map)
- [Build and run](#build-and-run)
- [Checks](#checks)
- [Code style](#code-style)
- [Commit and pull request checklist](#commit-and-pull-request-checklist)
- [Reporting bugs well](#reporting-bugs-well)
- [Security](#security)
- [Maintainers and decisions](#maintainers-and-decisions)
- [License and sign-off](#license-and-sign-off)

## Ways to contribute

- **Report a bug.** Use the [bug report form](https://github.com/ehsanenaloo/Mordex-Components/issues/new?template=bug_report.yml). Search [existing issues](https://github.com/ehsanenaloo/Mordex-Components/issues) first.
- **Suggest a feature.** Use the [feature request form](https://github.com/ehsanenaloo/Mordex-Components/issues/new?template=feature_request.yml). Describe the problem you have before the solution you want.
- **Review pull requests.** Try a change in your own project and say what you saw.
- **Fix an issue.** Pick one labeled `good first issue` or `help wanted`, say that you are on it, and open a pull request.

Not sure where to start? Open an issue and ask. It is fine to start small.

Before you start on an issue, leave a short comment such as "I would like to work on this." This avoids two people doing the same work. For a typo or a small fix, you can send a pull request directly.

## Project principles

These guide every review.

1. **Never crash the host form.** A control must handle bad input, an empty data source and a missing visual style without throwing out of a paint or layout call.
2. **Dispose what you create.** Brushes, pens, fonts, bitmaps, streams, timers and event subscriptions are released when the control is disposed.
3. **Stay responsive.** UI work stays on the UI thread. Animations and long work must not freeze the window.
4. **Do not trust file content.** Layout and settings files are untrusted input. Do not create a type from a name in a file without an allow-list, and read XML with DTD processing off.
5. **Fix the cause.** Do not hide a wrong state or a timing problem with a silent skip or an extra retry.
6. **Keep names clear.** Controls use the `Mx` prefix and namespaces start with `Mordex.Windows`. Do not add names like `temp`, `new`, `v2` or `legacy` to production code. Keep source files under 1500 lines.

A change that breaks one of these needs a discussion in an issue before any code.

## Workflow

1. **Fork** the repository on GitHub and clone your fork.

   ```bash
   git clone https://github.com/<your-username>/Mordex-Components.git
   cd Mordex-Components
   git remote add upstream https://github.com/ehsanenaloo/Mordex-Components.git
   ```

2. **Create a branch** from the latest `main`. Use a short name with a prefix:

   - `fix/datagrid-sort-crash`
   - `feat/scheduler-week-numbers`
   - `docs/readme-build-steps`
   - `chore/update-editorconfig`

3. **Make small commits.** One idea per commit. Each commit should leave the project building.

4. **Write the commit message in Conventional Commits style:** `type(scope): short summary`. Use the imperative mood ("fix", not "fixed"). Keep the first line under about 72 characters. Examples:

   ```text
   fix(datagrid): keep the sort order after a data refresh
   feat(scheduler): show week numbers
   docs(readme): list the build steps
   ci(build): pack the NuGet package
   ```

   Common types are `feat`, `fix`, `docs`, `refactor`, `perf`, `chore` and `ci`. Common scopes are the control names in lower case, `designer`, `common`, `data` and `package`.

5. **Keep your branch up to date by rebasing**, not by merging `main` into it.

   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

   If you have already pushed the branch, update it with `git push --force-with-lease`.

6. **Open a pull request** against `main`. Fill in the template. Mark it as a draft if it is not ready. Keep one topic per pull request. A small pull request is reviewed faster.

7. **Respond to review.** Push follow-up commits to the same branch. When a comment is fixed, say so in a short reply.

### What review looks like

The maintainer checks that the change fits the [principles](#project-principles), passes CI and is easy to read. You may get questions, requests for changes, or a suggestion to split the pull request. This is normal and is about the code, not about you.

Mordex Components has one maintainer, who works on it in spare time. Replies are best effort. A first response often comes within a week or two. If you have heard nothing after two weeks, a polite comment on the pull request is welcome.

## Project map

| Path | What it holds |
| --- | --- |
| `src/Mordex.Data` | Data and condition classes used by the grid and filter controls. |
| `src/Mordex.Windows.Common` | Shared base code: visual styles, layouts, renderers. |
| `src/Mordex.Windows.*` | One project per control family (DataGrid, Ribbon, Navigation, TaskPane, DatePicker, Scheduler, Timeline, UI, and others). |
| `src/*.Design` | Design-time code for Visual Studio. |
| `designer/` | The out-of-process designer client, server and protocol. |
| `pack/` | The `Mordex.Windows` NuGet package project. |
| `demo/` | A demo app and a smoke test that creates every control. |
| `build/`, `build.ps1`, `build.sh` | Build helpers. Assemblies are copied to `artifacts/bin`. |
| `.github/` | Issue forms, the pull request template, labels, the CI workflow and the community files. |

## Build and run

You need the .NET 10 SDK and Windows. Visual Studio is needed only to try the designer.

```bash
./build.ps1                                     # or ./build.sh
dotnet build Mordex.sln -c Release
dotnet run --project demo/Mordex.Demo
```

## Checks

Run these before you open a pull request:

```bash
dotnet build Mordex.sln -c Release              # no warnings, no errors
dotnet run --project demo/Mordex.Demo -c Release --no-build -- --smoke
```

`--smoke` creates every control, prints one line per control, and exits with 1 if any fails.

Each project has a `packages.lock.json` file. If you add or update a package, commit the changed lock files. The build server restores with `--locked-mode` and fails when a lock file does not match.

The designer, the toolbox and the About dialog are not covered by the smoke test. If your change touches them, try it by hand in Visual Studio and say so in the pull request.

For a bug fix, say in the pull request how you showed the bug and how you checked the fix.

## Code style

- Follow [.editorconfig](../.editorconfig): tabs in C# files, UTF-8, a final newline, no trailing spaces.
- Keep to the style of the file you are editing.
- Keep comments for code that is not obvious. Say why, not what.
- Use plain, short English in comments, docs and messages.

## Commit and pull request checklist

- The build has no warnings and the smoke test passes.
- The change is focused and does one thing.
- The pull request says how you checked the change.
- Objects that need disposing are disposed.
- User-visible changes are in `CHANGELOG.md`, under `Unreleased`, in one short plain sentence.
- Commit messages follow Conventional Commits.
- No secrets, personal data or real customer data in code or screenshots.

## Reporting bugs well

A good report has:

- the package version, and your Windows, .NET and Visual Studio versions,
- the control and where it happens (run time or designer),
- steps to repeat it, ideally in a small project,
- what you expected and what happened, with the exception text and stack trace if there is one.

## Security

Do not open a public issue for a security problem. Read [SECURITY.md](SECURITY.md) and use the private report.

## Maintainers and decisions

The maintainer is [@ehsanenaloo](https://github.com/ehsanenaloo). The maintainer decides what fits the project. A "no" is about scope, not about you.

## License and sign-off

By sending a contribution, you confirm that you wrote it or have the right to submit it, and that it can be released under the [MIT License](../LICENSE).
