using R2API.Utils;
using RoR2;
using UnityEngine;
using BepInEx.Configuration;
using static ThinkInvisible.ClassicItems.MiscUtil;
using System.Collections.Generic;

namespace ThinkInvisible.ClassicItems
{
    public class PhotonJetpack : ItemBoilerplate
    {
        public override string itemCodeName {get;} = "PhotonJetpack";

        public BuffIndex photonFuelBuff {get;private set;}

        private ConfigEntry<float> cfgCooldown;
        private ConfigEntry<float> cfgRecharge;
        private ConfigEntry<float> cfgFuel;
        private ConfigEntry<float> cfgStackFuel;
        private ConfigEntry<float> cfgGravMod;
        private ConfigEntry<float> cfgFallBoost;

        public float rchDelay {get;private set;}
        public float rchRate {get;private set;}
        public float baseFuel {get;private set;}
        public float stackFuel {get;private set;}
        public float gravMod {get;private set;}
        public float fallBoost {get;private set;}

        protected override void SetupConfigInner(ConfigFile cfl) {
            cfgCooldown = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "Cooldown"), 1.0f, new ConfigDescription(
                "Time in seconds that jump must be released before Photon Jetpack fuel begins recharging.",
                new AcceptableValueRange<float>(0f,float.MaxValue)));
            cfgRecharge = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "Recharge"), 1.0f, new ConfigDescription(
                "Seconds of Photon Jetpack fuel recharged per second realtime.",
                new AcceptableValueRange<float>(0f,float.MaxValue)));
            cfgFuel = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "Fuel"), 1.6f, new ConfigDescription(
                "Seconds of Photon Jetpack fuel capacity at first stack.",
                new AcceptableValueRange<float>(0f,float.MaxValue)));
            cfgStackFuel = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "StackFuel"), 1.6f, new ConfigDescription(
                "Seconds of Photon Jetpack fuel capacity per additional stack.",
                new AcceptableValueRange<float>(0f,float.MaxValue)));
            cfgGravMod = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "GravMod"), 1.2f, new ConfigDescription(
                "Multiplier for gravity reduction while Photon Jetpack is active. Effectively the thrust provided by the jetpack -- 0 = no effect, 1 = anti-grav, 2 = negative gravity, etc.",
                new AcceptableValueRange<float>(0f,float.MaxValue)));
            cfgFallBoost = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "FallBoost"), 2.0f, new ConfigDescription(
                "Added to Photon Jetpack's GravMod while the character is falling (negative vertical velocity) to assist in stopping falls.",
                new AcceptableValueRange<float>(0f,float.MaxValue)));

            rchDelay = cfgCooldown.Value;
            rchRate = cfgRecharge.Value;
            baseFuel = cfgFuel.Value;
            stackFuel = cfgStackFuel.Value;
            gravMod = cfgGravMod.Value;
            fallBoost = cfgFallBoost.Value;
        }
        
        protected override void SetupAttributesInner() {
            itemAIBDefault = true;

            modelPathName = "photonjetpackcard.prefab";
            iconPathName = "photonjetpack_icon.png";
            RegLang("Photon Jetpack",
            	"No hands.",
            	"Grants <style=cIsUtility>" + baseFuel.ToString("N1") + " second" + nplur(baseFuel, 1) + "</style> <style=cStack>(+" + stackFuel.ToString("N1") +" s per stack)</style> of <style=cIsUtility>flight</style> at <style=cIsUtility>" + gravMod.ToString("N1") + "g</style> <style=cStack>(+" + fallBoost.ToString("N1") + "g while falling)</style>, usable once you have no double jumps remaining. Fuel <style=cIsUtility>recharges</style> at <style=cIsUtility>" + pct(rchRate) + " speed</style> after a <style=cIsUtility>delay</style> of <style=cIsUtility>" + rchDelay.ToString("N0") + " second" + nplur(rchDelay) + "</style>.",
            	"A relic of times long past (ClassicItems mod)");
            _itemTags = new List<ItemTag>{ItemTag.Utility};
            itemTier = ItemTier.Tier3;
        }

        protected override void SetupBehaviorInner() {
            var PhotonJetpackBuff = new R2API.CustomBuff(new BuffDef {
                buffColor = Color.cyan,
                canStack = true,
                isDebuff = false,
                name = "PhotonFuel",
                iconPath = "@ClassicItems:Assets/ClassicItems/icons/" + iconPathName
            });
            photonFuelBuff = R2API.BuffAPI.Add(PhotonJetpackBuff);

            On.RoR2.CharacterBody.OnInventoryChanged += On_CBInventoryChanged;
            On.RoR2.CharacterBody.FixedUpdate += On_CBFixedUpdate;
        }

        private void On_CBFixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self) {
            orig(self);

            var cpt = self.GetComponent<PhotonJetpackComponent>();

            if(!self.characterMotor || !cpt || cpt.fuelCap == 0) return;

            uint oldstate = cpt.flyState;

            //0: on ground, or midair with jumps remaining
            //1: in air, no jumps remaining, space is held from last jump/from running out of fuel
            //2: in air, no jumps remaining, space may be held but has been released at least once since the last state=1

            bool jumpDn = self.inputBank.jump.down;
            bool hasJumps = self.characterMotor.jumpCount < self.maxJumpCount;
            bool isGrounded = self.characterMotor.isGrounded;

            uint newstate = oldstate;
            if(isGrounded || hasJumps) {
                newstate = 0;
            } else if(!hasJumps) {
                if(jumpDn && oldstate == 0) newstate = 1;
                if(!jumpDn && oldstate == 1) newstate = 2;
            }

            //float photonFuelCap = cpt.fuelCap;
                
            if(newstate == 2 && jumpDn) {
                //float fuel = (float)cPl.VGet(self, "photonJetpackFuel", 0.0f);
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
                    cpt.fuel = Mathf.Min(cpt.fuel+rchRate * Time.fixedDeltaTime, cpt.fuelCap);
            }

            cpt.flyState = newstate;

            int tgtFuelStacks = Mathf.CeilToInt(cpt.fuel/cpt.fuelCap*100f);
            int currFuelStacks = self.GetBuffCount(photonFuelBuff);
            if(tgtFuelStacks != currFuelStacks)
                self.SetBuffCount(photonFuelBuff, tgtFuelStacks);
        }

        private void On_CBInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self) {
            orig(self);
            var cpt = self.GetComponent<PhotonJetpackComponent>();
            if(!cpt) cpt = self.gameObject.AddComponent<PhotonJetpackComponent>();
                
            int stacks = GetCount(self);
            cpt.fuelCap = stacks>0 ? baseFuel + stackFuel * (stacks-1) : 0;
            if(cpt.fuel>cpt.fuelCap) cpt.fuel=cpt.fuelCap;
            if(cpt.fuelCap == 0)
                self.SetBuffCount(photonFuelBuff, 0);
        }
    }

    public class PhotonJetpackComponent : MonoBehaviour {
        public float fuelCap = 0f;
        public float fuel = 0f;
        public uint flyState = 0;
        public float cooldown = 0f;
    }
}
