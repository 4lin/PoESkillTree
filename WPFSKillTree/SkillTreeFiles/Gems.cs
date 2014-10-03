﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using POESKillTree.Model;
using Item = POESKillTree.ViewModels.ItemAttributes.Item;
using Mod = POESKillTree.ViewModels.ItemAttributes.Item.Mod;
using AttackSkill = POESKillTree.SkillTreeFiles.Compute.AttackSkill;
using DamageForm = POESKillTree.SkillTreeFiles.Compute.DamageForm;
using DamageNature = POESKillTree.SkillTreeFiles.Compute.DamageNature;
using DamageSource = POESKillTree.SkillTreeFiles.Compute.DamageSource;
using WeaponHand = POESKillTree.SkillTreeFiles.Compute.WeaponHand;
using WeaponType = POESKillTree.SkillTreeFiles.Compute.WeaponType;
using Weapon = POESKillTree.SkillTreeFiles.Compute.Weapon;

namespace POESKillTree.SkillTreeFiles
{
    /* Level of support:
     *     None = Gem is being completely ignored.
     *     Unknown = Gem wasn't tested and it doesn't have DB entry, so its statistics are probably incorrect (in +Level to Gems items they are for sure)
     *     Partial = Gem was partialy tested, but it doesn't have DB entry, so its statistics should be correct (except when used in items with +Level to Gems).
     *     Incomplete = Gem was tested, but DB entries are incomplete, so statistics at certain level could be incorrect.
     *     Full = Gem was tested and it has DB entry. It should show correct statistics or all of its modifiers should be applied in full range.
     * 
     * Strength skill gems:
     * ====================
     * Anger                                        None
     * Animate Guardian                             None
     * Cleave                                       Partial
     * Decoy Totem                                  None
     * Determination                                None
     * Devouring Totem                              None
     * Dominating Blow                              Partial
     * Enduring Cry                                 None
     * Flame Totem                                  None
     * Glacial Hammer                               Partial
     * Ground Slam                                  Partial
     * Heavy Strike                                 Full
     * Herald of Ash                                None
     * Immortal Call                                None
     * Infernal Blow                                Partial
     * Leap Slam                                    Incomplete
     * Lightning Strike                             Incomplete
     * Molten Shell                                 Incomplete
     * Molten Strike                                Incomplete
     * Punishment                                   None
     * Purity of Fire                               None
     * Rejuvenation Totem                           None
     * Searing Bond                                 None
     * Shield Charge                                Partial
     * Shockwave Totem                              None
     * Sweep                                        Partial
     * Vitality                                     None
     * Warlord's Mark                               None
     * 
     * Dexterity skill gems:
     * =====================
     * Animate Weapon                               None
     * Arctic Armour                                None
     * Barrage                                      Partial
     * Bear Trap                                    None
     * Blood Rage                                   None
     * Burning Arrow                                Partial
     * Cyclone                                      Partial
     * Desecrate                                    None
     * Detonate Dead                                None
     * Double Strike                                Partial
     * Dual Strike                                  Incomplete
     * Elemental Hit                                Partial
     * Ethereal Knives                              Partial
     * Explosive Arrow                              Partial
     * Fire Trap                                    None
     * Flicker Strike                               Partial
     * Freeze Mine                                  None
     * Frenzy                                       Partial
     * Grace                                        None
     * Haste                                        None
     * Hatred                                       None
     * Herald of Ice                                Incomplete
     * Ice Shot                                     Partial
     * Lightning Arrow                              Partial
     * Poacher's Mark                               None
     * Poison Arrow                                 Partial
     * Projectile Weakness                          None
     * Puncture                                     Partial
     * Purity of Ice                                None
     * Rain of Arrows                               Partial
     * Reave                                        Incomplete
     * Smoke Mine                                   None
     * Spectral Throw                               Partial
     * Split Arrow                                  Incomplete
     * Temporal Chains                              None
     * Tornado Shot                                 Partial
     * Viper Strike                                 Partial
     * Whirling Blades                              Partial
     *
     * Intelligence skill gems:
     * ========================
     * Arc                                          Partial
     * Arctic Breath                                Partial
     * Assassin's Mark                              None
     * Ball Lightning                               Partial
     * Bone Offering                                None
     * Clarity                                      None
     * Cold Snap                                    Partial
     * Conductivity                                 None
     * Conversion Trap                              None
     * Convocation                                  None
     * Critical Weakness                            None
     * Discharge                                    None
     * Discipline                                   None
     * Elemental Weakness                           None
     * Enfeeble                                     None
     * Fireball                                     Full
     * Firestorm                                    Partial
     * Flameblast                                   Partial
     * Flame Surge                                  Partial
     * Flammability                                 None
     * Flesh Offering                               None
     * Freezing Pulse                               Partial
     * Frost Wall                                   Partial
     * Frostbite                                    None
     * Glacial Cascade                              Partial
     * Ice Nova                                     Partial
     * Ice Spear                                    Partial
     * Incinerate                                   Partial
     * Lightning Trap                               None
     * Lightning Warp                               Incomplete
     * Power Siphon                                 Incomplete
     * Purity of Elements                           None
     * Purity of Lightning                          None
     * Raise Spectre                                None
     * Raise Zombie                                 None
     * Righteous Fire                               None
     * Shock Nova                                   Partial
     * Spark                                        Partial
     * Storm Call                                   Partial
     * Summon Raging Spirit                         None
     * Summon Skeletons                             None
     * Tempest Shield                               Incomplete
     * Vulnerability                                None
     * Wrath                                        None
     * 
     * Support gems:
     * =============
     * Added Chaos Damage                           Partial
     * Added Cold Damage                            Partial
     * Added Fire Damage                            Partial
     * Added Lightning Damage                       Partial
     * Additional Accuracy                          Partial
     * Blind                                        None
     * Block Chance Reduction                       None
     * Blood Magic                                  None
     * Cast on Critical Strike                      None
     * Cast on Death                                None
     * Cast on Melee Kill                           None
     * Cast when Damage Taken                       None
     * Cast when Stunned                            None
     * Chain                                        Partial
     * Chance to Flee                               None
     * Chance to Ignite                             None
     * Cold Penetration                             None
     * Cold to Fire                                 Partial
     * Concentrated Effect                          Partial
     * Culling Strike                               None
     * Curse on Hit                                 None
     * Elemental Proliferation                      None
     * Empower                                      None
     * Endurance Charge on Melee Stun               None
     * Enhance                                      None
     * Enlighten                                    None
     * Faster Attacks                               Partial
     * Faster Casting                               Partial
     * Faster Projectiles                           Partial
     * Fire Penetration                             None
     * Fork                                         Partial
     * Generosity                                   None
     * Greater Multiple Projectiles                 Partial
     * Increased Area of Effect                     None
     * Increased Burning Damage                     None
     * Increased Critical Damage                    Partial
     * Increased Critical Strikes                   Partial
     * Increased Duration                           None
     * Iron Grip                                    Partial
     * Iron Will                                    Partial
     * Item Quantity                                None
     * Item Rarity                                  None
     * Knockback                                    None
     * Lesser Multiple Projectiles                  Partial
     * Life Gain on Hit                             None
     * Life Leech                                   None
     * Lightning Penetration                        None
     * Mana Leech                                   None
     * Melee Damage on Full Life                    Partial
     * Melee Physical Damage                        Partial
     * Melee Splash                                 Partial
     * Minion and Totem Elemental Resistance        None
     * Minion Damage                                None
     * Minion Life                                  None
     * Minion Speed                                 None
     * Multiple Traps                               None
     * Multistrike                                  Partial
     * Physical Projectile Attack Damage            Partial
     * Pierce                                       None
     * Point Blank                                  Partial
     * Power Charge On Critical                     None
     * Ranged Attack Totem                          None
     * Reduced Duration                             None
     * Reduced Mana                                 None
     * Remote Mine                                  None
     * Slower Projectiles                           Partial
     * Spell Echo                                   Partial
     * Spell Totem                                  None
     * Stun                                         None
     * Trap                                         None
     * Weapon Elemental Damage                      Partial
     */
    public class Gems
    {
        class Gem
        {
            // Defines attribute value progression along the gem levels.
            internal Dictionary<string, Value> PerLevel;
            // Defines attribute value progression along the gem quality.
            internal Dictionary<string, Value> PerQuality;
            // Defines requirement of specific hand as source of damage.
            internal WeaponHand RequiredHand = WeaponHand.Any;
            // Defines requirement of specific weapon type as source of damage.
            internal WeaponType RequiredWeapon = WeaponType.Any;
            // Defines whether skill requires shield being equipped.
            internal bool RequiresEquippedShield = false;
            // Defines which form of skill should be ignored.
            internal DamageForm ExcludeForm = DamageForm.Any;
            // Defines which forms gem does not support.
            internal DamageForm ExcludeFormSupport = DamageForm.Any;
            // Defines which damage source nature of skill should be ignored.
            internal DamageSource ExcludeSource = DamageSource.Any;
            // Defines form which should be included to skill.
            internal DamageForm IncludeForm = DamageForm.Any;
            // Defines number of hits skill does per single attack.
            internal float HitsPerAttack = 1;
            // Defines whether skill strikes with both weapons at once instead of alternating weapons while dual wielding.
            internal bool StrikesWithBothWeapons = false;

            // Returns attributes of gem which have defined values for specified level.
            internal AttributeSet AttributesAtLevel(float level)
            {
                AttributeSet attrs = new AttributeSet();

                if (PerLevel != null)
                    foreach (var attr in PerLevel)
                    {
                        List<float> value = attr.Value.ValueAt(level);
                        if (value != null)
                            attrs.Add(attr.Key, value);
                    }

                return attrs;
            }

            // Returns attributes of gem which have defined values for specified quality.
            internal AttributeSet AttributesAtQuality(float quality)
            {
                AttributeSet attrs = new AttributeSet();

                if (PerQuality != null)
                    foreach (var attr in PerQuality)
                    {
                        List<float> value = attr.Value.ValueAt(quality);
                        if (value != null)
                            attrs.Add(attr.Key, value);
                    }

                return attrs;
            }
        }

        class Linear : Value
        {
            float A;
            float B;
            Rounding Style = Rounding.None;

            // f(level) = A * level + B
            internal Linear(float a, float b)
            {
                A = a;
                B = b;
            }

            internal Linear(float a, float b, Rounding style)
            {
                A = a;
                B = b;
                Style = style;
            }

            // Returns value for specified level, based on linear equation with defined coefficients.
            override internal List<float> ValueAt(float level)
            {
                return new List<float> { Round(Style, A * level + B) };
            }
        }

        class RangeMap : Value
        {
            internal class Range
            {
                internal int From;
                internal int To;
                internal float[] Fixed;
                internal Value Expr;

                // Return true if range containes specified level, false otherwise.
                internal bool Contains(float level)
                {
                    return level >= From && level <= To;
                }

                // Returns fixed or linear/table value at specified level.
                internal List<float> ValueAt(float level)
                {
                    return Expr == null ? new List<float>(Fixed) : (Expr is Table ? Expr.ValueAt(level - From + 1) : Expr.ValueAt(level));
                }
            }

            List<Range> Ranges = new List<Range>();

            // Arguments define following ranges: level in range <arg1, arg2> => arg3, level in range <arg4, arg5> => arg6, etc.
            // The value arguments (arg3, arg6, etc.) can be simple values of type int, float, string, or complex values of type Linear or Table.
            internal RangeMap(params object[] arguments)
            {
                if (arguments.Length % 3 > 0) throw new ArgumentException("Invalid number of arguments");

                for (int i = 0; i < arguments.Length; i += 3)
                {
                    Range range;

                    if (arguments[i + 2] is Value)
                        range = new Range { From = (int)arguments[i], To = (int)arguments[i + 1], Expr = (Value)arguments[i + 2] };
                    else
                        range = new Range { From = (int)arguments[i], To = (int)arguments[i + 1], Fixed = ValueOf(arguments[i + 2]) };

                    Ranges.Add(range);
                }
            }

            // Returns value for specified level.
            override internal List<float> ValueAt(float level)
            {
                foreach (Range range in Ranges)
                    if (range.Contains(level))
                        return range.ValueAt(level);

                return null;
            }
        }

        class Table : Value
        {
            internal int Count { get { return Values.Count; } }
            List<float[]> Values = new List<float[]>();

            // When used directly, values are mapped as follows: level 1 => arg1, level 2 => arg2, ..., level N => argN
            // When used as RangeMap value, values are mapped as follows: from => arg1, from + 1 => arg2, ..., to => argN
            internal Table(params object[] arguments)
            {
                for (int i = 0; i < arguments.Length; ++i)
                    Values.Add(ValueOf(arguments[i]));
            }

            // Returns value for specified level or null if value was not found in table.
            override internal List<float> ValueAt(float level)
            {
                if (level < 1 || level > Values.Count) return null;

                return new List<float>(Values[(int)level - 1]);
            }
        }

        abstract class Value {
            // Implementation in derived class must return either defined value or null if value isn't defined for specified level.
            abstract internal List<float> ValueAt(float level);

            static Regex ReRangeValue = new Regex("(\\d+)[-–](\\d+)");

            internal static float[] ValueOf(object any)
            {
                if (any is string)
                {
                    Match m = ReRangeValue.Match((string)any);
                    if (m.Success)
                        return new float[] { float.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture),
                                         float.Parse(m.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture) };

                    return new float[] { float.NaN, float.NaN };
                }
                else if (any is float)
                    return new float[] { (float)any };
                else if (any is int)
                    return new float[] { (int)any };

                return new float[] { float.NaN };
            }
        }

        enum Rounding
        {
            None, TruncToHalf
        }

        class Values : Dictionary<string, Value> { }

        // Returns attributes of gem in item.
        public static AttributeSet AttributesOf(Item gem, Item item)
        {
            AttributeSet attrs = new AttributeSet();

            // Collect gem attributes and modifiers at gem level.
            foreach (var attr in gem.Attributes)
                attrs.Add(attr.Key, new List<float>(attr.Value));
            foreach (Mod mod in gem.Mods)
                attrs.Add(mod.Attribute, new List<float>(mod.Value));

            // Check if gem is in database.
            if (DB.ContainsKey(gem.Name))
            {
                Gem entry = DB[gem.Name];

                // Process +Level modifiers from item.
                float level = 0;
                foreach (Mod mod in item.Mods)
                {
                    if (mod.Attribute == "+# to Level of Gems in this item")
                        level += mod.Value[0];
                    else
                    {
                        Match m = ReGemLevelKeyword.Match(mod.Attribute);
                        if (m.Success && gem.Keywords.Contains(m.Groups[1].Value))
                            level += mod.Value[0];
                    }
                }

                // Override attributes of gem (even not leveled up one).
                AttributeSet overrides = entry.AttributesAtLevel(level + LevelOf(gem));
                attrs.Override(overrides);

                // Override attributes if Quality attributes are defined.
                if (entry.PerQuality != null)
                {
                    float quality = QualityOf(gem);

                    if (quality > 0)
                    {
                        overrides = entry.AttributesAtQuality(quality);
                        attrs.Override(overrides);
                    }
                }
            }

            return attrs;
        }

        // Returns attributes of gem at specified level (Only used in UnitTests).
        public static AttributeSet AttributesOf(string gemName, float level)
        {
            return DB[gemName].AttributesAtLevel(level);
        }

        // Returns true if gem can support attack skill, false otherwise.
        public static bool CanSupport(AttackSkill skill, Item gem)
        {
            if (DB.ContainsKey(gem.Name))
            {
                Gem entry = DB[gem.Name];

                // No support for excluded forms.
                if (entry.ExcludeFormSupport != DamageForm.Any && skill.Nature.Is(entry.ExcludeFormSupport)) return false;
            }

            return true;
        }

        // Returns true if gem can use weapon, false otherwise.
        public static bool CanUse(Item gem, Weapon weapon)
        {
            if (DB.ContainsKey(gem.Name))
            {
                Gem entry = DB[gem.Name];

                if (entry.RequiredHand != WeaponHand.Any && !weapon.Is(entry.RequiredHand))
                    return false;

                if (entry.RequiresEquippedShield && !Compute.IsWieldingShield)
                    return false;

                // Weapon having "Counts as Dual Wielding" mod cannot be used to perform skills that require a two-handed weapon.
                // @see http://pathofexile.gamepedia.com/Wings_of_Entropy
                if (entry.RequiredWeapon != WeaponType.Any && (entry.RequiredWeapon & WeaponType.TwoHandedMelee) != 0
                    && weapon.Attributes.ContainsKey("Counts as Dual Wielding"))
                    return false;
            }

            return true;
        }

        // Returns numbner of hits skill gem does per single attack.
        public static float HitsPerAttackOf(Item gem)
        {
            return DB.ContainsKey(gem.Name) ? DB[gem.Name].HitsPerAttack : 1;
        }

        // Returns true if skill strikes with both weapons at once.
        public static bool IsStrikingWithBothWeaponsAtOnce(Item gem)
        {
            return DB.ContainsKey(gem.Name) && DB[gem.Name].StrikesWithBothWeapons ? true : false;
        }

        // Returns level of gem.
        public static float LevelOf(Item gem)
        {
            return gem.Attributes.ContainsKey("Level: #")
                   ? gem.Attributes["Level: #"][0]
                   : (gem.Attributes.ContainsKey("Level: # (Max)") ? gem.Attributes["Level: # (Max)"][0] : 1);
        }

        // Returns damage nature of gem.
        public static DamageNature NatureOf(Item gem)
        {
            // Implicit nature from keywords.
            DamageNature nature = new DamageNature(gem.Keywords);

            if (nature.Is(DamageSource.Attack))
            {
                // Attacks with melee form implicitly gets melee weapon type.
                if ((nature.Form & DamageForm.Melee) != 0)
                    nature.WeaponType |= WeaponType.Melee;
                // Attacks with ranged weapons implicitly gets projectile form.
                if (nature.Is(WeaponType.Ranged))
                    nature.Form |= DamageForm.Projectile;
            }

            if (DB.ContainsKey(gem.Name))
            {
                Gem entry = DB[gem.Name];

                // Override weapon type requirement if defined.
                if (entry.RequiredWeapon != WeaponType.Any)
                    nature.WeaponType = entry.RequiredWeapon;

                // Ignore form.
                if (entry.ExcludeForm != DamageForm.Any)
                    nature.Form ^= entry.ExcludeForm;

                // Ignore source.
                if (entry.ExcludeSource != DamageSource.Any)
                    nature.Source ^= entry.ExcludeSource;

                // Include form.
                if (entry.IncludeForm != DamageForm.Any)
                    nature.Form |= entry.IncludeForm;
            }

            return nature;
        }

        // Returns quality of gem.
        public static float QualityOf(Item gem)
        {
            return gem.Attributes.ContainsKey("Quality: +#%")
                   ? gem.Attributes["Quality: +#%"][0]
                   : (gem.Attributes.ContainsKey("Quality: +#% (Max)") ? gem.Attributes["Quality: +#% (Max)"][0] : 0);
        }

        // Returns rounded value.
        static float Round(Rounding style, float value)
        {
            switch (style)
            {
                case Rounding.None:
                    return value;

                case Rounding.TruncToHalf:
                    return (float)((int)(value * 2)) / 2f;

                default:
                    throw new NotSupportedException("No such rounding style: " + style);
            }
        }

        // Constant to match any level above first argument of RangeMap triplet.
        const int MaxLevel = int.MaxValue;

        readonly static Regex ReGemLevelClass = new Regex("\\+# to Level of (Strength|Dexterity|Intelligence) Gems in this item");
        readonly static Regex ReGemLevelKeyword = new Regex("\\+# to Level of (.+) Gems in this item");

        readonly static Dictionary<string, Gem> DB = new Dictionary<string, Gem> {
            {
                "Barrage",
                new Gem {
                    RequiredWeapon = WeaponType.Ranged
                }
            }, {
                "Cleave",
                new Gem {
                    PerLevel = new Values {
                        { "#% increased Physical Damage", new Linear(4, -4) }
                    },
                    RequiredWeapon = WeaponType.Axe | WeaponType.Sword,
                    StrikesWithBothWeapons = true
                }
            }, {
                "Cold Snap",
                new Gem {
                    IncludeForm = DamageForm.OnUse
                }
            }, {
                "Cyclone",
                new Gem {
                    PerLevel = new Values {
                        { "#% increased Physical Damage", new Linear(4, -4) },
                        { "#% increased Attack Speed", new Linear(2, -2) }
                    },
                    HitsPerAttack = 2
                }
            }, {
                "Dual Strike",
                new Gem {
                    PerLevel = new Values {
                        { "Mana Cost: #", new RangeMap(1, 5, 8, 6, 10, 9, 11, 15, 10, 16, MaxLevel, 11) },
                        { "#% increased Physical Damage", new Linear(3, -3) }
                    },
                    PerQuality = new Values {
                        { "#% increased Critical Strike Chance", new Linear(4, 0) }
                    },
                    RequiredHand = WeaponHand.DualWielded,
                    RequiredWeapon = WeaponType.Melee,
                    StrikesWithBothWeapons = true
                }
            }, {
                "Fireball",
                new Gem {
                    PerLevel = new Values {
                        { "Mana Cost: #", new RangeMap(1, 12, new Table(5, 6, 7, 8, 10, 11, 13, 15, 18, 20, 22, 23), 13, MaxLevel, new Linear(1, 12)) },
                        { "Deals #-# Fire Damage", new Table("5–10", "7–11", "9–14", "13–19", "17–25", "23–34", "32–48", "44–67", "63–95", "89–133",
                                                             "110–165", "135–203", "157–236", "183–274", "212–318", "245–368", "283–425", "326–489", "358–537", "393–590",
                                                             "431–647", "472–709", "518–776", "567–850", "620–930", "678–1017", "741–1111", "809–1214", "884–1326", "965–1447") }
                    },
                    ExcludeForm = DamageForm.AoE
                }
            }, {
                "Flameblast",
                new Gem {
                    IncludeForm = DamageForm.OnUse
                }
            }, {
                "Firestorm",
                new Gem {
                    IncludeForm = DamageForm.OnUse
                }
            }, {
                "Flicker Strike",
                new Gem {
                    PerLevel = new Values {
                        { "#% increased Physical Damage", new Linear(3, -3) }
                    },
                    IncludeForm = DamageForm.OnUse
                }
            }, {
                "Glacial Hammer",
                new Gem {
                    PerLevel = new Values {
                        { "Mana Cost: #", new RangeMap(1, 9, 11, 10, 15, 12, 16, MaxLevel, 13) },
                        { "#% increased Physical Damage", new Linear(4, -4) },
                        { "#% Chance to Freeze enemies", new RangeMap(1, 12, new Table(15, 16, 17, 18, 19, 20, 21, 21, 22, 22, 23, 23), 13, 15, 24, 16, 18, 25, 19, MaxLevel, 26) }
                    },
                    RequiredWeapon = WeaponType.Mace | WeaponType.Staff
                }
            }, {
                "Ground Slam",
                new Gem {
                    PerLevel = new Values {
                        { "#% increased Physical Damage", new Linear(4, -4) }
                    },
                    RequiredHand = WeaponHand.Main,
                    RequiredWeapon = WeaponType.Mace | WeaponType.Staff
                }
            }, {
                "Heavy Strike",
                new Gem {
                    PerLevel = new Values {
                        { "Mana Cost: #", new RangeMap(1, 9, 8, 10, 15, 9, 16, MaxLevel, 10) },
                        { "#% increased Physical Damage", new Linear(5, -5) },
                    },
                    PerQuality = new Values {
                        // XXX: This might be just plain table like 0.5%, 1%, 2%, 2.5%, 3%, etc., then no rounding is needed at all.
                        { "#% increased Attack Speed", new Linear(0.75f, 0, Rounding.TruncToHalf) }
                    },
                    RequiredWeapon = WeaponType.Mace | WeaponType.Axe | WeaponType.Sword | WeaponType.TwoHandedMelee
                }
            }, {
                "Herald of Ice",
                new Gem {
                    PerLevel = new Values {
                        { "Deals #-# Cold Damage", new Table("15–22", "18-27", "21-32", "25-38", "28-42", "33-49", "37-55", "43-64", "50-75", "55-82",
                                                             "61-91", "67-100", "74-110", "81-121", "89-133", "98-146", "107-161", "117-176", "129-193",
                                                             "141-211", "154-231", "168-253") }
                    },
                    ExcludeSource = DamageSource.Spell,
                    IncludeForm = DamageForm.OnUse
                }
            }, {
                "Leap Slam",
                new Gem {
                    PerLevel = new Values {
                        { "Attacks per Second: #", new RangeMap(1, MaxLevel, 1 / 1.4f) }, // Leap Slam has its own attack time of 1.40 seconds.
                        { "Mana Cost: #", new RangeMap(1, 5, 14, 6, 9, 15, 10, 13, 16, 14, 17, 17, 18, MaxLevel, 18) },
                        { "#% increased Physical Damage", new Linear(4, -4) },
                        { "#% Chance to Knock enemies Back on hit", new RangeMap(1, 9, new Table(10, 12, 14, 16, 18, 20, 21, 22, 23), 10, MaxLevel, 24) }
                    },
                    RequiredHand = WeaponHand.Main,
                    RequiredWeapon = WeaponType.Axe | WeaponType.Mace | WeaponType.Sword | WeaponType.Staff
                }
            }, {
                "Lightning Strike",
                new Gem {
                    PerLevel = new Values {
                        { "Mana Cost: #", new RangeMap(1, 9, 9, 10, MaxLevel, 10) },
                        { "#% increased Physical Damage", new Linear(3, -3) }
                    },
                    ExcludeForm = DamageForm.Projectile
                }
            }, {
                "Lightning Warp",
                new Gem {
                    PerLevel = new Values {
                        { "Mana Cost: #", new Table(26, 28, 30, 32, 33, 36, 37, 39, 41, 42, 44, 45, 46, 47, 48, 49, 49, 50, 51, 51, 52, 52) },
                        { "Deals #–# Lightning Damage", new Table("3–51", "3–62", "4–74", "5–88", "5–99", "6–117", "7–130", "8–153", "9–180", "10–199",
                                                                  "12–221", "13–244", "14–270", "16–299", "17–330", "19–364", "21–401", "23–441", "26–485", "28–534",
                                                                  "31–586", "34–644", "34–644", "34–644", "45-850") }
                    },
                    IncludeForm = DamageForm.OnUse
                }
            }, {
                "Melee Splash",
                new Gem {
                    PerLevel = new Values {
                        { "#% less Damage to main target", new RangeMap(1, MaxLevel, 15.5f) } // XXX: MeleeSplash 16% in JSON data seems rounded or damage range rounding bug.
                    },
                    ExcludeFormSupport = DamageForm.AoE | DamageForm.OnUse
                }
            }, {
                "Molten Shell",
                new Gem {
                    PerLevel = new Values {
                        { "Mana Cost: #", new RangeMap(1, 17, new Table(22, 24, 28, 30, 34, 36, 38, 42, 46, 50, 54, 58, 60, 62, 64, 66, 66), 18, MaxLevel, new Linear(1, 50)) },
                        { "Deals #–# Fire Damage", new Table("26–39", "35–52", "45–68", "59–88", "75–113", "95–143", "120–180", "161–241", "214–321", "283–425",
                                                             "372–558", "455–682", "554–831", "674–1010", "817–1226", "989–1483", "1195–1792", "1354–2031", "1533–2300", "1735–2602",
                                                             "1962–2943", "2217–3326") }
                    },
                    IncludeForm = DamageForm.OnUse
                }
            }, {
                "Molten Strike",
                new Gem {
                    PerLevel = new Values {
                        { "Mana Cost: #", new RangeMap(1, 1, 6, 2, 5, 7, 6, 18, 8, 19, MaxLevel, 10) },
                        { "#% increased Physical Damage", new Linear(4, -4) }
                    },
                    ExcludeForm = DamageForm.AoE | DamageForm.Projectile
                }
            }, {
                "Power Siphon",
                new Gem {
                    PerLevel = new Values {
                        { "Mana Cost: #", new RangeMap(1, 10, 13, 11, MaxLevel, 14 ) },
                        { "#% increased Physical Damage", new Linear(4, -4) }
                    },
                    RequiredWeapon = WeaponType.Wand
                }
            }, {
                "Reave",
                new Gem {
                    PerLevel = new Values {
                        { "Mana Cost: #", new RangeMap(1, 5, 5, 6, 12, 6, 7, 16, 7, 17, 20, 8, 21, MaxLevel, 9) },
                        { "#% increased Physical Damage", new Linear(4, -4) }
                    },
                    PerQuality = new Values {
                        { "#% increased Attack Speed", new Linear(0.5f, 0) }
                    },
                    RequiredWeapon = WeaponType.Dagger | WeaponType.Claw | WeaponType.OneHandedSword
                }
            }, {
                "Shield Charge",
                new Gem {
                    RequiresEquippedShield = true,
                    IncludeForm = DamageForm.OnUse
                }
            }, {
                "Spectral Throw",
                new Gem {
                    RequiredWeapon = WeaponType.Melee
                }
            }, {
                "Split Arrow",
                new Gem {
                    PerLevel = new Values {
                        { "Mana Cost: #", new RangeMap(1, 1, 5, 2, 4, 6, 5, 8, 7, 9, 16, 8, 17, MaxLevel, 10) },
                        { "#% increased Physical Damage", new Linear(3, -3) },
                        { "# additional Arrows", new RangeMap(1, 4, 2, 5, 9, 3, 10, 16, 4, 17, 20, 5, 21, MaxLevel, 6) }
                    }
                }
            }, {
                "Storm Call",
                new Gem {
                    IncludeForm = DamageForm.OnUse
                }
            }, {
                "Sweep",
                new Gem {
                    PerLevel = new Values {
                        { "Attacks per Second: #", new RangeMap(1, MaxLevel, 1 / 1.15f) }, // Sweep has its own attack time of 1.15 seconds.
                    },
                    RequiredWeapon = WeaponType.Staff | WeaponType.TwoHandedAxe | WeaponType.TwoHandedMace
                }
            }, {
                "Tempest Shield",
                new Gem {
                    PerLevel = new Values {
                        { "Deals #–# Lightning Damage", new Table("9–13", "11–17", "13–20", "16–24", "19–29", "23–34", "27–40", "32–49", "39–59", "47–70",
                                                                  "56–84", "64–96", "72–108", "82–123", "92–138", "104–156", "117–175", "126–189", "136–204", "147–220") }
                    },
                    RequiresEquippedShield = true,
                    IncludeForm = DamageForm.OnUse
                }
            }, {
                "Whirling Blades",
                new Gem {
                    PerLevel = new Values {
                        { "Attacks per Second: #", new RangeMap(1, MaxLevel, 1 / 2.75f) }, // XXX: Just a guess, it doesn't use weapon APS and no info on Wiki.
                        { "#% increased Physical Damage", new Linear(3, -3) }
                    },
                    RequiredWeapon = WeaponType.Dagger | WeaponType.Claw | WeaponType.OneHandedSword,
                    IncludeForm = DamageForm.OnUse
                }
            }
        };
    }
}
