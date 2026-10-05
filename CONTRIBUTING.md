# Contributing to OrionEnvelope

Thanks for taking the time to look at this. OrionEnvelope is one HTTP contract for the whole API: a typed `{ data, meta }` success, an RFC 9457 `problem+json` failure, projected from OrionResult. The project is small and the bar for contributions is "does it make the package clearer, faster, or safer without expanding the public surface needlessly."

## Before you open a PR

For anything beyond a typo, a docs tweak, or a one-line fix, please open an issue first. Five minutes of alignment up front saves an afternoon of rework later. State:

- The use case you are trying to solve
- What you tried that did not work
- Whether you want to send the patch yourself or are flagging the gap

For typos, docs polish, comment fixes, single-line changes, please skip the issue and send a PR directly. Title it `docs: ...` or `chore: ...` so it is obvious from the queue.

## Local development

```bash
git clone https://github.com/tunahanaliozturk/OrionEnvelope
cd OrionEnvelope
dotnet restore
dotnet build -c Release
dotnet test
```

.NET 8 SDK is required. Multi-target builds may need 9.0 / 10.0 SDKs installed; the multi-target dimension is intentional and not optional.

Branch from `master`. Name the branch after intent: `feat/...`, `fix/...`, `docs/...`, `refactor/...`, `chore/...`, `test/...`.

## Pull request shape

- One conceptual change per PR. Refactors and behaviour changes go in separate PRs even if the diff feels small.
- Conventional Commits style commit subject (`feat:`, `fix:`, `docs:`, etc.).
- New behaviour comes with tests. Bug fixes come with a failing-before, passing-after test.
- Public API additions need XML doc comments. Breaking changes need a CHANGELOG entry.
- No `Co-Authored-By` trailers. The author of the PR is the author of the work.

## Coding style

- The repo enforces analyzer warnings as errors and `latest-recommended` analysis level. Treat warnings as bugs.
- Match the surrounding code style. If the existing code does X, do X.
- Names are spelled out. No `mgr`, `svc`, `ctx`. The exceptions are well-known abbreviations (`Id`, `Db`, `Url`, `Json`).
- Comments explain why, not what. The code already says what.
- The wire types must stay source-gen serializable and reflection-free (no reflection-based `JsonSerializer` calls in the library, no types the source-gen context cannot cover). Any change to a wire shape must update the pinned JSON-shape tests in `EnvelopeSerializationTests`, and the envelope/problem JSON contract is stable — a change to a property name or casing is a breaking change.

## Tests

- xUnit with its built-in `Assert`.
- Test names are sentences with underscores: `A_failed_result_projects_to_problem_details_with_the_mapped_status`.
- Serialization is tested through a source-gen context configured with no reflection fallback, so a type that slips out of source-gen coverage fails the test rather than silently working via reflection. Wire shapes are pinned by asserting the exact serialized JSON string.
- Coverage is a side effect of writing tests for behaviour, not a target in itself.

## Reporting bugs

Open an issue with:

- A minimal reproduction (the `Result`/`Error` and the projection call, ideally less than 50 lines)
- The actual behaviour vs the expected behaviour
- The runtime (`dotnet --info` output) and the package version

If the bug has security implications, do not open a public issue; follow [SECURITY.md](SECURITY.md).

## Security

Do not file public issues for vulnerabilities. Report them privately through GitHub as described in [SECURITY.md](SECURITY.md).

## Conduct

Be kind. We follow the [Code of Conduct](CODE_OF_CONDUCT.md). Disagreement is fine; rudeness is not.

## License

By submitting a pull request, you agree your contribution is licensed under the repo's [MIT License](LICENSE).
