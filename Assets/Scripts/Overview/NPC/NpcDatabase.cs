using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Overview.NPC
{
    [CreateAssetMenu(fileName = "NpcDatabase", menuName = "Scriptable Objects/Character/NpcDatabase", order = 1)]
    public class NpcDatabase : ScriptableObject
    {
        public NpcDefinition[] npcDefinitions;
        Dictionary<string, NpcDefinition> _map; // npcId → definition

        void OnEnable()
        {
            _map = npcDefinitions.ToDictionary(n => n.npcId, n => n);
        }

        public bool TryGet(string id, out NpcDefinition def)
        {
            return _map.TryGetValue(id, out def);
        }

        public IEnumerable<NpcDefinition> GetAll()
        {
            if (_map == null)
            {
                Debug.LogError("NpcDatabase not initialized. Call OnEnable first.");
                return Enumerable.Empty<NpcDefinition>();
            }

            return _map.Values;
        }
    }
}
