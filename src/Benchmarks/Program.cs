using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Issue4852Benchmarks;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var options = args.Chunk(2).ToDictionary(x => x[0].TrimStart('-'), x => x[1]);
        string Get(string key, string fallback) => options.GetValueOrDefault(key, fallback);
        var version = Get("version", "candidate");
        var mode = Get("mode", "table");
        var output = Get("output", "samples.jsonl");
        var samples = int.Parse(Get("samples", "8"));
        var warmups = int.Parse(Get("warmups", "4"));
        var metadata = new
        {
            kind = "environment", version, mode, runtime = RuntimeInformation.FrameworkDescription,
            os = RuntimeInformation.OSDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            logicalProcessors = Environment.ProcessorCount, serverGc = System.Runtime.GCSettings.IsServerGC,
            tieredCompilation = Environment.GetEnvironmentVariable("DOTNET_TieredCompilation"),
            readyToRun = Environment.GetEnvironmentVariable("DOTNET_ReadyToRun"),
            startedUtc = DateTimeOffset.UtcNow, stopwatchFrequency = Stopwatch.Frequency,
            captureUnmatchedValues = typeof(AntDomComponentBase).GetProperty("AdditionalAttributes")?
                .GetCustomAttribute<ParameterAttribute>()?.CaptureUnmatchedValues,
            librarySha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                File.ReadAllBytes(typeof(AntDomComponentBase).Assembly.Location)))
        };
        using var writer = new StreamWriter(output, append: false) { AutoFlush = true };
        writer.WriteLine(JsonSerializer.Serialize(metadata));
        if (mode == "merge")
        {
            MergeBenchmarks.Run(writer, version, samples, warmups, bool.Parse(Get("empty-cases", "false")));
            return;
        }

        int rows = int.Parse(Get("rows", "1000")), columns = int.Parse(Get("columns", "10"));
        var cellMode = Get("cell-mode", "none");
        using var services = new ServiceCollection().AddLogging().AddAntDesign()
            .AddSingleton<IJSRuntime, NoOpJsRuntime>().BuildServiceProvider();
        var data = new[] { MakeRows(rows, columns, "A"), MakeRows(rows, columns, "B") };

        // Warm initialization, disposal, parameter setters, and both update paths.
        for (var i = 0; i < warmups; i++)
        {
            await using var warm = new TableSession(services, data[0], columns, cellMode);
            await warm.Mount();
            await warm.Validate(rows, columns);
            await warm.Update(data[0]);
            await warm.Update(data[1]);
        }

        for (var sample = 0; sample < samples; sample++)
        {
            // Exclude forced collection, disposal, fixture/data creation, JSON and validation
            // from operation timing. Actual automatic GCs remain inside the timed operation.
            Collect();
            await using var session = new TableSession(services, data[0], columns, cellMode);
            var mount = await Measure(session.Renderer, session.Mount);
            await session.Validate(rows, columns);
            Write("mount", mount);

            Collect();
            var same = await Measure(session.Renderer, () => session.Update(data[0]));
            await session.Validate(rows, columns);
            Write("same-data-update", same);

            Collect();
            var changed = await Measure(session.Renderer, () => session.Update(data[1]));
            await session.Validate(rows, columns, "B-0-0");
            Write("changed-data-update", changed);

            void Write(string operation, Measurement result) => writer.WriteLine(JsonSerializer.Serialize(new
            {
                kind = "sample", version, operation, rows, columns, cellMode, sample,
                outputSha256 = session.OutputSha256,
                validatedCells = rows * columns, validatedHeaders = columns,
                result.elapsedNs, result.allocatedBytes, result.gen0, result.gen1, result.gen2,
                result.batches, result.updatedComponents, result.edits
            }));
        }
    }

    private static BenchRow[] MakeRows(int rows, int columns, string prefix) => Enumerable.Range(0, rows)
        .Select(r => new BenchRow(r, Enumerable.Range(0, columns).Select(c => $"{prefix}-{r}-{c}").ToArray())).ToArray();

    internal static void Collect()
    {
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
    }

    private static async Task<Measurement> Measure(HeadlessRenderer renderer, Func<Task> action)
    {
        renderer.ResetCounters();
        var collections = Enumerable.Range(0, 3).Select(GC.CollectionCount).ToArray();
        var allocated = GC.GetTotalAllocatedBytes(precise: true);
        var start = Stopwatch.GetTimestamp();
        await action();
        var elapsed = Stopwatch.GetElapsedTime(start).TotalNanoseconds;
        allocated = GC.GetTotalAllocatedBytes(precise: true) - allocated;
        return new(elapsed, allocated, GC.CollectionCount(0) - collections[0],
            GC.CollectionCount(1) - collections[1], GC.CollectionCount(2) - collections[2],
            renderer.Batches, renderer.UpdatedComponents, renderer.Edits);
    }

    private record Measurement(double elapsedNs, long allocatedBytes, int gen0, int gen1, int gen2,
        int batches, int updatedComponents, int edits);
}

public sealed record BenchRow(int Id, string[] Values)
{
    public BenchRow() : this(0, Enumerable.Repeat(string.Empty, 64).ToArray()) { }
}

internal sealed class TableSession : IAsyncDisposable
{
    public HeadlessRenderer Renderer { get; }
    private readonly Dictionary<string, object?> _parameters;
    private readonly string _cellMode;
    private int _rootId;
    public string OutputSha256 { get; private set; } = "";

    private static readonly Dictionary<string, object> LegacyAttributes = new()
    {
        ["data-legacy"] = "cell", ["title"] = "legacy", ["aria-label"] = "legacy", ["data-source"] = "legacy"
    };
    private static readonly Dictionary<string, object> SharedAttributes = new()
    {
        ["data-shared"] = "cell", ["title"] = "shared", ["aria-label"] = "shared", ["data-action"] = "open"
    };

    public TableSession(IServiceProvider services, BenchRow[] data, int columns, string cellMode)
    {
        _cellMode = cellMode;
        Renderer = new(services);
        RenderFragment<BenchRow> content = row => builder =>
        {
            for (var col = 0; col < columns; col++)
            {
                builder.OpenComponent<Column<string>>(0);
                builder.SetKey(col);
                builder.AddAttribute(1, "Title", $"Column {col}");
                builder.AddAttribute(2, "Field", row.Values[col]);
                if (cellMode is "legacy" or "both")
                    builder.AddAttribute(3, "OnCell", (Func<AntDesign.TableModels.CellData, Dictionary<string, object>>)(_ => LegacyAttributes));
                if (cellMode is "shared" or "both")
                    builder.AddAttribute(4, "AdditionalAttributes", SharedAttributes);
                builder.CloseComponent();
            }
        };
        // No AdditionalAttributes parameter (even null) is supplied in "none" mode.
        // No pagination or virtualization: all requested rows must actually render.
        _parameters = new()
        {
            ["DataSource"] = data, ["HidePagination"] = true, ["PageSize"] = data.Length,
            ["RowKey"] = (Func<BenchRow, object>)(row => row.Id),
            ["ChildContent"] = content, ["RerenderStrategy"] = RerenderStrategy.Always
        };
    }

    public Task Mount() => Renderer.Dispatcher.InvokeAsync(async () =>
    {
        _rootId = Renderer.AttachTable();
        await Renderer.Render(_rootId, ParameterView.FromDictionary(_parameters));
        Renderer.ThrowIfFaulted();
    });

    public Task Update(BenchRow[] data) => Renderer.Dispatcher.InvokeAsync(async () =>
    {
        _parameters["DataSource"] = data;
        await Renderer.Render(_rootId, ParameterView.FromDictionary(_parameters));
        Renderer.ThrowIfFaulted();
    });

    public Task Validate(int rows, int columns, string? expectedText = null) => Renderer.Dispatcher.InvokeAsync(() =>
    {
        var snapshot = Renderer.Inspect(_rootId);
        OutputSha256 = Renderer.OutputHash(_rootId);
        if (snapshot.Cells != rows * columns || snapshot.Headers != columns)
            throw new InvalidOperationException($"Expected {rows * columns} cells/{columns} headers; got {snapshot}.");
        if (expectedText != null && !snapshot.Text.Contains(expectedText))
            throw new InvalidOperationException($"Update was skipped: missing {expectedText}.");
        var expectedShared = _cellMode is "shared" or "both" ? rows * columns + columns : 0;
        if (snapshot.Shared != expectedShared)
            throw new InvalidOperationException($"Expected {expectedShared} shared attributes; got {snapshot.Shared}.");
    });

    public ValueTask DisposeAsync() => Renderer.DisposeAsync();
}

internal sealed class HeadlessRenderer : Renderer
{
    public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();
    private Exception? _error;
    public int Batches { get; private set; }
    public int UpdatedComponents { get; private set; }
    public int Edits { get; private set; }

    public HeadlessRenderer(IServiceProvider services) : base(services, services.GetRequiredService<ILoggerFactory>()) { }
    public int AttachTable() => AssignRootComponentId(InstantiateComponent(typeof(Table<BenchRow>)));
    public Task Render(int id, ParameterView parameters) => RenderRootComponentAsync(id, parameters);
    protected override void HandleException(Exception exception) => _error ??= exception;
    public void ThrowIfFaulted() { if (_error != null) throw new InvalidOperationException("Render failed", _error); }
    public void ResetCounters() { Batches = UpdatedComponents = Edits = 0; }
    protected override Task UpdateDisplayAsync(in RenderBatch batch)
    {
        Batches++;
        UpdatedComponents += batch.UpdatedComponents.Count;
        for (var i = 0; i < batch.UpdatedComponents.Count; i++) Edits += batch.UpdatedComponents.Array[i].Edits.Count;
        return Task.CompletedTask;
    }

    // Native output only; component parameter frames and runtime event handler IDs are not DOM.
    // Delegate values are represented by their type. All scalar attribute values are retained.
    public string OutputHash(int root)
    {
        var tokens = new List<string?>();
        void Visit(int id)
        {
            var frames = GetCurrentRenderTreeFrames(id);
            Walk(frames.Array, 0, frames.Count);
        }
        void Walk(RenderTreeFrame[] frames, int start, int end)
        {
            for (var i = start; i < end; i++)
            {
                var f = frames[i];
                switch (f.FrameType)
                {
                    case RenderTreeFrameType.Component:
                        Visit(f.ComponentId); i += f.ComponentSubtreeLength - 1; break;
                    case RenderTreeFrameType.Element:
                        tokens.Add("element"); tokens.Add(f.ElementName);
                        Walk(frames, i + 1, i + f.ElementSubtreeLength);
                        tokens.Add("end"); i += f.ElementSubtreeLength - 1; break;
                    case RenderTreeFrameType.Region:
                        Walk(frames, i + 1, i + f.RegionSubtreeLength);
                        i += f.RegionSubtreeLength - 1; break;
                    case RenderTreeFrameType.Attribute:
                        tokens.Add("attribute"); tokens.Add(f.AttributeName);
                        tokens.Add(f.AttributeValue is Delegate d ? d.GetType().FullName :
                            Convert.ToString(f.AttributeValue, System.Globalization.CultureInfo.InvariantCulture));
                        break;
                    case RenderTreeFrameType.Text: tokens.Add("text"); tokens.Add(f.TextContent); break;
                    case RenderTreeFrameType.Markup: tokens.Add("markup"); tokens.Add(f.MarkupContent); break;
                }
            }
        }
        Visit(root);
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            JsonSerializer.SerializeToUtf8Bytes(tokens)));
    }

    public Snapshot Inspect(int root)
    {
        int cells = 0, headers = 0, shared = 0;
        var text = new HashSet<string>();
        void Visit(int id)
        {
            var frames = GetCurrentRenderTreeFrames(id);
            for (var i = 0; i < frames.Count; i++)
            {
                ref var frame = ref frames.Array[i];
                switch (frame.FrameType)
                {
                    case RenderTreeFrameType.Component: Visit(frame.ComponentId); break;
                    case RenderTreeFrameType.Element:
                        if (frame.ElementName == "td") cells++;
                        if (frame.ElementName == "th") headers++;
                        break;
                    case RenderTreeFrameType.Attribute:
                        if (frame.AttributeName == "data-shared") shared++;
                        break;
                    case RenderTreeFrameType.Text: text.Add(frame.TextContent); break;
                    case RenderTreeFrameType.Markup: text.Add(frame.MarkupContent); break;
                }
            }
        }
        Visit(root);
        return new(cells, headers, shared, text);
    }
    public record Snapshot(int Cells, int Headers, int Shared, HashSet<string> Text);
}

// Real .NET component lifecycle and render-tree diff run; browser/network work does not.
// Table's only JS call in this non-scrolling fixture is event-listener registration.
internal sealed class NoOpJsRuntime : IJSRuntime
{
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => new(default(TValue)!);
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        => InvokeAsync<TValue>(identifier, args);
}

internal static class MergeBenchmarks
{
    private sealed class Probe : AntDomComponentBase { }
    private delegate IReadOnlyDictionary<string, object>? Merger(AntDomComponentBase component, IReadOnlyDictionary<string, object>? legacy);
    private static object? _sink;

    public static void Run(StreamWriter writer, string version, int samples, int warmups, bool extendedEmptyCases)
    {
        var method = typeof(AntDomComponentBase).GetMethod("GetAdditionalAttributes", BindingFlags.Instance | BindingFlags.NonPublic);
        var merge = method == null ? LegacyPath : method.CreateDelegate<Merger>();
        var property = typeof(AntDomComponentBase).GetProperty("AdditionalAttributes");
        foreach (var size in new[] { 0, 1, 4, 8, 16 })
        foreach (var scenario in size == 0 ? new[] { "both-null", "both-empty" }
            : extendedEmptyCases ? new[] { "legacy-only", "shared-only", "both-disjoint", "both-overlap", "legacy-empty", "shared-empty" }
            : new[] { "legacy-only", "shared-only", "both-disjoint", "both-overlap" })
        {
            var probe = new Probe();
            Dictionary<string, object>? shared = scenario == "both-null" || scenario == "legacy-only" ? null : new();
            Dictionary<string, object>? legacy = scenario == "both-null" || scenario == "shared-only" ? null : new();
            for (var i = 0; i < size; i++)
            {
                if (shared != null && scenario != "shared-empty") shared[$"data-shared-{i}"] = "shared";
                if (legacy != null && scenario != "legacy-empty") legacy[scenario == "both-overlap" ? $"data-shared-{i}" : $"data-legacy-{i}"] = "legacy";
            }
            property?.SetValue(probe, shared);
            var operations = shared != null && legacy != null ? 100_000 : 2_000_000;
            for (var warmup = 0; warmup < warmups; warmup++) Loop(merge, probe, legacy, operations);
            for (var sample = 0; sample < samples; sample++)
            {
                Program.Collect();
                var allocated = GC.GetAllocatedBytesForCurrentThread();
                var start = Stopwatch.GetTimestamp();
                Loop(merge, probe, legacy, operations);
                var elapsed = Stopwatch.GetElapsedTime(start).TotalNanoseconds;
                allocated = GC.GetAllocatedBytesForCurrentThread() - allocated;
                writer.WriteLine(JsonSerializer.Serialize(new
                {
                    kind = "sample", version, operation = "dictionary-merge", scenario, size, sample, operations,
                    elapsedNs = elapsed / operations, allocatedBytes = (double)allocated / operations,
                    path = method == null ? "legacy: return OnCell dictionary" : "actual internal GetAdditionalAttributes delegate"
                }));
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IReadOnlyDictionary<string, object>? LegacyPath(AntDomComponentBase _, IReadOnlyDictionary<string, object>? legacy) => legacy;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Loop(Merger merge, Probe probe, IReadOnlyDictionary<string, object>? legacy, int operations)
    {
        for (var i = 0; i < operations; i++) _sink = merge(probe, legacy);
        GC.KeepAlive(_sink);
    }
}
