﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.ProjectSystem.Build;
using Microsoft.VisualStudio.Shell.BuildLogging;

namespace Microsoft.VisualStudio.ProjectSystem.Tools.Providers
{
    [Export(typeof(IBuildLoggerProviderAsync))]
    [AppliesTo(ProjectCapabilities.AlwaysApplicable)]
    [Export(typeof(IVsBuildLoggerProvider))]
    internal sealed class BuildLoggerProvider : IBuildLoggerProviderAsync, IVsBuildLoggerProvider
    {
        private readonly IBuildTableDataSource _dataSource;

        [ImportingConstructor]
        public BuildLoggerProvider(IBuildTableDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public LoggerVerbosity Verbosity => LoggerVerbosity.Diagnostic;

        public BuildLoggerEvents Events =>
            BuildLoggerEvents.BuildStartedEvent |
            BuildLoggerEvents.BuildFinishedEvent |
            BuildLoggerEvents.ErrorEvent |
            BuildLoggerEvents.WarningEvent |
            BuildLoggerEvents.HighMessageEvent |
            BuildLoggerEvents.NormalMessageEvent |
            BuildLoggerEvents.ProjectStartedEvent |
            BuildLoggerEvents.ProjectFinishedEvent |
            BuildLoggerEvents.TargetStartedEvent |
            BuildLoggerEvents.TargetFinishedEvent |
            BuildLoggerEvents.CommandLine |
            BuildLoggerEvents.TaskStartedEvent |
            BuildLoggerEvents.TaskFinishedEvent |
            BuildLoggerEvents.LowMessageEvent |
            BuildLoggerEvents.ProjectEvaluationStartedEvent |
            BuildLoggerEvents.ProjectEvaluationFinishedEvent |
            BuildLoggerEvents.CustomEvent;

        public ILogger GetLogger(string projectPath, IEnumerable<string> targets, IDictionary<string, string> properties, bool isDesignTimeBuild) => 
            _dataSource.IsLogging ? _dataSource.CreateLogger(isDesignTimeBuild) : null;

        public Task<IImmutableSet<ILogger>> GetLoggersAsync(IReadOnlyList<string> targets, IImmutableDictionary<string, string> properties, CancellationToken cancellationToken)
        {
            var loggers = (IImmutableSet<ILogger>)ImmutableHashSet<ILogger>.Empty;

            if (_dataSource.IsLogging)
            {
                var isDesignTime = properties.TryGetValue("DesignTimeBuild", out var value) &&
                   string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);

                loggers = loggers.Add(_dataSource.CreateLogger(isDesignTime));
            }

            return Task.FromResult(loggers);
        }
    }
}
