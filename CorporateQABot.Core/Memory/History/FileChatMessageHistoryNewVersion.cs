using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using LangChain.Memory;
using LangChain.Providers; // Message, HumanMessage, AiMessage, SystemMessage, ToolMessage

namespace CorporateQABot.Core.Memory.History;

/// <summary>
/// File-based implementation of chat message history.
/// - Loads on demand from a JSON file
/// - Persists on AddMessage / AddMessages / SetMessages / Clear
/// - Uses a DTO to serialize/deserialize Message polymorphically
/// </summary>
public sealed class FileChatMessageHistoryNewVersion : BaseChatMessageHistory
{
    /// <summary>
    /// Absolute path to the JSON file that stores the serialized chat log.
    /// </summary>
    private readonly string _path;
    /// <summary>
    /// Ensures that file IO and in-memory mutations are performed sequentially.
    /// </summary>
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// In-memory cache of messages currently loaded from disk.
    /// </summary>
    private readonly List<Message> _messages = new();

    /// <summary>
    /// Optional predicate used to skip messages during load or persistence (mirrors LangChain behavior).
    /// </summary>
    public Predicate<Message> IsMessageAccepted { get; set; } = _ => true;

    /// <summary>
    /// Serializer options shared by read/write operations to keep payloads consistent.
    /// </summary>
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // File payload (versioned for future changes)
    private sealed class FilePayload
    {
        /// <summary>
        /// Version marker for potential future migrations.
        /// </summary>
        public int Version { get; set; } = 1;
        /// <summary>
        /// Serialized chat messages persisted to disk.
        /// </summary>
        public List<PersistedMessage> Messages { get; set; } = new();
    }

    // DTO compatible across languages if needed
    private sealed class PersistedMessage
    {
        /// <summary>
        /// Role of the message (human, ai, system, tool, etc.).
        /// </summary>
        public MessageRole Role { get; set; } // human|ai|system|tool|unknown
        /// <summary>
        /// Message text content.
        /// </summary>
        public string Content { get; set; } = "";
        /// <summary>
        /// Optional metadata captured by callers when saving messages.
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }
        /// <summary>
        /// Timestamp recorded when the payload is created (used for debugging or retention policies).
        /// </summary>
        public DateTime Utc { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new history instance that loads/persists chat messages to the specified file.
    /// </summary>
    /// <param name="path">Target file path for serialized conversations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is null.</exception>
    public FileChatMessageHistoryNewVersion(string path)
    {
        _path = path ?? throw new ArgumentNullException(nameof(path));
        // Lazy load: actual load happens on first Messages access or write
    }

    /// <inheritdoc/>
    public override IReadOnlyList<Message> Messages
    {
        get
        {
            EnsureLoaded(); // best-effort sync load
            return _messages;
        }
    }

    /// <inheritdoc/>
    public override async Task AddMessage(Message message)
    {
        //message = message ?? throw new ArgumentNullException(nameof(message));

        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            await LoadIfNeededAsync().ConfigureAwait(false);
            if (IsMessageAccepted(message))
            {
                _messages.Add(message);
                await PersistAsync().ConfigureAwait(false);
            }
        }
        finally { _lock.Release(); }
    }

    /// <inheritdoc/>
    public override async Task AddMessages(IEnumerable<Message> messages)
    {
        messages = messages ?? throw new ArgumentNullException(nameof(messages));

        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            await LoadIfNeededAsync().ConfigureAwait(false);
            foreach (var m in messages)
            {
                if (m != null && IsMessageAccepted(m))
                    _messages.Add(m);
            }
            await PersistAsync().ConfigureAwait(false); // single write
        }
        finally { _lock.Release(); }
    }

    /// <inheritdoc/>
    public override async Task Clear()
    {
        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            _messages.Clear();
            await PersistAsync().ConfigureAwait(false);
        }
        finally { _lock.Release(); }
    }

    /// <inheritdoc/>
    public override async Task SetMessages(IEnumerable<Message> messages)
    {
        messages = messages ?? throw new ArgumentNullException(nameof(messages));

        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            _messages.Clear();
            foreach (var m in messages)
            {
                if (m != null && IsMessageAccepted(m))
                    _messages.Add(m);
            }
            await PersistAsync().ConfigureAwait(false);
        }
        finally { _lock.Release(); }
    }

    // ---------- Helpers ----------

    /// <summary>
    /// Loads messages synchronously if the cache is empty and the backing file exists.
    /// </summary>
    private void EnsureLoaded()
    {
        // Synchronous fast-path used by getter. Avoids deadlocks.
        if (_messages.Count > 0 || !File.Exists(_path)) return;

        // Minimal blocking read
        using var fs = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var payload = JsonSerializer.Deserialize<FilePayload>(fs, Json) ?? new FilePayload();
        _messages.Clear();
        _messages.AddRange(payload.Messages.Select(FromPersisted));
    }

    /// <summary>
    /// Loads messages asynchronously if the cache is empty and a persisted file is found.
    /// </summary>
    private async Task LoadIfNeededAsync()
    {
        if (_messages.Count > 0 || !File.Exists(_path)) return;

        await using var fs = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var payload = await JsonSerializer.DeserializeAsync<FilePayload>(fs, Json).ConfigureAwait(false) ?? new FilePayload();
        _messages.Clear();
        _messages.AddRange(payload.Messages.Select(FromPersisted));
    }

    /// <summary>
    /// Serializes the in-memory cache and writes it to disk atomically.
    /// </summary>
    private async Task PersistAsync()
    {
        var payload = new FilePayload
        {
            Version = 1,
            Messages = _messages.Select(ToPersisted).ToList()
        };

        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        await using var fs = File.Create(_path);
        await JsonSerializer.SerializeAsync(fs, payload, Json).ConfigureAwait(false);
    }

    /// <summary>
    /// Converts an in-memory <see cref="Message"/> to its DTO representation.
    /// </summary>
    private static PersistedMessage ToPersisted(Message m) =>
        m.Role switch
        {
            MessageRole.Human => new PersistedMessage { Role = MessageRole.Human, Content = m.Content },
            MessageRole.Ai => new PersistedMessage { Role = MessageRole.Ai, Content = m.Content },
            MessageRole.System => new PersistedMessage { Role = MessageRole.System, Content = m.Content },
            MessageRole.Chat => new PersistedMessage { Role = MessageRole.Chat, Content = m.Content },
            MessageRole.ToolCall => new PersistedMessage { Role = MessageRole.ToolCall, Content = m.Content },
            MessageRole.ToolResult => new PersistedMessage { Role = MessageRole.ToolResult, Content = m.Content },
            _ => new PersistedMessage { Role = MessageRole.Human, Content = m.Content } // fallback
        };

    /// <summary>
    /// Rehydrates a stored <see cref="PersistedMessage"/> into the correct chat message type.
    /// </summary>
    private static Message FromPersisted(PersistedMessage p) =>
        p.Role switch
        {
            MessageRole.Ai => Message.Ai(p.Content),
            MessageRole.System => new (p.Content, MessageRole.System),
            MessageRole.Chat => new (p.Content, MessageRole.Chat),
            MessageRole.ToolCall => new (p.Content, MessageRole.ToolCall),
            MessageRole.ToolResult => new (p.Content, MessageRole.ToolResult),
            MessageRole.Human => Message.Human(p.Content),
            _ => Message.Human(p.Content) // fallback
        };
}
