#!/usr/bin/env bash
# Fails when a CLI source file here has drifted from the canonical copy in the engine repo.
#
# These tools were forks. Not stale pins — forks, with different code. PhoenixmlDb.Xslt.Cli was
# missing the guard from BUGS.md #18, so `xslt -it name` with redirected stdin hung forever while
# the identically-named tool from the engine repo returned in under a second. It read sources
# with File.ReadAllTextAsync instead of XmlSourceReader, and the XQuery one could not serialise
# XDM arrays in adaptive output. No version bump fixes any of that: the fork's own source was
# behind, and the version number said nothing about it.
#
# Two packages ship a command called `xslt`. They must be the same program. This check is what
# makes "the same program" true rather than aspirational.
#
# Canonical is the engine repo, because that is where the engine is developed and where a fix
# lands first. Both repos are public, so this needs no token.
#
# A difference is a FAILURE, not a warning: it means one of the two tools is missing a fix.
# Resolve it by copying the canonical file, not by editing the canonical to match.
set -uo pipefail

RAW=https://raw.githubusercontent.com/phoenixmldb
REF="${CLI_SOURCE_REF:-main}"

# local path : canonical repo : canonical path
FILES=(
  "src/PhoenixmlDb.Xslt.Cli/Program.cs:phoenixmldb-xslt:src/PhoenixmlDb.Xslt.Cli/Program.cs"
  "src/PhoenixmlDb.XQuery.Cli/Program.cs:phoenixmldb-xquery:src/PhoenixmlDb.XQuery.Cli/Program.cs"
  "src/PhoenixmlDb.XQuery.Cli/ResultSerializer.cs:phoenixmldb-xquery:src/PhoenixmlDb.XQuery.Cli/ResultSerializer.cs"
  "src/PhoenixmlDb.XQuery.Cli/DocumentEnvironment.cs:phoenixmldb-xquery:src/PhoenixmlDb.XQuery.Cli/DocumentEnvironment.cs"
)

fail=0
for entry in "${FILES[@]}"; do
  local_path="${entry%%:*}"; rest="${entry#*:}"
  repo="${rest%%:*}"; remote_path="${rest#*:}"
  [ -f "$local_path" ] || { echo "FAIL  $local_path missing here"; fail=1; continue; }
  tmp=$(mktemp)
  if ! curl -sS --fail --max-time 30 "$RAW/$repo/$REF/$remote_path" -o "$tmp" 2>/dev/null; then
    echo "FAIL  $local_path: could not fetch canonical from $repo@$REF (cannot verify — not a pass)"
    fail=1; rm -f "$tmp"; continue
  fi
  if diff -q "$local_path" "$tmp" >/dev/null 2>&1; then
    echo "ok    $local_path matches $repo@$REF"
  else
    n=$(diff "$local_path" "$tmp" | grep -cE '^[<>]')
    echo "FAIL  $local_path has drifted from $repo@$REF ($n differing lines)"
    echo "      Copy the canonical file over this one:"
    echo "        curl -sS $RAW/$repo/$REF/$remote_path -o $local_path"
    fail=1
  fi
  rm -f "$tmp"
done
exit $fail
