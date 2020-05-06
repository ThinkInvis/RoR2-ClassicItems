using RoR2;
using UnityEngine;
using BepInEx.Configuration;
using static ThinkInvisible.ClassicItems.MiscUtil;
using System.Collections.ObjectModel;

namespace ThinkInvisible.ClassicItems {
    public class PhotonJetpack : Item<PhotonJetpack> {
        public override string displayName => "Photon Jetpack";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});
        public override bool itemAIBDefault => true;

        public BuffIndex photonFuelBuff {get;private set;}

        [AutoItemCfg("Time in seconds that jump must be released before Photon Jetpack fuel begins recharging.",default,0f,float.MaxValue)]
        public float rchDelay {get;private set;} = 1.0f;
        [AutoItemCfg("Seconds of Photon Jetpack fuel recharged per second realtime.",default,0f,float.MaxValue)]
        public float rchRate {get;private set;} = 1.0f;
        [AutoItemCfg("Seconds of Photon Jetpack fuel capacity at first stack.",default,0f,float.MaxValue)]
        public float baseFuel {get;private set;} = 1.6f;
        [AutoItemCfg("Seconds of Photon Jetpack fuel capacity per additional stack.",default,0f,float.MaxValue)]
        public float stackFuel {get;private set;} = 1.6f;
        [AutoItemCfg("Multiplier for gravity reduction while Photon Jetpack is active. Effectively the thrust provided by the jetpack -- 0 = no effect, 1 = anti-grav, 2 = negative gravity, etc.",default,0f,float.MaxValue)]
        public float gravMod {get;private set;} = 1.2f;
        [AutoItemCfg("Added to Photon Jetpack's GravMod while the character is falling (negative vertical velocity) to assist in stopping falls.",default,0f,float.MaxValue)]
        public float fallBoost {get;private set;} = 2.0f;

        public override void SetupAttributesInner() {
            RegLang(
            	"No hands.",
            	"Grants <style=cIsUtility>" + baseFuel.ToString("N1") + " second" + NPlur(baseFuel, 1) + "</style> <style=cStack>(+" + stackFuel.ToString("N1") +" s per stack)</style> of <style=cIsUtility>flight</style> at <style=cIsUtility>" + gravMod.ToString("N1") + "g</style> <style=cStack>(+" + fallBoost.ToString("N1") + "g while falling)</style>, usable once you have no double jumps remaining. Fuel <style=cIsUtility>recharges</style> at <style=cIsUtility>" + Pct(rchRate) + " speed</style> after a <style=cIsUtility>delay</style> of <style=cIsUtility>" + rchDelay.ToString("N0") + " second" + NPlur(rchDelay) + "</style>.",
            	"A relic of times long past (ClassicItems mod)");
        }

        public override void SetupBehaviorInner() {
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
