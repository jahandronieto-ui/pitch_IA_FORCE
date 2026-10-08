using UnityEngine;

namespace GuardianesFlujo
{
    [CreateAssetMenu(menuName = "Guardianes/Definicion de personaje")]
    public class DefinicionPersonaje : ScriptableObject
    {
        [Min(1)] public int id = 1;
        public string nombre;
        public Sprite retrato;
        public GameObject prefab;
        [Header("Cuerpo completo: reemplaza la imagen provisional del menu")]
        public Sprite reposoAbajo, reposoArriba, reposoIzquierda, reposoDerecha;
        [Header("Frames en orden de reproduccion")]
        public Sprite[] caminarAbajo = new Sprite[0];
        public Sprite[] caminarArriba = new Sprite[0];
        public Sprite[] caminarIzquierda = new Sprite[0];
        public Sprite[] caminarDerecha = new Sprite[0];
        [Range(1, 24)] public float cuadrosPorSegundo = 8;
        [Min(0.1f)] public float alturaEnMundo = 2;
        public bool reflejarLateralFaltante = true;
    }
}
