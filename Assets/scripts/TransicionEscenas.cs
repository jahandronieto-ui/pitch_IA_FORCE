using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransicionEscenas : MonoBehaviour
{
    public static TransicionEscenas Instancia { get; private set; }
    [Min(0)] public float duracion = 0.6f;
    private CanvasGroup cortina;
    private bool cambiando;
    private Coroutine animacion;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        // El gestor debe ser un objeto raiz independiente, no hijo del Canvas.
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        GameObject lienzo = new GameObject("CanvasTransicion", typeof(RectTransform),
            typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasGroup));
        lienzo.transform.SetParent(transform, false);
        Canvas canvas = lienzo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;
        cortina = lienzo.GetComponent<CanvasGroup>();
        cortina.alpha = 1;
        cortina.blocksRaycasts = true;

        GameObject negro = new GameObject("PantallaNegra", typeof(RectTransform), typeof(Image));
        negro.transform.SetParent(lienzo.transform, false);
        RectTransform rect = negro.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        negro.GetComponent<Image>().color = Color.black;
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void Start()
    {
        if (Instancia == this && animacion == null)
            animacion = StartCoroutine(Revelar());
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        // Cambiar ya controla el fundido completo. Una carga externa recibe
        // solo el fundido de entrada, sin detener la rutina de cambio.
        if (cambiando || modo == LoadSceneMode.Additive) return;
        if (animacion != null) StopCoroutine(animacion);
        cortina.alpha = 1;
        animacion = StartCoroutine(Revelar());
    }

    public void IrA(string nombreEscena)
    {
        if (cambiando) return;
        if (string.IsNullOrWhiteSpace(nombreEscena) ||
            !Application.CanStreamedLevelBeLoaded(nombreEscena))
        {
            Debug.LogError("Agrega la escena a la lista de compilacion: " + nombreEscena, this);
            return;
        }
        if (animacion != null) StopCoroutine(animacion);
        cambiando = true;
        animacion = StartCoroutine(Cambiar(nombreEscena));
    }

    private IEnumerator Cambiar(string nombre)
    {
        yield return Fundir(1);
        AsyncOperation carga = SceneManager.LoadSceneAsync(nombre, LoadSceneMode.Single);
        if (carga == null)
        {
            yield return Fundir(0);
            cambiando = false;
            animacion = null;
            yield break;
        }
        while (!carga.isDone) yield return null;
        // Permite que se ejecuten Start y se cree el jugador seleccionado.
        yield return null;
        yield return Fundir(0);
        cambiando = false;
        animacion = null;
    }

    private IEnumerator Revelar()
    {
        yield return null;
        yield return Fundir(0);
        animacion = null;
    }

    private IEnumerator Fundir(float destino)
    {
        cortina.blocksRaycasts = true;
        float origen = cortina.alpha;
        float tiempo = 0;
        if (duracion > 0)
        {
            while (tiempo < duracion)
            {
                tiempo += Time.unscaledDeltaTime;
                cortina.alpha = Mathf.Lerp(origen, destino, Mathf.Clamp01(tiempo / duracion));
                yield return null;
            }
        }
        cortina.alpha = destino;
        cortina.blocksRaycasts = destino > 0;
    }

    private void OnDestroy()
    {
        if (Instancia != this) return;
        SceneManager.sceneLoaded -= AlCargarEscena;
        Instancia = null;
    }
}
