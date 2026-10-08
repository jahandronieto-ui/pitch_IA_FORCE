using UnityEngine;

namespace GuardianesFlujo
{
    [CreateAssetMenu(menuName = "Guardianes/Catalogo del flujo")]
    public class CatalogoFlujo : ScriptableObject
    {
        public DefinicionPersonaje[] personajes = new DefinicionPersonaje[0];
        public DefinicionPersonaje Buscar(int id)
        {
            DefinicionPersonaje resultado = null;
            foreach (var p in personajes)
                if (p != null && p.id == id)
                {
                    if (resultado != null) return null; // No resolver IDs duplicados arbitrariamente.
                    resultado = p;
                }
            return resultado;
        }
    }
}
