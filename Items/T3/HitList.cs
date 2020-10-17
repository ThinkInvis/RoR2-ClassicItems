using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using R2API;
using System.Linq;
using System.Collections.Generic;
using static TILER2.StatHooks;

namespace ThinkInvisible.ClassicItems {
    public class HitList : Item_V2<HitList> {
        public override string displayName => "The Hit List";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Damage});
        
        [AutoConfigUpdateEventInfo(AutoConfigUpdateEventFlags.InvalidateLanguage)]
        [AutoConfig("Time per batch of marked enemies.",AutoConfigFlags.None,0f,float.MaxValue)]
        public float cooldown {get;private set;} = 10f;
        
        [AutoConfigUpdateEventInfo(AutoConfigUpdateEventFlags.InvalidateLanguage | AutoConfigUpdateEventFlags.InvalidateStats)]
        [AutoConfig("Additive bonus to base damage per marked enemy killed.",AutoConfigFlags.PreventNetMismatch,0f,float.MaxValue)]
        public float procDamage {get;private set;} = 0.5f;
        
        [AutoConfigUpdateEventInfo(AutoConfigUpdateEventFlags.InvalidateLanguage | AutoConfigUpdateEventFlags.InvalidateStats)]
        [AutoConfig("Maximum damage bonus from The Hit List.",AutoConfigFlags.PreventNetMismatch,0f,float.MaxValue)]
        public float maxDamage {get;private set;} = 20f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Killing marked enemies permanently increases damage.";
        protected override string GetDescString(string langid = null) => "Every <style=cIsUtility>" + cooldown.ToString("N0") + " seconds</style>, <style=cIsUtility>1</style> <style=cStack>(+1 per stack)</style> random enemy will be <style=cIsUtility>marked</style> for the same duration. Killing <style=cIsUtility>marked</style> enemies gives you <style=cIsDamage>+" + procDamage.ToString("N1") + " permanent base damage</style> <style=cStack>(up to a maximum of " + maxDamage.ToString("N1") + ")</style>.";
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public ItemIndex hitListTally {get; private set;}
        public BuffIndex markDebuff {get; private set;}
        public BuffIndex tallyBuff {get; private set;}

        public override void SetupAttributes() {
            base.SetupAttributes();

            var markDebuffDef = new CustomBuff(new BuffDef {
                buffColor = Color.yellow,
                canStack = false,
                isDebuff = true,
                name = modInfo.shortIdentifier + "HitListDebuff",
                iconPath = "@ClassicItems:Assets/ClassicItems/icons/hitlist_debuff_icon.png"
            });
            markDebuff = BuffAPI.Add(markDebuffDef);

            var tallyBuffDef = new CustomBuff(new BuffDef {
                buffColor = Color.yellow,
                canStack = true,
                isDebuff = false,
                name = modInfo.shortIdentifier + "HitListBuff",
                iconPath = "@ClassicItems:Assets/ClassicItems/icons/hitlist_buff_icon.png"
            });
            tallyBuff = BuffAPI.Add(tallyBuffDef);

            var hitListTallyDef = new CustomItem(new ItemDef {
                hidden = true,
                name = modInfo.shortIdentifier + "INTERNALTally",
                tier = ItemTier.NoTier,
                canRemove = false
            }, new ItemDisplayRuleDict(null));
            hitListTally = ItemAPI.Add(hitListTallyDef);
        }

        public override void Install() {
            base.Install();
            GlobalEventManager.onCharacterDeathGlobal += Evt_GEMOnCharacterDeathGlobal;
            On.RoR2.Run.FixedUpdate += On_RunFixedUpdate;
            GetStatCoefficients += Evt_TILER2GetStatCoefficients;
        }

        public override void Uninstall() {
            base.Uninstall();
            GlobalEventManager.onCharacterDeathGlobal -= Evt_GEMOnCharacterDeathGlobal;
            On.RoR2.Run.FixedUpdate -= On_RunFixedUpdate;
            GetStatCoefficients -= Evt_TILER2GetStatCoefficients;
        }

        private float stopwatch = 0f;
        private void On_RunFixedUpdate(On.RoR2.Run.orig_FixedUpdate orig, Run self) {
            orig(self);
            stopwatch -= Time.fixedDeltaTime;
            if(stopwatch > 0f) return;

            stopwatch = cooldown;
            var alive = AliveList();
            int[] totalVsTeam = {0, 0, 0};
            int totalVsAll = 0;
            foreach(var cm in alive) {
                var icnt = GetCount(cm);
                if(FriendlyFireManager.friendlyFireMode != FriendlyFireManager.FriendlyFireMode.Off) {
                    totalVsAll += icnt;
                }  else {
                    if(cm.teamIndex != TeamIndex.Neutral) totalVsTeam[0] += icnt;
                    if(cm.teamIndex != TeamIndex.Player) totalVsTeam[1] += icnt;
                    if(cm.teamIndex != TeamIndex.Monster) totalVsTeam[2] += icnt;
                }
            }
            if(totalVsAll > 0) {
                for(var i = 0; i < totalVsAll; i++) {
                    if(alive.Count <= 0) break;
                    var next = rng.NextElementUniform(alive);
                    if(next.hasBody)
                        next.GetBody().AddTimedBuff(markDebuff, cooldown);
                    else
                        i--;
                    alive.Remove(next);
                }
            } else {
                List<CharacterMaster>[] aliveTeam = {
                    alive.Where(cm => cm.teamIndex == TeamIndex.Neutral).ToList(),
                    alive.Where(cm => cm.teamIndex == TeamIndex.Player).ToList(),
                    alive.Where(cm => cm.teamIndex == TeamIndex.Monster).ToList()
                };
                for(var list = 0; list <= 2; list++) {
                    for(var i = 0; i < totalVsTeam[list]; i++) {                        
                        if(aliveTeam[list].Count <= 0) break;
                        var next = rng.NextElementUniform(aliveTeam[list]);
                        if(next.hasBody)
                            next.GetBody().AddTimedBuff(markDebuff, cooldown);
                        else
                            i--;
                        aliveTeam[list].Remove(next);
                    }
                }
            }
        }
        
        private void Evt_GEMOnCharacterDeathGlobal(DamageReport rep) {
            if((rep.victimBody?.HasBuff(markDebuff) ?? false) && GetCount(rep.attackerBody) > 0)
                rep.attackerBody.inventory.GiveItem(hitListTally);
        }
        
        private void Evt_TILER2GetStatCoefficients(CharacterBody sender, StatHookEventArgs args) {
            var add = Mathf.Clamp(procDamage * (sender.inventory?.GetItemCount(hitListTally) ?? 0), 0f, maxDamage);
            args.baseDamageAdd += add;
            sender.SetBuffCount(tallyBuff, Mathf.FloorToInt(add/procDamage));
        }
    }
}
