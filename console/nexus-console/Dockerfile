# Ch. 34 "Build, Theming, and Deployment" -- NEVER BUILT OR RUN in this
# environment (local disk/RAM headroom confirmed too tight for a Docker
# install plus this build -- see the Ch. 34 evidence file for the real
# numbers). Verified by static review only: manual line-by-line check
# (hadolint/`docker build` both unavailable here, confirmed directly, not
# assumed) plus `docker compose config` on docker-compose.console.yml.
#
# Two real stages, matching the book's own: node:20-alpine builds the real
# Angular production bundle; nginx:alpine serves it. See
# docker-compose.console.yml's header and docker/entrypoint-runtime-
# config.sh's own doc comment for the named gap this chapter's own
# investigation found (no Angular code reads the runtime-injected
# API_BASE_URL yet -- a pre-existing gap since the book's own Ch. 5, not
# something this Dockerfile closes).

FROM node:20-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine AS serve

# Real, confirmed output path -- checked directly against the actual dist/
# folder on disk, not assumed from angular.json's outputPath alone. The
# @angular-devkit/build-angular:application builder (Angular 17+) nests the
# real browser bundle one level deeper than outputPath, under browser/ --
# copying dist/nexus-console alone would serve an empty directory.
COPY --from=build /app/dist/nexus-console/browser /usr/share/nginx/html

COPY docker/entrypoint-runtime-config.sh /docker-entrypoint.d/40-runtime-config.sh
RUN chmod +x /docker-entrypoint.d/40-runtime-config.sh

EXPOSE 80

# Plain HTTP check against the served static root -- not a fabricated
# /health/ready the way the real .NET hosts have (Nexus1.Bff's own
# DbContextHealthCheck<T>, per-context, see Ch. 31). nginx has no
# application-level health logic to report here: a static file server is
# either answering requests or it isn't, so that is exactly what this
# checks, nothing more.
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget --spider -q http://localhost/ || exit 1
