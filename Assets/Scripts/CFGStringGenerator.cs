using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

// Uses Context Free Grammars to generate random strings
public class CFGStringGenerator {

    private static readonly TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

    public abstract class Symbol {
        public abstract string Expand(Dictionary<string, Symbol> rules);
    }

    // Takes a list of symbols and uses a random one to Expand
    public class SwitchSymbol : Symbol {
        private readonly Symbol[] symbols;
        public SwitchSymbol(Symbol[] symbols) { this.symbols = symbols; }
        public override string Expand(Dictionary<string, Symbol> rules) { return symbols[Random.Range(0, symbols.Length)].Expand(rules); }
    }

    // Takes a list of symbols and concatenates them on Expand
    public class ConcatSymbol : Symbol {
        private readonly Symbol[] symbols;
        public ConcatSymbol(Symbol[] symbols) { this.symbols = symbols; }
        public override string Expand(Dictionary<string, Symbol> rules) { return string.Join("", symbols.Select(s => s.Expand(rules)).ToArray()); }
    }

    // Used to reference a symbol in our dictionary
    public class RefSymbol : Symbol {
        private readonly string key;
        public RefSymbol(string key) { this.key = key; }
        public override string Expand(Dictionary<string, Symbol> rules) { return rules[key].Expand(rules); }
    }

    // Takes a string and returns it on Expand
    public class Terminal : Symbol {
        private readonly string s;
        public Terminal(string s) { this.s = s; }
        public override string Expand(Dictionary<string, Symbol> rules) { return s; }
    }

    // Utility terminal symbol to represent the empty string
    public static Terminal Empty = new Terminal("");

    // Note: keys must be alphanumeric!
    // Specifically, a key with a '=', ',', '"', or '|' character in it will mess up parsing
    public static CFGStringGenerator Parse(string filename) {
        CFGStringGenerator generator = new CFGStringGenerator();
        string fileText = (Resources.Load(filename) as TextAsset).text;
        foreach (string rule in fileText.Split('\n'))
            generator.ParseRule(rule);
        return generator;
    }

    public Symbol S;
    public Dictionary<string, Symbol> rules = new Dictionary<string, Symbol>();

    public string Generate() {
        return textInfo.ToTitleCase(S.Expand(rules));
    }

    private void ParseRule(string rule) {
        int eqIndex = rule.IndexOf('=');
        if (eqIndex == -1) {
            // Not a valid rule
            return;
        }

        string key = rule.Substring(0, eqIndex);
        Symbol symbol;

        // Regex: Find |s not inside quotes
        string[] switchSymbols = Regex.Split(rule.Substring(eqIndex + 1), "\\|(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
        if (switchSymbols.Length == 0) {
            // Not a valid rule
            return;
        } else if (switchSymbols.Length == 1) {
            // Single value - no switch needed
            rules.Add(key, symbol = ParseNonSwitchSymbol(switchSymbols[0].Trim()));
        } else {
            // Multiple values - switch needed
            rules.Add(key, symbol = new SwitchSymbol(switchSymbols.Select(s => ParseNonSwitchSymbol(s.Trim())).ToArray()));
        }

        if (key == "S") {
            S = symbol;
        }
    }

    private Symbol ParseNonSwitchSymbol(string symbol) {
        // Regex: Find commas not inside quotes
        string[] concatSymbols = Regex.Split(symbol, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
        if (concatSymbols.Length == 0) {
            // Empty rule
            return Empty;
        } else if (concatSymbols.Length == 1) {
            // No comma means only one part
            return ParseNonConcatSymbol(concatSymbols[0].Trim());
        } else {
            // Commas means multiple parts that need to be concatenated
            return new ConcatSymbol(concatSymbols.Select(s => ParseNonConcatSymbol(s.Trim())).ToArray());
        }
    }

    private Symbol ParseNonConcatSymbol(string symbol) {
        if (Regex.IsMatch(symbol, "\".*\"")) {
            // Quotes means this is a terminal
            // Note we use substring to remove the quotes
            return new Terminal(symbol.Substring(1, symbol.Length - 2));
        } else {
            // No quotes means this is a reference symbol
            return new RefSymbol(symbol.Trim());
        }
    }
}
