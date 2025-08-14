using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarCloudgamesLibrary
{
    [System.Serializable]
    public class UserEquippedData
    {
        public Dictionary<EquipmentType, string> equippedEquipment;
        public Dictionary<RelicType, List<string>> equippedRelic;
        public Dictionary<int, List<string>> equippedSkill;

        public int skillPresetId;

        private bool updated;

        public void Initialize()
        {
            equippedEquipment = new Dictionary<EquipmentType, string>();
            equippedRelic = new Dictionary<RelicType, List<string>>();
            equippedSkill = new Dictionary<int, List<string>>();
        }

        public void Updated()
        {
            updated = true;
        }
    }
}