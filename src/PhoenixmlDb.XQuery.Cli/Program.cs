using PhoenixmlDb.Core;
using PhoenixmlDb.XQuery.Cli;
using PhoenixmlDb.XQuery.Execution;
using PhoenixmlDb.Xdm.Nodes;

var options = CliOptions.Parse(args);

if (options.ShowHelp || (options.Query == null && options.QueryFile == null))
{
    PrintUsage();
    return options.ShowHelp ? 0 : 1;
}

if (options.ShowVersion)
{
    Console.WriteLine("xquery 1.0.0-preview.1 (PhoenixmlDb)");
    return 0;
}

try
{
    // Resolve the query text
    string query;
    if (options.QueryFile != null)
    {
        if (!File.Exists(options.QueryFile))
        {
            await Console.Error.WriteLineAsync($"Error: Query file not found: {options.QueryFile}").ConfigureAwait(true);
            return 1;
        }
        query = await File.ReadAllTextAsync(options.QueryFile).ConfigureAwait(true);
    }
    else
    {
        query = options.Query!;
    }

    // Set up the document environment
    var env = new DocumentEnvironment();
    XdmDocument? contextDocument = null;

    // Load input sources
    foreach (var source in options.Sources)
    {
        if (Uri.TryCreate(source, UriKind.Absolute, out var uri) &&
            (uri.Scheme == "http" || uri.Scheme == "https"))
        {
            var doc = env.LoadFromUrl(source);
            contextDocument ??= doc;
        }
        else if (Directory.Exists(source))
        {
            var docs = env.LoadDirectory(source);
            if (docs.Count > 0)
                contextDocument ??= docs[0];
        }
        else if (File.Exists(source))
        {
            var doc = env.LoadFile(source);
            contextDocument ??= doc;
        }
        else
        {
            await Console.Error.WriteLineAsync($"Warning: Source not found: {source}").ConfigureAwait(true);
        }
    }

    // Read from stdin if no sources given and stdin is piped
    if (options.Sources.Count == 0 && options.ReadStdin)
    {
        using var reader = new StreamReader(Console.OpenStandardInput());
        var xml = await reader.ReadToEndAsync().ConfigureAwait(true);
        if (!string.IsNullOrWhiteSpace(xml))
        {
            contextDocument = env.LoadFromString(xml, "stdin:");
        }
    }

    // Create and configure the query engine
    var engine = new QueryEngine(
        nodeProvider: env,
        documentResolver: env);

    // Execute the query
    var serializer = new ResultSerializer(env, Console.Out, options.OutputMethod);
    var hasOutput = false;

    await foreach (var result in engine.ExecuteAsync(query))
    {
        serializer.Serialize(result);
        hasOutput = true;
    }

    if (hasOutput)
    {
        serializer.WriteNewline();
    }

    return 0;
}
catch (Exception ex) when (ex is PhoenixmlDb.XQuery.Parser.XQueryParseException)
{
    await Console.Error.WriteLineAsync($"Parse error: {ex.Message}").ConfigureAwait(true);
    return 2;
}
catch (XQueryRuntimeException ex)
{
    await Console.Error.WriteLineAsync($"Runtime error [{ex.ErrorCode}]: {ex.Message}").ConfigureAwait(true);
    return 3;
}
catch (Exception ex)
{
    await Console.Error.WriteLineAsync($"Error: {ex.Message}").ConfigureAwait(true);
    if (options.Verbose)
    {
        await Console.Error.WriteLineAsync(ex.StackTrace).ConfigureAwait(true);
    }

    throw;
}

static void PrintUsage()
{
    Console.Error.WriteLine("""
        Usage: xquery [options] <expression> [sources...]
               xquery [options] -f <query-file> [sources...]
               command | xquery [options] <expression>

        Execute XQuery expressions against XML files, directories, or URLs.

        Arguments:
          <expression>       Inline XQuery expression to execute
          [sources...]       XML files, directories, or URLs to query

        Options:
          -f, --file <path>  Read XQuery from a file instead of inline
          -o, --output <method>
                             Output method: adaptive (default), xml, text
          --stdin            Read XML input from stdin (automatic when piped)
          -v, --verbose      Show detailed error information
          -h, --help         Show this help message
          --version          Show version information

        Sources:
          Files              Path to an XML file (loaded as fn:doc() target)
          Directories        Path to a directory (all *.xml files loaded)
          URLs               HTTP/HTTPS URL to fetch XML from

        When a single source is provided, it becomes the context item (.).
        All sources are available via fn:doc($uri) by their path or URL.
        fn:collection() returns all loaded documents.

        Examples:
          xquery '//title' books.xml
          xquery 'count(//item)' catalog.xml
          xquery -f transform.xq input.xml
          xquery 'collection()//product' ./data/
          xquery 'doc("http://example.com/feed.xml")//entry'
          curl http://example.com/data.xml | xquery '//item/@name'
        """);
}

/// <summary>
/// Parsed command-line options.
/// </summary>
file sealed class CliOptions
{
    public string? Query { get; init; }
    public string? QueryFile { get; init; }
    public List<string> Sources { get; init; } = [];
    public OutputMethod OutputMethod { get; init; } = OutputMethod.Adaptive;
    public bool ReadStdin { get; init; }
    public bool ShowHelp { get; init; }
    public bool ShowVersion { get; init; }
    public bool Verbose { get; init; }

    public static CliOptions Parse(string[] args)
    {
        string? query = null;
        string? queryFile = null;
        var sources = new List<string>();
        var outputMethod = OutputMethod.Adaptive;
        var readStdin = false;
        var showHelp = false;
        var showVersion = false;
        var verbose = false;
        var expectingFile = false;
        var expectingOutput = false;

        foreach (var arg in args)
        {
            if (expectingFile)
            {
                queryFile = arg;
                expectingFile = false;
                continue;
            }

            if (expectingOutput)
            {
                outputMethod = arg.ToLowerInvariant() switch
                {
                    "xml" => OutputMethod.Xml,
                    "text" => OutputMethod.Text,
                    _ => OutputMethod.Adaptive
                };
                expectingOutput = false;
                continue;
            }

            switch (arg)
            {
                case "-h" or "--help":
                    showHelp = true;
                    break;
                case "--version":
                    showVersion = true;
                    break;
                case "-f" or "--file":
                    expectingFile = true;
                    break;
                case "-o" or "--output":
                    expectingOutput = true;
                    break;
                case "--stdin":
                    readStdin = true;
                    break;
                case "-v" or "--verbose":
                    verbose = true;
                    break;
                default:
                    if (query == null && queryFile == null)
                        query = arg;
                    else
                        sources.Add(arg);
                    break;
            }
        }

        // Auto-detect stdin when piped and no sources given
        if (!readStdin && sources.Count == 0 && !Console.IsInputRedirected)
        {
            // Not piped, no stdin
        }
        else if (!readStdin && sources.Count == 0 && Console.IsInputRedirected)
        {
            readStdin = true;
        }

        return new CliOptions
        {
            Query = query,
            QueryFile = queryFile,
            Sources = sources,
            OutputMethod = outputMethod,
            ReadStdin = readStdin,
            ShowHelp = showHelp,
            ShowVersion = showVersion,
            Verbose = verbose
        };
    }
}
