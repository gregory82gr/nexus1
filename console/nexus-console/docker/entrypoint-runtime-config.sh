#!/bin/sh
# Real nginx:alpine idiom, not invented here: the official nginx image runs
# every executable *.sh file under /docker-entrypoint.d/ automatically, in
# alpha order, before nginx itself starts -- documented behavior of that
# image's own docker-entrypoint.sh. Placed as 40-runtime-config.sh so it
# runs after nginx's own default template scripts (10-, 15-, 20-) and before
# nginx forks (30- is the last stock hook).
#
# Twelve-factor idiom, matching the .NET side's own config-from-environment
# pattern (ConnectionStrings__X, BffContexts__Enabled__N): read the real
# backend URL from the environment at container START, not at image BUILD
# time, so the same image can point at a different backend per environment
# without a rebuild.
#
# NAMED GAP (Ch. 34 investigation, not fabricated as working): this script
# correctly writes the real $API_BASE_URL into a runtime JSON file the
# served static site could read -- but no Angular code in this app reads it
# yet. Every core/api/*.ts client (reactor-fleet-api.ts, overview-api.ts,
# security-api.ts, alarm-management-api.ts, and ~11 others) still hardcodes
# `const BFF_BASE_URL = 'http://localhost:5103'` as a build-time constant,
# each carrying the identical comment: "Named simplification, ahead of Ch.
# 5: the base URL is a plain constant here, not yet the chapter's own
# injected-config entrypoint script." That mechanism (the book's own Ch. 5)
# has never been built in this port, checked directly across every API
# client file before writing this script -- so this entrypoint is real,
# correct plumbing for when that chapter is tackled, and has no effect on
# this app's actual runtime behavior today.
set -eu

: "${API_BASE_URL:=http://localhost:5103}"

cat <<EOF > /usr/share/nginx/html/runtime-config.json
{"apiBaseUrl": "${API_BASE_URL}"}
EOF
