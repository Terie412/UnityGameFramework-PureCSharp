using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Modules.UI
{
    /// 将IEventSystemHandler的接口都放在一块儿方便在底层收敛管理，算是一种较为常见的做法
    /// 毕竟我们走不到UGUI的底层，对于Button和Toggle类似的点击控件要做处理的话，要分别处理
    [SelectionBase]
    public class UIEventListener : MonoBehaviour, ISelectHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Action<PointerEventData> onClick;
        public Action<PointerEventData> onPointerDown;
        public Action<PointerEventData> onPointerUp;
        public Action<PointerEventData> onBeginDrag;
        public Action<PointerEventData> onDrag;
        public Action<PointerEventData> onEndDrag;
        public Action<PointerEventData> onSelect;

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            onPointerDown?.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            onPointerUp?.Invoke(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (onBeginDrag != null)
            {
                onBeginDrag?.Invoke(eventData);
            }
            else
            {
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (onDrag != null)
            {
                onDrag?.Invoke(eventData);
            }
            else
            {
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (onEndDrag != null)
            {
                onEndDrag?.Invoke(eventData);
            }
            else
            {
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            onSelect?.Invoke(eventData as PointerEventData);
        }
    }
}