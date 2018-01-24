﻿using System;
using System.Collections.Generic;

namespace Cosmos.Logging.Configurations {
    public abstract class SinkConfiguration {
        public readonly string Name;

        protected SinkConfiguration(string name) {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            Name = name;
        }

        public Dictionary<string, string> LogLevel { get; set; } = new Dictionary<string, string>();
    }
}