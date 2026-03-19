# PhoenixML CLI Tools

Command-line tools for XSLT and XQuery processing, built on the [PhoenixML](https://github.com/phoenixmldb) engine. Installable as .NET global tools.

## Tools

### `xslt` — XSLT 3.0/4.0 Transformer

Transform XML documents using XSLT 3.0 and 4.0 stylesheets.

```bash
# Transform XML with a stylesheet
xslt stylesheet.xsl input.xml

# Write output to a file
xslt -o result.html report.xsl data.xml

# Call a named template (no source document needed)
xslt -it xsl:initial-template generate.xsl

# Pass parameters
xslt -p year=2026 -p format=html style.xsl data.xml

# Pipe input from stdin
curl https://example.com/feed.xml | xslt atom-to-html.xsl

# Secondary result documents written to a directory
xslt --output-dir ./pages book-to-html.xsl book.xml
```

**Options:**

| Flag | Description |
|------|-------------|
| `-o, --output <path>` | Write primary output to file instead of stdout |
| `--output-dir <dir>` | Base directory for `xsl:result-document` output |
| `-p, --param <name>=<value>` | Set a stylesheet parameter (repeatable) |
| `-it, --initial-template <name>` | Start with a named template |
| `-im, --initial-mode <name>` | Set the initial processing mode |
| `-v, --verbose` | Show detailed error information |

### `xquery` — XPath/XQuery 4.0 Processor

Evaluate XQuery expressions against XML files, directories, or URLs.

```bash
# Query an XML file
xquery '//title' books.xml

# Count elements
xquery 'count(//item)' catalog.xml

# Read query from a file
xquery -f transform.xq input.xml

# Query all XML files in a directory
xquery 'collection()//product' ./data/

# Fetch and query a remote document
xquery 'doc("https://example.com/feed.xml")//entry'

# Pipe XML from stdin
curl https://example.com/data.xml | xquery '//item/@name'
```

**Options:**

| Flag | Description |
|------|-------------|
| `-f, --file <path>` | Read XQuery from a file instead of inline |
| `-o, --output <method>` | Output method: `adaptive` (default), `xml`, `text` |
| `--stdin` | Read XML input from stdin (automatic when piped) |
| `-v, --verbose` | Show detailed error information |

## Installation

```bash
# Install both tools
dotnet tool install -g PhoenixmlDb.Xslt.Cli
dotnet tool install -g PhoenixmlDb.XQuery.Cli
```

## Features

- **XSLT 3.0** — Packages, streaming, higher-order functions, maps, arrays, JSON
- **XSLT 4.0** — Record types, `xsl:switch`, `xsl:for-each-member`, CSV output
- **XQuery 4.0** — 76+ new functions, `for member`, record/enum/union types, keyword arguments
- **XPath 4.0** — Thin arrow operator (`->`), `otherwise`, enhanced `for` expressions
- **No database required** — Pure XML processing, no storage engine dependency
- **Cross-platform** — Runs anywhere .NET 10 runs (Linux, macOS, Windows)

## License

Apache 2.0 — see [LICENSE](LICENSE)

## Related Projects

- [phoenixmldb-core](https://github.com/phoenixmldb/phoenixmldb-core) — Core types and XDM
- [phoenixmldb-xquery](https://github.com/phoenixmldb/phoenixmldb-xquery) — XPath/XQuery 4.0 engine
- [phoenixmldb-xslt](https://github.com/phoenixmldb/phoenixmldb-xslt) — XSLT 4.0 engine
