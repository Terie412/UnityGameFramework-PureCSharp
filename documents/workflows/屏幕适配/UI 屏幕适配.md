[TOC]

# UI 屏幕适配概念

不同的游戏有不同的UI适配策略，这里只是其中一种。屏幕适配做得越复杂，需要处理和关心的事情就越多（有一款手机上的游戏 《江南百景图》，可竖屏，可横屏，而且横竖屏的UI布局和样式还可以不同，屏幕适配做得很极致）。

这里的屏幕适配方案主要场合是手机上的横屏游戏，为了解决UI脱离安全区的问题。在有刘海屏和全面屏手势的IPhone上，存在一个安全区（safeArea）的概念。安全区是一个处于屏幕中央的矩形区域，处于安全区内部的UI显示不会受到的四角的圆角，刘海以及底部浮动条（Home Indicator）的影响。在安全区之外的，四个方向分别有不同宽度 inset 区域。

我们需要保证一些重要的可交互的，或者显示信息的UI处于安全区里面，同时背景类的UI要覆盖满整个手机屏幕。

当前方案会简化手机的安全区模型，将手机区域分为中间的安全区，以及左右两边等宽的 inset 区。inset的宽度会取实际的两边的 inset 中的最大值。而底部浮动条的影响则由UI设计去处理，即保证在设计UI的时候，不会把UI的位置设计得太接近底部。

1. 获取 inset 的宽度: ScreenAdapterManager.Instance.safeAreaInsetWidthNormalized

# UI Prefab 制作

1. UI 视角下，prefab是正常制作的，只需要注意的就是当锚点设置为全屏的时候，其实对应的是安全区内的全屏。如果想要把某个全屏UI扩展为屏幕全屏，只需要在UI上挂载 AntiSafeAreaRectTransform 组件即可
2. 程序视角下，全屏类UI的根节点需要挂载 SafeAreaRectTransform，使得当前UI始终在安全区内布局

比如要制作一个 LoginWindow 挂载在 MainCanvas 下，可能的节点结构如下：
- MainCanvas
	- LoginWindow: SafeAreaRectTransform
		- bg: AntiSafeAreaRectTransform
		- scrollRect
		- btnLogin
		- others

# 编辑器下模拟安全区

使用Unity Simulator 面板。

