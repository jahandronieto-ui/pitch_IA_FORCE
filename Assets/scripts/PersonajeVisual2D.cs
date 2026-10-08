using UnityEngine;

namespace GuardianesFlujo
{
    // El motor mueve la raiz (pies); este componente solo dibuja y anima el hijo Visual.
    public class PersonajeVisual2D : MonoBehaviour
    {
        public DefinicionPersonaje definicion;
        public SpriteRenderer visual;
        int direccion; // 0 abajo, 1 arriba, 2 izquierda, 3 derecha.
        bool caminando;
        float reloj, escala = 1;
        void Awake()
        {
            if (visual == null)
            {
                var hijo = new GameObject("Visual");
                hijo.transform.SetParent(transform, false);
                visual = hijo.AddComponent<SpriteRenderer>();
            }
            if (definicion == null) { Debug.LogError("Falta DefinicionPersonaje.", this); return; }
            Sprite baseSprite = Reposo(0);
            if (baseSprite == null) baseSprite = PrimerFrame();
            if (baseSprite != null && baseSprite.bounds.size.y > 0)
                escala = definicion.alturaEnMundo / baseSprite.bounds.size.y;
            visual.transform.localScale = Vector3.one * escala;
            Dibujar();
        }
        Sprite PrimerFrame()
        {
            foreach (var lista in new[] { definicion.caminarAbajo, definicion.caminarArriba, definicion.caminarIzquierda, definicion.caminarDerecha })
                if (lista != null) foreach (var s in lista) if (s != null) return s;
            return definicion.retrato;
        }
        Sprite Reposo(int d)
        {
            Sprite s = d == 1 ? definicion.reposoArriba : d == 2 ? definicion.reposoIzquierda : d == 3 ? definicion.reposoDerecha : definicion.reposoAbajo;
            if (s == null && definicion.reflejarLateralFaltante)
                s = d == 2 ? definicion.reposoDerecha : d == 3 ? definicion.reposoIzquierda : null;
            return s != null ? s : definicion.reposoAbajo;
        }
        Sprite[] Frames(int d)
        {
            return d == 1 ? definicion.caminarArriba : d == 2 ? definicion.caminarIzquierda : d == 3 ? definicion.caminarDerecha : definicion.caminarAbajo;
        }
        public void Movimiento(Vector2 movimiento)
        {
            bool mover = movimiento.sqrMagnitude > 0.0001f;
            int siguiente = direccion;
            if (mover) siguiente = Mathf.Abs(movimiento.x) > Mathf.Abs(movimiento.y) ? (movimiento.x < 0 ? 2 : 3) : (movimiento.y > 0 ? 1 : 0);
            if (siguiente != direccion || mover != caminando) reloj = 0;
            direccion = siguiente; caminando = mover;
        }
        void Update()
        {
            if (definicion == null || visual == null) return;
            if (caminando) reloj += Time.deltaTime;
            Dibujar();
        }
        void Dibujar()
        {
            if (definicion == null || visual == null) return;
            Sprite[] frames = Frames(direccion);
            bool reflejo = false;
            if ((frames == null || frames.Length == 0) && definicion.reflejarLateralFaltante && (direccion == 2 || direccion == 3))
            {
                frames = Frames(direccion == 2 ? 3 : 2);
                reflejo = frames != null && frames.Length > 0;
            }
            Sprite sprite = Reposo(direccion);
            if (caminando && frames != null && frames.Length > 0)
                sprite = frames[Mathf.FloorToInt(reloj * Mathf.Max(1, definicion.cuadrosPorSegundo)) % frames.Length];
            else if (sprite == null && frames != null && frames.Length > 0) sprite = frames[0];
            if (sprite == null) sprite = PrimerFrame();
            if (!caminando)
                reflejo = definicion.reflejarLateralFaltante && ((direccion == 2 && definicion.reposoIzquierda == null && definicion.reposoDerecha != null) || (direccion == 3 && definicion.reposoDerecha == null && definicion.reposoIzquierda != null));
            visual.sprite = sprite;
            visual.flipX = reflejo;
            if (sprite != null)
            {
                // Compensa el pivot, sin cambiar el tamaño entre frames.
                float centroX = reflejo ? -sprite.bounds.center.x : sprite.bounds.center.x;
                visual.transform.localPosition = new Vector3(-centroX * escala, -sprite.bounds.min.y * escala, 0);
            }
        }
    }
}
