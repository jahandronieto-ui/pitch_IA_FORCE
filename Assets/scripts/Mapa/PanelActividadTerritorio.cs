using UnityEngine;
using UnityEngine.Events;

namespace GuardianesTerritorio
{
    // Poner en el Panel de dialogo o minijuego, inicialmente inactivo.
    public class PanelActividadTerritorio : MonoBehaviour
    {
        public ProgresoTerritorio progreso;
        public string idMision;
        public UnityEvent alAbrir;
        JugadorTerritorio jugador;
        bool abierto;

        public void Abrir(JugadorTerritorio nuevoJugador)
        {
            if (abierto || nuevoJugador == null) return;
            jugador = nuevoJugador;
            abierto = true;
            jugador.Bloquear();
            gameObject.SetActive(true);
            alAbrir.Invoke();
        }

        // Conectar SOLO al resultado correcto o al final de la conversacion.
        public void Completar()
        {
            if (!abierto) return;
            if (progreso != null && !string.IsNullOrWhiteSpace(idMision)) progreso.Completar(idMision);
            Cerrar();
        }

        // Cancelar/cerrar no marca la mision como completada.
        public void Cerrar() { LiberarJugador(); gameObject.SetActive(false); }

        void LiberarJugador()
        {
            if (abierto && jugador != null) jugador.Desbloquear();
            abierto = false;
            jugador = null;
        }

        void OnDisable() { LiberarJugador(); }
    }
}
