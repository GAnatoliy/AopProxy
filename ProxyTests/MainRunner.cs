using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;


namespace ProxyTests
{
    public class MainRunner
    {
        private static readonly ProxyGenerator _generator = new ProxyGenerator();

        public static void Main()
        {
            IService service = new Service();

            var interseptor = new TaskWrapperInterceptor();
            var service2 = _generator.CreateInterfaceProxyWithTarget(service, interseptor);

            // Test method that return void.
            Console.WriteLine("Task that returns void");
            service2.Do();

            // Test method that return value.
            Console.WriteLine(service2.Get());
            Console.WriteLine();

            // Test asyn method that return value.
            Console.WriteLine();
            Console.WriteLine("Async with Task<> tests");
            Console.WriteLine();
            
            var asyncResult = service2.GetAsync().Result;
            Console.WriteLine("Result returned");
            Console.WriteLine(asyncResult);

            // Test async method that returns no value (Task)
            Console.WriteLine();
            Console.WriteLine("Async with Task tests");
            Console.WriteLine();

            //service.GetTaskAsync().Wait();
            service2.GetTaskAsync().Wait();
            Console.WriteLine("Result returned");
        }

        // NOTE: almost no difference.
        public static void TaskWrapperInterceptorPerformanceTests()
        {
            const int RUN_NUMBER = 100;

            IService service = new Service();
            var interseptor = new TaskWrapperInterceptor();
            IService proxyService = _generator.CreateInterfaceProxyWithTarget(service, interseptor);

            StringBuilder sb = new StringBuilder();

            // Test method that returns Task result.

            sb.AppendLine("Service async method that returns Task calls");
            var begin = DateTime.UtcNow;

            for (int i = 0; i < RUN_NUMBER; i++) {
                service.GetTaskWithoutLogAsync().Wait();
            }

            var end = DateTime.UtcNow;
            sb.AppendLine("Measured time: " + (end - begin).TotalMilliseconds + " ms.");

            sb.AppendLine("Interceptor Service async method that returns Task calls");
            begin = DateTime.UtcNow;

            for (int i = 0; i < RUN_NUMBER; i++) {
                proxyService.GetTaskWithoutLogAsync().Wait();
            }

            end = DateTime.UtcNow;
            sb.AppendLine("Measured time: " + (end - begin).TotalMilliseconds + " ms.");

            // Tests method that returns Task<> results

            sb.AppendLine("Service async method that returns Task<> calls");
            begin = DateTime.UtcNow;

            for (int i = 0; i < RUN_NUMBER; i++) {
                service.GetWihoutLogAsync().Wait();
            }

            end = DateTime.UtcNow;
            sb.AppendLine("Measured time: " + (end - begin).TotalMilliseconds + " ms.");

            sb.AppendLine("Interceptor service async method that returns Task<> calls");
            begin = DateTime.UtcNow;

            for (int i = 0; i < RUN_NUMBER; i++) {
                proxyService.GetWihoutLogAsync().Wait();
            }

            end = DateTime.UtcNow;
            sb.AppendLine("Measured time: " + (end - begin).TotalMilliseconds + " ms.");

            // Chain of interceptors (7 interceptors)
            var proxyServiceChain = _generator.CreateInterfaceProxyWithTarget(service, 
                interseptor, interseptor, interseptor, interseptor, interseptor, interseptor, interseptor);

            sb.AppendLine("Chain of interceptors (7 interceptors) service async method that returns Task<> calls");
            begin = DateTime.UtcNow;

            for (int i = 0; i < RUN_NUMBER; i++) {
                proxyServiceChain.GetWihoutLogAsync().Wait();
            }

            end = DateTime.UtcNow;
            sb.AppendLine("Measured time: " + (end - begin).TotalMilliseconds + " ms.");

            Console.WriteLine(sb.ToString());
        }

        public static void ContinueWithInterceptorTests()
        {
            IService service = new Service();

            var interseptor = new Interceptor();
            var service2 = _generator.CreateInterfaceProxyWithTarget(service, interseptor);

            // Test method that return void.
            Console.WriteLine("Task that returns void");
            service2.Do();

            // Test method that return value.
            Console.WriteLine(service2.Get());
            Console.WriteLine();

            // Test asyn method that return value.
            Console.WriteLine();
            Console.WriteLine("Async with Task<> tests");
            Console.WriteLine();

            var asyncResult = service2.GetAsync().Result;
            Console.WriteLine("Result returned");
            Console.WriteLine(asyncResult);

            // Test async method that returns no value (Task)
            Console.WriteLine();
            Console.WriteLine("Async with Task tests");
            Console.WriteLine();

            //service.GetTaskAsync().Wait();
            service2.GetTaskAsync().Wait();
            Console.WriteLine("Result returned");

            // Test method that throws exception.
            //Console.WriteLine();
            //Console.WriteLine("Async with exception Task<> tests");
            //Console.WriteLine();

            //service2.GetTaskWithExceptinAsync().Wait();
            //Console.WriteLine("Result returned");
        }

        // NOTE: almost no difference.
        public static void ContinueWithInterceptorPerformanceTests()
        {
            const int RUN_NUMBER = 100;

            IService service = new Service();
            var interseptor = new Interceptor();
            IService proxyService = _generator.CreateInterfaceProxyWithTarget(service, interseptor);

            StringBuilder sb = new StringBuilder();

            // Test method that returns Task result.

            sb.AppendLine("Service async method that returns Task calls");
            var begin = DateTime.UtcNow;

            for (int i = 0; i < RUN_NUMBER; i++)
            {
                service.GetTaskWithoutLogAsync().Wait();
            }

            var end = DateTime.UtcNow;
            sb.AppendLine("Measured time: " + (end - begin).TotalMilliseconds + " ms.");

            sb.AppendLine("Interceptor Service async method that returns Task calls");
            begin = DateTime.UtcNow;

            for (int i = 0; i < RUN_NUMBER; i++)
            {
                proxyService.GetTaskWithoutLogAsync().Wait();
            }

            end = DateTime.UtcNow;
            sb.AppendLine("Measured time: " + (end - begin).TotalMilliseconds + " ms.");

            // Tests method that returns Task<> results

            sb.AppendLine("Service async method that returns Task<> calls");
            begin = DateTime.UtcNow;

            for (int i = 0; i < RUN_NUMBER; i++)
            {
                service.GetWihoutLogAsync().Wait();
            }

            end = DateTime.UtcNow;
            sb.AppendLine("Measured time: " + (end - begin).TotalMilliseconds + " ms.");

            sb.AppendLine("Interceptor service async method that returns Task<> calls");
            begin = DateTime.UtcNow;

            for (int i = 0; i < RUN_NUMBER; i++)
            {
                proxyService.GetWihoutLogAsync().Wait();
            }

            end = DateTime.UtcNow;
            sb.AppendLine("Measured time: " + (end - begin).TotalMilliseconds + " ms.");

            // Chain of interceptors (7 interceptors)
            var proxyServiceChain = _generator.CreateInterfaceProxyWithTarget(service,
                interseptor, interseptor, interseptor, interseptor, interseptor, interseptor, interseptor);

            sb.AppendLine("Chain of interceptors (7 interceptors) service async method that returns Task<> calls");
            begin = DateTime.UtcNow;

            for (int i = 0; i < RUN_NUMBER; i++)
            {
                proxyServiceChain.GetWihoutLogAsync().Wait();
            }

            end = DateTime.UtcNow;
            sb.AppendLine("Measured time: " + (end - begin).TotalMilliseconds + " ms.");

            Console.WriteLine(sb.ToString());
        }
    }
}
