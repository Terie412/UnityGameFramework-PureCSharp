using System;
using System.Threading.Tasks;

namespace Core
{
    public interface ISceneLoadingEffect
    {
        public Task WaitForEnterSecondStage();
        public void EnterThirdStage(Action onThirdStageEnd);
    }
}