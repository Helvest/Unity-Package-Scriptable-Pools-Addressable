using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ScriptablePool.Addressable
{
	public abstract class PoolAdressable<C> : PoolAbstract where C : Component
	{

		#region Variables

		[SerializeField]
		private AssetReference _assetReference = null;

		[NonSerialized]
		private C _prefab = null;

		protected override GameObject _Prefab => _prefab.gameObject;

		public bool IsReady => _prefab != null;

		#endregion

		#region EnablePool & DisablePool

		protected override void OnEnablePool()
		{
#if UNITY_EDITOR
			GameObject go = new GameObject($"Pool {_assetReference.editorAsset.name}");
			go.SetActive(false);
			go.isStatic = true;

			_parent = go.transform;
			_parent.parent = _bigParent;
#endif

			var _asyncOperationHandle = Addressables.LoadAssetAsync<C>(_assetReference);
			_asyncOperationHandle.Completed += OnLoadDone;
		}

		protected override void OnDisablePool()
		{
			if (_prefab)
			{
				_assetReference.ReleaseAsset();
				_prefab = null;
			}
		}

		#endregion

		#region OnLoadDone

		private void OnLoadDone(AsyncOperationHandle<C> obj)
		{
			_prefab = obj.Result;

			for (int i = 0; i < _poolOverideSize; i++)
			{
				var go = Instantiate(_prefab, _parent).gameObject;
				go.SetActive(false);

				_poolList.Add(go);
				_queu.Enqueue(go);

#if UNITY_EDITOR
				go.name = $"{_prefab.name}_{i:000}";
#endif
			}
		}

		#endregion

	}
}
