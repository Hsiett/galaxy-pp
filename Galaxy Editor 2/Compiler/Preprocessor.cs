using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.lexer;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Generated.parser;
using Galaxy_Editor_2.Editor_control;

namespace Galaxy_Editor_2.Compiler
{
    class Preprocessor
    {
        public static void Parse(List<string> files, ErrorCollection errors)
        {
            /*
             * #macro <identifier>[(<identifiers>)]
             * anything...#arg1...anything
             * 
             * #endmacro
             * 
             * #<identifier>(anything_but_comma, #anything_but_hash#)
             * 
             * 
             */

            //Phase 1: locate all macros
            List<Macro> macros = new List<Macro>();
            for (int i = 0; i < files.Count; i++)
            {
                string source = files[i];


                Lexer lexer = new Lexer(new StringReader(source));
                List<Token> tokens = new List<Token>();
                Token token;
                while (!((token = lexer.Next()) is EOF))
                {
                    if (token is TWhiteSpace || token is TTraditionalComment || token is TDocumentationComment || token is TEndOfLineComment)
                        continue;
                    tokens.Add(token);
                }

                List<Util.Pair<TextPoint, TextPoint>> macroSpan = new List<Util.Pair<TextPoint, TextPoint>>();

                for (int j = 0; j < tokens.Count - 1; j++)
                {
                    int t = j;
                    if (tokens[t] is TSharp && tokens[t + 1].Text == "macro")
                    {
                        Macro macro = new Macro();
                        Token sharp = tokens[t];
                        macro.token = sharp;
                        TextPoint start = TextPoint.FromCompilerCoords(tokens[t]);
                        t += 2;
                        if (t >= tokens.Count || !(tokens[t] is TIdentifier))
                        {
                            errors.Add(new ErrorCollection.Error(sharp, "Expected macro name"));
                            continue;
                        }
                        macro.Name = tokens[t].Text;
                        //Find end
                        TextPoint end = new TextPoint(-1, -1);
                        for (int k = t; k < tokens.Count - 1; k++)
                        {
                            if (tokens[k] is TSharp)
                            {
                                if (tokens[k + 1].Text == "macro")
                                {
                                    errors.Add(new ErrorCollection.Error(tokens[k],
                                                                         "You can't declare new macros inside a macro.",
                                                                         false,
                                                                         new ErrorCollection.Error(sharp,
                                                                                                   "Current macro")));
                                }
                                else if (tokens[k + 1].Text == "endmacro")
                                {
                                    end = TextPoint.FromCompilerCoords(tokens[k]);
                                    break;
                                }
                            }
                        }
                        if (end.Line == -1)
                        {
                            errors.Add(new ErrorCollection.Error(sharp, "Found no matching #endmacro"));
                            continue;
                        }
                        //Check for parameters
                        t++;
                        TextPoint textStart;
                        if (tokens[t] is TLParen)
                        {
                            macro.Method = true;
                            bool needComma = false;
                            while (true)
                            {
                                t++;
                                if (tokens[t] is TRParen)
                                    break;
                                if (tokens[t] is TComma)
                                {
                                    if (!needComma)
                                    {
                                        errors.Add(new ErrorCollection.Error(tokens[t], "Expected identifier or right parenthesis"));
                                        break;
                                    }
                                    needComma = false;
                                    t++;
                                }
                                if (tokens[t] is TIdentifier)
                                {
                                    if (needComma)
                                    {
                                        errors.Add(new ErrorCollection.Error(tokens[t], "Expected comma or right parenthesis"));
                                        break;
                                    }
                                    macro.Parameters.Add(tokens[t].Text);
                                    needComma = true;
                                    continue;
                                }
                                if (needComma)
                                    errors.Add(new ErrorCollection.Error(tokens[t], "Expected comma or right parenthesis"));
                                else
                                    errors.Add(new ErrorCollection.Error(tokens[t], "Expected identifier or right parenthesis"));
                                break;
                            }
                            textStart = TextPoint.FromCompilerCoords(tokens[t]);
                            textStart.Pos++;
                            t++;
                        }
                        else
                        {
                            textStart = TextPoint.FromCompilerCoords(tokens[t - 1]);
                            textStart.Pos += macro.Name.Length;
                        }

                        macro.Text = GetText(source, textStart, end);
                        macroSpan.Add(new Util.Pair<TextPoint, TextPoint>(start, end));
                        macros.Add(macro);
                        j = t;
                    }
                }


                //Remove macros
                List<string> lines = source.Split('\n').ToList();
                for (int j = macroSpan.Count - 1; j >= 0; j--)
                {
                    TextPoint start = macroSpan[j].First;
                    TextPoint end = macroSpan[j].Second;

                    lines[start.Line] = lines[start.Line].Substring(0, start.Pos - 1) + lines[end.Line].Substring(end.Pos + "#endmacro".Length - 1);
                    for (int line = end.Line; line > start.Line; line--)
                    {
                        lines.RemoveAt(line);
                    }
                }

                StringBuilder builder = new StringBuilder();
                builder.Append(lines[0]);
                for (int j = 0; j < lines.Count; j++)
                {
                    builder.Append("\n" + lines[j]);
                }
                source = builder.ToString();

                files[i] = source;
            }

            //Check for dublicate macros
            for (int i = 0; i < macros.Count; i++)
            {
                List<Macro> conflicts = new List<Macro>(){macros[i]};
                for (int j = i + 1; j < macros.Count; j++)
                {
                    if (macros[i].Name == macros[j].Name &&
                        macros[i].Method == macros[j].Method &&
                        macros[i].Parameters.Count == macros[j].Parameters.Count)
                        conflicts.Add(macros[j]);
                }
                if (conflicts.Count > 1)
                {
                    List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                    foreach (Macro macro in conflicts)
                    {
                        subErrors.Add(new ErrorCollection.Error(macro.token, "Conflicting macro"));
                        macros.Remove(macro);
                    }
                    errors.Add(new ErrorCollection.Error(conflicts[0].token, "Found multiple macros with same signature.", false, subErrors.ToArray()));
                    i--;
                }
            }

            //Search for macro invocations

            for (int i = 0; i < files.Count; i++)
            {
                string source = files[i];
                ReplaceInvocations(ref source, macros, new List<Macro>(), errors);
                files[i] = source;
            }




        }

        private static void ReplaceInvocations(ref string source, List<Macro> macros, List<Macro> currentReplacePath, ErrorCollection errors)
        {

            Lexer lexer = new Lexer(new StringReader(source));
            List<Token> tokens = new List<Token>();
            Token token;
            while (!((token = lexer.Next()) is EOF))
            {
                if (token is TWhiteSpace || token is TTraditionalComment || token is TDocumentationComment || token is TEndOfLineComment)
                    continue;
                tokens.Add(token);
            }

            List<Util.Pair<Util.Pair<TextPoint, TextPoint>, string>> macroSpan = new List<Util.Pair<Util.Pair<TextPoint, TextPoint>, string>>();
            
            for (int j = 0; j < tokens.Count - 1; j++)
            {
                int t = j;
                if (tokens[t] is TSharp && tokens[t + 1] is TIdentifier)
                {
                    Token sharp = tokens[t];
                    TextPoint start = TextPoint.FromCompilerCoords(tokens[t]);
                    t++;
                    string Name = tokens[t].Text;
                    //Check for parameters
                    t++;
                    bool IsMethod = false;
                    List<string> arguments = new List<string>();
                    TextPoint end;
                    if (tokens[t] is TLParen)
                    {
                        IsMethod = true;
                        int parens = 1;
                        TextPoint argStart = TextPoint.FromCompilerCoords(tokens[t + 1]);
                        while (true)
                        {
                            t++;
                            if (t >= tokens.Count)
                            {
                                errors.Add(new ErrorCollection.Error(sharp, "Macro invocation not closed"));
                                t--;
                                break;
                            }

                            if (tokens[t] is TLParen || tokens[t] is TLBrace || tokens[t] is TLBracket)
                                parens++;
                            if (tokens[t] is TRParen || tokens[t] is TRBrace || tokens[t] is TRBracket)
                                parens--;
                            if (parens == 0)
                            {
                                if (!(tokens[t] is TRParen))
                                    errors.Add(new ErrorCollection.Error(tokens[t], "Expected right parenthesis"));
                                if (!(tokens[t - 1] is TLParen))
                                {
                                    TextPoint e = TextPoint.FromCompilerCoords(tokens[t]);
                                    e.Pos++;
                                    arguments.Add(GetText(source, argStart, e));
                                }

                                break;
                            }
                            if (parens > 1)
                                continue;
                            if (tokens[t] is TComma)
                            {
                                TextPoint e = TextPoint.FromCompilerCoords(tokens[t]);
                                e.Pos++;
                                arguments.Add(GetText(source, argStart, e));
                                argStart = e;
                            }
                        }
                        end = TextPoint.FromCompilerCoords(tokens[t]);
                        end.Pos++;
                    }
                    else
                    {
                        end = TextPoint.FromCompilerCoords(tokens[t - 1]);
                        end.Pos += Name.Length;
                    }

                    bool found = false;
                    foreach (Macro macro in macros)
                    {
                        if (macro.Name == Name &&
                            macro.Method == IsMethod &&
                            macro.Parameters.Count == arguments.Count)
                        {
                            found = true;
                            if (currentReplacePath.Contains(macro))
                            {
                                List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                for (int i = currentReplacePath.IndexOf(macro); i < currentReplacePath.Count; i++)
                                {
                                    subErrors.Add(new ErrorCollection.Error(currentReplacePath[i].token, "Macro in cycle"));
                                }
                                errors.Add(new ErrorCollection.Error(sharp, "Found a cycle in macros", false, subErrors.ToArray()));
                                break;
                            }
                            string macroText = macro.GetText(arguments);
                            List<Macro> newCurrentPath = new List<Macro>();
                            newCurrentPath.AddRange(currentReplacePath);
                            newCurrentPath.Add(macro);
                            ReplaceInvocations(ref macroText, macros, newCurrentPath, errors);
                            macroSpan.Add(new Util.Pair<Util.Pair<TextPoint, TextPoint>, string>(new Util.Pair<TextPoint, TextPoint>(start, end), macroText));
                        }
                    }
                    if (!found)
                        errors.Add(new ErrorCollection.Error(sharp, "No matching macro found."));
                    j = t - 1;
                }
            }


            //Remove macros
            for (int i = macroSpan.Count - 1; i >= 0; i--)
            {
                List<string> lines = source.Split('\n').ToList();
                TextPoint start = macroSpan[i].First.First;
                TextPoint end = macroSpan[i].First.Second;
                string text = macroSpan[i].Second;

                lines[start.Line] = lines[start.Line].Substring(0, start.Pos - 1) + text + lines[end.Line].Substring(end.Pos - 1);
                for (int line = end.Line; line > start.Line; line--)
                {
                    lines.RemoveAt(line);
                }

                StringBuilder builder = new StringBuilder();
                builder.Append(lines[0]);
                for (int j = 0; j < lines.Count; j++)
                {
                    builder.Append("\n" + lines[j]);
                }
                source = builder.ToString();
            }
        }


        private static string GetText(string source, TextPoint start, TextPoint end)
        {
            string[] lines = source.Split('\n');

            StringBuilder builder = new StringBuilder();
            if (start.Line == end.Line)
                builder.Append(lines[start.Line].Substring(start.Pos - 1, end.Pos - start.Pos - 1));
            else
            {
                builder.Append(lines[start.Line].Substring(start.Pos - 1) + "\n");
                for (int line = start.Line + 1; line < end.Line; line++)
                {
                    builder.Append(lines[line] + "\n");
                }
                builder.Append(lines[end.Line].Substring(0, end.Pos - 1));
            }
            return builder.ToString();
        }
        

        private class Macro
        {
            public string Name;
            public string Text;
            public bool Method;
            public Token token;
            public List<string> Parameters = new List<string>();

            public string GetText(List<string> arguments)
            {
                if (Parameters.Count == 0)
                    return Text;
                string source = Text;
                int index = 0;
                int argNr;
                while ((index = FirstIndexOf(source, index, Parameters, out argNr)) != -1)
                {
                    source = source.Substring(0, index) + arguments[argNr] +
                             source.Substring(index + Parameters[argNr].Length + 1);
                    index += arguments[argNr].Length;
                }
                return source;
            }

            private int FirstIndexOf(string source, int startIndex, List<string> strings, out int stringNr)
            {
                int index = -1;
                stringNr = -1;
                int strNr = 0;
                foreach (string s in strings)
                {
                    int i = source.IndexOf("#" + s, startIndex);
                    if (i != -1)
                    {
                        if (index == -1 || i < index)
                        {
                            index = i;
                            stringNr = strNr;
                        }
                    }
                    strNr++;
                }
                return index;
            }
        }

        private class MacroInvocation
        {
            public string Name;
            public bool Method;
            public List<string> Aruments = new List<string>();
        }
    }
}
