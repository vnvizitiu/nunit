// ***********************************************************************
// Copyright (c) 2016 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using NUnit.Common;
using NUnit.Engine.Internal;
using NUnit.Engine.Services;

namespace NUnit.Engine.Servers
{
    internal class ConsoleServer : ITestServer
    {
        static Logger _log = InternalTrace.GetLogger(typeof(ConsoleServer));
        TestAgency _testAgency;

        public ConsoleServer(TestAgency testAgency)
        {
            _testAgency = testAgency;
        }

        public void Dispose()
        {
        }

        public bool HandlesPlatform(TargetPlatform platform)
        {
            return platform == TargetPlatform.Portable;
        }

        public TestAgency.AgentRecord LaunchAgentProcess(TestPackage package, string defaultFramework)
        {
            Guid agentId = Guid.NewGuid();

            // Does x86 make sense for any of the platforms?
            bool useX86Agent = package.GetSetting(PackageSettings.RunAsX86, false);
            bool debugTests = package.GetSetting(PackageSettings.DebugTests, false);
            bool debugAgent = package.GetSetting(PackageSettings.DebugAgent, false);
            string traceLevel = package.GetSetting(PackageSettings.InternalTraceLevel, "Off");

            // Set options that need to be in effect before the package
            // is loaded by using the command line.
            string agentArgs = string.Empty;
            if (debugAgent)
                agentArgs += " --debug-agent";
            if (traceLevel != "Off")
                agentArgs += " --trace:" + traceLevel;

            TargetPlatform platform = TestAgency.GetTargetPlaform(package);

            _log.Info("Getting {0} agent for use under {1}", useX86Agent ? "x86" : "standard", platform);

            string agentExePath = GetTestAgentExePath(platform, useX86Agent);

            if (agentExePath == null)
                throw new ArgumentException(
                    string.Format("NUnit components for {0} platform are not installed",
                    platform), "platform");
            

            _log.Debug("Using nunit-agent at " + agentExePath);

            string arglist = agentId.ToString() + " " + agentArgs;

            var p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = agentExePath;
            p.StartInfo.Arguments = arglist;
            p.Start();
            _log.Info("Launched Agent process {0} - see nunit-agent_{0}.log", p.Id);
            _log.Info("Command line: \"{0}\" {1}", p.StartInfo.FileName, p.StartInfo.Arguments);

            return new TestAgency.AgentRecord(agentId, p, null, AgentStatus.Starting);
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void WaitForStop()
        {
        }

        private static string GetTestAgentExePath(TargetPlatform platform, bool requires32Bit)
        {
            string engineDir = NUnitConfiguration.EngineDirectory;
            if (engineDir == null) return null;

            string agentDir = Path.Combine(engineDir, "agents");

            string agentName = string.Format("nunit-{0}-agent{1}.exe", platform, requires32Bit ? "-x86" : "");

            string agentExePath = Path.Combine(agentDir, agentName);
            return File.Exists(agentExePath) ? agentExePath : null;
        }
    }
}
