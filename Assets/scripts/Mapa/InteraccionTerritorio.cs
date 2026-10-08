using UnityEngine;
using UnityEngine.Events;

namespace GuardianesTerritorio
{
    public class InteraccionTerritorio : MonoBehaviour
    {
        public PanelActividadTerritorio panel;
        public ProgresoTerritorio progreso;
        public string[] misionesPrevias;
        public UnityEvent alFaltarRequisitos;
        public UnityEvent alInteractuar;
        public void Activar(JugadorTerritorio jugador)
        {
            if (misionesPrevias != null && misionesPrevias.Length > 0)
            {
                foreach (string id in misionesPrevias)
                    if (progreso == null || !progreso.EstaCompleta(id))
                    {
                        alFaltarRequisitos.Invoke();
                        return;
                    }
            }
            if (panel != null) panel.Abrir(jugador);
            alInteractuar.Invoke();
        }
    }
}
