using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;


namespace ProxyTests
{
    public class Interceptor : IInterceptor
    {
        // Caches wrappers in order prevent build it for each call.
        // TODO: check how it will work with nested calsses.
        private static ConcurrentDictionary<Type, Func<Task, IInvocation, Task>>
            _wrapperCreators = new ConcurrentDictionary<Type, Func<Task, IInvocation, Task>>();

        /// <summary>
        /// Is called before intercepted method call.
        /// </summary>
        protected virtual void MethodStart(IInvocation invocation)
        {
            Console.WriteLine("TaskWrapperInterceptor call start");
        }

        /// <summary>
        /// Is called after intercepted method call.
        /// </summary>
        protected virtual void MethodEnd(IInvocation invocation)
        {
            Console.WriteLine("TaskWrapperInterceptor call end");
        }

        /// <summary>
        /// Is called if intercepted method throws exception.
        /// </summary>
        /// <returns>True if exception should be propagated or false in other case.</returns>
        protected virtual void ExceptionThrown(IInvocation invocation, Exception ex)
        {
            Console.WriteLine("Exception is thrown {0}", ex);
        }

        public void Intercept(IInvocation invocation)
        {
            // TODO: consider handle exceptions in other way then.
            // async/await pattern returns Task with IsFault flag and throws AggregateException only when task starts.
            // In case of exception in method start error will be occurred immediately.
            // TODO: consider wrap exceptions in AggregateException in case of async methods
            MethodStart(invocation);

            try {
                invocation.Proceed();
            // Handle exceptions.
            } catch (Exception ex) {
                ExceptionThrown(invocation, ex);
                throw;
            }
            
            // TODO: check how works with void.
            var returnType = invocation.Method.ReturnType;

            // Process task result.
            if (returnType == typeof (Task)) {
                var task = (Task) invocation.ReturnValue;
                invocation.ReturnValue = task.ContinueWith(continuation => {
                    if (continuation.IsFaulted && continuation.Exception != null) {
                        ExceptionThrown(invocation, continuation.Exception.InnerException);
                    } else {
                        MethodEnd(invocation);
                    }
                    return continuation;
                }).Unwrap();
            // Process task with value.
            } else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof (Task<>)) {
                var task = (Task) invocation.ReturnValue;
                invocation.ReturnValue = GetContinueWithFunctor(returnType)(task, invocation);
            } else {
                MethodEnd(invocation);
            }
        }

        // NOTE: method should be protected so reflection called from child class has access to this method.
        protected Task ContinueWith<T>(Task task, IInvocation invocation)
        {
            return DoContinueWith<T>((Task<T>)task, invocation);
        }

        private Task<T> DoContinueWith<T>(Task<T> task, IInvocation invocation)
        { 
            return task.ContinueWith(continuation => {
                MethodEnd(invocation);
                return continuation;
            }).Unwrap();
        }

        private Func<Task, IInvocation, Task> GetContinueWithFunctor(Type taskType)
        {
            return _wrapperCreators.GetOrAdd(taskType, t => {
                if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof (Task<>)) {
                    return (Func<Task, IInvocation, Task>)this.GetType()
                        .GetMethod("ContinueWith",
                            BindingFlags.Instance | BindingFlags.NonPublic)
                        .MakeGenericMethod(new[] { taskType.GenericTypeArguments[0] })
                        .CreateDelegate(typeof(Func<Task, IInvocation, Task>), this);
                } else {
                    // Other cases are not supported
                    throw new NotSupportedException(
                        string.Format("Creating wrapper for type {0} isn't supported.", taskType));
                }
            });
        }
    }
}