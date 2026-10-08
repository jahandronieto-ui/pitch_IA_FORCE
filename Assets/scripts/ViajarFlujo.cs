using UnityEngine;
using UnityEngine.UI;

namespace GuardianesFlujo
{
    public class ViajarFlujo : MonoBehaviour
    {
        public string escenaDestino, mundoRequerido;
        public string territorioAnterior;
        void Start()
        {
            var b = GetComponent<Button>();
            if (b != null && b.targetGraphic != null && !Disponible()) b.targetGraphic.color = new Color(0.65f, 0.65f, 0.65f, 0.8f);
        }
        bool Disponible() { return string.IsNullOrEmpty(territorioAnterior) || PlayerPrefs.GetInt("GT." + territorioAnterior + ".Completo", 0) == 1 || PlayerPrefs.GetInt("GT." + mundoRequerido + ".Desbloqueado", 0) == 1; }
        public void Viajar()
        {
            if (FlujoTransicion.Ocupada) return;
            if (!Disponible()) { FlujoUI.Mensaje(mundoRequerido, "Completa las actividades de " + territorioAnterior + " para desbloquear este territorio."); return; }
            FlujoTransicion.Cargar(escenaDestino);
        }
    }
}
