using UnityEngine;
using UnityEngine.UI;

namespace GuardianesFlujo
{
    public class ViajarFlujo : MonoBehaviour
    {
        public string escenaDestino, mundoRequerido;
        public string territorioAnterior;
        [Header("Estado visual del territorio")]
        public Image imagenNodo;
        public Sprite spriteBloqueado, spriteNumero;
        public Image[] estrellas = new Image[4];
        public Color colorEstrellaPendiente = new Color(0.25f, 0.28f, 0.32f, 1);
        public Color colorEstrellaCompleta = Color.white;
        void Start()
        {
            ActualizarEstado();
        }
        public void ActualizarEstado()
        {
            bool disponible = Disponible();
            if (imagenNodo == null) imagenNodo = GetComponent<Image>();
            if (imagenNodo != null)
            {
                Sprite sprite = disponible ? spriteNumero : spriteBloqueado;
                if (sprite != null) imagenNodo.sprite = sprite;
                imagenNodo.color = Color.white;
                imagenNodo.preserveAspect = true;
            }
            if (estrellas != null)
                for (int i = 0; i < estrellas.Length && i < ProgresoJuegoFlujo.Actividades.Length; i++)
                    if (estrellas[i] != null)
                    {
                        estrellas[i].gameObject.SetActive(true);
                        estrellas[i].raycastTarget = false;
                        estrellas[i].color = ProgresoJuegoFlujo.ActividadCompleta(mundoRequerido, ProgresoJuegoFlujo.Actividades[i])
                            ? colorEstrellaCompleta : colorEstrellaPendiente;
                    }
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
