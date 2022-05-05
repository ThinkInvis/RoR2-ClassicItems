using RoR2;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using R2API;

namespace ThinkInvisible.ClassicItems {
    public class GoldenGun : Item<GoldenGun> {
        public override string displayName => "Golden Gun";
		public override ItemTier itemTier => ItemTier.Tier2;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Damage});

        [AutoConfigRoOSlider("{0:P1}", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum multiplier to add to player damage.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageBoost {get;private set;} = 0.4f;

        [AutoConfigRoOIntSlider("${0:N0}", 0, 10000)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Gold required for maximum damage. Scales with difficulty level.", AutoConfigFlags.PreventNetMismatch, 0, int.MaxValue)]
        public int goldAmt {get;private set;} = 700;

        [AutoConfigRoOSlider("{0:P0}", 0f, 0.999f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Inverse-exponential multiplier for reduced GoldAmt per stack (higher = more powerful).", AutoConfigFlags.PreventNetMismatch, 0f, 0.999f)]
        public float goldReduc {get;private set;} = 0.5f;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, deployables (e.g. Engineer turrets) with Golden Gun will benefit from their master's money.",
            AutoConfigFlags.PreventNetMismatch)]
        public bool inclDeploys {get;private set;} = true;

        private bool ilFailed = false;
        
        public BuffDef goldenGunBuff {get;private set;}
        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "More gold, more damage.";
        protected override string GetDescString(string langid = null) => "Deal <style=cIsDamage>bonus damage</style> based on your <style=cIsUtility>money</style>, up to <style=cIsDamage>" + Pct(damageBoost) + "</style> at <style=cIsUtility>$" + goldAmt.ToString("N0") + "</style> <style=cStack>(cost increases with difficulty, -" + Pct(goldReduc) + " per stack)</style>.";
        protected override string GetLoreString(string langid = null) => "Order: Golden Gun\n\nTracking Number: 149***********\nEstimated Delivery: 12/19/1974\nShipping Method: Priority\nShipping Address: James B., ??\nShipping Details:\n\nWas this supposed to.. intimidate me? I do like its look, however; perhaps I'll set it above my fireplace.";

        public GoldenGun() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/goldengun_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/GoldenGun.prefab");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            goldenGunBuff = ScriptableObject.CreateInstance<BuffDef>();
            goldenGunBuff.buffColor = new Color(0.85f, 0.8f, 0.3f);
            goldenGunBuff.canStack = true;
            goldenGunBuff.isDebuff = false;
            goldenGunBuff.name = $"{modInfo.shortIdentifier}GoldenGun";
            goldenGunBuff.iconSprite = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/goldengun_icon.png");
            ContentAddition.AddBuffDef(goldenGunBuff);
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
        }

        public override void Install() {
            base.Install();
            IL.RoR2.HealthComponent.TakeDamage += IL_CBTakeDamage;
            if(ilFailed) IL.RoR2.HealthComponent.TakeDamage -= IL_CBTakeDamage;
            else {
                On.RoR2.CharacterBody.FixedUpdate += On_CBFixedUpdate;
                On.RoR2.CharacterBody.OnInventoryChanged += On_CBInventoryChanged;
            }
        }

        public override void Uninstall() {
            base.Uninstall();
            IL.RoR2.HealthComponent.TakeDamage -= IL_CBTakeDamage;
            On.RoR2.CharacterBody.FixedUpdate -= On_CBFixedUpdate;
            On.RoR2.CharacterBody.OnInventoryChanged -= On_CBInventoryChanged;
        }

        private void On_CBInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self) {
            orig(self);
            var cpt = self.GetComponent<GoldenGunComponent>();
            if(!cpt) cpt = self.gameObject.AddComponent<GoldenGunComponent>();
            var newIcnt = GetCount(self);
            if(cpt.cachedIcnt != newIcnt) {
                cpt.cachedIcnt = newIcnt;
                UpdateGGBuff(self);
            }
        }

        private void On_CBFixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self) {
            orig(self);
            if(!self) return;
            var cpt = self.GetComponent<GoldenGunComponent>();
            if(!cpt) return;
            var newMoney = self.master?.money ?? 0;
            if(inclDeploys) {
                var dplc = self.GetComponent<Deployable>();
                if(dplc) newMoney += dplc.ownerMaster?.money ?? 0;
            }
            if(cpt.cachedMoney != newMoney || cpt.cachedDiff != Run.instance.difficultyCoefficient) {
                cpt.cachedMoney = newMoney;
                cpt.cachedDiff = Run.instance.difficultyCoefficient;
                UpdateGGBuff(self);
            }
        }

        void UpdateGGBuff(CharacterBody cb) {
            var cpt = cb.GetComponent<GoldenGunComponent>();
            int tgtBuffStacks = (cpt.cachedIcnt<1) ? 0 : Mathf.Clamp(Mathf.FloorToInt(cpt.cachedMoney / (Run.instance.GetDifficultyScaledCost(goldAmt) * Mathf.Pow(goldReduc, cpt.cachedIcnt - 1)) * 100f), 0, 100);
                
            int currBuffStacks = cb.GetBuffCount(goldenGunBuff);
            if(tgtBuffStacks != currBuffStacks)
                cb.SetBuffCount(goldenGunBuff.buffIndex, tgtBuffStacks);
        }

        private void IL_CBTakeDamage(ILContext il) {
            var c = new ILCursor(il);

            bool ILFound;

            int locDmg = -1;
            ILFound = c.TryGotoNext(
                x=>x.MatchLdarg(1),
                x=>x.MatchLdfld<DamageInfo>("damage"),
                x=>x.MatchStloc(out locDmg));
            
            if(!ILFound) {
                ilFailed = true;
                ClassicItemsPlugin._logger.LogError("Failed to apply Golden Gun IL patch (damage var read), item will not work; target instructions not found");
                return;
            }

            int locChrm = -1;
            ILFound = c.TryGotoNext(
                x=>x.MatchLdloc(out locChrm),
                x=>x.MatchCallOrCallvirt<CharacterMaster>("get_inventory"),
                x=>x.MatchLdsfld("RoR2.RoR2Content/Items", "Crowbar"))
            && c.TryGotoPrev(MoveType.After,
                x=>x.OpCode == OpCodes.Brfalse);

            if(ILFound) {
                c.Emit(OpCodes.Ldloc, locChrm);
                c.Emit(OpCodes.Ldloc, locDmg);
                c.EmitDelegate<Func<CharacterMaster,float,float>>((chrm, origdmg) => {
                    var icnt = GetCount(chrm.inventory);
                    if(icnt == 0) return origdmg;
                    var moneyFac = chrm.money;
                    if(inclDeploys) {
                        var dplc = chrm.GetComponent<Deployable>();
                        if(dplc) moneyFac += dplc.ownerMaster.money;
                    }
                    var moneyCoef = moneyFac / (Run.instance.GetDifficultyScaledCost(goldAmt) * Mathf.Pow(goldReduc, icnt - 1));
                    return origdmg * (1 + Mathf.Lerp(0,damageBoost,moneyCoef));
                });
                c.Emit(OpCodes.Stloc, locDmg);
            } else {
                ilFailed = true;
                ClassicItemsPlugin._logger.LogError("Failed to apply Golden Gun IL patch (damage var write), item will not work; target instructions not found");
                return;
            }
        }
    }

    public class GoldenGunComponent : MonoBehaviour {
        public uint cachedMoney = 0u;
        public int cachedIcnt = 0;
        public float cachedDiff = 0f;
    }
}
