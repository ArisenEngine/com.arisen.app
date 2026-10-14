# com.arisen.app

The application shell every product depends on.

It owns the boot plumbing that must behave identically for every product:

- **Startup target activation.** Before the first frame the shell activates the process's startup
  target: `--startup-scene <packageId>:<guid>` or `--startup-world <packageId>:<guid>` when the
  command line names one, otherwise the workspace manifest's `StartupScene`/`StartupWorld`. A world
  wins over a scene, and a process that resolves to no target fails loudly.
- **The interactive runtime viewer.** Free-fly camera navigation and the world streaming source it
  publishes. It idles in editor builds and in bounded smoke runs, where the editor host or the
  smoke scenario owns world activation.

## Why it exists

One workspace hosts several products - the game and the sample gallery - and a profile decides the
package graph at boot. Without a shell, launching anything other than the manifest's startup
identity would mean editing the workspace.

## Rules

- **One target per process.** `--startup-scene` and `--startup-world` are mutually exclusive; the
  selection is resolved once, at boot, and never rewritten at runtime.
- **The manifest is the default.** Arguments override the declared target; they never edit it.
- **The shell never names content.** It activates whatever the selection resolves to, so products
  and samples decide what is shown.
