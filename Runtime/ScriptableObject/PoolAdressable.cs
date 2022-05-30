using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ScriptablePool.Addressable
{
	public abstract class PoolAdressable<T> : PoolAbstract<T> where T : Component
	{

		#region Fields

		[SerializeField]
		private AssetReference _assetReference = null;

		[NonSerialized]
		private T _prefab = null;

		protected override T _Prefab => _prefab;

		public bool IsReady => _prefab != null;

		#endregion

		#region EnablePool & DisablePool

		protected override void OnEnablePool()
		{
			Addressables.LoadAssetAsync<T>(_assetReference).Completed += OnLoadDone;
		}

		protected override void OnDisablePool()
		{
			if (_prefab)
			{
				_prefab = null;
				_assetReference.ReleaseAsset();
			}
		}

		#endregion

		#region OnLoadDone

		private void OnLoadDone(AsyncOperationHandle<T> obj)
		{
			_prefab = obj.Result;

			for (int i = 0; i < poolOverideSize; i++)
			{
				var component = Instantiate(_prefab, poolParent);

				var poolObject = new PoolObject<T>(component);

				poolObject.gameObject.SetActive(false);

				_poolList.Add(poolObject);
				_queu.Enqueue(poolObject);

#if UNITY_EDITOR
				component.name = $"{_prefab.name}_{i:000}";
#endif
			}
		}

		#endregion

	}
}
