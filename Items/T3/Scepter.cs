using RoR2;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static TILER2.SkillUtil;
using RoR2.Skills;
using System.Collections.Generic;
using R2API;
using System.Linq;

namespace ThinkInvisible.ClassicItems {
    public abstract class ScepterSkill {
        public abstract SkillDef myDef {get; protected set;}
        public abstract string oldDescToken {get; protected set;}
        public abstract string newDescToken {get; protected set;}
        public abstract string overrideStr {get;}
        internal abstract void SetupAttributes();
        internal virtual void LoadBehavior() { }
        internal virtual void UnloadBehavior() { }
        public abstract string targetBody {get;}
        public abstract SkillSlot targetSlot {get;}
        public abstract int targetVariantIndex {get;}
    }

    public class Scepter : Item<Scepter> {
        public override string displayName => "Ancient Scepter";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Any});
        public override bool itemAIB {get; protected set;} = true;

        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) => "Upgrades one of your skills.";
        protected override string NewLangDesc(string langid = null) => "While held, one of your selected character's <style=cIsUtility>skills</style> <style=cStack>(unique per character)</style> becomes a <style=cIsUtility>more powerful version</style>."
            + $" <style=cStack>{(rerollExtras ? "Extra/unusable" : "Unusable (but NOT extra)")} pickups will reroll into other red items.</style>";
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";
        
        [AutoItemConfig("If true, TR12-C Gauss Compact will recharge faster to match the additional stock.")]
        public bool engiTurretAdjustCooldown {get; private set;} = false;

        [AutoItemConfig("If true, TR58-C Carbonizer Mini will recharge faster to match the additional stock.")]
        public bool engiWalkerAdjustCooldown {get; private set;} = false;
        
        [AutoItemConfig("If true, any stacks picked up past the first will reroll to other red items. If false, this behavior will only be used for characters which cannot benefit from the item at all.")]
        public bool rerollExtras {get; private set;} = true;

        public void PatchLang() {
            foreach(var skill in skills) {
                if(skill.oldDescToken == null) {
                    ClassicItemsPlugin._logger.LogError(skill.GetType().Name + " oldDescToken is null!");
                    continue;
                }
                LanguageAPI.Add(skill.newDescToken, Language.GetString(skill.oldDescToken) + skill.overrideStr, Language.currentLanguageName);
            }
        }

        internal List<ScepterSkill> skills = new List<ScepterSkill>();

        public Scepter() {
            skills.Add(new ArtificerFlamethrower2());
            skills.Add(new ArtificerFlyUp2());
            skills.Add(new CaptainAirstrike2());
            skills.Add(new CommandoBarrage2());
            skills.Add(new CommandoGrenade2());
            skills.Add(new CrocoDisease2());
            skills.Add(new EngiTurret2());
            skills.Add(new EngiWalker2());
            skills.Add(new HuntressBallista2());
            skills.Add(new HuntressRain2());
            skills.Add(new LoaderChargeFist2());
            skills.Add(new LoaderChargeZapFist2());
            skills.Add(new MercEvis2());
            skills.Add(new MercEvisProjectile2());
            skills.Add(new ToolbotDash2());
            skills.Add(new TreebotFlower2_2());

            ConfigEntryChanged += (sender, args) => {
                switch(args.target.boundProperty.Name) {
                    case nameof(engiTurretAdjustCooldown):
                        var engiSkill = skills.First(x => x is EngiTurret2);
                        engiSkill.myDef.baseRechargeInterval = EngiTurret2.oldDef.baseRechargeInterval * (((bool)args.newValue) ? 2f/3f : 1f);
                        GlobalUpdateSkillDef(engiSkill.myDef);
                        break;
                    case nameof(engiWalkerAdjustCooldown):
                        var engiSkill2 = skills.First(x => x is EngiWalker2);
                        engiSkill2.myDef.baseRechargeInterval = EngiWalker2.oldDef.baseRechargeInterval / (((bool)args.newValue) ? 2f : 1f);
                        GlobalUpdateSkillDef(engiSkill2.myDef);
                        break;
                    default:
                        break;
                }
            };

            onAttrib += (tokenIdent, namePrefix) => {
                foreach(var skill in skills) {
                    skill.SetupAttributes();
                    RegisterScepterSkill(skill.myDef, skill.targetBody, skill.targetSlot, skill.targetVariantIndex);
                }
            };
        }

        protected override void LoadBehavior() {
            On.RoR2.CharacterBody.OnInventoryChanged += On_CBOnInventoryChanged;
            On.RoR2.CharacterMaster.GetDeployableSameSlotLimit += On_CMGetDeployableSameSlotLimit;
            
            foreach(var skill in skills) {
                skill.LoadBehavior();
            }
        }

        protected override void UnloadBehavior() {
            On.RoR2.CharacterBody.OnInventoryChanged -= On_CBOnInventoryChanged;
            On.RoR2.CharacterMaster.GetDeployableSameSlotLimit -= On_CMGetDeployableSameSlotLimit;
            
            foreach(var cm in AliveList()) {
                if(!cm.hasBody) continue;
                HandleScepterSkill(cm.GetBody(), true);
            }
            
            foreach(var skill in skills) {
                skill.UnloadBehavior();
            }
        }

        private int On_CMGetDeployableSameSlotLimit(On.RoR2.CharacterMaster.orig_GetDeployableSameSlotLimit orig, CharacterMaster self, DeployableSlot slot) {
            var retv = orig(self, slot);
            if(slot != DeployableSlot.EngiTurret) return retv;
            var sp = self.GetBody()?.skillLocator?.special;
            if(!sp) return retv;
            if(sp.skillDef == skills.First(x => x is EngiTurret2).myDef)
                return retv + 1;
            if(sp.skillDef == skills.First(x => x is EngiWalker2).myDef)
                return retv + 2;
            return retv;
        }

        private class ScepterReplacer {
            public string bodyName;
            public SkillSlot slotIndex;
            public int variantIndex;
            public SkillDef replDef;
        }

        private readonly List<ScepterReplacer> scepterReplacers = new List<ScepterReplacer>();
        private readonly Dictionary<string, SkillSlot> scepterSlots = new Dictionary<string, SkillSlot>();

        public bool RegisterScepterSkill(SkillDef replacingDef, string targetBodyName, SkillSlot targetSlot, int targetVariant) {
            if(targetVariant < 0) {
                ClassicItemsPlugin._logger.LogError("Can't register a scepter skill to negative variant index");
                return false;
            }
            if(scepterReplacers.Exists(x => x.bodyName == targetBodyName && (x.slotIndex != targetSlot || x.variantIndex == targetVariant))) {
                ClassicItemsPlugin._logger.LogError("A scepter skill already exists for this character; can't add multiple for different slots nor for the same variant");
                return false;
            }
            scepterReplacers.Add(new ScepterReplacer {bodyName = targetBodyName, slotIndex = targetSlot, variantIndex = targetVariant, replDef = replacingDef});
            scepterSlots[targetBodyName] = targetSlot;
            return true;
        }

        bool handlingInventory = false;
        private void On_CBOnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self) {
            orig(self);
            if(handlingInventory) return;
            handlingInventory = true;
            if(!HandleScepterSkill(self)) {
                if(GetCount(self) > 0) {
                    Reroll(self, GetCount(self));
                }
            } else if(GetCount(self) > 1 && rerollExtras) {
                 Reroll(self, GetCount(self) - 1);
            }
            handlingInventory = false;
        }

        private void Reroll(CharacterBody self, int count) {
            if(count <= 0) return;
            var list = Run.instance.availableTier3DropList.Except(new[] {pickupIndex}).ToList();
            for(var i = 0; i < count; i++) {
                self.inventory.RemoveItem(regIndex, 1);
                self.inventory.GiveItem(PickupCatalog.GetPickupDef(list[UnityEngine.Random.Range(0, list.Count)]).itemIndex);
            }
        }
        
        private bool HandleScepterSkill(CharacterBody self, bool forceOff = false) {
            if(self.skillLocator && self.master?.loadout != null) {
                var bodyName = BodyCatalog.GetBodyName(self.bodyIndex);

                var repl = scepterReplacers.FindAll(x => x.bodyName == bodyName);
                if(repl.Count > 0) {
                    SkillSlot targetSlot = scepterSlots[bodyName];
                    var targetSkill = self.skillLocator.GetSkill(targetSlot);
                    var targetSlotIndex = self.skillLocator.GetSkillSlotIndex(targetSkill);
                    if(!targetSkill) return false;
                    var targetVariant = self.master.loadout.bodyLoadoutManager.GetSkillVariant(self.bodyIndex, targetSlotIndex);
                    var replVar = repl.Find(x => x.variantIndex == targetVariant);
                    if(replVar == null) return false;
                    if(GetCount(self) > 0 && !forceOff)
                        targetSkill.SetSkillOverride(self, replVar.replDef, GenericSkill.SkillOverridePriority.Upgrade);
                    else
                        targetSkill.UnsetSkillOverride(self, replVar.replDef, GenericSkill.SkillOverridePriority.Upgrade);
                    return true;
                }
            }
            return false;
        }
    }
}
