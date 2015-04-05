using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Interceptors.Tests
{
    public class TestService: ITestService
    {
        private BlockingCollection<string> _eventsCollection;
        public TestService(BlockingCollection<string> eventsCollection)
        {
            _eventsCollection = eventsCollection;
        }

        public void ReturnsVoid(bool throwsException = false)
        {
            _eventsCollection.Add("ReturnsVoid call start");
            if (throwsException) {
                throw new Exception("ReturnsVoid exception");
            }
            _eventsCollection.Add("ReturnsVoid call end");
        }

        public async void ReturnsVoidAsync()
        {
            _eventsCollection.Add("ReturnsVoidAsync call start");
            await Task.Delay(10);
            _eventsCollection.Add("ReturnsVoidAsync call end");
        }

        public async Task ReturnsTaskAsync(bool throwsExceptionBeforeAwait = false, bool throwsExceptionAfterAwait = false)
        {
            _eventsCollection.Add("ReturnsTaskAsync call start");
            if (throwsExceptionBeforeAwait) {
                throw new Exception("ReturnsTaskAsync exception before await");
            }

            await Task.Delay(10);

            if (throwsExceptionAfterAwait) {
                throw new Exception("ReturnsTaskAsync exception after await");
            }
            _eventsCollection.Add("ReturnsTaskAsync call end");
        }

        public int ReturnsValue(bool throwsException = false)
        {
            _eventsCollection.Add("ReturnsValue call start");
            if (throwsException) {
                throw new Exception("ReturnsValue exception");
            }
            _eventsCollection.Add("ReturnsValue call end");
            return 7;
        }

        public async Task<int> ReturnsValueAsync()
        {
            _eventsCollection.Add("ReturnsValueAsync call start");
            await Task.Delay(10);
            _eventsCollection.Add("ReturnsValueAsync call end");
            return 7;
        }
    }
}