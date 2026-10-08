using UnityEngine;
using UnityEngine.EventSystems;

namespace GuardianesTerritorio
{
    // Poner una copia en cada flecha de movimiento del Canvas.
    public class ControlesTactilesTerritorio : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public CrearJugadorTerritorio creador;
        public Vector2 direccion = Vector2.up;
        bool presionado;
        public void OnPointerDown(PointerEventData e) { presionado = true; }
        public void OnPointerUp(PointerEventData e) { Soltar(); }
        void Update()
        {
            if (presionado && creador != null && creador.Jugador != null)
                creador.Jugador.EntradaTactil(direccion);
        }
        void Soltar()
        {
            if (!presionado) return;
            presionado = false;
            if (creador != null && creador.Jugador != null) creador.Jugador.EntradaTactil(Vector2.zero);
        }
        void OnDisable() { Soltar(); }
    }
}
