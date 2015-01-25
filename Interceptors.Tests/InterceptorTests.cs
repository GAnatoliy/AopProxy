using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Castle.DynamicProxy;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using ProxyTests;


namespace Interceptors.Tests
{
    [TestFixture]
    public class InterceptorTests
    {
        private BlockingCollection<string> _eventsCollection;
        private ITestService _proxy;
        private TestInterceptor _interceptor;
        private static readonly ProxyGenerator _generator = new ProxyGenerator();

        private class TestInterceptor : Interceptor
        {
            private BlockingCollection<string> _eventsCollection;

            public bool ThrowExceptionAtTheStart { get; set; }
            public bool ThrowExceptionAtTheEnd { get; set; }

            public bool ThrowWrapperAtTheExceptionThrown { get; set; }

            public TestInterceptor(BlockingCollection<string> eventsCollection)
            {
                _eventsCollection = eventsCollection;
            }
            
            protected override void MethodStart(IInvocation invocation)
            {
                _eventsCollection.Add("TestInterceptor call start");
                if (ThrowExceptionAtTheStart) {
                    throw new Exception("TestInterceptor exception at the start");
                }
            }

            protected override void MethodEnd(IInvocation invocation)
            {
                _eventsCollection.Add("TestInterceptor call end");
                if (ThrowExceptionAtTheEnd) {
                    throw new Exception("TestInterceptor exception at the end");
                }
            }

            protected override void ExceptionThrown(IInvocation invocation, Exception ex)
            {
                _eventsCollection.Add("TestInterceptor exception was thrown");
                if (ThrowWrapperAtTheExceptionThrown) {
                    throw new Exception("TestInterceptor exception wrapper", ex);
                }
            }
        }

        [SetUp]
        public void SetUp()
        {
            _eventsCollection = new BlockingCollection<string>();
            ITestService service = new TestService(_eventsCollection);
            _interceptor = new TestInterceptor(_eventsCollection);

            _proxy = _generator.CreateInterfaceProxyWithTarget(service, _interceptor);
        }

        #region Test intercepting method that returns void

        [Test]
        public void TestIntercept_WhenMethodReturnsVoid_StartAndEndAreCalled()
        {
            _proxy.ReturnsVoid();

            Assert.AreEqual(4, _eventsCollection.Count);
            Assert.AreEqual("TestInterceptor call start", _eventsCollection.ToList()[0]);
            Assert.AreEqual("ReturnsVoid call start", _eventsCollection.ToList()[1]);
            Assert.AreEqual("ReturnsVoid call end", _eventsCollection.ToList()[2]);
            Assert.AreEqual("TestInterceptor call end", _eventsCollection.ToList()[3]);
        }

        [Test]
        public void TestIntercept_WhenMethodReturnsVoidAndStartThrowsException_ThrowsExceptionAndDoesntCallInterceptedMethod()
        {
            _interceptor.ThrowExceptionAtTheStart = true;
            var ex = Assert.Throws<Exception>(() => _proxy.ReturnsVoid());
            Assert.AreEqual("TestInterceptor exception at the start", ex.Message);

            Assert.AreEqual(1, _eventsCollection.Count);
            Assert.AreEqual("TestInterceptor call start", _eventsCollection.ToList()[0]);
        }

        [Test]
        public void TestIntercept_WhenMethodReturnsVoidAndEndThrowsException_ThrowsException()
        {
            _interceptor.ThrowExceptionAtTheEnd = true;
            var ex = Assert.Throws<Exception>(() => _proxy.ReturnsVoid());
            Assert.AreEqual("TestInterceptor exception at the end", ex.Message);

            Assert.AreEqual(4, _eventsCollection.Count);
            Assert.AreEqual("TestInterceptor call start", _eventsCollection.ToList()[0]);
            Assert.AreEqual("ReturnsVoid call start", _eventsCollection.ToList()[1]);
            Assert.AreEqual("ReturnsVoid call end", _eventsCollection.ToList()[2]);
            Assert.AreEqual("TestInterceptor call end", _eventsCollection.ToList()[3]);
        }

        [Test]
        public void TestIntercept_WhenMethodReturnsVoidAndThrowsException_ExceptionThrownAreCalled()
        {
            var ex = Assert.Throws<Exception>(() => _proxy.ReturnsVoid(true));
            Assert.AreEqual("ReturnsVoid exception", ex.Message);

            Assert.AreEqual(3, _eventsCollection.Count);
            Assert.AreEqual("TestInterceptor call start", _eventsCollection.ToList()[0]);
            Assert.AreEqual("ReturnsVoid call start", _eventsCollection.ToList()[1]);
            Assert.AreEqual("TestInterceptor exception was thrown", _eventsCollection.ToList()[2]);
        }

        [Test]
        public void TestIntercept_WhenMethodReturnsVoidAndExceptionThrownMethodThrowsExceptionWrapper_ThrowsWraperException()
        {
            _interceptor.ThrowWrapperAtTheExceptionThrown = true;
            var ex = Assert.Throws<Exception>(() => _proxy.ReturnsVoid(true));
            Assert.AreEqual("TestInterceptor exception wrapper", ex.Message);

            Assert.AreEqual(3, _eventsCollection.Count);
            Assert.AreEqual("TestInterceptor call start", _eventsCollection.ToList()[0]);
            Assert.AreEqual("ReturnsVoid call start", _eventsCollection.ToList()[1]);
            Assert.AreEqual("TestInterceptor exception was thrown", _eventsCollection.ToList()[2]);
        }
        #endregion

        [Test]
        [Ignore("decide how to handle void asyn methods.")]
        public void TestIntercept_WhenAsynMethodReturnsVoid_StartAndEndAreCalled()
        {
            _proxy.ReturnsVoidAsync();

            // Wait until ReturnsVoidAsync ends work.
            Thread.Sleep(100);

            Assert.AreEqual(4, _eventsCollection.Count);
            Assert.AreEqual("TestInterceptor call start", _eventsCollection.ToList()[0]);
            Assert.AreEqual("ReturnsVoidAsync call start", _eventsCollection.ToList()[1]);
            Assert.AreEqual("ReturnsVoidAsync call end", _eventsCollection.ToList()[2]);
            Assert.AreEqual("TestInterceptor call end", _eventsCollection.ToList()[3]);
        }

        #region Test intercepting method that returns Task
        [Test]
        public void TestIntercept_WhenMethodReturnsTask_StartAndEndAreCalled()
        {
            _proxy.ReturnsTaskAsync().Wait();

            Assert.AreEqual(4, _eventsCollection.Count);
            Assert.AreEqual("TestInterceptor call start", _eventsCollection.ToList()[0]);
            Assert.AreEqual("ReturnsTaskAsync call start", _eventsCollection.ToList()[1]);
            Assert.AreEqual("ReturnsTaskAsync call end", _eventsCollection.ToList()[2]);
            Assert.AreEqual("TestInterceptor call end", _eventsCollection.ToList()[3]);
        }
        #endregion

        #region Test intercepting method that returns value
        [Test]
        public void TestIntercept_WhenMethodReturnsValue_StartAndEndAreCalled()
        {
            var value = _proxy.ReturnsValue();

            Assert.AreEqual(4, _eventsCollection.Count);
            Assert.AreEqual("TestInterceptor call start", _eventsCollection.ToList()[0]);
            Assert.AreEqual("ReturnsValue call start", _eventsCollection.ToList()[1]);
            Assert.AreEqual("ReturnsValue call end", _eventsCollection.ToList()[2]);
            Assert.AreEqual("TestInterceptor call end", _eventsCollection.ToList()[3]);
        }

        [Test]
        public void TestIntercept_WhenMethodReturnsValueAndStartThrowsException_ThrowsExceptionAndDoesntCallInterceptedMethod()
        {
            _interceptor.ThrowExceptionAtTheStart = true;
            var ex = Assert.Throws<Exception>(() => _proxy.ReturnsValue());
            Assert.AreEqual("TestInterceptor exception at the start", ex.Message);

            Assert.AreEqual(1, _eventsCollection.Count);
            Assert.AreEqual("TestInterceptor call start", _eventsCollection.ToList()[0]);
        }

        [Test]
        public void TestIntercept_WhenMethodReturnsValueAndEndThrowsException_ThrowsException()
        {
            _interceptor.ThrowExceptionAtTheEnd = true;
            var ex = Assert.Throws<Exception>(() => _proxy.ReturnsValue());
            Assert.AreEqual("TestInterceptor exception at the end", ex.Message);

            Assert.AreEqual(4, _eventsCollection.Count);
            Assert.AreEqual("TestInterceptor call start", _eventsCollection.ToList()[0]);
            Assert.AreEqual("ReturnsValue call start", _eventsCollection.ToList()[1]);
            Assert.AreEqual("ReturnsValue call end", _eventsCollection.ToList()[2]);
            Assert.AreEqual("TestInterceptor call end", _eventsCollection.ToList()[3]);
        }

        [Test]
        public void TestIntercept_WhenMethodReturnsValueAndThrowsException_ExceptionThrownAreCalled()
        {
            var ex = Assert.Throws<Exception>(() => _proxy.ReturnsValue(true));
            Assert.AreEqual("ReturnsValue exception", ex.Message);

            Assert.AreEqual(3, _eventsCollection.Count);
            Assert.AreEqual("TestInterceptor call start", _eventsCollection.ToList()[0]);
            Assert.AreEqual("ReturnsValue call start", _eventsCollection.ToList()[1]);
            Assert.AreEqual("TestInterceptor exception was thrown", _eventsCollection.ToList()[2]);
        }

        [Test]
        public void TestIntercept_WhenMethodReturnsValueAndExceptionThrownMethodThrowsExceptionWrapper_ThrowsWraperException()
        {
            _interceptor.ThrowWrapperAtTheExceptionThrown = true;
            var ex = Assert.Throws<Exception>(() => _proxy.ReturnsValue(true));
            Assert.AreEqual("TestInterceptor exception wrapper", ex.Message);

            Assert.AreEqual(3, _eventsCollection.Count);
            Assert.AreEqual("TestInterceptor call start", _eventsCollection.ToList()[0]);
            Assert.AreEqual("ReturnsValue call start", _eventsCollection.ToList()[1]);
            Assert.AreEqual("TestInterceptor exception was thrown", _eventsCollection.ToList()[2]);
        }
        #endregion

        [Test]
        public void TestIntercept_WhenMethodReturnsValueAsyn_StartAndEndAreCalled()
        {
            _proxy.ReturnsValueAsync().Wait();

            Assert.AreEqual(4, _eventsCollection.Count);
            Assert.AreEqual("TestInterceptor call start", _eventsCollection.ToList()[0]);
            Assert.AreEqual("ReturnsValueAsync call start", _eventsCollection.ToList()[1]);
            Assert.AreEqual("ReturnsValueAsync call end", _eventsCollection.ToList()[2]);
            Assert.AreEqual("TestInterceptor call end", _eventsCollection.ToList()[3]);
        }
    }
}
