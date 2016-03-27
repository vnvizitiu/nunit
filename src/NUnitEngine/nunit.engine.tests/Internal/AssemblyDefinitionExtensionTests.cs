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
using System.Text;
using NUnit.Framework;
using NUnit.Engine.Internal;
using Mono.Cecil;

namespace NUnit.Engine.Tests.Internal
{
    [TestFixture]
    public class AssemblyDefinitionExtensionTests
    {
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
        public void CanIdentifyTargetPlatform(string platform, TargetPlatform expected)
        {
            var assemblyPath = string.Format("../../mocks/{0}/{0}.dll", platform);
            Assume.That(assemblyPath, Does.Exist);
            
            var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
            Assert.That(assembly, Is.Not.Null);

            Assert.That(assembly.GetTargetPlatform(), Is.EqualTo(expected));
        }
    }
}
