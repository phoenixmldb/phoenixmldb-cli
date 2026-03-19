# PhoenixmlDb CLI

Command-line tools for XSLT and XQuery processing, built on the PhoenixML engine.

## Tools

### phoenixml-xquery
Evaluate XQuery expressions and queries from the command line.

```bash
# Evaluate an expression
phoenixml-xquery -e "1 + 1"

# Query a file
phoenixml-xquery -f query.xq -i input.xml
```

### phoenixml-xslt (planned)
Transform XML using XSLT stylesheets.

```bash
# Transform XML
phoenixml-xslt -s stylesheet.xsl -i input.xml -o output.xml
```

## Installation

```bash
dotnet tool install -g PhoenixmlDb.Cli
```

## License

Apache 2.0 — see [LICENSE](LICENSE)

## Related Projects

- [phoenixmldb-core](https://github.com/phoenixmldb/phoenixmldb-core) — Core types and XDM
- [phoenixmldb-xquery](https://github.com/phoenixmldb/phoenixmldb-xquery) — XPath/XQuery 4.0 engine
- [phoenixmldb-xslt](https://github.com/phoenixmldb/phoenixmldb-xslt) — XSLT 4.0 engine
