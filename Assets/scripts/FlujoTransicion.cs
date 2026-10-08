using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GuardianesFlujo
{
    public class FlujoTransicion : MonoBehaviour
    {
        static FlujoTransicion instancia;
        public static bool Ocupada { get { return instancia != null && instancia.ocupada; } }
        [Min(0.05f)] public float duracion = 0.5f;
        Image velo;
        bool ocupada;
        void Awake()
        {
            if (instancia != null && instancia != this) { Destroy(gameObject); return; }
            instancia = this; DontDestroyOnLoad(gameObject);
            var g = new GameObject("CanvasTransicion", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            g.transform.SetParent(transform, false);
            var canvas = g.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 32760;
            var imagen = new GameObject("Fundido", typeof(RectTransform), typeof(Image)); imagen.transform.SetParent(g.transform, false);
            var r = imagen.GetComponent<RectTransform>(); r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
            velo = imagen.GetComponent<Image>(); velo.color = Color.black;
            ocupada = true;
        }
        IEnumerator Start()
        {
            if (instancia != this) yield break;
            yield return null;
            yield return Fundir(0); ocupada = false;
        }
        public static bool Cargar(string escena)
        {
            if (Ocupada) return false;
            if (string.IsNullOrWhiteSpace(escena) || !Application.CanStreamedLevelBeLoaded(escena))
            { FlujoUI.Mensaje("Territorio no disponible", "Este destino todavía no está disponible para jugar."); return false; }
            if (instancia == null) { Debug.LogError("Falta el objeto TransicionGlobal en la escena."); return false; }
            instancia.ocupada = true;
            instancia.StartCoroutine(instancia.Viajar(escena)); return true;
        }
        IEnumerator Viajar(string escena)
        {
            Time.timeScale = 1;
            yield return Fundir(1);
            var operacion = SceneManager.LoadSceneAsync(escena);
            if (operacion != null) while (!operacion.isDone) yield return null;
            else Debug.LogError("No se pudo iniciar la carga de " + escena, this);
            yield return null;
            yield return Fundir(0); ocupada = false;
        }
        IEnumerator Fundir(float destino)
        {
            velo.raycastTarget = true;
            float desde = velo.color.a, tiempo = 0, d = Mathf.Max(0.05f, duracion);
            while (tiempo < d) { tiempo += Time.unscaledDeltaTime; velo.color = new Color(0, 0, 0, Mathf.Lerp(desde, destino, tiempo / d)); yield return null; }
            velo.color = new Color(0, 0, 0, destino); velo.raycastTarget = destino > 0;
        }
        void OnDestroy() { if (instancia == this) instancia = null; }
    }
}
