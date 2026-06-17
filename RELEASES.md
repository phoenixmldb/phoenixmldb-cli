# PhoenixmlDb CLI release history

Both `PhoenixmlDb.Xslt.Cli` (the `xslt` global tool) and `PhoenixmlDb.XQuery.Cli` (the `xquery` global tool) ship from this repo with a single shared version.

## 1.4.9 (2026-06-16)

Aligns both CLIs to the current engine generation: PhoenixmlDb.Core 1.1.9, PhoenixmlDb.XQuery 1.4.5, PhoenixmlDb.Xslt 1.4.9.

### Fix: both CLIs synced to the canonical engine-repo drivers

Both tools had drifted into stale forks. The `xquery` CLI carried a bespoke node environment that couldn't resolve computed result nodes, so computed attributes (`element e { attribute a {...} }`) serialized as element text instead of attributes; it now uses the canonical store-backed environment and serializer, so output matches the engine exactly (including correct attribute-value whitespace escaping via the shared CharacterEscaper, and `fn:transform` support). The `xslt` CLI driver is synced to the current engine-repo version (adds `--no-stream` and related driver updates). Both CLIs now behave identically to their in-engine-repo counterparts.

### Fix: `xquery` CLI attribute-value whitespace escaping

The `xquery` CLI's result serializer escaped attribute values with its own copy of the escaping rules, which left tab/newline/carriage-return literal — XML attribute-value normalization then collapsed them to spaces on re-read. It now routes attribute escaping through the shared `PhoenixmlDb.Xdm.Serialization.CharacterEscaper`, emitting `&#x9;`/`&#xA;`/`&#xD;`, so the CLI's output matches the engine library exactly. Requires PhoenixmlDb.Core 1.1.9 and PhoenixmlDb.XQuery 1.4.5.

## 1.4.8 (2026-06-14)

Aligns the CLIs to the current engine generation: PhoenixmlDb.Xslt 1.4.8, PhoenixmlDb.XQuery 1.4.4, PhoenixmlDb.Core 1.1.8.

Picks up XSLT 1.4.8: a parsed JSON array fed to `apply-templates` is treated as a single item rather than iterated as its members, and the item-processing instructions (`for-each`, `iterate`, `for-each-group`, `perform-sort`, `merge`) no longer flatten a selected array/map. (1.4.7 was never published; its JSON-`XdmSequence` round-trip fixes are included here.)

## 1.4.7 (2026-06-14)

Aligns the CLIs to the current engine generation: PhoenixmlDb.Xslt 1.4.7, PhoenixmlDb.XQuery 1.4.4, PhoenixmlDb.Core 1.1.8.

Picks up the XSLT 1.4.7 fix: JSON map/array values fed through the `XdmSequence` transform overload serialize as JSON instead of a CLR type name, and a top-level JSON array is no longer unwrapped to its first member. Everything in 1.4.6 below is included (1.4.6 was never published).

## 1.4.6 (2026-06-14)

Aligns the CLIs to the current engine generation: PhoenixmlDb.Xslt 1.4.6, PhoenixmlDb.XQuery 1.4.4, PhoenixmlDb.Core 1.1.8.

### XSLT
- Deeply-recursive stylesheets now raise a catchable error instead of exhausting the native stack and crashing the process.
- `key()` now matches attribute/text `use` values (which atomize to `xs:untypedAtomic`) against string lookup keys, so keys defined with `use="@attr"` resolve.
- `parse-json` of a top-level JSON array yields `array(*)`, and `indent="yes"` applies to output built from a JSON/map initial context item.

### XQuery
- Numeric operators and aggregates (unary `+`/`-`, `avg`, `sum`, `min`, `max`, `round-half-to-even`, `abs`, `ceiling`, `floor`) handle derived integer types (`xs:long`, `xs:int`, …) correctly and exactly.
- `array:sort` honors the default/declared or supplied collation instead of always sorting by codepoint.
- A free-standing attribute node under the xml/html output method raises `SENR0001`; the xml/html methods escape CR/NEL/LINE-SEPARATOR/C1 controls as numeric character references.

## 1.4.1 (2026-06-02)

Catch-up release aligning CLIs to current engine generation.

### Engine bumps

- `PhoenixmlDb.XQuery` 1.4.0 → **1.4.1** — QT3 production sweep round: EQName parser fix (`Q{uri}localname` with reserved local names), `fn:deep-equal` collation atomization, `fn:format-number` module-local decimal-format isolation, module library functions use own copy-namespaces mode, parse-time XPST0051 honors constructor-local default-element-namespace, value-comparison XPTY0004 for date/time vs incompatible type, `function(*)` matches map/array values, more.
- `PhoenixmlDb.Xslt` 1.3.22 → **1.4.1** — two-release jump covering:
  - **1.3.23 streaming output overhaul** (Martin Honnen 2026-05-28 memory report): incremental `TransformAsync(XmlReader, TextWriter)` / `TransformAsync(Stream, TextWriter)`, peak working-set ~270× reduction for streamed identity (17.2 MiB on 1M items vs hundreds of MiB before); `TransformAsync(Stream, Stream)` skips `ReadToEndAsync` fast-path when streamable; for-each in `xsl:source-document` now drives the streaming pass.
  - **1.4.0 streaming conformance overhaul**: W3C XSLT 3.0 streaming jumped 71.3% → 80.5% (1898/2358 tests, 36+ sets at 100%). Mixed sequences in for-each select; `text()` KindTest tail; attribute-axis tail with attribute-as-context-item; last-step predicates; `fn:data()` unwrap; SimpleMap recognition; descendant axis with nested same-name; snapshot+tail composition; function-item args for HOFs; motionless predicates on aggregation watchers; `XdmElement._stringValue` populated by variable construction; `xsl:copy/@select` arity check.
  - **1.4.1 hotfix** (Martin Honnen 2026-06-01 report): Blazor WebAssembly DocBook XSLT regression. `XsltTransformProvider.LoadStylesheetAsync` now consults `PreloadedResources` before `HttpResourceLoader` (was throwing `FOXT0001` on WASM); `HttpImportPreloader` walker recognizes `(fn:)?transform()` with literal `stylesheet-location` URLs (DocBook TNG pipeline modules now auto-discovered).

### CLI behavior

No CLI-side feature changes in this release — pure engine-version catch-up. The CLI tools transparently inherit all engine improvements above.

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
