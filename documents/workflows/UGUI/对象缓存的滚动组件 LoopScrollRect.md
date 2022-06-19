# VertialLoopScrollRect

其他类型的LoopScrollRect类似。

1. 菜单 UI/LoopVertialScrollRect 创建组件
2. 在 content 下面放置好一个元素，它将作为一个模板初始化一个对象缓存池，并在滚动地过程中，组件会不断地从缓存池中获取和返回滚动元素
3. 接口初始化
```c#
var ls = go.GetComponent<LoopScrollRectBase>();
// 设置总数量
ls.totalCount = totalCount;
// 设置元素的刷新方法
ls.onItemBuild.AddListener((obj, index) =>
{
    obj.transform.GetChild(0).GetComponent<Text>().text = index.ToString();
});
// 刷新整个组件
ls.RefillCells();
```