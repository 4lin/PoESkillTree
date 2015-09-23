﻿namespace POESKillTree.TreeGenerator.Model.PseudoAttributes
{
    public class OffHandCondition : ICondition
    {
        public string Alias { get; private set; }

        public OffHandCondition(string alias)
        {
            Alias = alias;
        }
        
        public bool Eval(ConditionSettings settings, params string[] placeholder)
        {
            return settings.OffHand.HasAlias(string.Format(Alias, placeholder));
        }
    }
}