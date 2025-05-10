using UnityEngine;

namespace Blade.Entities
{
    [CreateAssetMenu(fileName = "EntityFinder", menuName = "SO/Entity/Finder", order = 0)]
    public class EntityFinderSO : ScriptableObject
    {
        public Entity Target { get; private set; } 

        public void SetTarget(Entity target)
        {
            Target = target;
        }
    }
}