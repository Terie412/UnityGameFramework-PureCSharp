using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SplitableWindow : EditorWindow
{
    protected SplitContext curSplitContext => contexts[splitStack.Peek() - 1]; // Current Split Context
    private int splitCount;
    private readonly Stack<int> splitStack = new Stack<int>();
    private readonly List<SplitContext> contexts = new List<SplitContext>();

    protected void BeginHorizontalSplit(float defaultSize = 200, bool canResize = true)
    {
        splitCount++;
        splitStack.Push(splitCount);
        SplitContext context;
        if (contexts.Count < splitCount)
        {
            context = new SplitContext();
            context.canResize = canResize;
            context.splitType = SplitContext.SplitType.H;
            contexts.Add(context);
        }
        else
        {
            context = contexts[splitCount - 1];
        }

        var editorRect = EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        if (editorRect != Rect.zero)
        {
            context.rect = editorRect;
            if (context.firstSize < 0)
            {
                context.firstSize = defaultSize;
            }
        }

        context.scrollPos_1 = EditorGUILayout.BeginScrollView(context.scrollPos_1, GUILayout.Width(context.firstSize), GUILayout.ExpandHeight(true));
    }

    protected void BeginVerticalSplit(float defaultSize = 200, bool canResize = true)
    {
        splitCount++;
        splitStack.Push(splitCount);
        SplitContext context;
        if (contexts.Count < splitCount)
        {
            context = new SplitContext();
            context.canResize = canResize;
            context.splitType = SplitContext.SplitType.V;
            contexts.Add(context);
        }
        else
        {
            context = contexts[splitCount - 1];
        }

        var editorRect = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
        if (editorRect != Rect.zero)
        {
            context.rect = editorRect;
            if (context.firstSize < 0)
            {
                context.firstSize = defaultSize;
            }
        }

        context.scrollPos_1 = EditorGUILayout.BeginScrollView(context.scrollPos_1, GUILayout.Height(context.firstSize), GUILayout.ExpandWidth(true));
    }

    protected void Split()
    {
        EditorGUILayout.EndScrollView();
        var context = contexts[splitStack.Peek() - 1];
        context.scrollPos_2 = EditorGUILayout.BeginScrollView(context.scrollPos_2, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
    }

    protected void EndSplit()
    {
        var count = splitStack.Pop();
        var context = contexts[count - 1];
        EditorGUILayout.EndScrollView();

        if (context.canResize)
        {
            ProcessMouseEvent(context);
        }

        if (context.splitType == SplitContext.SplitType.H)
        {
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.EndVertical();
        }
    }

    private void ProcessMouseEvent(SplitContext context)
    {
        var rect = new Rect();
        if (context.splitType == SplitContext.SplitType.H)
        {
            rect.x = context.rect.x + context.firstSize - 2;
            rect.y = context.rect.y;
            rect.width = 4;
            rect.height = context.rect.height;
        }
        else
        {
            rect.x = context.rect.x;
            rect.y = context.rect.y + context.firstSize - 2;
            rect.width = context.rect.width;
            rect.height = 4;
        }

        // GUI.Box(rect, "");

        EditorGUIUtility.AddCursorRect(rect, context.splitType == SplitContext.SplitType.H ? MouseCursor.ResizeHorizontal : MouseCursor.ResizeVertical);

        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            context.isResizing = true;
        }

        if (Event.current.type == EventType.MouseUp)
            context.isResizing = false;

        if (context.isResizing && Event.current.type == EventType.MouseDrag)
        {
            var delta = Event.current.delta;
            if (context.splitType == SplitContext.SplitType.H)
            {
                context.firstSize += delta.x;
            }
            else
            {
                context.firstSize += delta.y;
            }

            context.firstSize = Mathf.Max(2, context.firstSize);

            Repaint();
        }
    }

    protected void InitSplitEnvironment()
    {
        splitCount = 0;
    }
}

public class SplitContext
{
    public Rect rect;
    public Vector2 scrollPos_1;
    public Vector2 scrollPos_2;
    public float firstSize;
    public bool isResizing;
    public bool canResize;
    public SplitType splitType = SplitType.H;

    public enum SplitType
    {
        H, // Horizontal
        V // Vertical
    };

    public SplitContext()
    {
        rect = Rect.zero;
        scrollPos_1 = Vector2.zero;
        scrollPos_2 = Vector2.zero;
        firstSize = -1;
        isResizing = false;
    }
}