﻿using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using R2API;

namespace ThinkInvisible.ClassicItems {
    public class PhotonJetpack : Item<PhotonJetpack> {
        public override string displayName => "Photon Jetpack";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});
        public override bool itemIsAIBlacklisted {get; protected set;} = true;

        public BuffDef photonFuelBuff {get;private set;}

        [AutoConfigRoOSlider("{0:N1} s", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Time in seconds that jump must be released before Photon Jetpack fuel begins recharging.",AutoConfigFlags.PreventNetMismatch,0f,float.MaxValue)]
        public float rchDelay {get;private set;} = 1.0f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Seconds of Photon Jetpack fuel recharged per second realtime.",AutoConfigFlags.PreventNetMismatch,0f,float.MaxValue)]
        public float rchRate {get;private set;} = 1.0f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Seconds of Photon Jetpack fuel capacity at first stack.",AutoConfigFlags.PreventNetMismatch,0f,float.MaxValue)]
        public float baseFuel {get;private set;} = 1.6f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Seconds of Photon Jetpack fuel capacity per additional stack.",AutoConfigFlags.PreventNetMismatch,0f,float.MaxValue)]
        public float stackFuel {get;private set;} = 1.6f;

        [AutoConfigRoOSlider("{0:N2}g", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Multiplier for gravity reduction while Photon Jetpack is active. Effectively the thrust provided by the jetpack -- 0 = no effect, 1 = anti-grav, 2 = negative gravity, etc.",AutoConfigFlags.PreventNetMismatch,0f,float.MaxValue)]
        public float gravMod {get;private set;} = 1.2f;

        [AutoConfigRoOSlider("{0:N2}g", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Added to Photon Jetpack's GravMod while the character is falling (negative vertical velocity) to assist in stopping falls.",AutoConfigFlags.PreventNetMismatch,0f,float.MaxValue)]
        public float fallBoost {get;private set;} = 2.0f;

        public enum ExtraJumpInteractionType {
            Simultaneous,
            UseJumpsFirst,
            UseJetpackFirst,
            ConvertJumpsToFuel
        }

        [AutoConfigRoOChoice()]
        [AutoConfig("What to do when both Photon Jetpack and extra jumps may be used.", AutoConfigFlags.PreventNetMismatch)]
        public ExtraJumpInteractionType extraJumpInteraction { get; private set; } = ExtraJumpInteractionType.UseJumpsFirst;

        [AutoConfig("If ExtraJumpInteraction is ConvertJumpsToFuel: seconds of fuel to provide per extra jump.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float jumpFuel { get; private set; } = 0.8f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "No hands.";
        protected override string GetDescString(string langid = null) {
            string desc = "Grants <style=cIsUtility>" + baseFuel.ToString("N1") + " second" + NPlur(baseFuel, 1) + "</style>";
            if(stackFuel > 0f) desc += "<style=cStack>(+" + stackFuel.ToString("N1") + " seconds per stack)</style>";
            desc += " of <style=cIsUtility>flight</style> at <style=cIsUtility>" + gravMod.ToString("N1") + "·g</style> <style=cStack>(+" + fallBoost.ToString("N1") + "·g while falling)</style>, usable once you have no double jumps remaining. Fuel <style=cIsUtility>recharges</style> at <style=cIsUtility>" + Pct(rchRate) + " speed</style> after a <style=cIsUtility>delay</style> of <style=cIsUtility>" + rchDelay.ToString("N0") + " second" + NPlur(rchDelay) + "</style>.";
            return desc;
        }
        protected override string GetLoreString(string langid = null) => "Order: Photon Jetpack\n\nTracking Number: 662***********\nEstimated Delivery: 12/29/2056\nShipping Method: High Priority/Fragile\nShipping Address: Floor 77, Corp INC, Jupiter\nShipping Details:\n\nHere it is, sir. Please just be careful; I'm not quite sure what you are planning, but I don't think the jetpack lasts long enough to fly over to the other office. 77 floors is a long way to fall, sir.";

        public PhotonJetpack() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/photonjetpack_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/PhotonJetpack.prefab");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            photonFuelBuff = ScriptableObject.CreateInstance<BuffDef>();
            photonFuelBuff.buffColor = Color.cyan;
            photonFuelBuff.canStack = true;
            photonFuelBuff.isDebuff = false;
            photonFuelBuff.name = modInfo.shortIdentifier + "PhotonFuel";
            photonFuelBuff.iconSprite = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/PhotonJetpack_icon.png");

            ContentAddition.AddBuffDef(photonFuelBuff);
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
        }

        public override void Install() {
            base.Install();
            On.RoR2.CharacterBody.OnInventoryChanged += On_CBInventoryChanged;
            On.RoR2.CharacterBody.FixedUpdate += On_CBFixedUpdate;
            On.EntityStates.GenericCharacterMain.ProcessJump += GenericCharacterMain_ProcessJump;
            ConfigEntryChanged += Evt_ConfigEntryChanged;
        }
        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.CharacterBody.OnInventoryChanged -= On_CBInventoryChanged;
            On.RoR2.CharacterBody.FixedUpdate -= On_CBFixedUpdate;
            On.EntityStates.GenericCharacterMain.ProcessJump -= GenericCharacterMain_ProcessJump;
            ConfigEntryChanged -= Evt_ConfigEntryChanged;
        }

        private void Evt_ConfigEntryChanged(object sender, AutoConfigUpdateActionEventArgs args) {
            if(args.target.boundProperty.Name == nameof(baseFuel)
            || args.target.boundProperty.Name == nameof(stackFuel)
            || args.target.boundProperty.Name == nameof(extraJumpInteraction))
                AliveList().ForEach(cm => {
                    if(cm.hasBody) UpdatePhotonFuel(cm.GetBody());
                });
        }

        private void GenericCharacterMain_ProcessJump(On.EntityStates.GenericCharacterMain.orig_ProcessJump orig, EntityStates.GenericCharacterMain self) {
            int origJumps = 0;
            bool doJumpOverride = false;
            if(self.hasCharacterMotor && self.characterBody && !self.characterMotor.isGrounded &&
                (
                    (
                        extraJumpInteraction == ExtraJumpInteractionType.UseJetpackFirst
                        && (self.characterBody.GetComponent<PhotonJetpackComponent>()?.fuel ?? 0) > 0
                    ) || (
                        extraJumpInteraction == ExtraJumpInteractionType.ConvertJumpsToFuel
                        && GetCount(self.characterBody) > 0
                    )
                )
            ) {
                doJumpOverride = true;
                origJumps = self.characterMotor.jumpCount;
                self.characterMotor.jumpCount = self.characterBody.maxJumpCount;
            }
            orig(self);
            if(doJumpOverride)
                self.characterMotor.jumpCount = origJumps;
        }

        private void On_CBFixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self) {
            orig(self);

            var cpt = self.GetComponent<PhotonJetpackComponent>();

            if(!self.characterMotor || !cpt || cpt.fuelCap == 0) return;

            uint oldstate = cpt.flyState;
            uint newstate = oldstate;

            bool jumpDn = self.inputBank.jump.down;
            bool isGrounded = self.characterMotor.isGrounded;
            bool hasJumps = self.characterMotor.jumpCount < self.maxJumpCount;
            bool isJumpIndependent = extraJumpInteraction != ExtraJumpInteractionType.UseJumpsFirst;

            if(isGrounded || (hasJumps && !isJumpIndependent)) {
                newstate = 0;
            } else if(!hasJumps || isJumpIndependent) {
                if(jumpDn && oldstate == 0) newstate = 1;
                if(!jumpDn && oldstate == 1) newstate = 2;
            }

            if(newstate == 2 && jumpDn) {
                if(cpt.fuel > 0.0f) {
                    cpt.cooldown = rchDelay;
                    cpt.fuel -= Time.fixedDeltaTime;
                    self.characterMotor.velocity.y -= Time.fixedDeltaTime * Physics.gravity.y
                    * (gravMod + ((self.characterMotor.velocity.y < 0) ? fallBoost : 0f));
                }
                if(cpt.fuel <= 0.0f) {
                    newstate = 1;
                }
            } else {
                cpt.cooldown -= Time.fixedDeltaTime;
                if(cpt.cooldown < 0.0f)
                    cpt.fuel = Mathf.Min(cpt.fuel + rchRate * Time.fixedDeltaTime, cpt.fuelCap);
            }

            cpt.flyState = newstate;

            int tgtFuelStacks = Mathf.CeilToInt(cpt.fuel/cpt.fuelCap*100f);
            int currFuelStacks = self.GetBuffCount(photonFuelBuff);
            if(tgtFuelStacks != currFuelStacks)
                self.SetBuffCount(photonFuelBuff.buffIndex, tgtFuelStacks);
        }

        private void On_CBInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self) {
            orig(self);
            UpdatePhotonFuel(self);
        }

        private void UpdatePhotonFuel(CharacterBody tgt) {
            var cpt = tgt.GetComponent<PhotonJetpackComponent>();
            if(!cpt) cpt = tgt.gameObject.AddComponent<PhotonJetpackComponent>();
                
            int stacks = GetCount(tgt);
            cpt.fuelCap = stacks>0 ? baseFuel + stackFuel * (stacks-1) : 0;
            if(stacks > 0 && extraJumpInteraction == ExtraJumpInteractionType.ConvertJumpsToFuel)
                cpt.fuelCap += (tgt.maxJumpCount - 1) * jumpFuel;
            if(cpt.fuel>cpt.fuelCap) cpt.fuel=cpt.fuelCap;
            if(cpt.fuelCap == 0)
                tgt.SetBuffCount(photonFuelBuff.buffIndex, 0);
        }
    }

    public class PhotonJetpackComponent : MonoBehaviour {
        public float fuelCap = 0f;
        public float fuel = 0f;
        public uint flyState = 0;
        public float cooldown = 0f;
    }
}
