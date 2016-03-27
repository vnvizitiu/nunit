// ***********************************************************************
// Copyright (c) 2015 Charlie Poole
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
using System.Text;
using NUnit.Engine.Internal;
using NUnit.Framework;

namespace NUnit.Engine.Services.Tests
{
    public class RuntimeFrameworkServiceTests
    {
        private RuntimeFrameworkService _runtimeService;

        [SetUp]
        public void CreateServiceContext()
        {
            var services = new ServiceContext();
            _runtimeService = new RuntimeFrameworkService();
            services.Add(_runtimeService);
            services.ServiceManager.StartServices();
        }

        [TearDown]
        public void StopService()
        {
            _runtimeService.StopService();
        }

        [Test]
        public void ServiceIsStarted()
        {
            Assert.That(_runtimeService.Status, Is.EqualTo(ServiceStatus.Started));
        }

        [TestCase("mock-assembly.exe", "2.0.50727", false)]
        [TestCase("net-2.0/mock-assembly.exe", "2.0.50727", false)]
        [TestCase("net-4.0/mock-assembly.exe", "4.0.30319", false)]
        // TODO: Change this case when the 4.0/4.5 bug is fixed
        [TestCase("net-4.5/mock-assembly.exe", "4.0.30319", false)]
        [TestCase("mock-cpp-clr-x64.dll", "4.0.30319", false)]
        [TestCase("mock-cpp-clr-x86.dll", "4.0.30319", true)]
        [TestCase("nunit-agent.exe", "2.0.50727", false)]
        [TestCase("nunit-agent-x86.exe", "2.0.50727", true)]
        // TODO: Make the following cases work correctly in case we want to use
        // the engine to run them in the future.
        [TestCase("netcf-3.5/mock-assembly.exe", "2.0.50727", false)]
        [TestCase("sl-5.0/mock-assembly.dll", "4.0.30319", false)]
        [TestCase("portable/mock-assembly.dll", "4.0.30319", false)]
        public void SelectRuntimeFramework(string assemblyName, string expectedVersion, bool runAsX86)
        {
            // Some files don't actually exist on our CI servers
            Assume.That(assemblyName, Does.Exist);
            var package = new TestPackage(assemblyName);

            var returnValue = _runtimeService.SelectRuntimeFramework(package);
            var framework = RuntimeFramework.Parse(returnValue);

            Assert.That(package.GetSetting("RuntimeFramework", ""), Is.EqualTo(returnValue));
            Assert.That(package.GetSetting("RunAsX86", false), Is.EqualTo(runAsX86));
            Assert.That(framework.ClrVersion.ToString(), Is.EqualTo(expectedVersion));
        }

        [Test]
        public void AvailableFrameworks()
        {
            var available = _runtimeService.AvailableRuntimes;
            Assert.That(available.Count, Is.GreaterThan(0));
            foreach (var framework in available)
                Console.WriteLine("Available: {0}", framework.DisplayName);
        }

        [Test]
        public void IfAssemblyTargetsDesktopThenAgentNotRequired()
        {
            string desktop = GetMockAssemblyForPlatform("Net45");
            var package = new TestPackage(desktop);

            _runtimeService.SelectRuntimeFramework(package);

            Assert.That(package.GetSetting("ImageRequiresAgent", false), Is.False);
        }

        [Test]
        public void IfAssemblyTargetsNonDesktopThenAgentRequired()
        {
            string desktop = GetMockAssemblyForPlatform("Android");
            var package = new TestPackage(desktop);

            _runtimeService.SelectRuntimeFramework(package);

            Assert.That(package.GetSetting("ImageRequiresAgent", false), Is.True);
        }

        [Test]
        public void IfAllAssembliesAreDesktopPlatformThenDoesntRequiresAgent()
        {
            string desktop = GetMockAssemblyForPlatform("Net45");
            string android = GetMockAssemblyForPlatform("Net20");
            var package = new TestPackage(new string[] { desktop, android });

            _runtimeService.SelectRuntimeFramework(package);

            Assert.That(package.GetSetting("ImageRequiresAgent", false), Is.False);
        }

        [Test]
        public void IfAnyAssemblyIsNonDesktopPlatformThenRequiresAgent()
        {
            string desktop = GetMockAssemblyForPlatform("Net45");
            string android = GetMockAssemblyForPlatform("Android");
            var package = new TestPackage(new string[] { desktop, android });

            _runtimeService.SelectRuntimeFramework(package);

            Assert.That(package.GetSetting("ImageRequiresAgent", false), Is.True);
        }

        [Test]
        public void IfAllAssembliesTargetSamePlatformThenMultipleAgentsAreNotRequired()
        {
            string desktop = GetMockAssemblyForPlatform("Net45");
            string android = GetMockAssemblyForPlatform("Net20");
            var package = new TestPackage(new string[] { desktop, android });

            _runtimeService.SelectRuntimeFramework(package);

            Assert.That(package.GetSetting("ImageTargetPlatform", ""), Is.Not.EqualTo(TargetPlatform.Multiple.ToString()));
        }

        [Test]
        public void IfAnyAssemblyTargetsDifferentPlatformThenMultipleAgentsAreRequired()
        {
            string desktop = GetMockAssemblyForPlatform("Net45");
            string android = GetMockAssemblyForPlatform("Android");
            var package = new TestPackage(new string[] { desktop, android });

            _runtimeService.SelectRuntimeFramework(package);

            Assert.That(package.GetSetting("ImageTargetPlatform", ""), Is.EqualTo(TargetPlatform.Multiple.ToString()));
        }

        [TestCase("Android", TargetPlatform.Android)]
        [TestCase("iOS", TargetPlatform.Ios)]
        [TestCase("Net20", TargetPlatform.Desktop)]
        [TestCase("Net45", TargetPlatform.Desktop)]
        [TestCase("NetCore", TargetPlatform.NetCore)]
        [TestCase("Portable", TargetPlatform.Portable)]
        [TestCase("Silverlight", TargetPlatform.Silverlight)]
        [TestCase("UniversalWindows", TargetPlatform.UniversalWindows)]
        [TestCase("Win81", TargetPlatform.Win81)]
        [TestCase("WinPhone80Silverlight", TargetPlatform.WinPhone80Silverlight)]
        [TestCase("WinPhone81", TargetPlatform.WinPhone81)]
        [TestCase("WinPhone81Silverlight", TargetPlatform.WinPhone81Silverlight)]
        public void SetsTargetPlatform(string platform, TargetPlatform expected)
        {
            string asm = GetMockAssemblyForPlatform(platform);
            var package = new TestPackage(asm);

            _runtimeService.SelectRuntimeFramework(package);

            Assert.That(package.GetSetting("ImageTargetPlatform", ""), Is.EqualTo(expected.ToString()));
        }

        string GetMockAssemblyForPlatform(string platform)
        {
            var assemblyPath = string.Format("../../mocks/{0}/{0}.dll", platform);
            Assume.That(assemblyPath, Does.Exist);
            return assemblyPath;
        }
    }
}
