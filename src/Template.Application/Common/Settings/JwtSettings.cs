﻿namespace Template.Application.Common.Settings
{
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; } = 60;
        public int RememberMeExpiryMinutes { get; set; } = 43200; // 30 days
    }
}
