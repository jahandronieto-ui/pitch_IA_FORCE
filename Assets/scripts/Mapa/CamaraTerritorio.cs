using UnityEngine;

namespace GuardianesTerritorio
{
    [RequireComponent(typeof(Camera))]
    public class CamaraTerritorio : MonoBehaviour
    {
        public Transform objetivo;
        public SpriteRenderer fondo;
        [Min(0)] public float suavizado = 6f;
        Camera camara;

        void Awake()
        {
            camara = GetComponent<Camera>();
            camara.orthographic = true;
        }

        void LateUpdate()
        {
            if (objetivo == null) return;
            Vector3 destino = objetivo.position;
            destino.z = transform.position.z;
            Vector3 posicion = Vector3.Lerp(transform.position, destino, 1f - Mathf.Exp(-suavizado * Time.deltaTime));
            if (fondo != null)
            {
                Bounds b = fondo.bounds;
                float alto = camara.orthographicSize;
                float ancho = alto * camara.aspect;
                posicion.x = b.size.x <= ancho * 2 ? b.center.x : Mathf.Clamp(posicion.x, b.min.x + ancho, b.max.x - ancho);
                posicion.y = b.size.y <= alto * 2 ? b.center.y : Mathf.Clamp(posicion.y, b.min.y + alto, b.max.y - alto);
            }
            transform.position = posicion;
        }
    }
}
