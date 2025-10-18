namespace Template.Application.Common.Settings
{
    public class CacheOptions
    {
        public int DefaultDurationSeconds { get; set; } = 1800;
        public bool EnableLogging { get; set; } = true;
        public string DefaultKeyPrefix { get; set; } = "cache";
    }

    public class InMemoryCacheOptions
    {
        public int DefaultDurationSeconds { get; set; } = 300;
        public bool DefaultUseSlidingExpiration { get; set; } = false;
        public string DefaultKeyPrefix { get; set; } = "mem";
        public int MaxCacheSize { get; set; } = 1000;
        public int DefaultEntrySize { get; set; } = 1;
        public bool EnableLogging { get; set; } = true;
        public string DefaultPriority { get; set; } = "Normal";
    }
}
