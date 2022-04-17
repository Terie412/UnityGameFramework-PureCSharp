using System;
using System.Threading.Tasks;

namespace QTC.Modules.UI
{
    public interface ISceneLoadingEffect
    {
        public Task WaitForEnterSecondStage();
        public void EnterThirdStage(Action onThirdStageEnd);
    }
}