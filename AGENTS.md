# Agent instructions

## Workflow

Do not implement the entire project in one large batch or create a single commit at the end.

Work issue by issue. For each issue:
- Create a feature branch from up-to-date `main`. Never push directly to `main`.
- Open a draft pull request right after the first push: CI runs on pull requests and on `main`, not on bare branch pushes.
- Develop in small, logically independent stages. Each commit must build and pass all tests on its own.

For each completed stage:
1. Run the relevant tests and quality checks (build, linters, formatters, scanners). Fix failures; never skip, disable or weaken a test or check to make it pass.
2. Review the staged diff: only related changes, no build output, no secrets or credentials.
3. Commit with a descriptive [Conventional Commits](https://www.conventionalcommits.org/) message (`feat:`, `fix:`, `test:`, `ci:`, `docs:`...) explaining what and why.
4. Push the branch to the existing GitHub repository.
5. Verify on GitHub that CI passed for that exact commit; if it failed, fix it in a new commit.

When the issue is done:
- Mark the pull request ready for review. It references the issue (`Closes #N`) and describes what changed and how it was tested.
- Merge only when CI is green, using a merge commit (squash and rebase merges are disabled), then delete the branch and confirm CI on `main` is green.

Rules:
- Commit as the repository owner: `alef-us <248708568+alef-us@users.noreply.github.com>`, so contributions count on the profile.
- Do not use force push, and do not amend, rebase or otherwise rewrite commits that are already pushed.
- No empty or backdated commits.
- Stop and ask before design decisions with several reasonable options, and before creating anything that costs money (cloud resources); destroy temporary cloud resources after use.

## Build and test

.NET 10 SDK, pinned in [global.json](global.json). Run from the repository root:

```bash
dotnet format --verify-no-changes   # style gate, same as CI
dotnet build -c Release             # warnings are errors (Directory.Build.props)
dotnet test -c Release              # unit and integration tests
actionlint                          # GitHub Actions workflows
hadolint --failure-threshold info Dockerfile
shellcheck scripts/*.sh
scripts/smoke-test.sh <base-url> [expected-version]   # against any running instance
```

CI is [.github/workflows/ci.yml](.github/workflows/ci.yml). The roadmap and the meaning of each step are in [README.md](README.md) and the `roadmap` issues.

## Conventions

- Line endings are LF everywhere ([.gitattributes](.gitattributes), [.editorconfig](.editorconfig)), also when working on Windows.
- Keep test coverage honest: do not exclude code from coverage reports to raise the number.
- Use the latest major version of GitHub Actions; check the release before bumping. Pin third-party actions by commit SHA with the version in a comment.
- Dependabot PRs with major version bumps: confirm in the CI log that all tests were still discovered and coverage was still collected before merging.
- Docker build args and environment variables must not share a name with an MSBuild property (`VERSION`, `CONFIGURATION`...): MSBuild reads the environment as properties, case-insensitively. The version arg is `APP_VERSION`.
- A green security or test step is not proof on its own: check that it actually scanned or ran something.
