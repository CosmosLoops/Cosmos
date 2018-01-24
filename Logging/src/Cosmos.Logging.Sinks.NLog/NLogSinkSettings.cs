﻿using System;
using Cosmos.Logging.Configurations;
using Cosmos.Logging.Events;

namespace Cosmos.Logging.Sinks.NLog {
    public class NLogSinkSettings : ILogSinkSettings {
        public string Key => Internals.Constants.SinkKey;
        public string Name { get; set; } = $"{Internals.Constants.SinkPrefix}_{DateTime.Now:yyyyMMdd_HHmmssffff}";
        public LogEventLevel? Level { get; set; }

        public global::NLog.Config.LoggingConfiguration OriginConfiguration { get; set; }

        public void UseDefaultOriginConfigFilePath() => OriginConfigFilePath = "nlog.config";
        public string OriginConfigFilePath { get; set; }

        public void EnableUsingDefaultConfig() => DoesUsedDefaultConfig = true;
        public void DisableUsingDefaultConfig() => DoesUsedDefaultConfig = false;
        internal bool DoesUsedDefaultConfig { get; set; } = true;
    }
}