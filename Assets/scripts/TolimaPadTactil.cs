using UnityEngine;
using UnityEngine.EventSystems;

namespace GuardianesTolima
{
    public class TolimaPadTactil : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public TolimaMundo mundo;
        public Vector2 direccion;
        bool pulsado;
        public void OnPointerDown(PointerEventData e) { pulsado = true; }
        public void OnPointerUp(PointerEventData e) { Soltar(); }
        void Update()
        {
            if (pulsado && mundo != null && mundo.Jugador != null) mundo.Jugador.EntradaTactil(direccion);
        }
        void Soltar()
        {
            if (!pulsado) return;
            pulsado = false;
            if (mundo != null && mundo.Jugador != null) mundo.Jugador.EntradaTactil(Vector2.zero);
        }
        void OnDisable() { Soltar(); }
    }
}
