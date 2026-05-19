using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BomBomLemon.PlayerSetup
{
    /// <summary>
    /// ScrollRect 内の InputField でもスクロールできるよう、
    /// 縦方向ドラッグを親 ScrollRect へ転送する。
    /// </summary>
    public class ScrollDragForwarder : MonoBehaviour,
        IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        ScrollRect _scroll;
        bool _forwarding;

        void Awake() => _scroll = GetComponentInParent<ScrollRect>();

        public void OnInitializePotentialDrag(PointerEventData e)
            => _scroll?.OnInitializePotentialDrag(e);

        public void OnBeginDrag(PointerEventData e)
        {
            Vector2 delta = e.position - e.pressPosition;
            _forwarding = Mathf.Abs(delta.y) > Mathf.Abs(delta.x);
            if (_forwarding)
            {
                EventSystem.current?.SetSelectedGameObject(null);
                _scroll?.OnBeginDrag(e);
            }
        }

        public void OnDrag(PointerEventData e)
        {
            if (_forwarding) _scroll?.OnDrag(e);
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (_forwarding) { _scroll?.OnEndDrag(e); _forwarding = false; }
        }
    }
}
