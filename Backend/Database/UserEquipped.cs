using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Presets;

namespace StarCloudgamesLibrary
{
    public partial class UserDatabaseController
    {
        private const int MaxSkillPresetId = 4;
        private const int MaxSkillPerPreset = 5;

        private readonly Dictionary<RelicType, int> relicEquipLimit = new()
        {
            { RelicType.Artifact, 5 },
            { RelicType.Gauntlet, 1 },
            { RelicType.Necklace, 1},
            { RelicType.Ring, 10},
            { RelicType.GauntletJewel, 4}
        };

        private readonly RelicType[] relicTypes = (RelicType[])Enum.GetValues(typeof(RelicType));

        #region "Get Common API"

        public List<IStat> GetEquippedEquippableStats(ScriptableStatType statType)
        {
            switch(statType)
            {
                case ScriptableStatType.EquipmentStat:
                    return GetEquippedEquipments();
                case ScriptableStatType.RelicStat:
                    return GetEquippedRelics();
                case ScriptableStatType.SkillStat:
                    return GetEquippedPassiveSkills();
                default: //add more
                    return new List<IStat>();
            }
        }

        #endregion

        #region "Equipped Updated"

        private void EquippedEquipmentUpdated()
        {
            StatManager.instance.SetEquippedEquipments(GetEquippedEquipments());
            UserEquippedData.Updated();
        }

        private void EquippedSkillUpdated()
        {
            StatManager.instance.SetEquippedPassiveSkills(GetEquippedPassiveSkills());
            UserEquippedData.Updated();
        }

        private void EquippedRelicUpdated()
        {
            StatManager.instance.SetEquippedRelicStats(GetEquippedRelics());
        }

        #endregion

        #region "Get Equipped List"

        private List<IStat> GetEquippedEquipments()
        {
            var result = new List<IStat>();

            if(UserEquippedData.equippedEquipment == null || UserEquippedData.equippedEquipment.Count == 0) return result;

            foreach(var equipment in UserEquippedData.equippedEquipment)
            {
                var equipmentStat = ScriptableStatManager.instance.GetStat(ScriptableStatType.EquipmentStat, equipment.Value);
                if(equipmentStat != null)
                {
                    result.Add(equipmentStat);
                }
            }

            return result;
        }

        private List<IStat> GetEquippedPassiveSkills()
        {
            var currentSkillList = GetSkillList(UserEquippedData.skillPresetId);
            if(currentSkillList == null || currentSkillList.Count == 0) return new List<IStat>();

            return currentSkillList.Where(skill => skill.skillType == SkillType.Passive).Cast<IStat>().ToList();
        }

        private List<IStat> GetEquippedRelics()
        {
            var result = new List<IStat>();

            foreach(var relicType in relicTypes)
            {
                var relicStats = GetEquippedRelicStats(relicType);
                if(relicStats != null && relicStats.Count > 0)
                {
                    result.AddRange(relicStats);
                }
            }

            return result;
        }

        #endregion

        #region "Equipment"

        public bool IsEquippedEquipment(EquipmentStatScriptable equipmentStat)
        {
            if(UserEquippedData.equippedEquipment.TryGetValue(equipmentStat.equipmentType, out var equippedData))
            {
                if(equippedData == equipmentStat.StatName())
                {
                    return true;
                }
            }

            return false;
        }

        public void EquipEquipment(EquipmentStatScriptable equipmentStat)
        {
            UserEquippedData.equippedEquipment[equipmentStat.equipmentType] = equipmentStat.StatName();
            EquippedEquipmentUpdated();
        }

        #endregion

        #region "Skill"

        public void SetSkillPresetId(int skillPresetId)
        {
            UserEquippedData.skillPresetId = skillPresetId;
        }

        public List<SkillStatScriptable> GetSkillList(int presetId)
        {
            var newSkillList = new List<SkillStatScriptable>();

            if (UserEquippedData.equippedSkill.TryGetValue(presetId, out var list))
            {
                foreach (var item in list)
                {
                    var skillStat = (SkillStatScriptable)ScriptableStatManager.instance.GetStat(ScriptableStatType.SkillStat, item);
                    if (skillStat != null)
                    {
                        newSkillList.Add(skillStat);
                    }
                }
            }

            return newSkillList;
        }

        public bool EquipSkill(int presetId, SkillStatScriptable skillStat)
        {
            if(presetId < 0 || presetId > MaxSkillPresetId || IsSkillEquipped(skillStat))
            {
                return false;
            }

            if(!UserEquippedData.equippedSkill.TryGetValue(presetId, out var equippedSkillList))
            {
                equippedSkillList = new List<string>();
                UserEquippedData.equippedSkill[presetId] = equippedSkillList;
            }

            if(equippedSkillList.Count >= MaxSkillPerPreset) return false;

            equippedSkillList.Add(skillStat.StatName());
            
            EquippedSkillUpdated();

            return true;
        }

        public bool IsSkillEquipped(SkillStatScriptable skillstat)
        {
            return GetSkillList(UserEquippedData.skillPresetId).Contains(skillstat);
        }

        public int CurrentPresetId() => UserEquippedData.skillPresetId;

        public bool UnequipSkill(SkillStatScriptable skillStat)
        {
            if(IsSkillEquipped(skillStat))
            {
                if(UserEquippedData.equippedSkill.TryGetValue(UserEquippedData.skillPresetId, out var equippedSkillList))
                {
                    equippedSkillList.Remove(skillStat.StatName());
                    EquippedSkillUpdated();
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region "Relic"

        public bool EquipRelic(RelicStatScriptable relic)
        {
            if (IsRelicEquipped(relic)) return false;

            if(!UserEquippedData.equippedRelic.TryGetValue(relic.relicType, out var equippedRelics))
            {
                equippedRelics = new List<string>();
                UserEquippedData.equippedRelic[relic.relicType] = equippedRelics;
            }

            if(equippedRelics.Count >= relicEquipLimit[relic.relicType]) return false;

            UserEquippedData.equippedRelic[relic.relicType].Add(relic.StatName());
            EquippedRelicUpdated();

            return true;
        }

        public bool UnequipRelic(RelicStatScriptable relic)
        {
            if (IsRelicEquipped(relic))
            {
                if(UserEquippedData.equippedRelic.TryGetValue(relic.relicType, out var equippedRelics))
                {
                    equippedRelics.Remove(relic.StatName());
                    EquippedRelicUpdated();
                    return true;
                }
            }

            return false;
        }

        public List<RelicStatScriptable> GetEquippedRelicStats(RelicType type)
        {
            if(UserEquippedData.equippedRelic.TryGetValue(type, out var equippedRelicStats))
            {
                var relicStats = new List<RelicStatScriptable>();
                foreach (var relic in equippedRelicStats)
                {
                    var relicStat = ScriptableStatManager.instance.GetStat(ScriptableStatType.RelicStat, relic);
                    if (relicStat is RelicStatScriptable relicStatScriptable)
                    {
                        relicStats.Add(relicStatScriptable);
                    }
                }
                return relicStats;
            }

            return null;
        }

        public bool IsRelicEquipped(RelicStatScriptable statScriptable)
        {
            if (UserEquippedData.equippedRelic.TryGetValue(statScriptable.relicType, out var relicList))
            {
                if (relicList.Contains(statScriptable.StatName()))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region "Ring Elemental"

        public Dictionary<ElementalType, int> GetEquippedRingElementalCounts()
        {
            var result = new Dictionary<ElementalType, int>();

            var equippedRings = GetEquippedRelicStats(RelicType.Ring);
            if(equippedRings == null) return result;

            foreach(var ring in equippedRings)
            {
                result[ring.elementalType] = result.GetValueOrDefault(ring.elementalType) + 1;
            }

            return result;
        }

        public bool CanActivate(ElementalType targetElementalType, out int equippedCount)
        {
            equippedCount = 0;

            var currentEquippedElementalCounts = GetEquippedRingElementalCounts();
            if(!currentEquippedElementalCounts.TryGetValue(targetElementalType, out var count)) return false;

            equippedCount = count;
            return true;
        }

        #endregion
    }
}