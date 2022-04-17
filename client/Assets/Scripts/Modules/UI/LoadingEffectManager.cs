using System.Threading.Tasks;
using QTC.Modules.UI;
using UnityEngine;

namespace QTC.Modules.UI
{
    public class LoadingEffectManager: SingleTon<LoadingEffectManager>
    {
        public GameObject curEffect;
        private Transform root => UIManager.Instance.LoadingEffectCanvas.transform;

        public async Task<GameObject> LoadAsync(string effectName)
        {
            if (curEffect != null)
            {
                Object.Destroy(curEffect);
                curEffect = null;
            }
            var go = await AssetManager.Instance.LoadAndInstantiateGameObjectAsync(effectName, root);
            curEffect = go;
            return go;
        }
    }
}

