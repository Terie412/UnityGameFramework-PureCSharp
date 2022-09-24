using System;
using System.Threading.Tasks;

namespace Core
{
    public class CommonProgressEffect: UIBase, ISceneLoadingEffect
    {
        private Action onEnterSecondStage;
        private Action onThirdStageEnd;
        
        public void InitAndPlay(Action onEnterSecondStage, Action onThirdStageEnd)
        {
            this.onEnterSecondStage = onEnterSecondStage;
            this.onThirdStageEnd = onThirdStageEnd;
        }

        public async Task WaitForEnterSecondStage()
        {
            return;
        }

        public void EnterThirdStage(Action onThirdStageEnd)
        {
            this.onThirdStageEnd = onThirdStageEnd;
        }
    }
}