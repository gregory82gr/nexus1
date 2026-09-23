# Evidence: Angular console, Ch. 34 — Build, Theming, and Deployment

## Scope

The final book chapter. Console-only Dockerfile + a standalone compose
fragment (Option A, directed) — no backend `db`/`migrator`/`api` compose
stack fabricated, since none exists in this repo.

## Investigation, reviewed before writing final code

Checked directly, not assumed, before building anything:

1. **No existing docker-compose/Dockerfile topology anywhere in this
   solution.** Zero `docker-compose*.yml`, zero `Dockerfile`, zero
   `nginx*` config, zero `.yml`/`.yaml` file of any kind outside
   `node_modules`. No `.github/workflows`. No Aspire `AppHost` project
   (`Nexus1.ServiceDefaults` is a plain OpenTelemetry helper, confirmed
   by reading it). Decisive: `CLAUDE.md` line 148 itself already names
   why — *"Docker / Docker Compose for local reproducible runtime
   (introduced later, per book Ch. 55 — not needed for the first compile
   gate)"* — the backend's own companion book plans this as its own
   later, and still-unbuilt, milestone. The Angular book's Ch. 34
   premise (extend an existing 3-service compose.yml) doesn't hold here.
2. **Real Angular build output**: `angular.json`'s `outputPath` says
   `dist/nexus-console`, but the actual on-disk output (checked directly,
   not inferred) is one level deeper: `dist/nexus-console/browser/` —
   the modern `@angular-devkit/build-angular:application` builder nests
   the real browser bundle under `browser/`. Copying `dist/nexus-console`
   alone would serve an empty directory.
3. **No existing health-check convention on the static side** (nothing
   to check — never containerized). The .NET side's real idiom is
   `/health/live`/`/health/ready` (and Ch. 31's `/health/contexts`) —
   nginx has no equivalent application-level logic for a static file
   server, so the honest match is a plain HTTP check against the served
   root, not a fabricated `/health/ready`.

**Found mid-build, not in the original investigation report** (worth
naming precisely): every one of this app's ~15 `core/api/*.ts` clients
still hardcodes its backend URL as a build-time constant
(`const BFF_BASE_URL = 'http://localhost:5103'`), each carrying the
identical comment *"Named simplification, ahead of Ch. 5: ... not yet
the chapter's own injected-config entrypoint script"* — confirmed this
mechanism (the book's own Ch. 5) has never been built in this port,
across every API client file, not just one. So the entrypoint script
below is real, correct nginx/shell plumbing, but it has **no effect on
this app's actual runtime behavior today** — named explicitly in the
script's own doc comment and in the compose fragment's header, not
silently built as if it worked.

## What was built

- `console/nexus-console/Dockerfile` — two real stages: `node:20-alpine`
  (`npm ci` → `npm run build`, production default) → `nginx:alpine`,
  copying from the confirmed real path `dist/nexus-console/browser`.
  `HEALTHCHECK` via `wget --spider -q http://localhost/` — a plain
  reachability check, not a fabricated app-level health endpoint.
- `console/nexus-console/docker/entrypoint-runtime-config.sh` — a real
  `/docker-entrypoint.d/*.sh` hook (the official nginx image's own,
  documented mechanism, not invented here), reading `$API_BASE_URL` at
  container start and writing it to a runtime JSON file. Its own doc
  comment states the named gap above in full.
- `console/nexus-console/.dockerignore` — excludes `node_modules`,
  `dist`, `.angular`, `test-results`, `playwright-report`, `e2e`, logs,
  `.git` from the build context.
- `console/nexus-console/docker-compose.console.yml` — the `console`
  service alone. Header states all three required boundaries: (a) never
  built or run here, disk/RAM confirmed too tight; (b) not attached to
  any real backend stack, none exists, matching the backend's own
  deferred Ch. 55 milestone; (c) `API_BASE_URL` has no real effect on
  this app yet either, per the entrypoint script's own gap. `depends_on`
  and a real `api` target are left commented out, explicitly pointing at
  where they'd go once the backend stack exists — no `db`/`migrator`/
  `api` service fabricated.

## Static-only verification (no live build/run — directed, and correct given the constraints)

**Tooling availability, checked directly:**
```
docker --version   -> command not found (no Docker CLI installed at all, not just "daemon not running")
hadolint           -> command not found
```

**Real disk/RAM headroom, confirmed directly (this is the actual reason live verification was skipped, not an assumption):**
```
df -h /                    -> C:/Program Files/Git   166G total   157G used   9.2G available   95% full
Total Memory                -> 7.9 GB
Free Memory (at check time) -> 1.2 GB
```
9.2 GB free disk and 1.2 GB free RAM is not enough headroom for a Docker
Desktop install plus building and running a multi-container stack in
this environment — confirmed, not guessed.

**`docker compose config` substitute**: not runnable (no Docker CLI).
Used `js-yaml` (already present as a transitive dependency in this
project) to parse `docker-compose.console.yml` directly — confirms valid
YAML syntax and that the commented-out `depends_on`/`api` block correctly
stays out of the parsed structure:
```js
{
  services: {
    console: {
      build: { context: '.', dockerfile: 'Dockerfile' },
      image: 'nexus1-console:local',
      ports: [ '8080:80' ],
      environment: [ 'API_BASE_URL=http://api:8081' ]
    }
  }
}
```
This confirms syntax and structure, not full Compose-schema semantic
validation (that needs the real `docker compose config`) — named
honestly as a substitute, not equivalent.

**Manual Dockerfile review** (hadolint unavailable):
- Stage order and `COPY --from=build` reference correct.
- `COPY package.json package-lock.json ./` before `COPY . .` — correct
  layer-caching order; `npm ci` (not `npm install`) for reproducible
  builds against the real, confirmed `package-lock.json` (630 KB, present
  on disk).
- `COPY --from=build /app/dist/nexus-console/browser` — matches the
  real, confirmed output path exactly (see investigation point 2).
- `nginx:alpine`'s busybox base includes `wget` — the `HEALTHCHECK`
  command needs no extra package install.
- `/docker-entrypoint.d/40-runtime-config.sh` naming respects the
  official image's own alpha-order convention (its own stock hooks are
  `10-`/`15-`/`20-`/`30-`).
- `.dockerignore` correctly excludes `e2e/`, keeping the build context
  to what `npm run build` (which only touches `src/`) actually needs.

## Honest boundary

Three named, disclosed deferrals for this chapter — same treatment as
the Ch. 29/Root Cause and backend Ch. 55/Docker deferrals elsewhere in
this project:

1. **Never built or run.** This Dockerfile and compose fragment have
   never been built or executed in this environment — confirmed real
   disk (9.2 GB free of 166 GB, 95% full) and RAM (1.2 GB free of 7.9 GB
   total) headroom too tight for a Docker install plus this build.
   Verified by static review only (see above). Deferred, not skipped
   silently.
2. **Not attached to a real backend stack.** No `db`/`migrator`/`api`
   compose service exists anywhere in this repo to attach to — the
   backend's own compose milestone is itself a named, deferred item
   (book Ch. 55, per this project's own `CLAUDE.md`). The `api` target
   in `docker-compose.console.yml` is a commented-out placeholder, never
   fabricated as real.
3. **The injected-config mechanism has no effect on this app's current
   behavior.** All ~15 `core/api/*.ts` clients still hardcode their
   backend URL as a build-time constant (each carrying an "ahead of
   Ch. 5" comment) — the underlying Ch. 5 config-injection mechanism was
   never built in this port. `docker/entrypoint-runtime-config.sh`
   correctly writes the real `$API_BASE_URL` into a runtime file at
   container start, but nothing in this app reads that file yet. This is
   a real, separate, named gap, found mid-build and disclosed in both the
   entrypoint script's own comment and the compose fragment's header —
   deferred, not fixed, in this chapter, per Option A direction (a
   Ch.5-scale refactor across ~15 files is out of this deployment
   chapter's real scope).

## Summary

Read the full chapter before building. Confirmed the book's own Ch. 34
premise (extend an existing db/migrator/api compose.yml) has no
foundation in this repo — the backend's own compose milestone (book
Ch. 55) is itself a named, deferred item per this project's own
CLAUDE.md, not something to fabricate here. Built exactly the real,
buildable slice: the console's own Dockerfile and a standalone compose
fragment, both structurally correct and idiomatically matched to this
project's real conventions (nginx's own entrypoint-hook mechanism, the
.NET side's env-var-driven config pattern, a health check honestly
scoped to what a static file server can report). Found and named a
second gap mid-build, not anticipated in the original investigation:
the runtime-injected `API_BASE_URL` this chapter wires up has no real
consumer yet, since the book's own Ch. 5 injected-config mechanism was
never built in this port — every API client still hardcodes its backend
URL at compile time. Verification is static-only throughout, as
directed: real tool-availability checks (both absent), real disk/RAM
numbers (9.2 GB / 1.2 GB, genuinely insufficient), a YAML-parse
substitute for `docker compose config`, and a manual line-by-line
Dockerfile review in hadolint's place.
