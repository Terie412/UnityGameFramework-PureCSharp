using System.Threading.Tasks;

namespace Framework.ScreenAdapter
{
    public class IPhoneSimulator: SingleTon<IPhoneSimulator>
    {
        private ScreenSize referenceScreenSize = new(2436, 1125); // 当编辑器的分辨率等于该参考分辨率的时候，则使用IPhone的一些适配数据进行适配模拟
        public readonly float safeAreaInsetWidthNormalized = 132f / 2436f; // 模拟的安全区侧边的大小

        public async Task Init(ScreenSize curScreenSize)
        {
            if (curScreenSize.Equals(referenceScreenSize))
            {
                await AssetManager.Instance.LoadAndInstantiateGameObjectAsync("IPhoneSimulatorCanvas", null);
            }
        }
    }
}
