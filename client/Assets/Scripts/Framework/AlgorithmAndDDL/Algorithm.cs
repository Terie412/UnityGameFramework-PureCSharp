using System;
using UnityEngine;
using Random = System.Random;

namespace Framework
{
    public static class Algorithm
    {
        private static Random random;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            Debug.Log("初始化 Algorithm");
            random = new(DateTime.Now.Millisecond);
        }

        /// Pick K integers at random from [1, N] with medium probability
        public static int[] NChooseK(int N, int K)
        {
            var result = new int[K];
            if (N <= 0 || K > N)
            {
                for (var i = 0; i < result.Length; i++)
                {
                    result[i] = i + 1;
                }

                return result;
            }

            var selection = K;
            var remains = N;
            for (int i = 1; i < N + 1; i++)
            {
                if (selection < 1)
                    break;

                if (random.Next(1, remains) <= selection)
                {
                    result[K - selection] = i;
                    selection--;
                }

                remains--;
            }

            return result;
        }
    }
}