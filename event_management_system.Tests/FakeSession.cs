using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace event_management_system.Tests
{
    /// <summary>
    /// In-memory ISession implementation for unit tests.
    /// </summary>
    public class FakeSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public string Id => Guid.NewGuid().ToString();
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[] value)
            => _store.TryGetValue(key, out value!);
    }

    /// <summary>
    /// ISessionFeature that returns a FakeSession.
    /// </summary>
    public class FakeSessionFeature : ISessionFeature
    {
        public ISession Session { get; set; } = new FakeSession();
    }
}