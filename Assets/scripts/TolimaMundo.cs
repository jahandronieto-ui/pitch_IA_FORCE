using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GuardianesTolima
{
    public class TolimaMundo : MonoBehaviour
    {
        [Header("Territorio y progreso independiente")]
        public string territorio = "Tolima", nombreTerritorio = "Tolima", siguienteTerritorio = "Huila";
        [TextArea] public string textoBienvenida = "Recorre los senderos y conversa con la comunidad. Aprende a prevenir quemas agrícolas, gestionar residuos y evitar el riesgo de colillas.";
        public string temasActividades = "quemas agrícolas, residuos rurales y colillas";
        public void ConfigurarActividades(Pregunta[][] nuevas) { actividades = nuevas; }
        [Serializable] public class Personaje { public int id; public GameObject prefab; }
        [Serializable] public class Sitio
        {
            public string id, titulo;
            public Transform punto;
            public GameObject marcador;
            public SpriteRenderer visual;
        }
        [Serializable] public class Ruta { public Vector2[] puntos; [Min(0.1f)] public float ancho = 1.8f; }
        [Header("Referencias de la escena")]
        public SpriteRenderer fondo;
        public Camera camara;
        public Canvas canvas;
        public Transform puntoInicio;
        public Transform contenedorJugador;
        [Header("Mismos ID del selector. Prefabs reales de tu proyecto")]
        public Personaje[] personajes = new Personaje[0];
        [Header("Interacciones y caminos")]
        public Sitio[] sitios = new Sitio[0];
        public Ruta[] rutas = new Ruta[0];
        public bool restringirACaminos = true;
        [Tooltip("Mascara de suelo transitable del fondo ilustrado. Si no se asigna, usa las rutas originales.")]
        public TextAsset mascaraTransitable;
        [Serializable] class Mascara { public int ancho, alto; public string celdas; }
        Mascara navegacion;
        public float radioInteraccion = 2.3f;
        public float velocidadJugador = 5f;
        public float tamanoCamara = 6.2f;
        [Header("Contenedor de indicaciones: movimiento y hablar")]
        public Vector2 tamanoContenedorAyuda = new Vector2(1050, 100);
        public Vector2 posicionContenedorAyuda = new Vector2(0, 95);
        public Color colorContenedorAyuda = new Color(0.02f, 0.15f, 0.20f, 0.92f);
        public Color colorTextoAyuda = Color.white;
        [Min(12)] public int tamanoTextoAyuda = 25;
        [Tooltip("Opcional: imagen del panel. Para bordes escalables usa un sprite con Border configurado.")]
        public Sprite imagenContenedorAyuda;
        public string escenaMapa = "Mapa";
        [Header("Prueba directa sin pasar por seleccion")]
        public bool usarPersonajePorDefecto = true;
        public int idPersonajePorDefecto = 1;
        [Tooltip("Permite probar sin pasar por seleccion, solo en el Editor.")]
        public bool permitirPruebaEditor;
        public int idPruebaEditor;
        public TolimaJugadorMotor Jugador { get; private set; }
        public bool Bloqueado { get { return panelAbierto || viajando || GuardianesFlujo.FlujoTransicion.Ocupada; } }
        RectTransform ui;
        GameObject modal;
        Text estado, sugerencia, tituloPanel, contenido, feedback;
        Button accionPrincipal, cerrar;
        Button[] opciones;
        Image cortina;
        Font fuente;
        int sitioActivo = -1, preguntaActual;
        bool panelAbierto, viajando, verFinalPendiente;
        readonly string[] ids = { "bienvenida", "actividad1", "actividad2", "actividad3", "compromiso" };
        string Clave(string id) { return "GT." + territorio + ".Mision." + id; }
        bool Completa(string id) { return PlayerPrefs.GetInt(Clave(id), 0) == 1; }
        [Serializable] public class Pregunta
        {
            public string texto;
            public string[] respuestas;
            public int correcta;
            public Pregunta(string t, int c, params string[] r) { texto = t; correcta = c; respuestas = r; }
        }
        Pregunta[][] actividades = {
            new [] {
                new Pregunta("¿Qué alternativa ayuda a prevenir incendios al gestionar restos de cultivos?", 1, "Quemarlos cerca del bosque", "Gestionarlos sin quemarlos", "Dejarlos ardiendo sin vigilancia"),
                new Pregunta("¿Qué condición favorece que un fuego se propague?", 0, "Vegetación seca y viento", "Ausencia de materiales combustibles", "Suelo sin vegetación ni residuos"),
                new Pregunta("¿Qué decisión protege la cobertura vegetal?", 2, "Encender fuego junto al cultivo seco", "Quemar para limpiar rápidamente", "Evitar quemas y planificar la gestión de restos")
            },
            new [] {
                new Pregunta("¿Qué hacer con los residuos de la comunidad?", 0, "Separarlos y llevarlos al lugar adecuado", "Quemarlos al lado del camino", "Arrojarlos a la quebrada"),
                new Pregunta("¿Cuál es una práctica de riesgo?", 2, "Clasificar materiales", "Usar recipientes adecuados", "Prender fuego a una pila de basura"),
                new Pregunta("¿Cómo mantener limpia la zona rural?", 1, "Dejar residuos en el cultivo", "Organizar su recolección y disposición", "Ocultarlos entre los árboles")
            },
            new [] {
                new Pregunta("¿Qué debe evitarse con una colilla?", 1, "Disponerla apagada en un recipiente adecuado", "Arrojarla encendida a la vegetación", "Evitar dejarla en el suelo"),
                new Pregunta("¿Qué lugar presenta riesgo si recibe una colilla encendida?", 0, "Pasto y hojas secas", "Un recipiente adecuado sin combustible", "Una superficie sin material combustible"),
                new Pregunta("¿Qué mensaje compartirías con la comunidad?", 2, "Las colillas no afectan el entorno", "Podemos dejarlas en cualquier sendero", "Evitemos colillas en el suelo y prácticas que generen fuego")
            }
        };

        void Start()
        {
            if (mascaraTransitable != null)
            {
                navegacion = JsonUtility.FromJson<Mascara>(mascaraTransitable.text);
                if (navegacion == null || navegacion.ancho <= 0 || navegacion.alto <= 0 || navegacion.celdas == null || navegacion.celdas.Length != navegacion.ancho * navegacion.alto)
                { Debug.LogError("Mascara de navegacion invalida; se usaran las rutas.", this); navegacion = null; }
            }
            ConstruirInterfaz();
            if (fondo == null || fondo.sprite == null || camara == null || puntoInicio == null)
            {
                ErrorConfiguracion("Falta el fondo, la cámara o el punto de inicio. Importa esta escena en el proyecto donde están tus sprites.");
                return;
            }
            fondo.sortingOrder = -30000;
            camara.orthographic = true;
            camara.orthographicSize = tamanoCamara;
            int id;
            if (PlayerPrefs.HasKey("ID_Seleccionada")) id = PlayerPrefs.GetInt("ID_Seleccionada");
            else
            {
#if UNITY_EDITOR
                if (permitirPruebaEditor) id = idPruebaEditor;
                else
#endif
                {
                    if (usarPersonajePorDefecto) { id = idPersonajePorDefecto; PlayerPrefs.SetInt("ID_Seleccionada", id); PlayerPrefs.Save(); }
                    else { ErrorConfiguracion("Primero selecciona un personaje."); return; }
                }
            }
            GameObject elegido = null;
            int coincidencias = 0;
            foreach (Personaje p in personajes)
                if (p != null && p.id == id) { elegido = p.prefab; coincidencias++; }
            if (coincidencias != 1 || elegido == null)
            {
                ErrorConfiguracion("Asigna el prefab del personaje con ID " + id + " en Sistemas/GestorTolima, o pulsa Conectar catálogo existente en su Inspector. No se usará un personaje distinto.");
                return;
            }
            GameObject instancia = Instantiate(elegido, puntoInicio.position, Quaternion.identity, contenedorJugador);
            instancia.name = "JugadorSeleccionado_ID_" + id;
            // Evita dos motores de movimiento de los paquetes anteriores.
            foreach (MonoBehaviour script in instancia.GetComponentsInChildren<MonoBehaviour>(true))
                if (script != null && (script.GetType().Name == "JugadorTerritorio" || script.GetType().Name == "OrdenPorPiesTerritorio")) script.enabled = false;
            Jugador = instancia.GetComponent<TolimaJugadorMotor>();
            if (Jugador == null) Jugador = instancia.AddComponent<TolimaJugadorMotor>();
            Jugador.mundo = this;
            Jugador.velocidad = velocidadJugador;
            Jugador.PrepararAnimator();
            if (instancia.GetComponentInChildren<Collider2D>() == null)
            {
                CircleCollider2D pies = instancia.AddComponent<CircleCollider2D>();
                pies.radius = 0.18f;
            }
            camara.transform.position = new Vector3(Jugador.transform.position.x, Jugador.transform.position.y, -10);
            ActualizarProgreso();
        }

        public void ConstruirInterfaz()
        {
            if (canvas == null)
            {
                GameObject g = new GameObject("CanvasHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = g.GetComponent<Canvas>();
            }
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.transform.localScale = Vector3.one;
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            if (canvas.GetComponent<GraphicRaycaster>() == null) canvas.gameObject.AddComponent<GraphicRaycaster>();
            fuente = ObtenerFuente();
            Transform anterior = canvas.transform.Find("InterfazTolimaGenerada");
            if (anterior != null) { anterior.gameObject.SetActive(false); Destroy(anterior.gameObject); }
            ui = Rect("InterfazTolimaGenerada", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image cabecera = Imagen("Cabecera", ui, new Vector2(0, 1), new Vector2(0, 1), new Vector2(340, -90), new Vector2(640, 140), new Color(0.02f, 0.15f, 0.22f, 0.93f));
            Texto("Titulo", cabecera.transform, nombreTerritorio.ToUpperInvariant() + " · Guardianes del territorio", new Vector2(0, 35), new Vector2(610, 55), 28);
            estado = Texto("Progreso", cabecera.transform, "Misiones: 0/5    Estrellas: 0/12", new Vector2(0, -20), new Vector2(610, 50), 25);
            Vector2 tamanoAyuda = new Vector2(Mathf.Max(100, tamanoContenedorAyuda.x), Mathf.Max(50, tamanoContenedorAyuda.y));
            Image contenedorAyuda = Imagen("ContenedorAyuda", ui,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                posicionContenedorAyuda, tamanoAyuda, colorContenedorAyuda);
            contenedorAyuda.raycastTarget = false;
            if (imagenContenedorAyuda != null)
            {
                contenedorAyuda.sprite = imagenContenedorAyuda;
                contenedorAyuda.type = imagenContenedorAyuda.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
            }
            sugerencia = Texto("AyudaInteraccion", contenedorAyuda.transform,
                "Camina con WASD o las flechas. Pulsa E o Hablar.", Vector2.zero,
                tamanoAyuda - new Vector2(60, 20), Mathf.Max(12, tamanoTextoAyuda));
            sugerencia.color = colorTextoAyuda;
            Boton("VolverMapa", ui, "Volver al mapa", new Vector2(790, 465), new Vector2(270, 65), VolverMapa);
            Boton("Hablar", ui, "Hablar / Interactuar", new Vector2(760, -420), new Vector2(320, 85), Interactuar);
            CrearFlecha("Arriba", "▲", new Vector2(-770, -330), Vector2.up);
            CrearFlecha("Abajo", "▼", new Vector2(-770, -465), Vector2.down);
            CrearFlecha("Izquierda", "◀", new Vector2(-850, -400), Vector2.left);
            CrearFlecha("Derecha", "▶", new Vector2(-690, -400), Vector2.right);
            Image velo = Imagen("PanelDialogoActividad", ui, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.72f));
            modal = velo.gameObject;
            Image caja = Imagen("Contenido", velo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100, 680), new Color(0.02f, 0.14f, 0.20f, 1));
            tituloPanel = Texto("Titulo", caja.transform, "Actividad", new Vector2(0, 260), new Vector2(1020, 80), 36);
            contenido = Texto("Texto", caja.transform, "", new Vector2(0, 125), new Vector2(1000, 170), 30);
            feedback = Texto("Feedback", caja.transform, "", new Vector2(0, -250), new Vector2(1000, 34), 22);
            opciones = new Button[3];
            for (int i = 0; i < 3; i++)
            {
                int opcion = i;
                opciones[i] = Boton("Opcion" + (i + 1), caja.transform, "", new Vector2(0, -20 - i * 85), new Vector2(980, 70), () => Responder(opcion));
            }
            accionPrincipal = Boton("Continuar", caja.transform, "Continuar", new Vector2(170, -304), new Vector2(380, 58), ConfirmarDialogo);
            cerrar = Boton("Cerrar", caja.transform, "Cerrar", new Vector2(-290, -304), new Vector2(300, 58), CerrarPanel);
            modal.SetActive(false);
            cortina = Imagen("Fundido", ui, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.clear);
            cortina.raycastTarget = false;
        }

        Font ObtenerFuente()
        {
            try { Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); if (f != null) return f; } catch (Exception) { }
            try { return Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch (Exception) { return null; }
        }
        RectTransform Rect(string nombre, Transform padre, Vector2 min, Vector2 max, Vector2 pos, Vector2 size)
        {
            GameObject g = new GameObject(nombre, typeof(RectTransform));
            RectTransform r = g.GetComponent<RectTransform>(); r.SetParent(padre, false);
            r.anchorMin = min; r.anchorMax = max; r.anchoredPosition = pos; r.sizeDelta = size;
            return r;
        }
        Image Imagen(string n, Transform p, Vector2 min, Vector2 max, Vector2 pos, Vector2 size, Color color)
        {
            Image i = Rect(n, p, min, max, pos, size).gameObject.AddComponent<Image>(); i.color = color; return i;
        }
        Text Texto(string n, Transform p, string valor, Vector2 pos, Vector2 size, int tamano)
        {
            Text t = Rect(n, p, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size).gameObject.AddComponent<Text>();
            t.font = fuente; t.fontSize = tamano; t.text = valor; t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter; t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; return t;
        }
        Button Boton(string n, Transform p, string etiqueta, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction accion)
        {
            Image i = Imagen(n, p, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size, new Color(0.08f, 0.38f, 0.37f, 1));
            Button b = i.gameObject.AddComponent<Button>(); b.targetGraphic = i; b.onClick.AddListener(accion);
            Texto("Etiqueta", b.transform, etiqueta, Vector2.zero, size - new Vector2(20, 8), 25); return b;
        }
        void CrearFlecha(string n, string texto, Vector2 pos, Vector2 direccion)
        {
            Button b = Boton(n, ui, texto, pos, new Vector2(75, 75), () => { });
            TolimaPadTactil pad = b.gameObject.AddComponent<TolimaPadTactil>(); pad.mundo = this; pad.direccion = direccion;
        }

        void ErrorConfiguracion(string mensaje)
        {
            Debug.LogError(mensaje, this);
            cortina.color = Color.clear; cortina.raycastTarget = false;
            MostrarMensaje("Falta configurar el jugador", mensaje, false);
            accionPrincipal.gameObject.SetActive(false);
        }

        public void Interactuar()
        {
            if (Jugador == null || Bloqueado) return;
            int indice = Cercano();
            if (indice < 0) return;
            Sitio sitio = sitios[indice]; sitioActivo = indice;
            if (sitio.id != "bienvenida" && !Completa("bienvenida"))
            { MostrarMensaje("Habla primero con la guía", "La guía está en la plaza central. Completa su bienvenida antes de empezar las actividades.", false); return; }
            if (sitio.id == "compromiso" && (!Completa("actividad1") || !Completa("actividad2") || !Completa("actividad3")))
            { MostrarMensaje("Aún faltan actividades", "Completa las actividades de " + temasActividades + ". Después regresa al tablón.", false); return; }
            if (Completa(sitio.id))
            { MostrarMensaje(sitio.titulo, "Esta misión ya está completada. Continúa por el sendero o vuelve al mapa.", false); return; }
            if (sitio.id == "bienvenida")
            { MostrarMensaje("Guía de " + nombreTerritorio, textoBienvenida + " Al terminar, vuelve al tablón para cerrar tu compromiso.", true); return; }
            if (sitio.id == "compromiso")
            { MostrarMensaje("Compromiso con el territorio", "Has completado las tres actividades de " + nombreTerritorio + ". Confirma tu compromiso de cuidar el territorio." + (string.IsNullOrEmpty(siguienteTerritorio) ? " ¡Completaste el recorrido por los cuatro territorios!" : " Se desbloqueará " + siguienteTerritorio + "."), true); return; }
            preguntaActual = 0; panelAbierto = true; Jugador.Detener(); modal.SetActive(true); MostrarPregunta();
        }

        int Cercano()
        {
            if (Jugador == null) return -1;
            int indice = -1; float mejor = radioInteraccion * radioInteraccion;
            for (int i = 0; i < sitios.Length; i++)
            {
                Sitio s = sitios[i]; if (s == null || s.punto == null) continue;
                float d = ((Vector2)s.punto.position - (Vector2)Jugador.transform.position).sqrMagnitude;
                if (d > mejor) continue; mejor = d; indice = i;
            }
            return indice;
        }

        void MostrarMensaje(string titulo, string texto, bool confirmar)
        {
            panelAbierto = true; if (Jugador != null) Jugador.Detener(); modal.SetActive(true);
            tituloPanel.text = titulo; contenido.fontSize = 24; contenido.text = texto; feedback.text = "";
            foreach (Button b in opciones) b.gameObject.SetActive(false);
            accionPrincipal.gameObject.SetActive(confirmar);
            accionPrincipal.GetComponentInChildren<Text>().text = "Confirmar";
        }
        void ConfirmarDialogo()
        {
            if (verFinalPendiente) { GuardianesFlujo.FlujoTransicion.Cargar("Final_Juego"); return; }
            if (sitioActivo < 0) return;
            bool compromiso = sitios[sitioActivo].id == "compromiso";
            Completar(sitios[sitioActivo].id); CerrarPanel();
            if (compromiso && GuardianesFlujo.ProgresoJuegoFlujo.JuegoCompleto())
            {
                MostrarMensaje("¡Recorrido completado!", "Completaste los cuatro territorios. Mira tus estrellas y resultados finales.", true);
                verFinalPendiente = true;
                accionPrincipal.GetComponentInChildren<Text>().text = "Ver final del juego";
                return;
            }
            if (compromiso)
                MostrarMensaje(string.IsNullOrEmpty(siguienteTerritorio) ? "Compromiso final completado" : nombreTerritorio + " completado",
                    string.IsNullOrEmpty(siguienteTerritorio) ? "Terminaste las cinco misiones de " + nombreTerritorio + ". Vuelve al mapa para revisar tu recorrido." : "Completaste las cinco misiones y desbloqueaste " + siguienteTerritorio + ". Vuelve al mapa para continuar.", false);
        }
        int ActividadActual()
        {
            string id = sitios[sitioActivo].id;
            return id == "actividad1" ? 0 : id == "actividad2" ? 1 : 2;
        }
        void MostrarPregunta()
        {
            Pregunta p = actividades[ActividadActual()][preguntaActual];
            tituloPanel.text = sitios[sitioActivo].titulo + " · " + (preguntaActual + 1) + "/3";
            contenido.fontSize = 30; contenido.text = p.texto; feedback.text = "";
            accionPrincipal.gameObject.SetActive(false);
            for (int i = 0; i < opciones.Length; i++)
            { opciones[i].gameObject.SetActive(true); opciones[i].GetComponentInChildren<Text>().text = p.respuestas[i]; }
        }
        void Responder(int indice)
        {
            if (!panelAbierto || sitioActivo < 0) return;
            Pregunta p = actividades[ActividadActual()][preguntaActual];
            if (indice != p.correcta) { feedback.text = "Revisa la situación y elige una práctica que prevenga el riesgo."; return; }
            preguntaActual++;
            if (preguntaActual < 3) { MostrarPregunta(); return; }
            Completar(sitios[sitioActivo].id);
            MostrarMensaje("Actividad completada", "¡Bien! Ganaste 4 estrellas. Continúa con las otras actividades y regresa al tablón cuando termines.", false);
        }
        void Completar(string id)
        {
            if (Completa(id)) return;
            PlayerPrefs.SetInt(Clave(id), 1);
            bool todas = true; foreach (string m in ids) todas &= Completa(m);
            if (todas)
            {
                PlayerPrefs.SetInt("GT." + territorio + ".Completo", 1);
                if (!string.IsNullOrEmpty(siguienteTerritorio)) PlayerPrefs.SetInt("GT." + siguienteTerritorio + ".Desbloqueado", 1);
                else if (PlayerPrefs.GetInt("GT.Tolima.Completo", 0) == 1 && PlayerPrefs.GetInt("GT.Huila.Completo", 0) == 1 && PlayerPrefs.GetInt("GT.Caqueta.Completo", 0) == 1)
                    PlayerPrefs.SetInt("GT.Juego.Completo", 1);
            }
            PlayerPrefs.Save(); ActualizarProgreso();
        }
        void ActualizarProgreso()
        {
            int misiones = 0, estrellas = 0;
            foreach (string id in ids)
                if (Completa(id)) { misiones++; if (id.StartsWith("actividad")) estrellas += 4; }
            estado.text = "Misiones: " + misiones + "/5    Estrellas: " + estrellas + "/12";
            foreach (Sitio s in sitios) if (s != null && s.marcador != null) s.marcador.SetActive(!Completa(s.id));
        }
        void CerrarPanel()
        {
            verFinalPendiente = false; panelAbierto = false; modal.SetActive(false); sitioActivo = -1;
            if (Jugador != null) Jugador.Detener();
        }

        public bool PuedeCaminar(Vector2 posicion)
        {
            if (fondo == null) return false;
            Bounds b = fondo.bounds;
            if (posicion.x < b.min.x + 0.2f || posicion.x > b.max.x - 0.2f || posicion.y < b.min.y + 0.2f || posicion.y > b.max.y - 0.2f) return false;
            if (!restringirACaminos) return true;
            if (navegacion != null)
            {
                int x = Mathf.Clamp(Mathf.FloorToInt((posicion.x - b.min.x) / b.size.x * navegacion.ancho), 0, navegacion.ancho - 1);
                int y = Mathf.Clamp(Mathf.FloorToInt((posicion.y - b.min.y) / b.size.y * navegacion.alto), 0, navegacion.alto - 1);
                return navegacion.celdas[y * navegacion.ancho + x] == '1';
            }
            foreach (Ruta ruta in rutas)
            {
                if (ruta == null || ruta.puntos == null) continue;
                for (int i = 1; i < ruta.puntos.Length; i++)
                {
                    Vector2 a = AWorld(ruta.puntos[i - 1]), z = AWorld(ruta.puntos[i]);
                    Vector2 segmento = z - a;
                    float t = segmento.sqrMagnitude > 0 ? Mathf.Clamp01(Vector2.Dot(posicion - a, segmento) / segmento.sqrMagnitude) : 0;
                    if ((posicion - (a + segmento * t)).sqrMagnitude <= ruta.ancho * ruta.ancho * 0.25f) return true;
                }
            }
            // Claros para hablar junto a los puntos reales de la escena.
            foreach (Sitio s in sitios)
                if (s != null && s.punto != null && ((Vector2)s.punto.position - posicion).sqrMagnitude < 2.5f * 2.5f) return true;
            return false;
        }
        public Vector2 AWorld(Vector2 uv)
        {
            Bounds b = fondo.bounds; return new Vector2(b.min.x + uv.x * b.size.x, b.min.y + uv.y * b.size.y);
        }
        void LateUpdate()
        {
            if (Jugador == null || fondo == null || camara == null) return;
            Bounds b = fondo.bounds; float h = camara.orthographicSize, w = h * camara.aspect;
            Vector3 p = Vector3.Lerp(camara.transform.position, new Vector3(Jugador.transform.position.x, Jugador.transform.position.y, -10), 1 - Mathf.Exp(-6 * Time.deltaTime));
            p.x = b.size.x <= w * 2 ? b.center.x : Mathf.Clamp(p.x, b.min.x + w, b.max.x - w);
            p.y = b.size.y <= h * 2 ? b.center.y : Mathf.Clamp(p.y, b.min.y + h, b.max.y - h);
            camara.transform.position = p;
            int cercano = Cercano();
            if (!Bloqueado) sugerencia.text = cercano >= 0 ? "E / Hablar: " + sitios[cercano].titulo : "Camina con WASD o las flechas. Busca los marcadores !";
            foreach (SpriteRenderer r in Jugador.GetComponentsInChildren<SpriteRenderer>())
                r.sortingOrder = 1000 - Mathf.RoundToInt(Jugador.transform.position.y * 100);
            foreach (Sitio s in sitios)
            {
                if (s == null || s.punto == null || s.visual == null) continue;
                s.visual.sortingOrder = 1000 - Mathf.RoundToInt((s.visual.transform.parent != null ? s.visual.transform.parent.position.y : s.visual.transform.position.y) * 100);
                if (s.marcador != null)
                    foreach (SpriteRenderer r in s.marcador.GetComponentsInChildren<SpriteRenderer>()) r.sortingOrder = 5000;
            }
        }
        public void VolverMapa()
        {
            if (viajando) return;
            if (string.IsNullOrWhiteSpace(escenaMapa) || !Application.CanStreamedLevelBeLoaded(escenaMapa))
            { MostrarMensaje("Configura la escena del mapa", "En GestorTolima escribe el nombre real de tu mapa y agrégalo a la lista de escenas de compilación.", false); return; }
            if (Jugador != null) Jugador.Detener();
            viajando = GuardianesFlujo.FlujoTransicion.Cargar(escenaMapa);
        }
        void OnDrawGizmosSelected()
        {
            if (fondo == null) return;
            Gizmos.color = Color.cyan;
            foreach (Ruta ruta in rutas)
                if (ruta != null && ruta.puntos != null)
                    for (int i = 1; i < ruta.puntos.Length; i++) Gizmos.DrawLine(AWorld(ruta.puntos[i - 1]), AWorld(ruta.puntos[i]));
            Gizmos.color = Color.yellow;
            foreach (Sitio s in sitios) if (s != null && s.punto != null) Gizmos.DrawWireSphere(s.punto.position, radioInteraccion);
        }
    }
}
