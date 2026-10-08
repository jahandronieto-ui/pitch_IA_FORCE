using UnityEngine;

namespace GuardianesTerritorio
{
    public class BotonInteractuarTerritorio : MonoBehaviour
    {
        public CrearJugadorTerritorio creador;
        public void Interactuar()
        {
            if (creador != null && creador.Jugador != null) creador.Jugador.Interactuar();
        }
    }
}
