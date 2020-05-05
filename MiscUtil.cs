using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using R2API.Utils;
using System.Collections;

namespace ThinkInvisible.ClassicItems {
    public static class MiscUtil {
        //Collection of unique class instances which all inherit the same type
        public class FilingDictionary<T> : IEnumerable<T> {
            private readonly Dictionary<Type, T> _dict = new Dictionary<Type, T>();

            public void Add(T inst) {
                _dict.Add(inst.GetType(), inst);
            }

            public void Add<subT>(subT inst) where subT : T {
                _dict.Add(typeof(subT), inst);
            }

            public void Set<subT>(subT inst) where subT : T {
                _dict[typeof(subT)] = inst;
            }

            public subT Get<subT>() where subT : T {
                return (subT)_dict[typeof(subT)];
            }

            public void Remove(T inst) {
                _dict.Remove(inst.GetType());
            }

            public void RemoveWhere(Func<T, bool> predicate) {
                foreach (var key in _dict.Values.Where(predicate).ToList()) {
                    _dict.Remove(key.GetType());
                }
            }

            public IEnumerator<T> GetEnumerator() {
                return _dict.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        }

        public static string pct(float tgt, uint prec = 0, float mult = 100f) {
            return (tgt*mult).ToString("N" + prec) + "%";
        }
        public static string nplur(float tgt, uint prec = 0) {
            if(prec == 0)
                return (tgt == 1 || tgt == -1) ? "" : "s";
            else
                return (Math.Abs(Math.Abs(tgt)-1) < Math.Pow(10,-prec)) ? "" : "s";
        }
        public static float getDifficultyCoeffIncreaseAfter(float time, int stages) {
			DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty);
			float num2 = Mathf.Floor((Run.instance.GetRunStopwatch() + time) * 0.0166666675f);
			float num4 = 0.7f + (float)Run.instance.participatingPlayerCount * 0.3f;
			float num7 = 0.046f * difficultyDef.scalingValue * Mathf.Pow((float)Run.instance.participatingPlayerCount, 0.2f);
			float num9 = Mathf.Pow(1.15f, (float)Run.instance.stageClearCount + (float)stages);
			return (num4 + num7 * num2) * num9 - Run.instance.difficultyCoefficient;
        }
        //from DropGoldAfterDeath mod: https://github.com/exel80/DropGoldAfterDeath/blob/master/DropGoldAfterDeath.cs
        public static List<CharacterMaster> aliveList(bool playersOnly = false) {
            if(playersOnly) return PlayerCharacterMasterController.instances.Where(x=>x.isConnected && x.master && !x.master.IsDeadAndOutOfLivesServer()).Select(x=>x.master).ToList();
            else return CharacterMaster.readOnlyInstancesList.Where(x=>!x.IsDeadAndOutOfLivesServer()).ToList();
        }
        public static void spawnItemFromBody(CharacterBody src, int tier) {
            List<PickupIndex> spawnList;
            switch(tier) {
                case 1:
                    spawnList = Run.instance.availableTier2DropList;
                    break;
                case 2:
                    spawnList = Run.instance.availableTier3DropList;
                    break;
                case 3:
                    spawnList = Run.instance.availableLunarDropList;
                    break;
                case 4:
                    spawnList = Run.instance.availableNormalEquipmentDropList;
                    break;
                case 5:
                    spawnList = Run.instance.availableLunarEquipmentDropList;
                    break;
                case 0:
                    spawnList = Run.instance.availableTier1DropList;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("tier", tier, "spawnItemFromBody: Item tier must be between 0 and 5 inclusive");
            }
            PickupDropletController.CreatePickupDroplet(spawnList[Run.instance.spawnRng.RangeInt(0,spawnList.Count)], src.transform.position, new Vector3(UnityEngine.Random.Range(-5.0f, 5.0f), 20f, UnityEngine.Random.Range(-5.0f, 5.0f)));
        }

        public static bool RemoveOccupiedNode(this DirectorCore self, RoR2.Navigation.NodeGraph nodeGraph, RoR2.Navigation.NodeGraph.NodeIndex nodeIndex) {
            var ocnf = self.GetType().GetFieldCached("occupiedNodes");
            Array ocn = (Array)ocnf.GetValue(self);
            if(ocn.Length == 0) {
                Debug.LogWarning("ClassicItems: RemoveOccupiedNode has no nodes to remove");
                return false;
            }
            Array ocnNew = (Array)Activator.CreateInstance(ClassicItemsPlugin.nodeRefTypeArr, ocn.Length - 1);
            IEnumerable ocne = ocn as IEnumerable;
            int i = 0;
            foreach(object o in ocne) {
                var scanInd = o.GetFieldValue<RoR2.Navigation.NodeGraph.NodeIndex>("nodeIndex");
                var scanGraph = o.GetFieldValue<RoR2.Navigation.NodeGraph>("nodeGraph");
                if(object.Equals(scanGraph, nodeGraph) && scanInd.Equals(nodeIndex))
                    continue;
                else if(i == ocn.Length - 1) {
                    Debug.LogWarning("ClassicItems: RemoveOccupiedNode was passed an already-removed or otherwise nonexistent node");
                    return false;
                }
                ocnNew.SetValue(o, i);
                i++;
            }
            ocnf.SetValue(self, ocnNew);
            return true;
        }
    }
}
