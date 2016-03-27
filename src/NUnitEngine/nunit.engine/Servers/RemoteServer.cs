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
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using NUnit.Engine.Internal;
using NUnit.Common;
using System.IO;
using System.Diagnostics;
using NUnit.Engine.Services;

namespace NUnit.Engine.Servers
{
    /// <summary>
    /// Summary description for ServerBase.
    /// </summary>
    internal class RemoteServer : IDisposable
    {
        static Logger _log = InternalTrace.GetLogger(typeof(RemoteServer));

        string _uri;
        int _port;

        TestAgency _testAgency;
        TcpChannel _channel;
        bool _isMarshalled;

        object _lock = new object();

        /// <summary>
        /// Constructor used to provide
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="port"></param>
        public RemoteServer(string uri, int port, TestAgency testAgency)
        {
            _uri = uri;
            _port = port;
            _testAgency = testAgency;
        }

        public string ServerUrl
        {
            get { return string.Format("tcp://127.0.0.1:{0}/{1}", _port, _uri); }
        }

        public void Start()
        {
            if (_uri != null && _uri != string.Empty)
            {
                lock (_lock)
                {
                    this._channel = RemoteServerUtilities.GetTcpChannel(_uri + "Channel", _port, 100);

                    RemotingServices.Marshal(_testAgency, _uri);
                    this._isMarshalled = true;
                }

                if (this._port == 0)
                {
                    ChannelDataStore store = this._channel.ChannelData as ChannelDataStore;
                    if (store != null)
                    {
                        string channelUri = store.ChannelUris[0];
                        this._port = int.Parse(channelUri.Substring(channelUri.LastIndexOf(':') + 1));
                    }
                }
            }
        }

        [System.Runtime.Remoting.Messaging.OneWay]
        public void Stop()
        {
            lock( _lock )
            {
                if ( this._isMarshalled )
                {
                    RemotingServices.Disconnect( _testAgency );
                    this._isMarshalled = false;
                }

                if ( this._channel != null )
                {
                    ChannelServices.UnregisterChannel( this._channel );
                    this._channel = null;
                }

                Monitor.PulseAll( _lock );
            }
        }

        public void WaitForStop()
        {
            lock( _lock )
            {
                Monitor.Wait( _lock );
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    Stop();

                _disposed = true;
            }
        }

        #endregion

        public TestAgency.AgentRecord LaunchAgentProcess(TestPackage package, string defaultFramework)
        {
            RuntimeFramework targetRuntime = RuntimeFramework.CurrentFramework;
            string runtimeSetting = package.GetSetting(PackageSettings.RuntimeFramework, "");
            if (runtimeSetting != "")
                targetRuntime = RuntimeFramework.Parse(runtimeSetting);

            if (targetRuntime.Runtime == RuntimeType.Any)
                targetRuntime = new RuntimeFramework(RuntimeFramework.CurrentFramework.Runtime, targetRuntime.ClrVersion);

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

            _log.Info("Getting {0} agent for use under {1}", useX86Agent ? "x86" : "standard", targetRuntime);

            if (!targetRuntime.IsAvailable)
                throw new ArgumentException(
                    string.Format("The {0} framework is not available", targetRuntime),
                    "framework");

            string agentExePath = GetTestAgentExePath(targetRuntime.ClrVersion, useX86Agent);

            if (agentExePath == null)
                throw new ArgumentException(
                    string.Format("NUnit components for version {0} of the CLR are not installed",
                    targetRuntime.ClrVersion.ToString()), "targetRuntime");

            _log.Debug("Using nunit-agent at " + agentExePath);

            var p = new Process();
            p.StartInfo.UseShellExecute = false;
            Guid agentId = Guid.NewGuid();
            string arglist = agentId.ToString() + " " + ServerUrl + " " + agentArgs;
            
            if (targetRuntime.ClrVersion.Build < 0)
                targetRuntime = RuntimeFramework.GetBestAvailableFramework(targetRuntime);

            switch (targetRuntime.Runtime)
            {
                case RuntimeType.Mono:
                    p.StartInfo.FileName = NUnitConfiguration.MonoExePath;
                    string monoOptions = "--runtime=v" + targetRuntime.ClrVersion.ToString(3);
                    if (debugTests || debugAgent) monoOptions += " --debug";
                    p.StartInfo.Arguments = string.Format("{0} \"{1}\" {2}", monoOptions, agentExePath, arglist);
                    break;
                    
                case RuntimeType.Net:
                    p.StartInfo.FileName = agentExePath;
                    string envVar = "v" + targetRuntime.ClrVersion.ToString(3);
                    p.StartInfo.EnvironmentVariables["COMPLUS_Version"] = envVar;
                    p.StartInfo.Arguments = arglist;
                    break;
                    
                default:
                    p.StartInfo.FileName = agentExePath;
                    p.StartInfo.Arguments = arglist;
                    break;
            }

            //p.Exited += new EventHandler(OnProcessExit);
            p.Start();
            _log.Info("Launched Agent process {0} - see nunit-agent_{0}.log", p.Id);
            _log.Info("Command line: \"{0}\" {1}", p.StartInfo.FileName, p.StartInfo.Arguments);

            return new TestAgency.AgentRecord(agentId, p, null, AgentStatus.Starting);
        }

        private static string GetTestAgentExePath(Version v, bool requires32Bit)
        {
            string engineDir = NUnitConfiguration.EngineDirectory;
            if (engineDir == null) return null;

            string agentName = v.Major > 1 && requires32Bit
                ? "nunit-agent-x86.exe"
                : "nunit-agent.exe";

            string agentExePath = Path.Combine(engineDir, agentName);
            return File.Exists(agentExePath) ? agentExePath : null;
        }
    }
}
