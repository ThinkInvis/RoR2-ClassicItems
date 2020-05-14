using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using RoR2.Skills;
using EntityStates;
using System.Collections.Generic;

namespace ThinkInvisible.ClassicItems {
    public class Scepter : Item<Scepter> {
        public override string displayName => "Ancient Scepter";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Any});
        public override bool itemAIB {get; protected set;} = true;

        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) => "Upgrades one of your skills.";
        protected override string NewLangDesc(string langid = null) => "While held, one of your selected character's <style=cIsUtility>skills</style> <style=cStack>(unique per character)</style> becomes a <style=cIsUtility>more powerful version</style>.";
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";
        
        [AutoItemConfig("If true, TR12-C Gauss Compact will recharge faster to match the additional stock.")]
        public bool engiTurretAdjustCooldown {get; private set;} = false;

        [AutoItemConfig("If true, TR58-C Carbonizer Mini will recharge faster to match the additional stock.")]
        public bool engiWalkerAdjustCooldown {get; private set;} = false;

        public Scepter() {
            ConfigEntryChanged += (sender, args) => {
                switch(args.target.boundProperty.Name) {
                    case nameof(engiTurretAdjustCooldown):
                        EngiTurret2.myDef.baseRechargeInterval = EngiTurret2.oldDef.baseRechargeInterval * (((bool)args.newValue) ? 2f/3f : 1f);
                        GlobalUpdateSkillDef(EngiTurret2.myDef);
                        break;
                    case nameof(engiWalkerAdjustCooldown):
                        EngiWalker2.myDef.baseRechargeInterval = EngiWalker2.oldDef.baseRechargeInterval / (((bool)args.newValue) ? 2f : 1f);
                        GlobalUpdateSkillDef(EngiWalker2.myDef);
                        break;
                    default:
                        break;
                }
            };

            onAttrib += (tokenIdent, namePrefix) => {
			    Language.SetCurrentLanguage(Language.currentLanguage);

                ArtificerFlamethrower2.SetupAttributes();
                ArtificerFlyUp2.SetupAttributes();
                CommandoBarrage2.SetupAttributes();
                CommandoGrenade2.SetupAttributes();
                CrocoDisease2.SetupAttributes();
                EngiTurret2.SetupAttributes();
                EngiWalker2.SetupAttributes();
                HuntressBallista2.SetupAttributes();
                HuntressRain2.SetupAttributes();
                LoaderChargeFist2.SetupAttributes();
                LoaderChargeZapFist2.SetupAttributes();
                MercEvis2.SetupAttributes();
                MercEvisProjectile2.SetupAttributes();
                ToolbotDash2.SetupAttributes();
                TreebotFlower2_2.SetupAttributes();

                RegisterScepterSkill(ArtificerFlamethrower2.myDef, "MageBody", SkillSlot.Special, 0);
                RegisterScepterSkill(ArtificerFlyUp2.myDef, "MageBody", SkillSlot.Special, 1);
                RegisterScepterSkill(CommandoBarrage2.myDef, "CommandoBody", SkillSlot.Special, 0);
                RegisterScepterSkill(CommandoGrenade2.myDef, "CommandoBody", SkillSlot.Special, 1);
                RegisterScepterSkill(CrocoDisease2.myDef, "CrocoBody", SkillSlot.Special, 0);
                RegisterScepterSkill(EngiTurret2.myDef, "EngiBody", SkillSlot.Special, 0);
                RegisterScepterSkill(EngiWalker2.myDef, "EngiBody", SkillSlot.Special, 1);
                RegisterScepterSkill(HuntressRain2.myDef, "HuntressBody", SkillSlot.Special, 0);
                RegisterScepterSkill(HuntressBallista2.myDef, "HuntressBody", SkillSlot.Special, 1);
                RegisterScepterSkill(LoaderChargeFist2.myDef, "LoaderBody", SkillSlot.Utility, 0);
                RegisterScepterSkill(LoaderChargeZapFist2.myDef, "LoaderBody", SkillSlot.Utility, 1);
                RegisterScepterSkill(MercEvis2.myDef, "MercBody", SkillSlot.Special, 0);
                RegisterScepterSkill(MercEvisProjectile2.myDef, "MercBody", SkillSlot.Special, 1);
                RegisterScepterSkill(ToolbotDash2.myDef, "ToolbotBody", SkillSlot.Utility, 0);
                RegisterScepterSkill(TreebotFlower2_2.myDef, "TreebotBody", SkillSlot.Special, 0);
            };
        }

        protected override void LoadBehavior() {
            On.RoR2.CharacterBody.OnInventoryChanged += On_CBOnInventoryChanged;
            On.RoR2.CharacterMaster.GetDeployableSameSlotLimit += On_CMGetDeployableSameSlotLimit;

            ArtificerFlamethrower2.LoadBehavior();
            ArtificerFlyUp2.LoadBehavior();
            CommandoBarrage2.LoadBehavior();
            CommandoGrenade2.LoadBehavior();
            CrocoDisease2.LoadBehavior();
            HuntressBallista2.LoadBehavior();
            HuntressRain2.LoadBehavior();
            LoaderChargeFist2.LoadBehavior();
            LoaderChargeZapFist2.LoadBehavior();
            MercEvis2.LoadBehavior();
            MercEvisProjectile2.LoadBehavior();
            ToolbotDash2.LoadBehavior();
            TreebotFlower2_2.LoadBehavior();
        }

        protected override void UnloadBehavior() {
            On.RoR2.CharacterBody.OnInventoryChanged -= On_CBOnInventoryChanged;
            On.RoR2.CharacterMaster.GetDeployableSameSlotLimit -= On_CMGetDeployableSameSlotLimit;
            
            foreach(var cm in AliveList()) {
                if(!cm.hasBody) continue;
                HandleScepterSkill(cm.GetBody(), true);
            }

            ArtificerFlamethrower2.UnloadBehavior();
            ArtificerFlyUp2.UnloadBehavior();
            CommandoBarrage2.UnloadBehavior();
            CommandoGrenade2.UnloadBehavior();
            CrocoDisease2.UnloadBehavior();
            HuntressBallista2.UnloadBehavior();
            HuntressRain2.UnloadBehavior();
            LoaderChargeFist2.UnloadBehavior();
            LoaderChargeZapFist2.UnloadBehavior();
            MercEvis2.UnloadBehavior();
            MercEvisProjectile2.UnloadBehavior();
            ToolbotDash2.UnloadBehavior();
            TreebotFlower2_2.UnloadBehavior();
        }

        private int On_CMGetDeployableSameSlotLimit(On.RoR2.CharacterMaster.orig_GetDeployableSameSlotLimit orig, CharacterMaster self, DeployableSlot slot) {
            var retv = orig(self, slot);
            if(slot != DeployableSlot.EngiTurret) return retv;
            var sp = self.GetBody()?.skillLocator?.special;
            if(!sp) return retv;
            if(sp.skillDef == EngiTurret2.myDef)
                return retv + 1;
            if(sp.skillDef == EngiWalker2.myDef)
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
                Debug.LogError("ClassicItems: Can't register a scepter skill to negative variant index");
                return false;
            }
            if(scepterReplacers.Exists(x => x.bodyName == targetBodyName && (x.slotIndex != targetSlot || x.variantIndex == targetVariant))) {
                Debug.LogError("ClassicItems: A scepter skill already exists for this character; can't add multiple for different slots nor for the same variant");
                return false;
            }
            scepterReplacers.Add(new ScepterReplacer {bodyName = targetBodyName, slotIndex = targetSlot, variantIndex = targetVariant, replDef = replacingDef});
            scepterSlots[targetBodyName] = targetSlot;
            return true;
        }

        private void On_CBOnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self) {
            orig(self);
            HandleScepterSkill(self);
        }
        
        private void HandleScepterSkill(CharacterBody self, bool forceOff = false) {
            if(self.skillLocator && self.master?.loadout != null) {
                var bodyName = BodyCatalog.GetBodyName(self.bodyIndex);

                var repl = scepterReplacers.FindAll(x => x.bodyName == bodyName);
                if(repl.Count > 0) {
                    SkillSlot targetSlot = scepterSlots[bodyName];
                    var targetSkill = self.skillLocator.GetSkill(targetSlot);
                    var targetSlotIndex = self.skillLocator.GetSkillSlotIndex(targetSkill);
                    if(!targetSkill) return;
                    var targetVariant = self.master.loadout.bodyLoadoutManager.GetSkillVariant(self.bodyIndex, targetSlotIndex);
                    var replVar = repl.Find(x => x.variantIndex == targetVariant);
                    if(replVar == null) return;
                    if(GetCount(self) > 0 && !forceOff)
                        targetSkill.SetSkillOverride(self, replVar.replDef, GenericSkill.SkillOverridePriority.Upgrade);
                    else
                        targetSkill.UnsetSkillOverride(self, replVar.replDef, GenericSkill.SkillOverridePriority.Upgrade);
                }
            }
        }
    }
}
