#!/usr/bin/env bash
# Smoke test a running instance of the API, wherever it runs (process, container, cluster).
#
# Usage: scripts/smoke-test.sh <base-url> [expected-version]
#   scripts/smoke-test.sh http://localhost:8080
#   scripts/smoke-test.sh http://localhost:8080 "$GITHUB_SHA"
# Needs curl and jq. Exits non-zero on the first failed check.
set -euo pipefail

base="${1:?usage: smoke-test.sh <base-url> [expected-version]}"
base="${base%/}"
expected_version="${2:-}"
body_file="$(mktemp)"
trap 'rm -f "$body_file"' EXIT

pass() { echo "ok   $*"; }
fail() { echo "FAIL $*" >&2; echo "     response: $(head -c 500 "$body_file")" >&2; exit 1; }

# request METHOD PATH [JSON]: sets $status and leaves the body in $body_file.
request() {
    local args=(--silent --show-error --max-time 10 --output "$body_file" --write-out '%{http_code}' -X "$1")
    [ $# -ge 3 ] && args+=(-H 'Content-Type: application/json' --data "$3")
    status="$(curl "${args[@]}" "$base$2")" || status="000"
}

expect() { # expect STATUS DESCRIPTION
    [ "$status" = "$1" ] || fail "$2: HTTP $status, want $1"
    pass "$2 (HTTP $status)"
}

for _ in $(seq 1 30); do
    request GET /readyz
    [ "$status" = 200 ] && break
    sleep 1
done
expect 200 "GET /readyz becomes ready within 30 s"

request GET /healthz
expect 200 "GET /healthz"
[ "$(cat "$body_file")" = "Healthy" ] || fail "/healthz body is not 'Healthy'"

request GET /
expect 200 "GET /"
[ "$(jq -r .service "$body_file")" = "devops-platform-api" ] || fail "GET / has the wrong service name"
if [ -n "$expected_version" ]; then
    actual="$(jq -r .version "$body_file")"
    [ "$actual" = "$expected_version" ] || fail "GET / reports version '$actual', want '$expected_version'"
    pass "version is $expected_version"
fi

request POST /api/todos '{"title":"smoke test"}'
expect 201 "POST /api/todos"
id="$(jq -r .id "$body_file")"

request GET "/api/todos/$id"
expect 200 "GET /api/todos/{id}"
[ "$(jq -r .title "$body_file")" = "smoke test" ] || fail "the created todo has the wrong title"

request POST /api/todos '{"title":""}'
expect 400 "POST /api/todos with an empty title is rejected"

request DELETE "/api/todos/$id"
expect 204 "DELETE /api/todos/{id}"

request GET "/api/todos/$id"
expect 404 "GET /api/todos/{id} after delete"

echo "smoke test passed: $base"
