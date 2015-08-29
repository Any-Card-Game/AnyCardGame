using System.Threading.Tasks;

namespace Common.Utils
{
    public interface ILateTaskManager<T>
    {
        Task<T> Build(string key);
        bool Exists(string key);
        void Resolve(string key, T item);
    }
}