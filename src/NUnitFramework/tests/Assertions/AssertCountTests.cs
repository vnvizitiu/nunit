// ***********************************************************************
// Copyright (c) 2014 Charlie Poole
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
#if NET_4_0 || NET_4_5 || PORTABLE
using System.Threading.Tasks;
#endif
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace NUnit.Framework.Assertions
{
    public class AssertCountTests
    {
        [Test]
        public void SimpleTest()
        {
            DoAsserts(2);
            DoAsserts(3);

            CheckAssertCount(5);
        }
            
        #if NET_4_0 || NET_4_5 || PORTABLE
        [Test]
        public async Task AsyncTest_NoAwait()
        {
            DoAsserts(2);
            DoAsserts(3);

            CheckAssertCount(5);
        }

        [TestCase(0, 0)]
        [TestCase(0, 1)]
        [TestCase(1, 0)]
        [TestCase(1, 1)]
        [TestCase(2, 3)]
        public async Task AsyncTest_AwaitsFakeAsyncMethod(int before, int after)
        {
            DoAsserts(before);
            await FakeAsyncMethod();
            DoAsserts(after);

            CheckAssertCount(before + after);
        }   

        [TestCase(0, 0)]
        [TestCase(0, 1)]
        [TestCase(1, 0)]
        [TestCase(1, 1)]
        [TestCase(2, 3)]
        public async Task AsyncTest_AwaitsRealAsyncMethod(int before, int after)
        {
            DoAsserts(before);
            await AsyncMethod();
            DoAsserts(after);

            CheckAssertCount(before + after);
        }   

        [TestCase(0, 0, 0)]
        [TestCase(1, 0, 0)]
        [TestCase(0, 1, 0)]
        [TestCase(0, 0, 1)]
        [TestCase(1, 5, 0)]
        [TestCase(1, 0, 5)]
        [TestCase(0, 1, 5)]
        [TestCase(1, 2, 4)]
        public async Task AsyncTest_AwaitsTwoFakeAsyncMethods(int before, int between, int after)
        {
            DoAsserts(before);
            await FakeAsyncMethod();
            DoAsserts(between);
            await FakeAsyncMethod();
            DoAsserts(after);

            CheckAssertCount(before + between + after);
        }

        [TestCase(0, 0, 0)]
        [TestCase(1, 0, 0)]
        [TestCase(0, 1, 0)]
        [TestCase(0, 0, 1)]
        [TestCase(1, 5, 0)]
        [TestCase(1, 0, 5)]
        [TestCase(0, 1, 5)]
        [TestCase(1, 2, 4)]
        public async Task AsyncTest_AwaitsTwoRealAsyncMethods(int before, int between, int after)
        {
            DoAsserts(before);
            await AsyncMethod();
            DoAsserts(between);
            await AsyncMethod();
            DoAsserts(after);

            CheckAssertCount(before + between + after);
        }

        #endif

        #region Helper Methods

        private void DoAssert()
        {
            Assert.True(true);
        }

        private void DoAsserts(int howMany)
        {
            while (--howMany >= 0)
                DoAssert();
        }

        private void CheckAssertCount(int expected)
        {
            Assert.That(TestExecutionContext.CurrentContext.AssertCount, Is.EqualTo(expected));
        }

        #if NET_4_0 || NET_4_5 || PORTABLE
        private async Task FakeAsyncMethod()
        {
        }

        private async Task AsyncMethod()
        {
            await Task.Delay(1);
        }
        #endif

        #endregion
    }
}

