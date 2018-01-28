﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Logging.Collectors;
using Cosmos.Logging.Core;
using Cosmos.Logging.Core.Sinks;
using Cosmos.Logging.Events;
using Cosmos.Logging.Filters;
using Exceptionless;
using Exceptionless.Logging;

namespace Cosmos.Logging.Sinks.Exceptionless {
    public class ExceptionlessPayloadClient : ILogEventSink, ILogPayloadClient {
        private readonly IFormatProvider _formatProvider;
        private readonly ExceptionlessSinkConfiguration _sinkConfiguration;

        public ExceptionlessPayloadClient(string name, ExceptionlessSinkConfiguration sinkConfiguration, IFormatProvider formatProvider = null) {
            _sinkConfiguration = sinkConfiguration ?? throw new ArgumentNullException(nameof(sinkConfiguration));
            Name = name;
            Level = _sinkConfiguration.GetDefaultMinimumLevel();
            _formatProvider = formatProvider;
        }

        public string Name { get; set; }
        public LogEventLevel? Level { get; set; }

        public Task WriteAsync(ILogPayload payload, CancellationToken cancellationToken = default(CancellationToken)) {
            if (payload != null) {
                var legalityEvents = LogEventSinkFilter.Filter(payload, _sinkConfiguration).ToList();
                var source = payload.SourceType.FullName;
                using (var logger = ExceptionlessClient.Default) {
                    foreach (var logEvent in legalityEvents) {
                        var exception = logEvent.Exception;
                        var level = LogLevelSwitcher.Switch(logEvent.Level);
                        var stringBuilder = new StringBuilder();
                        using (var output = new StringWriter(stringBuilder, _formatProvider)) {
                            logEvent.RenderMessage(output, _formatProvider);
                        }

                        var builder = logger.CreateLog(source, stringBuilder.ToString(), level);
                        builder.Target.Date = logEvent.Timestamp;

                        if (level == LogLevel.Fatal) {
                            builder.MarkAsCritical();
                        }

                        if (exception != null) {
                            builder.SetException(exception);
                        }

                        //todo 此处需要使用 LogEvent 的属性（日后会增加），将之扁平化 Flatten 后使用 builder.SetProperty 加入

                        var legalityOpts = logEvent.GetAdditionalOperations(typeof(ExceptionlessPayloadClient), AdditionalOperationTypes.ForLogSink);
                        foreach (var opt in legalityOpts) {
                            if (opt.GetOpt() is Func<EventBuilder, EventBuilder> eventBuilderFunc) {
                                eventBuilderFunc.Invoke(builder);
                            }
                        }

                        foreach (var extra in logEvent.ExtraProperties) {
                            var property = extra.Value;
                            if (property != null) {
                                builder.SetProperty(property.Name, property.Value);
                            }
                        }

                        builder.Submit();
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}