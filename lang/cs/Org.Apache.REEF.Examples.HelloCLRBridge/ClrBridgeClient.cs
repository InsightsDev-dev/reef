﻿/**
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using System;
using System.IO;
using Org.Apache.REEF.Client.API;
using Org.Apache.REEF.Client.Local;
using Org.Apache.REEF.Client.YARN;
using Org.Apache.REEF.Common.Evaluator;
using Org.Apache.REEF.Driver;
using Org.Apache.REEF.Driver.Defaults;
using Org.Apache.REEF.Examples.HelloCLRBridge.Handlers;
using Org.Apache.REEF.Tang.Annotations;
using Org.Apache.REEF.Tang.Implementations.Tang;
using Org.Apache.REEF.Tang.Interface;
using Org.Apache.REEF.Tang.Util;
using Org.Apache.REEF.Utilities.Logging;

namespace Org.Apache.REEF.Examples.HelloCLRBridge
{
    public sealed class ClrBridgeClient
    {
        private const string Local = "local";
        private const string YARN = "yarn";
        private readonly IREEFClient _reefClient;
        private readonly JobSubmissionBuilderFactory _jobSubmissionBuilderFactory;

        [Inject]
        private ClrBridgeClient(IREEFClient reefClient, JobSubmissionBuilderFactory jobSubmissionBuilderFactory)
        {
            _reefClient = reefClient;
            _jobSubmissionBuilderFactory = jobSubmissionBuilderFactory;
        }

        private void Run()
        {
            var helloDriverConfiguration = DriverConfiguration.ConfigurationModule
                .Set(DriverConfiguration.OnEvaluatorAllocated, GenericType<HelloAllocatedEvaluatorHandler>.Class)
                .Set(DriverConfiguration.OnEvaluatorAllocated, GenericType<AnotherHelloAllocatedEvaluatorHandler>.Class)
                .Set(DriverConfiguration.OnContextActive, GenericType<HelloActiveContextHandler>.Class)
                .Set(DriverConfiguration.OnTaskMessage, GenericType<HelloTaskMessageHandler>.Class)
                .Set(DriverConfiguration.OnEvaluatorFailed, GenericType<HelloFailedEvaluatorHandler>.Class)
                .Set(DriverConfiguration.OnTaskFailed, GenericType<HelloFailedTaskHandler>.Class)
                .Set(DriverConfiguration.OnTaskRunning, GenericType<HelloRunningTaskHandler>.Class)
                .Set(DriverConfiguration.OnDriverStarted, GenericType<HelloDriverStartHandler>.Class)
                .Set(DriverConfiguration.OnHttpEvent, GenericType<HelloHttpHandler>.Class)
                .Set(DriverConfiguration.OnEvaluatorCompleted, GenericType<HelloCompletedEvaluatorHandler>.Class)
                .Set(DriverConfiguration.CustomTraceListeners, GenericType<DefaultCustomTraceListener>.Class)
                .Set(DriverConfiguration.CustomTraceLevel, Level.Info.ToString())
                .Set(DriverConfiguration.OnDriverRestarted, GenericType<HelloRestartHandler>.Class)
                .Set(DriverConfiguration.OnDriverReconnect, GenericType<DefaultLocalHttpDriverConnection>.Class)
                .Set(DriverConfiguration.OnDirverRestartContextActive, GenericType<HelloDriverRestartActiveContextHandler>.Class)
                .Set(DriverConfiguration.OnDriverRestartTaskRunning, GenericType<HelloDriverRestartRunningTaskHandler>.Class)
                .Build();

            var helloJobSubmission = _jobSubmissionBuilderFactory.GetJobSubmissionBuilder()
                .AddDriverConfiguration(helloDriverConfiguration)
                .AddGlobalAssemblyForType(typeof(HelloDriverStartHandler))
                .SetJobIdentifier("HelloDriverStartHandler")
                .Build();

            _reefClient.Submit(helloJobSubmission);
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static IConfiguration GetRuntimeConfiguration(string name)
        {
            switch (name)
            {
                case Local:
                    var dir = Path.Combine(Directory.GetCurrentDirectory(), "REEF_LOCAL_RUNTIME");
                    return LocalRuntimeClientConfiguration.ConfigurationModule
                        .Set(LocalRuntimeClientConfiguration.NumberOfEvaluators, "2")
                        .Set(LocalRuntimeClientConfiguration.RuntimeFolder, dir)
                        .Build();
                case YARN:
                    return YARNClientConfiguration.ConfigurationModule.Build();
                default:
                    throw new Exception("Unknown runtime: " + name);
            }
        }

        public static void Main(string[] args)
        {
            TangFactory.GetTang().NewInjector(GetRuntimeConfiguration(args.Length > 0 ? args[0] : Local)).GetInstance<ClrBridgeClient>().Run();
        }
    }
}