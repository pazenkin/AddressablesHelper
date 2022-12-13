using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Utilities
{
    /// <summary>
    ///  Класс-помощник для управления адрессаблами
    /// </summary>
    public class AddressablesHelper
    {
        public AddressablesHelper()
        {
            _handles = new HandlesDictionary();
        }

        /// <summary>
        /// True, если осуществляется загрузка адрессабла
        /// </summary>
        public bool InProgress => _inProgress > 0;
        
        private readonly HandlesDictionary _handles;
        private int _inProgress;

        /// <summary>
        /// Получить игровой объект по ключу адрессабла.
        /// Автоматически ключ будет добавлен в список используемых.
        /// </summary>
        /// <param name="key">Ключ (AssetReference)</param>
        /// <returns>GameObject</returns>
        public async UniTask<GameObject> GetGameObject(object key)
        {
            _inProgress++;
            var result = await _handles.GetHandleResult(key);
            _inProgress--;
            return result;
        }

        /// <summary>
        /// Выгрузить из памяти загруженный ранее адрессабл.
        /// Важно! Осуществляется не уничтожение через Destroy, а выгрузка через Addressables.ReleaseInstance().
        /// </summary>
        /// <param name="key">Ключ (AssetReference)</param>
        public void Dispose(object key)
        {
            _handles.Dispose(key).Forget();
        }

        /// <summary>
        /// Выгрузить из памяти все загруженные ранее адрессаблы.
        /// Важно! Осуществляется не уничтожение через Destroy, а выгрузка через Addressables.ReleaseInstance().
        /// </summary>
        public void DisposeAll()
        {
            _handles.DisposeAll();
        }

        /// <summary>
        /// Очистить список используемых объектов
        /// </summary>
        public void ClearListOfUsedObjects()
        {
            _handles.ClearListOfUsedObjects();
        }

        /// <summary>
        /// Выгрузить из памяти неиспользуемые адрессаблы.
        /// Важно! Осуществляется не уничтожение через Destroy, а выгрузка через Addressables.ReleaseInstance().
        /// </summary>
        public void DisposeUnused()
        {
            _handles.DisposeUnused();
        }
    }
    
    /// <summary>
    /// Сервисный класс для управления загруженными адрессаблами в единой библиотеке
    /// </summary>
    public class HandlesDictionary
    {
        public HandlesDictionary()
        {
            _handles = new Dictionary<object, AsyncOperationHandle<GameObject>>();
            _keys = new List<object>();
            _used = new List<object>();
        }
        
        private readonly Dictionary<object, AsyncOperationHandle<GameObject>> _handles;
        private readonly List<object> _keys;
        private readonly List<object> _used;

        public async UniTask<GameObject> GetHandleResult(object key)
        {
            if (key == null) return null;
            
            if (!_used.Contains(key))
            {
                _used.Add(key);
            }
            
            if (!_keys.Contains(key))
            {
                if (!_handles.ContainsKey(key))
                {
                    await AddHandle(key);
                }
                else
                {
                    await UniTask.WaitWhile(() => _handles.ContainsKey(key));
                    await AddHandle(key);
                }
            }

            if (!_handles.ContainsKey(key) && _keys.Contains(key))
            {
                await UniTask.WaitWhile(() => !_handles.ContainsKey(key));
            }

            return _handles[key].Result;
        }

        private async UniTask AddHandle(object key)
        {
            _keys.Add(key);
            var handle = Addressables.LoadAssetAsync<GameObject>(key);
            await handle;
            _handles.Add(key, handle);
        }

        public async UniTask Dispose(object key)
        {
            if (_used.Contains(key))
            {
                _used.Remove(key);
            }
            
            if (!_keys.Contains(key)) return;
            _keys.Remove(key);
            
            if (!_handles.ContainsKey(key))
            {
                await UniTask.WaitWhile(() => !_handles.ContainsKey(key));
            }
            Addressables.ReleaseInstance(_handles[key]);
            _handles.Remove(key);
        }

        public void DisposeAll()
        {
            foreach (var handle in _handles.Values)
            {
                Dispose(handle).Forget();
            }

            _handles.Clear();
        }
        
        public void ClearListOfUsedObjects()
        {
            _used.Clear();
        }

        public void DisposeUnused()
        {
            foreach (var handle in _handles.Values)
            {
                if (!_used.Contains(handle)) continue;
                Dispose(handle).Forget();
            }
        }
    }
}
