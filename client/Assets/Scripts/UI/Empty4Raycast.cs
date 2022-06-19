using UnityEngine;
using UnityEngine.UI;

// 用一块儿不增加渲染开销的透明块接收UGUI事件，貌似是钱康来提的方案
[RequireComponent(typeof(CanvasRenderer))]
public class Empty4Raycast : MaskableGraphic
{
    protected Empty4Raycast()
    {
        useLegacyMeshGeneration = false;
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        toFill.Clear();
    }
}