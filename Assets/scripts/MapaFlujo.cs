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
            int completos = 0, estrellas = 0;
            foreach (string region in new[] { "Tolima", "Huila", "Caqueta", "Putumayo" })
            {
                if (PlayerPrefs.GetInt("GT." + region + ".Completo", 0) == 1) completos++;
                foreach (string actividad in new[] { "actividad1", "actividad2", "actividad3" })
                    if (PlayerPrefs.GetInt("GT." + region + ".Mision." + actividad, 0) == 1) estrellas += 4;
            }
            var progreso = FlujoUI.Texto(canvas.transform, "Territorios: " + completos + "/4   ·   Estrellas: " + estrellas + "/48", Vector2.zero, new Vector2(720, 60));
            var rp = progreso.rectTransform; rp.anchorMin = rp.anchorMax = new Vector2(0.5f, 0); rp.anchoredPosition = new Vector2(0, 45);
            if (ProgresoJuegoFlujo.JuegoCompleto())
            {
                var final = FlujoUI.Boton(canvas.transform, "Ver final del juego", Vector2.zero, new Vector2(360, 65), () => FlujoTransicion.Cargar("Final_Juego"));
                var rf = final.GetComponent<RectTransform>(); rf.anchorMin = rf.anchorMax = new Vector2(0.5f, 1); rf.anchoredPosition = new Vector2(0, -50);
            }
            var b = FlujoUI.Boton(canvas.transform, "Cambiar personaje", Vector2.zero, new Vector2(310, 60), () => FlujoTransicion.Cargar("SeleccionPersonaje"));
            var rb = b.GetComponent<RectTransform>(); rb.anchorMin = rb.anchorMax = new Vector2(1, 1); rb.anchoredPosition = new Vector2(-190, -50);
        }
    }
}
