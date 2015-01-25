using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;


namespace ProxyTests
{
    /// <summary>
    /// If methods are async then just wrap task in the wrapper that will be returned instead.
    /// 
    /// Idea is taken here https://msdn.microsoft.com/en-us/magazine/dn574805.aspx
    /// </summary>
    class TaskWrapperInterceptor: IInterceptor
    {
        // Caches wrappers in order prevent build it for each call.
        // TODO: check how it will work with nested calsses.
        private static ConcurrentDictionary<Type, Func<Task, IInvocation, Task>>
            _wrapperCreators = new ConcurrentDictionary<Type, Func<Task, IInvocation, Task>>();

        /// <summary>
        /// Is called before intercepted method call.
        /// </summary>
        protected virtual void MethodStart() {
            Console.WriteLine("TaskWrapperInterceptor call start");
        }

        /// <summary>
        /// Is called after intercepted method call.
        /// </summary>
        protected virtual void MethodEnd() {
            Console.WriteLine("TaskWrapperInterceptor call end");
        }

        /// <summary>
        /// Is called if intercepted method throws exception.
        /// </summary>
        protected virtual void ExceptionThrown() {
            
        }
        //public void Intercept(IInvocation invocation)
        //{
        //    Console.WriteLine("TaskWrapperInterceptor call start");
        //    invocation.Proceed();

        //    // TODO: check how works with void.
        //    var returnType = invocation.Method.ReturnType;

        //    // Process task result.
        //    // TODO: check it.
        //    if (returnType == typeof (Task)) {
        //        var task = (Task) invocation.ReturnValue;
        //        invocation.ReturnValue = task.ContinueWith(
        //            continuation => Console.WriteLine("TaskWrapperInterceptor call end"));

        //        //invocation.ReturnValue = task.ConfigureAwait(false);
        //        // Process task with value.
        //    } else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>)) {
        //        Console.WriteLine(returnType.GetGenericTypeDefinition());
        //        Console.WriteLine(returnType);
        //        Console.WriteLine(invocation.ReturnValue);
        //        //var task2 = (Task<object>)invocation.ReturnValue;
        //        var task2 = (Task)invocation.ReturnValue;

        //        //MethodInfo method = returnType.GetMethod("ContinueWith");
        //        //MethodInfo genericMethod = method.MakeGenericMethod(returnType);

        //        //genericMethod.Invoke(task2, new object [];);
                
        //        //var t = returnType.GetGenericTypeDefinition();
        //        //var task2 = Transform<Task<>>(invocation.ReturnValue);
                
        //        var continueTask2 = task2.ContinueWith((Task continuation) => {
        //            Console.WriteLine("TaskWrapperInterceptor call end");
        //            var result = continuation.GetType()
        //                .GetProperty("Result")
        //                .GetValue(continuation, null);

        //            invocation.ReturnValue = result;
        //            //return continuation;
        //        });
        //        //invocation.ReturnValue = continueTask2;
        //    } else {
        //        Console.WriteLine("TaskWrapperInterceptor call end");
        //    }
        //}

        //private T2 Transform<T2>(object input) where T2: class
        //{
        //    return input as T2;
        //}

        public void Intercept(IInvocation invocation)
        {
            MethodStart();

            invocation.Proceed();

            // Process task result.
            if (invocation.ReturnValue is Task) {
                var task = (Task)invocation.ReturnValue;
                // Get wrapper creator for return type and returns wrapper task.
                invocation.ReturnValue = GetWrapperCreator(invocation.Method.ReturnType)(task, invocation);
            } else {
                MethodEnd();
            }
        }
         
        private async Task CreateWrapperTask(Task task, IInvocation invocation)
        {
            try {
                await task.ConfigureAwait(false);
                MethodEnd();
            } catch (Exception e) {
                ExceptionThrown();
                throw;
            }
        }

        private Task CreateGenericWrapperTask<T>(Task task, IInvocation invocation)
        {
            return this.DoCreateGenericWrapperTask<T>((Task<T>)task, invocation);
        }

        private async Task<T> DoCreateGenericWrapperTask<T>(Task<T> task, IInvocation invocation)
        {
            try {
                T value = await task.ConfigureAwait(false);
                MethodEnd();
                return value;
            } catch (Exception e) {
                ExceptionThrown();
                throw;
            }
        }

        private Func<Task, IInvocation, Task> GetWrapperCreator(Type taskType)
        {
            return _wrapperCreators.GetOrAdd(taskType, t => {
                if (taskType == typeof (Task)) {
                    return CreateWrapperTask;
                } else if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof (Task<>)) {
                    return (Func<Task, IInvocation, Task>) this.GetType()
                        .GetMethod("CreateGenericWrapperTask",
                            BindingFlags.Instance | BindingFlags.NonPublic)
                        .MakeGenericMethod(new[] { taskType.GenericTypeArguments[0] })
                        .CreateDelegate(typeof (Func<Task, IInvocation, Task>), this);
                } else {
                    // Other cases are not supported
                    //return (task, _) => task;
                    throw new NotSupportedException(
                        string.Format("Creating wrapper for type {0} isn't supported.", taskType));
                }
            });
        }
    }
}
