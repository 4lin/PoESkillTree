﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.Model.ItemFilter
{
    public class MatchStrings : Match, IMergeableMatch
    {
        // Public due to MatchClass.Learn method.
        public List<string> Values;

        protected MatchStrings(MatchStrings copy)
            : base(copy)
        {
            if (copy.Values != null)
                Values = new List<string>(copy.Values);
        }

        public MatchStrings(string[] strings)
        {
            Values = new List<string>(strings);
        }

        public override Match Clone()
        {
            return new MatchStrings(this);
        }

        public bool Complements(Match match)
        {
            // String matches cannot complement other matches.
            return false;
        }

        // Comparison is done in way that, less specific match follows more specific one.
        // TODO: Also check these, e.g. MatchStrings(["A", "C D"])) vs. MatchStrings(["A B", "C"])).
        public override int CompareTo(Match match)
        {
            int result = base.CompareTo(match);
            if (result != 0) return result;

            // If our string contains any of match strings, we are more specific thus should precede match.
            foreach (string str in Values)
                if ((match as MatchStrings).Values.Exists(s => s != str && str.Contains(s)))
                    return -1;

            // If match string contains any of our strings, we are less specific thus should follow match.
            foreach (string str in (match as MatchStrings).Values)
                if (Values.Exists(s => s != str && str.Contains(s)))
                    return 1;

            return 0;
        }

        public override bool Equals(Match match)
        {
            return base.Equals(match) && (match as MatchStrings).Values.Count == Values.Count && Values.Intersect((match as MatchStrings).Values).Count() == Values.Count;
        }

        public void Merge(Match match)
        {
            // If our string contains any of match strings, remove it.
            foreach (string str in Values.ToArray())
                if ((match as MatchStrings).Values.Exists(s => str.Contains(s)))
                    Values.Remove(str);

            // Add those strings from match, which doesn't contain any of our strings.
            foreach (string str in (match as MatchStrings).Values)
                if (!Values.Exists(s => str.Contains(s)))
                    Values.Add(str);
        }

        public virtual bool Subsets(Match match)
        {
            if (Priority != match.Priority) return false;

            // This match subsets other match, when all our strings contain strings of the other match.
            foreach (string str in Values)
                if (!(match as MatchStrings).Values.Exists(s => str.Contains(s)))
                    return false;

            return true;
        }

        public override string ToString()
        {
            string[] quoted = new string[Values.Count];

            for (int i = 0; i < Values.Count; ++i)
                quoted[i] = Values[i].Contains(" ") ? "\"" + Values[i] + "\"" : Values[i];

            return Keyword + " " + string.Join(" ", quoted);
        }
    }
}
