using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.Logic;
using Oasys.Common.Menu;
using Oasys.SDK.Tools;
using SharpDX;
using SWRevamped.Base;
using SWRevamped.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SWRevamped.Champions
{
    internal sealed class CaitQCalc : EffectCalc
    {
        internal int[] BaseDamage = new int[] { 0, 50, 90, 130, 170, 210 };
        internal float[] ADScaling = new float[] { 0, 1.25F, 1.45F, 1.65F, 1.85F, 2.05F };

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel >= 1)
            {
                damage = BaseDamage[Getter.QLevel];
                damage += ADScaling[Getter.QLevel] * Getter.TotalAD;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage);
            }
            return damage;
        }
    }

    internal sealed class CaitECalc : EffectCalc
    {
        internal int[] BaseDamage = new int[] { 0, 80, 130, 180, 230, 280 };
        internal float APScaling = 0.8F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.ELevel >= 1)
            {
                damage = BaseDamage[Getter.ELevel];
                damage += APScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class CaitRCalc : EffectCalc
    {
        internal int[] BaseDamage = new int[] { 0, 300, 525, 750 };
        internal float ADScaling = 2F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel >= 1)
            {
                damage += BaseDamage[Getter.RLevel];
                damage += ADScaling * Getter.BonusAD;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage);
            }
            return damage;
        }
    }

    internal sealed class Caitlyn : ChampionModule
    {

    }
}