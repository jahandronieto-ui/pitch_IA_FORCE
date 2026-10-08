using UnityEngine;

namespace GuardianesTerritorio
{
    public class OrdenPorPiesTerritorio : MonoBehaviour
    {
        public SpriteRenderer imagen;
        public Transform pies;
        public int ordenBase;
        public int unidadesPorMetro = 100;
        void LateUpdate()
        {
            if (imagen == null) return;
            Transform referencia = pies != null ? pies : transform;
            imagen.sortingOrder = ordenBase - Mathf.RoundToInt(referencia.position.y * unidadesPorMetro);
        }
    }
}
