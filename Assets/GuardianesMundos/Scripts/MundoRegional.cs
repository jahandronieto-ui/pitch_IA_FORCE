using System.Collections.Generic;
using GuardianesFlujo;
using GuardianesTolima;
using UnityEngine;

namespace GuardianesMundos
{
    // Construye un mapa base editable al iniciar Play. No depende de nuevas texturas externas.
    public class MundoRegional : MonoBehaviour
    {
        public enum Region { Huila, Caqueta, Putumayo }
        public Region region;
        public CatalogoFlujo catalogo;
        public Sprite fondoPersonalizado;
        public Sprite spriteGuia, spriteHabitante, spriteArbol, spriteCasa;
        [Header("Arte por capas: atlas de 2 x 2, vinculado por escena")]
        public Texture2D complementosAtlas, comunesAtlas;
        [HideInInspector] public Rect[] recortesComplementos, recortesComunes;
        public Sprite spritePuente, spriteTablon, spriteResiduos, spriteCultivos;
        [Header("Escena armada: referencias persistentes del Editor")]
        public TolimaMundo mundoConfigurado;
        public SpriteRenderer[] visualesConfigurados;
        public SpriteRenderer[] objetosConfigurados;
        readonly List<Sprite> spritesAtlasGenerados = new List<Sprite>();
        public bool dibujarDecoracion = true;
        public bool dibujarCaminos = true;
        public float velocidadJugador = 5, tamanoCamara = 6.2f;
        [Header("Para probar una escena directamente, incluso si está bloqueada en el mapa")]
        public bool permitirPruebaDirecta = true;
        Sprite cuadrado, circulo;
        TolimaMundo mundo;
        Transform mapa;
        Font fuente;
        Color verde, suelo, sendero, agua;

        void Awake()
        {
            CrearSprites();
            PrepararComplementos();
            fuente = FlujoUI.Fuente();
            if (mundoConfigurado != null)
            {
                mundo = mundoConfigurado;
                mundo.territorio = region.ToString();
                mundo.nombreTerritorio = region == Region.Caqueta ? "Caquetá" : region.ToString();
                mundo.siguienteTerritorio = region == Region.Huila ? "Caqueta" : region == Region.Caqueta ? "Putumayo" : "";
                ConfigurarRetos();
                if (visualesConfigurados != null)
                    for (int i = 0; i < visualesConfigurados.Length; i++)
                        if (visualesConfigurados[i] != null)
                            visualesConfigurados[i].sprite = i == 4 ? spriteTablon : i == 0 ? spriteGuia : spriteHabitante;
                if (objetosConfigurados != null)
                    for (int i = 0; i < objetosConfigurados.Length; i++)
                        if (objetosConfigurados[i] != null) objetosConfigurados[i].sprite = i == 0 ? spriteCultivos : spriteResiduos;
                foreach (var s in mundo.sitios)
                    if (s != null && s.marcador != null)
                    {
                        var t = s.marcador.GetComponent<TextMesh>();
                        if (t == null) t = s.marcador.AddComponent<TextMesh>();
                        t.text = "!"; t.font = fuente; t.fontSize = 48; t.characterSize = 0.35f;
                        t.anchor = TextAnchor.MiddleCenter; t.color = Color.yellow;
                        var renderer = t.GetComponent<MeshRenderer>();
                        if (fuente != null) renderer.sharedMaterial = fuente.material;
                        renderer.sortingOrder = 5500;
                    }
                return;
            }
            mapa = new GameObject("MapaRegionalGenerado").transform; mapa.SetParent(transform, false);
            Paleta();
            var camara = Camera.main;
            if (camara == null)
            {
                var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)); go.tag = "MainCamera"; camara = go.GetComponent<Camera>();
            }
            camara.orthographic = true; camara.orthographicSize = tamanoCamara; camara.backgroundColor = suelo; camara.clearFlags = CameraClearFlags.SolidColor;
            camara.transform.position = new Vector3(-15, -8, -10);
            var fondo = Pintar("FondoMapa", Vector2.zero, new Vector2(40, 24), suelo, -30000);
            if (fondoPersonalizado != null)
            {
                fondo.sprite = fondoPersonalizado;
                fondo.color = Color.white;
                fondo.transform.localScale = new Vector3(40 / fondoPersonalizado.bounds.size.x, 24 / fondoPersonalizado.bounds.size.y, 1);
            }
            var inicio = new GameObject("PuntoInicioJugador").transform; inicio.SetParent(mapa, false); inicio.position = new Vector3(-15, -8, 0);
            var jugadores = new GameObject("Jugadores").transform; jugadores.SetParent(transform, false);
            var sistemas = new GameObject("Sistemas").transform; sistemas.SetParent(transform, false);
            var gestor = new GameObject("Gestor" + region); gestor.transform.SetParent(sistemas, false);
            mundo = gestor.AddComponent<TolimaMundo>();
            mundo.territorio = region.ToString(); mundo.nombreTerritorio = region == Region.Caqueta ? "Caquetá" : region.ToString();
            mundo.siguienteTerritorio = region == Region.Huila ? "Caqueta" : region == Region.Caqueta ? "Putumayo" : "";
            mundo.fondo = fondo; mundo.camara = camara; mundo.puntoInicio = inicio; mundo.contenedorJugador = jugadores;
            mundo.velocidadJugador = velocidadJugador; mundo.tamanoCamara = tamanoCamara; mundo.escenaMapa = "Mapa";
            mundo.usarPersonajePorDefecto = permitirPruebaDirecta; mundo.idPersonajePorDefecto = 1;
            var personajes = new List<TolimaMundo.Personaje>();
            if (catalogo != null)
                foreach (var p in catalogo.personajes)
                    if (p != null) personajes.Add(new TolimaMundo.Personaje { id = p.id, prefab = p.prefab });
            mundo.personajes = personajes.ToArray();
            ConstruirRecorrido();
            ConfigurarRetos();
            if (dibujarDecoracion) Decorar();
        }

        void PrepararComplementos()
        {
            if (complementosAtlas != null)
            {
                if (spriteGuia == null) spriteGuia = Cortar(complementosAtlas, 0, 1);
                if (spriteHabitante == null) spriteHabitante = Cortar(complementosAtlas, 1, 1);
                if (spriteArbol == null) spriteArbol = Cortar(complementosAtlas, 0, 0);
                if (spriteCasa == null) spriteCasa = Cortar(complementosAtlas, 1, 0);
            }
            if (comunesAtlas != null)
            {
                if (spritePuente == null) spritePuente = Cortar(comunesAtlas, 0, 1);
                if (spriteTablon == null) spriteTablon = Cortar(comunesAtlas, 1, 1);
                if (spriteResiduos == null) spriteResiduos = Cortar(comunesAtlas, 0, 0);
                if (spriteCultivos == null) spriteCultivos = Cortar(comunesAtlas, 1, 0);
            }
        }
        Sprite Cortar(Texture2D textura, int columna, int fila)
        {
            int w = textura.width / 2, h = textura.height / 2;
            int indice = columna + (1 - fila) * 2;
            Rect[] recortes = textura == complementosAtlas ? recortesComplementos : recortesComunes;
            Rect rect = recortes != null && indice < recortes.Length ? recortes[indice] : new Rect(columna * w, fila * h, w, h);
            var sprite = Sprite.Create(textura, rect, new Vector2(0.5f, 0.5f), 100);
            spritesAtlasGenerados.Add(sprite); return sprite;
        }

        void CrearSprites()
        {
            var textura = new Texture2D(2, 2); textura.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white }); textura.Apply();
            cuadrado = Sprite.Create(textura, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 1);
            var disco = new Texture2D(64, 64); disco.filterMode = FilterMode.Bilinear;
            var pixeles = new Color[64 * 64];
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++) pixeles[y * 64 + x] = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f)) <= 30 ? Color.white : Color.clear;
            disco.SetPixels(pixeles); disco.Apply(); circulo = Sprite.Create(disco, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 32);
        }
        void Paleta()
        {
            if (region == Region.Huila) { suelo = new Color(0.73f, 0.67f, 0.39f); verde = new Color(0.29f, 0.45f, 0.19f); sendero = new Color(0.91f, 0.81f, 0.55f); }
            else if (region == Region.Caqueta) { suelo = new Color(0.30f, 0.52f, 0.31f); verde = new Color(0.07f, 0.32f, 0.19f); sendero = new Color(0.79f, 0.68f, 0.43f); }
            else { suelo = new Color(0.30f, 0.58f, 0.46f); verde = new Color(0.08f, 0.38f, 0.28f); sendero = new Color(0.82f, 0.76f, 0.57f); }
            agua = new Color(0.14f, 0.54f, 0.70f);
        }
        SpriteRenderer Pintar(string nombre, Vector2 posicion, Vector2 tamaño, Color color, int orden, Sprite sprite = null)
        {
            var g = new GameObject(nombre); g.transform.SetParent(mapa, false); g.transform.position = posicion;
            var r = g.AddComponent<SpriteRenderer>(); r.sprite = sprite != null ? sprite : cuadrado; r.color = color; r.sortingOrder = orden;
            g.transform.localScale = new Vector3(tamaño.x / r.sprite.bounds.size.x, tamaño.y / r.sprite.bounds.size.y, 1); return r;
        }
        void Rotulo(string nombre, string texto, Vector2 posicion, float tamano = 0.2f)
        {
            var g = new GameObject(nombre); g.transform.SetParent(mapa, false); g.transform.position = posicion;
            var t = g.AddComponent<TextMesh>(); t.text = texto; t.font = fuente; t.fontSize = 48; t.characterSize = tamano; t.anchor = TextAnchor.MiddleCenter; t.alignment = TextAlignment.Center; t.color = Color.white;
            var render = g.GetComponent<MeshRenderer>(); if (fuente != null) render.sharedMaterial = fuente.material; render.sortingOrder = 5500;
        }
        Vector2 UV(Vector2 punto) { return new Vector2((punto.x + 20) / 40, (punto.y + 12) / 24); }
        void ConstruirRecorrido()
        {
            Vector2 centro = new Vector2(-3, -2);
            Vector2 a = region == Region.Huila ? new Vector2(-13, 6) : region == Region.Caqueta ? new Vector2(-13, 7) : new Vector2(-12, 5);
            Vector2 b = region == Region.Huila ? new Vector2(10, 7) : region == Region.Caqueta ? new Vector2(11, 5) : new Vector2(9, 7);
            Vector2 c = region == Region.Huila ? new Vector2(13, -2) : region == Region.Caqueta ? new Vector2(13, -4) : new Vector2(13, -3);
            Vector2 final = new Vector2(8, -8);
            var recorridos = new[] {
                new[] { new Vector2(-15, -8), new Vector2(-9, -7), centro },
                new[] { centro, new Vector2(-9, 1), a },
                new[] { centro, new Vector2(3, 3), b },
                new[] { centro, new Vector2(5, -2), c },
                new[] { centro, new Vector2(2, -7), final }
            };
            var rutas = new List<TolimaMundo.Ruta>();
            int numero = 0;
            foreach (var recorrido in recorridos)
            {
                var uv = new Vector2[recorrido.Length]; for (int i = 0; i < uv.Length; i++) uv[i] = UV(recorrido[i]);
                rutas.Add(new TolimaMundo.Ruta { puntos = uv, ancho = 2.5f });
                if (!dibujarCaminos) continue;
                for (int i = 1; i < recorrido.Length; i++)
                {
                    Vector2 delta = recorrido[i] - recorrido[i - 1];
                    var r = Pintar("Camino_" + numero++, (recorrido[i] + recorrido[i - 1]) * 0.5f, new Vector2(delta.magnitude + 0.4f, 2.5f), sendero, -19000);
                    r.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
                }
                foreach (var p in recorrido) Pintar("UnionCamino", p, new Vector2(2.5f, 2.5f), sendero, -18999, circulo);
            }
            mundo.rutas = rutas.ToArray();
            var ids = new[] { "bienvenida", "actividad1", "actividad2", "actividad3", "compromiso" };
            var titulos = region == Region.Huila ? new[] { "Guía comunitaria", "Agua y cultivos", "Suelo protegido", "Residuos de la comunidad", "Compromiso de Huila" } : region == Region.Caqueta ? new[] { "Guía del bosque", "Bosque protegido", "Quebrada limpia", "Fauna y convivencia", "Compromiso de Caquetá" } : new[] { "Guía del río", "Rivera cuidada", "Residuos responsables", "Acuerdo comunitario", "Compromiso de Putumayo" };
            var puntos = new[] { centro, a, b, c, final };
            var sitios = new List<TolimaMundo.Sitio>();
            for (int i = 0; i < puntos.Length; i++)
            {
                var claro = Pintar("Plaza_" + ids[i], puntos[i], new Vector2(4.7f, 4.7f), sendero, -18998, circulo);
                var punto = new GameObject("Punto_" + ids[i]).transform; punto.SetParent(mapa, false); punto.position = puntos[i];
                SpriteRenderer habitante;
                if (i == 4) habitante = Pintar("TablonCompromiso", puntos[i] + Vector2.up * 1.8f, new Vector2(2.1f, 1.8f), spriteTablon != null ? Color.white : new Color(0.36f, 0.21f, 0.12f), 1000 - Mathf.RoundToInt(puntos[i].y * 100), spriteTablon);
                else
                {
                    Sprite sprite = i == 0 ? spriteGuia : spriteHabitante;
                    habitante = Pintar(i == 0 ? "Guia" : "Habitante_" + i, puntos[i] + Vector2.up * 1.5f, new Vector2(1, 1.7f), sprite != null ? Color.white : new Color(0.85f, 0.47f, 0.25f), 1000 - Mathf.RoundToInt(puntos[i].y * 100), sprite);
                    if (sprite == null) Pintar("Cabeza_" + i, puntos[i] + Vector2.up * 2.6f, new Vector2(0.7f, 0.7f), new Color(0.95f, 0.75f, 0.53f), 500, circulo);
                }
                var indicador = new GameObject("Marcador_" + ids[i]); indicador.transform.SetParent(mapa, false);
                Rotulo("TextoMarcador_" + i, "!", puntos[i] + Vector2.up * 3.1f, 0.34f);
                mapa.GetChild(mapa.childCount - 1).SetParent(indicador.transform, true);
                Rotulo("NombreSitio_" + i, titulos[i], puntos[i] + Vector2.down * 1.6f, 0.15f);
                sitios.Add(new TolimaMundo.Sitio { id = ids[i], titulo = titulos[i], punto = punto, marcador = indicador, visual = habitante });
                Sprite complemento = i == 3 ? spriteResiduos : i == 1 ? spriteCultivos : null;
                if (complemento != null) Pintar("ComplementoActividad_" + i, puntos[i] + new Vector2(1.7f, 0.6f), new Vector2(1.7f, 1.3f), Color.white, 1000 - Mathf.RoundToInt(puntos[i].y * 100), complemento);
            }
            mundo.sitios = sitios.ToArray();
        }
        void Decorar()
        {
            // Semilla local estable: no altera el estado global de Random del juego.
            var random = new System.Random(314 + (int)region);
            if (region != Region.Huila)
            {
                var rio = Pintar("Rio", new Vector2(4, 0), new Vector2(region == Region.Putumayo ? 4 : 2.7f, 29), agua, -25000);
                rio.transform.rotation = Quaternion.Euler(0, 0, region == Region.Putumayo ? -20 : 13);
                DibujarPuentes();
            }
            for (int i = 0; i < 85; i++)
            {
                Vector2 p = new Vector2((float)random.NextDouble() * 37 - 18.5f, (float)random.NextDouble() * 21 - 10.5f);
                bool cerca = false;
                foreach (var s in mundo.sitios) if (Vector2.Distance(p, s.punto.position) < 3.5f) cerca = true;
                if (cerca || mundo.PuedeCaminar(p)) continue;
                Pintar("Arbol_" + i, p, new Vector2(1.8f, 2.1f), spriteArbol != null ? Color.white : verde, 1000 - Mathf.RoundToInt(p.y * 100), spriteArbol != null ? spriteArbol : circulo);
            }
            foreach (Vector2 p in new[] { new Vector2(-5, -10), new Vector2(16, 8), new Vector2(-17, 1) })
                Pintar("CasaComunitaria", p, new Vector2(2.4f, 1.8f), spriteCasa != null ? Color.white : new Color(0.65f, 0.34f, 0.18f), 1000 - Mathf.RoundToInt(p.y * 100), spriteCasa);
            Rotulo("NombreRegion", mundo.nombreTerritorio.ToUpperInvariant(), new Vector2(0, 10.6f), 0.24f);
        }
        void DibujarPuentes()
        {
            if (spritePuente == null) return;
            float angulo = (region == Region.Putumayo ? -20 : 13) * Mathf.Deg2Rad;
            Vector2 ejeRio = new Vector2(-Mathf.Sin(angulo), Mathf.Cos(angulo));
            Vector2 origen = new Vector2(4, 0);
            int numero = 0;
            foreach (var ruta in mundo.rutas)
                for (int i = 1; i < ruta.puntos.Length; i++)
                {
                    Vector2 a = mundo.AWorld(ruta.puntos[i - 1]), b = mundo.AWorld(ruta.puntos[i]);
                    Vector2 tramo = b - a, haciaRio = origen - a;
                    float cruz = tramo.x * ejeRio.y - tramo.y * ejeRio.x;
                    if (Mathf.Abs(cruz) < 0.001f) continue;
                    float t = (haciaRio.x * ejeRio.y - haciaRio.y * ejeRio.x) / cruz;
                    if (t < 0 || t > 1) continue;
                    Vector2 centro = a + tramo * t;
                    if (Mathf.Abs(Vector2.Dot(centro - origen, ejeRio)) > 14.5f) continue;
                    float anchoRio = region == Region.Putumayo ? 4 : 2.7f;
                    float largo = anchoRio / Mathf.Max(0.25f, Mathf.Abs(cruz) / tramo.magnitude) + 1;
                    var puente = Pintar("Puente_" + numero++, centro, new Vector2(largo, 3.3f), Color.white, -18000, spritePuente);
                    puente.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(tramo.y, tramo.x) * Mathf.Rad2Deg);
                }
        }
        static TolimaMundo.Pregunta P(string texto, int correcta, params string[] opciones) { return new TolimaMundo.Pregunta(texto, correcta, opciones); }
        void ConfigurarRetos()
        {
            if (region == Region.Huila)
            {
                mundo.temasActividades = "agua y cultivos, suelo protegido y residuos";
                mundo.textoBienvenida = "Bienvenido a Huila. Conversa con la comunidad sobre el uso responsable del agua, la protección del suelo y la gestión de residuos.";
                mundo.ConfigurarActividades(new[] {
                    new[] { P("¿Qué decisión evita desperdiciar agua?", 0, "Reparar una fuga", "Dejar una llave abierta", "Regar sin revisar la necesidad"), P("¿Cómo decidir cuándo regar?", 1, "Regar siempre aunque llueva", "Revisar las necesidades del cultivo", "Usar toda el agua disponible"), P("¿Qué acción protege una fuente de agua?", 2, "Dejar envases en la orilla", "Arrojar residuos al cauce", "Mantenerla libre de residuos") },
                    new[] { P("¿Qué práctica ayuda a cuidar el suelo?", 1, "Dejarlo siempre descubierto", "Mantener cobertura vegetal", "Quemar toda la vegetación"), P("¿Qué alternativa evita quemar restos vegetales?", 0, "Aprovecharlos mediante manejo adecuado", "Prenderles fuego al final del día", "Quemarlos junto al camino"), P("¿Qué decisión cuida una zona con vegetación?", 2, "Ampliar una quema", "Dejar fuego sin vigilancia", "Elegir un manejo sin quemas") },
                    new[] { P("¿Qué hacer con los residuos?", 0, "Separarlos y usar la recolección adecuada", "Quemarlos en el patio", "Dejarlos en una quebrada"), P("¿Qué objeto conviene reutilizar cuando sea adecuado?", 2, "Una bolsa abandonada en el río", "Un recipiente de sustancias peligrosas sin control", "Un recipiente reutilizable en buen estado"), P("¿Qué acción ayuda a la comunidad?", 1, "Ocultar basura entre plantas", "Organizar puntos adecuados de disposición", "Dejar residuos junto al cultivo") }
                });
            }
            else if (region == Region.Caqueta)
            {
                mundo.temasActividades = "protección del bosque, quebrada limpia y convivencia con fauna";
                mundo.textoBienvenida = "Bienvenido a Caquetá. Recorre este bosque de juego y dialoga sobre el cuidado de la vegetación, las quebradas y la fauna.";
                mundo.ConfigurarActividades(new[] {
                    new[] { P("¿Qué acción protege la vegetación?", 2, "Quemar para ampliar un claro", "Cortar árboles sin planificación", "Conservar la cobertura vegetal"), P("¿Qué práctica evita introducir fuego al bosque?", 0, "Gestionar residuos sin quemarlos", "Prender fogatas junto a hojas secas", "Dejar brasas en el sendero"), P("¿Qué iniciativa favorece el cuidado del bosque?", 1, "Acumular basura en sus bordes", "Organizar acciones comunitarias de conservación", "Retirar toda la vegetación") },
                    new[] { P("¿Dónde disponer los residuos de una visita?", 1, "En el agua", "En el sistema de recolección adecuado", "Bajo una piedra del cauce"), P("¿Qué acción cuida una quebrada?", 0, "Mantener su orilla libre de residuos", "Arrojar envases al agua", "Dejar bolsas en la corriente"), P("¿Qué hacer con la vegetación de la ribera?", 2, "Quemarla para despejar", "Retirarla sin revisar", "Conservarla y evitar dañarla") },
                    new[] { P("¿Cómo observar animales silvestres?", 2, "Capturarlos", "Darles comida para acercarlos", "Mantener distancia y evitar molestarlos"), P("¿Qué acción respeta su entorno?", 0, "Evitar destruir refugios y nidos", "Llevarse un nido", "Dejar basura cerca de sus refugios"), P("¿Qué acuerdo ayuda a convivir con la fauna?", 1, "Comprar animales silvestres", "Proteger sus hábitats", "Capturarlos como mascotas") }
                });
            }
            else
            {
                mundo.temasActividades = "rivera cuidada, residuos responsables y acuerdos comunitarios";
                mundo.textoBienvenida = "Bienvenido a Putumayo, último territorio del recorrido. Aprende a cuidar las orillas del río, disponer los residuos y construir acuerdos para proteger el entorno.";
                mundo.ConfigurarActividades(new[] {
                    new[] { P("¿Qué ayuda a conservar la ribera?", 0, "Proteger su vegetación", "Quemar sus plantas", "Dejar escombros junto al agua"), P("¿Qué debe evitarse junto al río?", 2, "Recoger residuos de manera organizada", "Usar puntos adecuados de disposición", "Verter residuos al agua"), P("¿Qué acción respeta el cauce?", 1, "Obstruirlo con basura", "Mantenerlo libre de residuos", "Dejar materiales en la corriente") },
                    new[] { P("¿Qué hacer con los residuos de una jornada?", 2, "Dejarlos en el sendero", "Quemarlos junto al río", "Separarlos y entregarlos adecuadamente"), P("¿Qué alternativa reduce residuos?", 0, "Usar recipientes reutilizables apropiados", "Abandonar envases tras usarlos", "Cambiar todo por productos desechables"), P("¿Dónde dejar los residuos recogidos?", 1, "En otra orilla", "En un punto autorizado de recolección", "En un hueco del camino") },
                    new[] { P("¿Qué acuerdo protege el territorio?", 1, "Quemar residuos cada semana", "Organizar el cuidado del entorno", "Dejar la limpieza a una sola persona"), P("¿Cómo compartir lo aprendido?", 2, "Ocultar las buenas prácticas", "Promover quemas sin control", "Dialogar y proponer acciones de cuidado"), P("¿Qué compromiso resume tu recorrido?", 0, "Cuidar agua, suelo, vegetación y comunidad", "Arrojar basura lejos de casa", "Quemar para limpiar todo rápidamente") }
                });
            }
        }
        void OnDestroy()
        {
            foreach (var sprite in spritesAtlasGenerados) if (sprite != null) Destroy(sprite);
            if (cuadrado != null) { Destroy(cuadrado.texture); Destroy(cuadrado); }
            if (circulo != null) { Destroy(circulo.texture); Destroy(circulo); }
        }
    }
}
