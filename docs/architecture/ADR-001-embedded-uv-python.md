# ADR-001: Embed uv + Managed Python inside MCP for Unity Package

- **Status**: Accepted
- **Date**: 2026-04-19
- **Deciders**: ashuatz (project owner), Claude Code (CCGS coordination)
- **Related TR**: (none — tooling decision, not gameplay)

## Context

`com.coplaydev.unity-mcp` (the MCP for Unity bridge) starts a Python-based MCP
server via `uv` / `uvx`. The upstream setup assumes each developer installs
Python 3.10+ and the `uv` package manager manually from the OS side.

For this project the developer machine has neither Python nor `uv`
pre-installed, and the preferred workflow is to clone the repository and run
Unity without running any external installers. "Install Python from the
Microsoft Store" is not an acceptable first-run step.

We considered three options:

| Option | Summary | Trade-off |
|---|---|---|
| **A. Bundle uv binary in the package** | Ship `uv.exe` inside the UPM package under `Editor~/`, copy to a user-local runtime dir on first load, let `uv` install Python. | Small package-size cost (~23 MB compressed), zero-install UX. |
| **B. Pure C# MCP server port** | Re-implement the MCP server in C# inside the Unity Editor, drop Python entirely. | Removes the entire toolchain, but requires porting 100+ tools and keeping them in sync with upstream — weeks of work, massive ongoing maintenance. |
| **C. Bootstrap script** | Ship a PowerShell/bash script that the user runs once to install Python + uv. | Still requires a manual step and breaks CI/fresh-clone ergonomics. |

## Decision

Adopt **Option A — bundle uv in the package (seed) and provision a managed
Python runtime on first editor load (runtime)**.

Layout:

```
Packages/com.coplaydev.unity-mcp/Editor~/Bundled/uv/win-x64/uv.exe   ← SEED (git, immutable)
%LOCALAPPDATA%/MCPForUnity/uv/uv.exe                                  ← RUNTIME (updatable)
%LOCALAPPDATA%/uv/python/                                             ← Managed Python (uv default)
```

- **Seed** is copied into the runtime directory on first load. `Editor~` is the
  UPM convention for "files that exist in the package on disk but are not
  imported by Unity's asset pipeline" — no `.meta`, no asset processing.
- **Runtime** is a plain copy of the seed. We do **not** use `uv self update`:
  `astral-sh/uv` only supports self-update for binaries installed via the
  standalone PowerShell/sh installer, not for binaries extracted from the ZIP
  release we ship. Any call to `uv self update` against our runtime prints a
  `Self-update is only available for uv binaries installed via the standalone
  installation scripts` error. Instead, upgrades happen by replacing the seed
  binary in the package (git PR) — `IsSeedDifferentFromRuntime` detects the
  swap on next editor load and re-copies seed → runtime automatically.
- **Managed Python** is provisioned once per machine via `uv python install
  3.13`. Subsequent `uvx`/`uv run` calls use this interpreter automatically.

Initial rollout is **Windows x64 only (B1)**. macOS and Linux seeds are
deferred to a follow-up PR because the current dev machine only runs Windows
and the bundled binaries would double the package size without immediate use.

## Consequences

**Positive**
- Clone-and-run experience: no Python install, no Microsoft Store, no PATH
  fiddling. Opening Unity provisions uv + Python transparently.
- Offline-first: `uv.exe` is on disk before any network call happens, so a
  disconnected developer can still launch the bridge (Python install itself
  still requires network on first run, which is documented).
- Deterministic toolchain: every developer starts with the same uv version
  (tracked in git), eliminating "works on my machine" skew around uv releases.

**Negative**
- Repository grows by ~23 MB (the compressed `uv.exe`). Tolerable — still well
  under Git LFS thresholds and only one binary per supported RID.
- No auto-upgrade path for `uv` itself. If upstream CoplayDev ships a
  breaking change that requires a newer `uv`, we must bump the seed binary
  manually (replace `uv.exe` under `Editor~/Bundled/uv/<rid>/` and commit).
  The `Reinstall Bundled uv + Python` menu forces a re-copy for users who
  want to skip the automatic seed-vs-runtime check.
- Automatic re-copy on seed replacement relies on file-size/mtime diff; a
  hand-edited seed binary of identical size may evade detection. Force
  reinstall via menu handles that corner.
- Only Windows today. Non-Windows contributors fall back to system-installed
  uv (same path as upstream today) until the follow-up PR.

**Neutral**
- Embed replaces the git-URL dependency in `Packages/manifest.json`. Upstream
  upgrades become an explicit copy/merge rather than an automatic pull. This
  is also the standard cost of any UPM embed and is justified because we need
  to layer the bundled-dependency bootstrapping on top.

## Implementation Notes

Files touched:

- `Packages/manifest.json` — `file:` reference instead of git URL.
- `.gitignore` — exception rules so `Editor~/Bundled/**/uv.exe` is tracked
  despite the global `*.exe` ignore.
- `Packages/com.coplaydev.unity-mcp/Editor~/Bundled/uv/win-x64/` — seed
  binaries (`uv.exe`, `uvx.exe`, `uvw.exe` from `astral-sh/uv` v0.11.7).
- `Editor/Dependencies/BundledDependencyInstaller.cs` — seed→runtime copy,
  `uv self update` (best-effort), `uv python install 3.13`.
- `Editor/Dependencies/BundledDependencyBootstrap.cs` — `InitializeOnLoad`
  hook that runs the installer once per editor session on a background task.
- `Editor/Dependencies/PlatformDetectors/WindowsPlatformDetector.cs` —
  `GetPathAdditions()` now lists the runtime dir first so the bundled uv
  wins the detection race against a stale system install.
- `Editor/MenuItems/MCPForUnityMenu.cs` — `Window/MCP For Unity/Dependencies/
  Reinstall Bundled uv + Python` escape-hatch action.

## Follow-ups

1. B2 rollout: seed `osx-arm64`, `osx-x64`, `linux-x64` binaries.
2. CI: add a job that verifies the seed binary reports a version and that
   `uv python install` succeeds on a fresh Windows runner.
3. Revisit `uv self update` frequency — currently runs on every first-seen
   seed version. Consider throttling to once per N days if it proves noisy.
