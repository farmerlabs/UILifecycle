# Contributing

Thanks for taking an interest in this package.

**English** | [日本語](CONTRIBUTING.ja.md)

## Project status

This package is developed and maintained loosely, in the author's spare time,
and it is published primarily as a showcase of the author's own work. Please set
your expectations accordingly:

- **Pull requests are generally not accepted**, including small fixes and typo
  corrections. Keeping the codebase authored by one person is intentional.
- **Bug reports are welcome**, and are the most useful thing you can contribute.
- **Replies can be slow, and may not come at all.** This is not a judgement of
  your report; there is simply limited time for it.
- **If you need a change, fork it.** The MIT license allows that, and for a
  package this size a fork is usually the faster path.

This does not mean contributions are unwelcome — it means they are accepted in a
narrow form. Report through issues; change through forks.

## Reporting a bug

Open an issue using the **Bug report** template. Please include the Unity
version, the package version, and the smallest set of steps that reproduces the
problem. A stripped-down scene or a failing test is the most useful thing you
can attach.

You do not need to write the fix (it cannot be accepted anyway). Just describe
**what happened**.

## Requesting a feature

Open an issue using the **Feature request** template.

This package keeps a deliberately small API, and its names follow the vocabulary
rules in [the package README](Packages/com.farmerlab.uilifecycle/README.md#3-vocabulary-rules).
A request will not necessarily be implemented, but knowing which problem you ran
into is valuable in itself.

## Errors in the documentation

**The Japanese documents are the authoritative version.** `README.ja.md` and
`CONTRIBUTING.ja.md` are what the author writes and maintains; the English
documents are translations of them. Where the two disagree, the Japanese text is
correct.

If you spot wrong or awkward English, please point it out in an issue.

## Forking

This repository is a Unity project that hosts the package under `Packages/`, so
you can clone it and open it directly.

| Item | Value |
|---|---|
| Unity | 2022.3 or later |
| Dependencies | UniTask 2.5.11 or later |
| Samples | additionally require TextMeshPro |

Run the tests from **Window > General > Test Runner > PlayMode** and select the
`UiLifecycle.Tests` assembly.

What you do in your fork is up to you. Just keep to the terms of the MIT license
— retain the copyright notice and the permission notice.