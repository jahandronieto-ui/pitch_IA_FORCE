using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuInicioAnimado : MonoBehaviour
{
    [Header("Elementos del Canvas")]
    public RectTransform logo;
    public RectTransform hojas;
    public RectTransform hojas2;
    public RectTransform personaje;
    public RectTransform letrero;

    [Header("Paneles (asignar los objetos completos)")]
    public RectTransform panelInstrucciones;
    public RectTransform panelConfiguraciones;
    public RectTransform panelCredito;

    [Header("Cambio de escena")]
    public string escenaJuego = "SeleccionPersonaje";
    [Min(0.05f)] public float duracionFundido = 0.5f;

    [Header("Entrada")]
    [Min(0.05f)] public float duracionEntrada = 0.65f;
    [Min(0f)] public float pausaEntreElementos = 0.12f;
    [Min(0.05f)] public float duracionPanel = 0.3f;

    [Header("Movimiento independiente: hojas 1")]
    public bool animarHojas = true;
    [Range(0f, 0.1f)] public float amplitudHojas = 0.025f;
    [Min(0f)] public float velocidadHojas = 1.8f;

    [Header("Movimiento independiente: hojas 2")]
    public bool animarHojas2 = true;
    [Range(0f, 0.1f)] public float amplitudHojas2 = 0.035f;
    [Min(0f)] public float velocidadHojas2 = 1.4f;

    [Header("Movimiento independiente: personaje")]
    public bool animarPersonaje = true;
    [Range(0f, 0.1f)] public float amplitudPersonaje = 0.012f;
    [Min(0f)] public float velocidadPersonaje = 1.2f;

    private class Elemento
    {
        public RectTransform rect;
        public CanvasGroup grupo;
        public Vector3 escala;
        public Vector2 posicion;
        public Elemento(RectTransform r)
        {
            rect = r;
            escala = r.localScale;
            posicion = r.anchoredPosition;
            grupo = r.GetComponent<CanvasGroup>();
            if (grupo == null) grupo = r.gameObject.AddComponent<CanvasGroup>();
        }
    }

    private Elemento eLogo, eHojas, eHojas2, ePersonaje, eLetrero;
    private Elemento[] paneles;
    private Elemento panelActual;
    private Coroutine transicionPanel;
    private CanvasGroup fundido;
    private bool listo, cambiandoEscena;

    private Elemento Crear(RectTransform r) { return r != null ? new Elemento(r) : null; }

    private void Awake()
    {
        eLogo = Crear(logo);
        eHojas = Crear(hojas);
        eHojas2 = Crear(hojas2);
        ePersonaje = Crear(personaje);
        eLetrero = Crear(letrero);
        paneles = new[] { Crear(panelInstrucciones), Crear(panelConfiguraciones), Crear(panelCredito) };
        foreach (Elemento p in paneles)
            if (p != null) { p.grupo.alpha = 0; p.rect.gameObject.SetActive(false); }
        OcultarEntrada(eLogo);
        OcultarEntrada(ePersonaje);
        OcultarEntrada(eLetrero);
        CrearFundido();
    }

    private void OcultarEntrada(Elemento e)
    {
        if (e == null) return;
        e.rect.gameObject.SetActive(true);
        e.grupo.alpha = 0;
        e.grupo.interactable = false;
        e.grupo.blocksRaycasts = false;
    }

    private IEnumerator Start()
    {
        yield return Animar(eLogo, true, duracionEntrada, new Vector2(0, 50));
        yield return new WaitForSecondsRealtime(pausaEntreElementos);
        yield return Animar(ePersonaje, true, duracionEntrada, new Vector2(-100, 0));
        yield return new WaitForSecondsRealtime(pausaEntreElementos);
        yield return Animar(eLetrero, true, duracionEntrada, new Vector2(100, 0));
        listo = true;
    }

    private void Update()
    {
        float t = Time.unscaledTime;
        Pulso(eHojas, t * velocidadHojas, animarHojas ? amplitudHojas : 0f);
        Pulso(eHojas2, t * velocidadHojas2 + Mathf.PI, animarHojas2 ? amplitudHojas2 : 0f);
        if (listo) Pulso(ePersonaje, t * velocidadPersonaje, animarPersonaje ? amplitudPersonaje : 0f);
    }

    private void Pulso(Elemento e, float t, float amplitud)
    {
        if (e != null && e.rect.gameObject.activeInHierarchy)
            e.rect.localScale = e.escala * (1f + Mathf.Sin(t) * amplitud);
    }

    private IEnumerator Animar(Elemento e, bool mostrar, float duracion, Vector2 desplazamiento)
    {
        if (e == null) yield break;
        e.rect.gameObject.SetActive(true);
        e.grupo.interactable = false;
        e.grupo.blocksRaycasts = false;
        Vector3 desdeEscala = mostrar ? e.escala * 0.88f : e.rect.localScale;
        Vector3 hastaEscala = mostrar ? e.escala : e.escala * 0.88f;
        Vector2 desdePos = mostrar ? e.posicion + desplazamiento : e.rect.anchoredPosition;
        Vector2 hastaPos = mostrar ? e.posicion : e.posicion + desplazamiento;
        float desdeAlpha = mostrar ? 0f : e.grupo.alpha;
        float tiempo = 0;
        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float progreso = Mathf.Clamp01(tiempo / Mathf.Max(0.01f, duracion));
            float suave = progreso * progreso * (3f - 2f * progreso);
            e.rect.localScale = Vector3.LerpUnclamped(desdeEscala, hastaEscala, suave);
            e.rect.anchoredPosition = Vector2.LerpUnclamped(desdePos, hastaPos, suave);
            e.grupo.alpha = Mathf.Lerp(desdeAlpha, mostrar ? 1f : 0f, suave);
            yield return null;
        }
        e.grupo.alpha = mostrar ? 1f : 0f;
        e.rect.localScale = e.escala;
        e.rect.anchoredPosition = e.posicion;
        e.grupo.interactable = mostrar;
        e.grupo.blocksRaycasts = mostrar;
        if (!mostrar) e.rect.gameObject.SetActive(false);
    }

    public void AbrirInstrucciones() { AbrirPanel(0); }
    public void AbrirConfiguraciones() { AbrirPanel(1); }
    public void AbrirCreditos() { AbrirPanel(2); }

    private void AbrirPanel(int indice)
    {
        if (!listo || cambiandoEscena || transicionPanel != null || paneles[indice] == null) return;
        if (panelActual == paneles[indice]) return;
        transicionPanel = StartCoroutine(CambiarPanel(paneles[indice]));
    }

    public void CerrarPanel()
    {
        if (cambiandoEscena || transicionPanel != null || panelActual == null) return;
        transicionPanel = StartCoroutine(CambiarPanel(null));
    }

    private IEnumerator CambiarPanel(Elemento siguiente)
    {
        // Desactiva los botones del letrero mientras hay un panel abierto.
        if (eLetrero != null)
        {
            eLetrero.grupo.interactable = false;
            eLetrero.grupo.blocksRaycasts = false;
        }
        if (panelActual != null)
            yield return Animar(panelActual, false, duracionPanel, new Vector2(0, -40));
        panelActual = siguiente;
        if (panelActual != null)
        {
            panelActual.rect.SetAsLastSibling();
            fundido.transform.SetAsLastSibling();
            yield return Animar(panelActual, true, duracionPanel, new Vector2(0, -40));
        }
        else if (eLetrero != null)
        {
            eLetrero.grupo.interactable = true;
            eLetrero.grupo.blocksRaycasts = true;
        }
        transicionPanel = null;
    }

    public void Jugar() { CambiarEscena(escenaJuego); }

    public void CambiarEscena(string nombre)
    {
        if (!listo || cambiandoEscena) return;
        if (string.IsNullOrWhiteSpace(nombre) || !Application.CanStreamedLevelBeLoaded(nombre))
        {
            Debug.LogError("MenuInicioAnimado: agrega la escena '" + nombre + "' a la lista de escenas de compilación y revisa su nombre.", this);
            return;
        }
        cambiandoEscena = true;
        StartCoroutine(CargarEscena(nombre));
    }

    private IEnumerator CargarEscena(string nombre)
    {
        fundido.transform.SetAsLastSibling();
        fundido.blocksRaycasts = true;
        float tiempo = 0;
        while (tiempo < duracionFundido)
        {
            tiempo += Time.unscaledDeltaTime;
            fundido.alpha = Mathf.Clamp01(tiempo / Mathf.Max(0.01f, duracionFundido));
            yield return null;
        }
        fundido.alpha = 1;
        yield return SceneManager.LoadSceneAsync(nombre);
    }

    private void CrearFundido()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = logo != null ? logo.GetComponentInParent<Canvas>() : null;
        if (canvas == null) canvas = letrero != null ? letrero.GetComponentInParent<Canvas>() : null;
        if (canvas == null)
        {
            Debug.LogError("MenuInicioAnimado debe estar dentro del Canvas.", this);
            enabled = false;
            return;
        }
        GameObject objeto = new GameObject("FundidoEscena", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        objeto.transform.SetParent(canvas.transform, false);
        RectTransform rt = objeto.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        objeto.GetComponent<Image>().color = Color.black;
        fundido = objeto.GetComponent<CanvasGroup>();
        fundido.alpha = 0;
        fundido.interactable = false;
        fundido.blocksRaycasts = false;
    }
}