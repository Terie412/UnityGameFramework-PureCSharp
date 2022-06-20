using System;
using System.Threading.Tasks;

namespace Modules.UI
{
    public interface ISceneLoadingEffect
    {
        public Task WaitForEnterSecondStage();
        public void EnterThirdStage(Action onThirdStageEnd);
    }
}