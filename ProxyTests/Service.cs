using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyTests
{
    public class Service: IService
    {
        public void Do()
        {
            Console.WriteLine("Service.Do call");
        }

        public virtual int Get()
        {
            Console.WriteLine("Service.Get call start");
            var rez = 10;
            Console.WriteLine("Service.Get call end");
            return rez;
        }

        public async Task<int> GetAsync()
        {
            Console.WriteLine("Service.GetAsync call start");
            await Task.Delay(10);
            Console.WriteLine("Service.GetAsync call end");
            return 7;
        }

        public async Task GetTaskAsync()
        {
            Console.WriteLine("Service.GetTaskAsync call start");
            await Task.Delay(10);
            Console.WriteLine("Service.GetTaskAsync call end");
        }

        public async Task<int> GetWihoutLogAsync()
        {
            await Task.Delay(10);
            return 7;
        }

        public async Task GetTaskWithoutLogAsync()
        {
            await Task.Delay(10);
        }

        public Task GetTaskWithExceptinAsync()
        {
            Console.WriteLine("Service.GetWithExceptinAsync call");
            throw new Exception("Exception from GetWithExceptinAsync");
        }
    }
}
