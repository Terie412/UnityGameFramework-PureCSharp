using Framework;
using UnityEngine;

public class TestInitializeOnRuntime : MonoBehaviour
{
    private void Start()
    {
        GameLogger.Instance.Init();
    }
}
