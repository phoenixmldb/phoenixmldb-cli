# PhoenixmlDb CLI release history

Both `PhoenixmlDb.Xslt.Cli` (the `xslt` global tool) and `PhoenixmlDb.XQuery.Cli` (the `xquery` global tool) ship from this repo with a single shared version.

## 1.4.0 (2026-05-24)

**First release on the modern engine line.** Previously the CLI shipped against engine packages from the 1.0.x line, which by mid-2026 had fallen ~100 releases behind. This release moves the CLI to the current engine generation; subsequent releases will track the engine via CPM pin bumps as they happen.

### Engine pin updates
- `PhoenixmlDb.Core` 1.0.13 → 1.1.5
- `PhoenixmlDb.XQuery` 1.0.17 → 1.4.0
- `PhoenixmlDb.Xslt` 1.0.15 → 1.3.22

### CLI changes

**xquery CLI**
- New `--param name=value` (repeatable): binds external variable declarations in the query prolog. The query's `declare variable $foo external;` now picks up the bound value at execution time. Supports the same use cases as Saxon's `?foo=value` syntax.
- `--timing` now reports peak working set + total managed allocations alongside the existing parse/compile/execute breakdown.

**xslt CLI**
- `--timing` now reports peak working set + total managed allocations. Useful for verifying streaming actually keeps memory bounded under large inputs: peak working set stays roughly flat for streaming pipelines, grows linearly with input size for tree-based pipelines.

### Notable engine fixes now in CLI

**XSLT** (via PhoenixmlDb.Xslt 1.3.22)
- `TransformAsync(XdmSequence)` preserves map/array head across JSON-chained transforms (Martin Honnen)
- Streaming `xsl:merge` runtime, streaming `xsl:fork`
- Full source-location coverage (LSP foundation)
- Phase 2.5 perf — 2.6× win on hot paths
- `fn:transform` raw-delivery cross-store node fixes
- Streamable identity transforms
- `load-xquery-module` HTTPS resolution

**XQuery** (via PhoenixmlDb.XQuery 1.4.0)
- Schema-aware element construction: `declare construction preserve|strip` now affects runtime, XQTY0086 raised for namespace-sensitive types under preserve
- `fn:abs` / `fn:ceiling` / `fn:floor` / `fn:round` of `XsTypedInteger` no longer throws (1.3.16)
- JSON serializer conformance: QT3 `method-json` 64/74 → 73/74 (1.3.15)
- Per-character character maps inside JSON string content
- Per-module decimal-format scoping
- Cancellation tokens honored in FLWOR + quantified expressions
- Full source-location coverage

**Core** (via PhoenixmlDb.Core 1.1.5)
- `XdmAttribute.TypeAnnotation` populated from `XmlReader.SchemaInfo` during schema-validated load
- XsTypedInteger wrapper for XSD integer subtype identity
- Multi-target net8.0;net10.0

### Build-side
- `CA1031` added to project NoWarn for the defensible `catch { }` blocks around best-effort memory stats and top-level Main exception handling.

## Previous

The CLIs published at 1.0.0 (default version, never explicitly set) carried `PhoenixmlDb.*` 1.0.x pins. No prior `RELEASES.md` existed; older changes are visible in git history.
