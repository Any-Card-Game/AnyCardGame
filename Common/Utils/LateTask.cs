using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Utils
{

    public class LateTaskResult<T>
    {
        public Task<T> Task { get; set; }
        public T Result { get; set; }
    }
    public class LateTaskManager<T>
    {
        private Dictionary<string, LateTaskResult<T>> tasks = new Dictionary<string, LateTaskResult<T>>();

        public Task<T> Build(string key)
        {
            var lateTaskResult = new LateTaskResult<T>();
            tasks.Add(key, lateTaskResult);
            var deferred = new Task<T>(() =>
            {
                tasks.Remove(key);
                return lateTaskResult.Result;
            });
            lateTaskResult.Task = deferred;
            return deferred;
        }

        public bool Exists(string key)
        {
            return tasks.ContainsKey(key);
        }
        public void Resolve(string key, T item)
        {
            tasks[key].Result = item;
            tasks[key].Task.Start();
        }

    }
}