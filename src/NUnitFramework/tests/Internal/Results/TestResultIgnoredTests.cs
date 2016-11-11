// ***********************************************************************
// Copyright (c) 2010-2016 Charlie Poole
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

using NUnit.Framework.Interfaces;

namespace NUnit.Framework.Internal.Results
{
    public class TestResultIgnoredTests : TestResultTests
    {
        [SetUp]
        public void SimulateTestRun()
        {
            _testResult.SetResult(ResultState.Ignored, "because");
            _suiteResult.AddResult(_testResult);
        }

        [Test]
        public void TestResultIsIgnored()
        {
            Assert.AreEqual(ResultState.Ignored, _testResult.ResultState);
            Assert.AreEqual("because", _testResult.Message);
        }

        [Test]
        public void SuiteResultIsIgnored()
        {
            Assert.AreEqual(ResultState.Ignored, _suiteResult.ResultState);
            Assert.AreEqual(TestStatus.Skipped, _suiteResult.ResultState.Status);
            Assert.AreEqual(TestResult.CHILD_IGNORE_MESSAGE, _suiteResult.Message);

            Assert.AreEqual(0, _suiteResult.PassCount);
            Assert.AreEqual(0, _suiteResult.FailCount);
            Assert.AreEqual(1, _suiteResult.SkipCount);
            Assert.AreEqual(0, _suiteResult.InconclusiveCount);
            Assert.AreEqual(0, _suiteResult.AssertCount);
        }

        [Test]
        public void TestResultXmlNodeIsIgnored()
        {
            TNode testNode = _testResult.ToXml(true);

            Assert.AreEqual("Skipped", testNode.Attributes["result"]);
            Assert.AreEqual("Ignored", testNode.Attributes["label"]);
            Assert.AreEqual(null, testNode.Attributes["site"]);

            TNode reason = testNode.SelectSingleNode("reason");
            Assert.NotNull(reason);
            Assert.NotNull(reason.SelectSingleNode("message"));
            Assert.AreEqual("because", reason.SelectSingleNode("message").Value);
            Assert.Null(reason.SelectSingleNode("stack-trace"));
        }

        [Test]
        public void SuiteResultXmlNodeIsIgnored()
        {
            TNode suiteNode = _suiteResult.ToXml(true);

            Assert.AreEqual("Skipped", suiteNode.Attributes["result"]);
            Assert.AreEqual("Ignored", suiteNode.Attributes["label"]);
            Assert.AreEqual(null, suiteNode.Attributes["site"]);
            Assert.AreEqual("0", suiteNode.Attributes["passed"]);
            Assert.AreEqual("0", suiteNode.Attributes["failed"]);
            Assert.AreEqual("1", suiteNode.Attributes["skipped"]);
            Assert.AreEqual("0", suiteNode.Attributes["inconclusive"]);
            Assert.AreEqual("0", suiteNode.Attributes["asserts"]);
        }
    }
}
