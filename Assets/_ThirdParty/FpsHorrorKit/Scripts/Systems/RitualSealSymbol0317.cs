using UnityEngine;

namespace FpsHorrorKit
{
    [DisallowMultipleComponent]
    public sealed class RitualSealSymbol0317 : MonoBehaviour, IInteractable
    {
        [SerializeField] private RitualDoorSeal0317 seal;
        [SerializeField] private int order;
        [SerializeField] private Renderer symbolRenderer;
        [SerializeField] private Light symbolLight;
        private bool solved;
        public int Order { get { return order; } }
        public bool IsSolved { get { return solved; } }
        public void Interact() { if (!solved && seal != null) seal.ActivateSymbol(this); }
        public void Highlight() { if (!solved) PlayerInteract.Instance?.ChangeInteractText("Nhấn [E] để chạm vào ấn"); }
        public void HoldInteract() { }
        public void UnHighlight() { }
        public void SetSolved(bool value) { solved=value; if(symbolLight!=null) symbolLight.enabled=value; if(symbolRenderer!=null) symbolRenderer.material.color=value?new Color(1f,.35f,.02f):new Color(.4f,.04f,.02f); }
    }
}
