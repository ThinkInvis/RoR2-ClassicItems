using UnityEngine;
using RoR2.Skills;
using static TILER2.MiscUtil;
using R2API.Utils;
using EntityStates.Huntress;
using RoR2;
using R2API;
using RoR2.Projectile;

namespace ThinkInvisible.ClassicItems {
    public class HuntressRain2 : ScepterSkill {
        private static GameObject projReplacer;
        public override SkillDef myDef {get; protected set;}

        public override string oldDescToken {get; protected set;}
        public override string newDescToken {get; protected set;}
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: +50% radius and duration. Inflicts burn.</color>";
        
        public override string targetBody => "HuntressBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        internal override void SetupAttributes() {
            var oldDef = Resources.Load<SkillDef>("skilldefs/huntressbody/HuntressBodyArrowRain");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPHUNTRESS_RAINNAME";
            newDescToken = "CLASSICITEMS_SCEPHUNTRESS_RAINDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Burning Rain";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Resources.Load<Sprite>("@ClassicItems:Assets/ClassicItems/icons/scepter/huntress_arrowrainicon.png");

            LoadoutAPI.AddSkillDef(myDef);

            projReplacer = Resources.Load<GameObject>("prefabs/projectiles/HuntressArrowRain").InstantiateClone("CIScepHuntressRain");
            projReplacer.GetComponent<ProjectileDamage>().damageType |= DamageType.IgniteOnHit;
            projReplacer.GetComponent<ProjectileDotZone>().lifetime *= 1.5f;
            projReplacer.transform.localScale = new Vector3(22.5f, 15f, 22.5f);
            var fx = projReplacer.transform.Find("FX");
            var afall = fx.Find("ArrowsFalling");
            afall.GetComponent<ParticleSystemRenderer>().material.SetVector("_TintColor", new Vector4(3f, 0.1f, 0.04f, 1.5f));
            var aimp = fx.Find("ImpaledArrow");
            aimp.GetComponent<ParticleSystemRenderer>().material.SetVector("_TintColor", new Vector4(3f, 0.1f, 0.04f, 1.5f));
            var radInd = fx.Find("RadiusIndicator");
            radInd.GetComponent<MeshRenderer>().material.SetVector("_TintColor", new Vector4(3f, 0.1f, 0.04f, 1.25f));
            var flash = fx.Find("ImpactFlashes");
            var psm = flash.GetComponent<ParticleSystem>().main;
            psm.startColor = new Color(1f, 0.7f, 0.4f);
            flash.GetComponent<ParticleSystemRenderer>().material.SetVector("_TintColor", new Vector4(3f, 0.1f, 0.04f, 1.5f));
            var flashlight = flash.Find("Point Light");
            flashlight.GetComponent<Light>().color = new Color(1f, 0.5f, 0.3f);
            flashlight.GetComponent<Light>().range = 15f;
            flashlight.gameObject.SetActive(true);

            ProjectileCatalog.getAdditionalEntries += (list) => list.Add(projReplacer);
        }

        internal override void LoadBehavior() {
            On.EntityStates.Huntress.ArrowRain.DoFireArrowRain += On_ArrowRain_DoFireArrowRain;
        }

        internal override void UnloadBehavior() {
            On.EntityStates.Huntress.ArrowRain.DoFireArrowRain -= On_ArrowRain_DoFireArrowRain;
        }

        private void On_ArrowRain_DoFireArrowRain(On.EntityStates.Huntress.ArrowRain.orig_DoFireArrowRain orig, ArrowRain self) {
            var origPrefab = ArrowRain.projectilePrefab;
            if(Scepter.instance.GetCount(self.outer.commonComponents.characterBody) > 0) ArrowRain.projectilePrefab = projReplacer;
            orig(self);
            ArrowRain.projectilePrefab = origPrefab;
        }
    }
}