using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyTests
{
    /// <summary>
    /// Test service interface.
    /// </summary>
    public interface IService
    {
        void Do();

        int Get();

        Task<int> GetAsync();

        Task GetTaskAsync();

        Task<int> GetWihoutLogAsync();

        Task GetTaskWithoutLogAsync();

        Task GetTaskWithExceptinAsync();
    }
}
