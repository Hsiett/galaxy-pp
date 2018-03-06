using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler;
using Galaxy_Editor_2.Compiler.Contents;
using Galaxy_Editor_2.Compiler.Generated.lexer;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Editor_control
{
    class Line
    {
        class Interval
        {
            public int Start, Length;

            public Interval(int start, int length)
            {
                Start = start;
                Length = length;
            }

            public bool Contains(int i)
            {
                return i >= Start && i < Start + Length;
            }
        }

        //This should be null if no block is started at the line
        public Line BlockEndLine;
        private bool blockVisible = true;

        public bool BlockVisible
        {
            get { return blockVisible; }
            set { blockVisible = value; }
        }

        public bool LineVisible = true;


        public string Text;
        public bool Invalidated;
        public bool edited = true;
        public int Indents
        {
            get
            {
                int indents = 0;
                foreach (char c in Text)
                {
                    if (c == ' ')
                        indents++;
                    else if (c == '\t')
                        indents += 4;
                    else
                        return indents/4;
                }
                return indents/4;
            }
            set
            {
                Text = Text.TrimStart(' ', '\t');
                for (int i = 0; i < value; i++)
                {
                    if (Options.Editor.ReplaceTabsWithSpaces)
                        Text = "    " + Text;
                    else
                        Text = "\t" + Text;
                }
                edited = Invalidated = true;
            }
        }

        private int shortndentDepth = 0;
        public int GetWantedIndents(FontScheme fonts, List<Line> lines, int index)
        {
            if (edited) Restyle(fonts, lines, index);
            int indents = index > 0 ? lines[index - 1].Indents : 0;
            shortndentDepth = 0;
            
            if (index > 0)
            {
                /* if previous had
                 * <if, while, for, else>
                 * without semicolon or block begin, indent + 1
                 */
                if (!tokens.Any(t => t is TLBrace))
                {
                    for (int i = lines[index - 1].tokens.Count - 1; i >= 0; i--)
                    {
                        if (lines[index - 1].tokens[i] is TSemicolon || lines[index - 1].tokens[i] is TLBrace)
                            break;
                        if (lines[index - 1].tokens[i] is TIf || lines[index - 1].tokens[i] is TWhile ||
                            lines[index - 1].tokens[i] is TFor || lines[index - 1].tokens[i] is TElse)
                        {
                            shortndentDepth = lines[index - 1].shortndentDepth + 1;
                            indents++;
                            break;
                        }
                    }
                }
                if (shortndentDepth == 0)
                {
                    indents -= lines[index - 1].shortndentDepth;
                }
                //If previos had any open unmatched paraenthsis ({, [ or (), add indents
                int openParens = 0;
                for (int i = 0; i < lines[index - 1].tokens.Count; i++)
                {
                    if (lines[index - 1].tokens[i] is TLParen || lines[index - 1].tokens[i] is TLBrace || lines[index - 1].tokens[i] is TLBracket)
                        openParens++;
                    if (openParens > 0 && (lines[index - 1].tokens[i] is TRParen || lines[index - 1].tokens[i] is TRBrace || lines[index - 1].tokens[i] is TRBracket))
                        openParens--;
                }
                if (openParens > 0)
                    indents += openParens;
                //If current has any unmatched close paranthisiaq (}, ] or )), remove indents
                openParens = 0;
                for (int i = 0; i < tokens.Count; i++)
                {
                    if (tokens[i] is TLParen || tokens[i] is TLBrace || tokens[i] is TLBracket)
                        openParens++;
                    if (tokens[i] is TRParen || tokens[i] is TRBrace || tokens[i] is TRBracket)
                        openParens--;
                }
                if (openParens < 0)
                    indents += openParens;
                //In switch, case <exp>: is pushed one step left
                if (lines[index - 1].tokens.Any(t => t is TColon) && lines[index - 1].tokens.Any(t => t is TCase))
                    indents++;
                //Except from the first one
                if (tokens.Any(t => t is TColon) && tokens.Any(t => t is TCase))
                {
                    bool isFirstSwitch = true;
                    //If switch is on same line.. ignore
                    int colonIndex = tokens.IndexOf(tokens.First(t => t is TColon));
                    if (!tokens.Any(t => t is TSwitch) || tokens.IndexOf(tokens.First(t => t is TSwitch)) > colonIndex)
                    {
                        for (int line = index - 1; line >= 0; line--)
                        {
                            if (lines[line].tokens.Any(t => t is TColon))
                            {
                                isFirstSwitch = false;
                                break;
                            }
                            if (lines[line].tokens.Any(t => t is TSwitch))
                            {
                                break;
                            }
                        }
                    }
                    if (!isFirstSwitch)
                        indents--;
                }
                //The } to end a switch should be pushed an extra step back.
                for (int i = 0; i < tokens.Count; i++)
                {
                    if (tokens[i] is TRBrace)
                    {
                        openParens = 1;
                        for (int j = i - 1; j >= 0; j--)
                        {
                            if (tokens[j] is TRBrace)
                                openParens++;
                            if (tokens[j] is TLBrace)
                                openParens--;
                        }
                        if (openParens > 0)
                        {
                            int stage = 1;
                            for (int line = index - 1; line >= 0 && stage < 5; line--)
                            {
                                for (int j = lines[line].tokens.Count - 1; j >= 0 && stage < 5; j--)
                                {
                                    switch (stage)
                                    {
                                        case 1://Look for matching {
                                            if (lines[line].tokens[j] is TRBrace)
                                                openParens++;
                                            if (lines[line].tokens[j] is TLBrace)
                                            {
                                                openParens--;
                                                if (openParens == 0)
                                                    stage++;
                                            }
                                            break;
                                        case 2://Previos MUST be )
                                            if (lines[line].tokens[j] is TWhiteSpace)
                                                break;
                                            if (lines[line].tokens[j] is TRParen)
                                            {
                                                stage++;
                                                openParens = 1;
                                            }
                                            else
                                                stage = 5;
                                            break;
                                        case 3://Look for matching (
                                            if (lines[line].tokens[j] is TRParen)
                                                openParens++;
                                            if (lines[line].tokens[j] is TLParen)
                                            {
                                                openParens--;
                                                if (openParens == 0)
                                                    stage++;
                                            }
                                            break;
                                        case 4://If previous is switch, indent more
                                            if (lines[line].tokens[j] is TWhiteSpace)
                                                break;
                                            if (lines[line].tokens[j] is TSwitch)
                                                indents = lines[line].Indents;
                                            stage++;
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (indents < 0)
                indents = 0;
            return indents;


            /*
             * Old algorithm
             * 
             * 
            int indents = 0;
            bool haveOpenBrace = false;
            if (index > 0)
            {
                for (int i = 0; i < lines[index - 1].Text.Length; i++)
                {
                    if (lines[index - 1].Text[i] == '{')
                    {
                        indents++;
                        haveOpenBrace = true;
                    }
                    if (lines[index - 1].Text[i] == '}' && indents > 0)
                        indents--;
                    if (lines[index - 1].Text[i] == ':')
                        indents++;
                }
                indents += lines[index - 1].Indents;
                if (lines[index - 1].oneArmedIndent)
                    indents--;
            }
            int newOpens = 0;
            for (int i = 0; i < lines[index].Text.Length; i++)
            {
                if (lines[index].Text[i] == '{')
                {
                    newOpens++;
                    haveOpenBrace = true;
                }
                if (lines[index].Text[i] == '}')
                    if (newOpens > 0)
                        newOpens--;
                    else if (indents > 0)
                        indents--;
                if (lines[index].Text[i] == ':' && !haveOpenBrace)
                    indents--;
            }
            //If we have a for, while, if or else on previous line with no { on that line or on this line.. increase indents
            oneArmedIndent = false;
            if (!haveOpenBrace && index > 0)
            {
                if (lines[index - 1].Text.Contains(" if ") || 
                    lines[index - 1].Text.Contains(" if(") || 
                    lines[index - 1].Text.Contains(" for ") || 
                    lines[index - 1].Text.Contains(" for(") || 
                    lines[index - 1].Text.Contains(" while ") || 
                    lines[index - 1].Text.Contains(" while(") || 
                    lines[index - 1].Text.Contains(" else ") ||
                    lines[index - 1].Text.EndsWith(" else"))
                {
                    oneArmedIndent = true;
                    indents++;
                }
            }
            return indents;*/
        }

        private Dictionary<Interval, Options.FontStyles> fontMods = new Dictionary<Interval, Options.FontStyles>();

        public Line(string text)
        {
            Text = text;
        }

        public float GetWidth(FontScheme fonts)
        {
            Graphics g = Graphics.FromImage(new Bitmap(1, 1));
            return g.MeasureString(Text, fonts.Base).Width;
        }

        public FontModification GetFontStyle(int charNr)
        {
            foreach (KeyValuePair<Interval, Options.FontStyles> fontMod in fontMods)
            {
                if (fontMod.Key.Contains(charNr))
                    return Options.Editor.GetMod(fontMod.Value);
            }
            //return new FontModification(Options.Editor.Font.Style, Color.Black);
            return Options.Editor.GetMod(Options.FontStyles.Normal);
        }



        private bool blockCommentAtEnd;
        List<Token> tokens = new List<Token>();
        public void Restyle(FontScheme fonts, List<Line> list, int lineNr)
        {
            fontMods.Clear();
            bool inBlockComment = lineNr > 0 && list[lineNr - 1].blockCommentAtEnd;
            bool blockCommetAtStart = inBlockComment;
            bool hadBlockCommentAtEnd = blockCommentAtEnd;
            blockCommentAtEnd = false;
            Lexer lexer = new Lexer(new StringReader(Text));
            tokens.Clear();
            List<Token> tokensNoWS = new List<Token>();
            Token token = null;
            while (true)
            {
                try
                {
                    token = lexer.Next();
                    if (token is EOF)
                        break;
                }
                catch (Exception)
                {
                    continue;
                }
                //Don't add comments to the list
                if (!(inBlockComment || token is TCommentBegin || token is TTraditionalComment || token is TEndOfLineComment || token is TDocumentationComment))
                {
                    tokens.Add(token);
                    if (!(token is TWhiteSpace))
                        tokensNoWS.Add(token);
                }
                if (inBlockComment)
                {
                    if (token is TCommentEnd)
                    {
                        fontMods.Add(new Interval(0, token.Pos + 1), Options.FontStyles.Comments);
                        inBlockComment = false;
                        continue;
                    }
                }
                else
                {
                    if (token is TTraditionalComment || token is TEndOfLineComment || token is TDocumentationComment)
                    {
                        fontMods.Add(new Interval(token.Pos - 1, token.Text.Length), Options.FontStyles.Comments);
                        continue;
                    }
                    if (token is TCommentBegin)
                    {
                        fontMods.Add(new Interval(token.Pos - 1, Text.Length - token.Pos + 1), Options.FontStyles.Comments);
                        inBlockComment = true;
                        continue;
                    }
                    if (token is TStringLiteral || token is TCharLiteral)
                    {
                        fontMods.Add(new Interval(token.Pos - 1, token.Text.Length), Options.FontStyles.Strings);
                        continue;
                    }
                    if (token is TUnknown && (token.Text == "\"" || token.Text == "'"))
                    {
                        fontMods.Add(new Interval(token.Pos - 1, Text.Length - token.Pos + 1), Options.FontStyles.Strings);
                        continue;
                    }

                    foreach (KeyValuePair<Options.FontStyles, List<string>> modification in fonts.Modifications)
                    {
                        if (modification.Value.Any(m => m == token.Text))
                        {
                            fontMods.Add(new Interval(token.Pos - 1, token.Text.Length), modification.Key);
                            continue;
                        }
                    }

                    if (token is TLParen && tokensNoWS.Count > 1)
                    {
                        Token prevToken = tokensNoWS[tokensNoWS.Count - 2];
                        if (prevToken is TIdentifier && Form1.Form.compiler.libraryData.Methods.Any(m => m.GetName().Text == prevToken.Text) &&
                            prevToken.Text != "InitMap")
                        {
                            bool isCustom = false;
                            if (tokensNoWS.Count > 2)
                            {
                                Token prevToken2 = tokensNoWS[tokensNoWS.Count - 3];
                                if (prevToken2 is TDot || prevToken2 is TArrow)
                                    isCustom = true;
                            }
                            if (!isCustom)
                            {
                                fontMods.Add(new Interval(prevToken.Pos - 1, prevToken.Text.Length), Options.FontStyles.NativeCalls);
                                continue;
                            }
                        }
                    }
                }
            }
            
            blockCommentAtEnd = inBlockComment;

            if (blockCommetAtStart == blockCommentAtEnd && blockCommetAtStart && fontMods.Count == 0)
            {
                fontMods.Add(new Interval(0, Text.Length), Options.FontStyles.Comments);
            }

            if (blockCommentAtEnd != hadBlockCommentAtEnd && lineNr + 1 < list.Count)
                list[lineNr + 1].edited = list[lineNr + 1].Invalidated = true;

            //Structs: identifier <identifer, star, lbracket, lbrace, nothing, dot>
            List<string> structNames = new List<string>();
            foreach (SourceFileContents sourceFile in Form1.Form.compiler.ParsedSourceFiles)
            {
                foreach (StructDescription str in sourceFile.GetAllStructs())
                {
                    structNames.Add(str.Name);
                }
            }
            for (int i = 0; i < tokensNoWS.Count; i++)
            {
                token = tokensNoWS[i];
                if (token is TIdentifier && !fontMods.Any(k => k.Key.Contains(token.Pos)) &&
                    structNames.Contains(token.Text))
                {
                    if (i < tokensNoWS.Count - 1 && !(tokensNoWS[i + 1] is TIdentifier ||
                        tokensNoWS[i + 1] is TStar || tokensNoWS[i + 1] is TLBracket ||
                        tokensNoWS[i + 1] is TLBrace || tokensNoWS[i + 1] is TDot || tokensNoWS[i + 1] is TLt))
                    {
                        if (
                            !(i > 0 &&
                              (tokensNoWS[i - 1] is TStruct || tokensNoWS[i - 1] is TClassToken ||
                               tokensNoWS[i - 1] is TRBracket) && tokensNoWS[i + 1] is TColon))
                            continue;
                    }
                    fontMods.Add(new Interval(token.Pos - 1, token.Text.Length), Options.FontStyles.Structs);
                }
            }


            edited = false;

            
        }
    }
}
