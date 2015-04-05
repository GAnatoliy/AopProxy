using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;


namespace Interceptors.Tests
{
    public interface ITestService
    {
        void ReturnsVoid(bool throwsException = false);
        void ReturnsVoidAsync();

        Task ReturnsTaskAsync(bool throwsExceptionBeforeAwait = false, bool throwsExceptionAfterAwait = false);

        int ReturnsValue(bool throwsException = false);
        Task<int> ReturnsValueAsync();
    }
}
