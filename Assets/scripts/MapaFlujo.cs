using UnityEngine;

namespace GuardianesFlujo
{
    public class MapaFlujo : MonoBehaviour
    {
        public CatalogoFlujo catalogo;
        void Start()
        {
            var canvas = FlujoUI.Canvas("HUDMapaFlujo");
            var personaje = catalogo != null ? catalogo.Buscar(PlayerPrefs.GetInt("ID_Seleccionada", 1)) : null;
            var nombre = FlujoUI.Texto(canvas.transform, personaje != null ? personaje.nombre : "Elige un personaje", Vector2.zero, new Vector2(560, 70));
            var r = nombre.rectTransform; r.anchorMin = r.anchorMax = new Vector2(0, 1); r.anchoredPosition = new Vector2(300, -45);
            var b = FlujoUI.Boton(canvas.transform, "Cambiar personaje", Vector2.zero, new Vector2(310, 60), () => FlujoTransicion.Cargar("SeleccionPersonaje"));
            var rb = b.GetComponent<RectTransform>(); rb.anchorMin = rb.anchorMax = new Vector2(1, 1); rb.anchoredPosition = new Vector2(-190, -50);
        }
    }
}
