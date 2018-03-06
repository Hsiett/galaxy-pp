using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Contents;
using Galaxy_Editor_2.Editor_control;

namespace Galaxy_Editor_2
{
    class GalaxyKeywords
    {
        public static GalaxyKeywords Primitives = new GalaxyKeywords(Options.FontStyles.Primitives,
                                                                     new[]
                                                                         {
                                                                            "int", "point", "marker", "string", "abilcmd",
                                                                            "wave", "unit", "bool", "void", "fixed",
                                                                            "unitgroup", "waveinfo", "text", "wavetarget",
                                                                            "actorscope", "actor", "doodad", "bank",
                                                                            "camerainfo", "color", "aifilter", "byte",
                                                                            "order", "playergroup", "region", "revealer",
                                                                            "sound", "soundlink", "timer",
                                                                            "transmissionsource", "trigger", "unitfilter",
                                                                            "unitref", "handle", "char"
                                                                         });

        public static GalaxyKeywords NullablePrimitives =
            new GalaxyKeywords(Options.FontStyles.Primitives,
                               new[]
                                   {
                                       "point", "marker", "string", "abilcmd",
                                       "wave", "unit", 
                                       "unitgroup", "waveinfo", "text", "wavetarget",
                                       "actorscope", "actor", "doodad", "bank",
                                       "camerainfo", "aifilter",
                                       "order", "playergroup", "region", "revealer",
                                       "sound", "soundlink", "timer",
                                       "transmissionsource", "trigger", "unitfilter",
                                       "unitref", "handle"//, "null"
                                   });

        public static GalaxyKeywords InMethodKeywords = new GalaxyKeywords(Options.FontStyles.Keywords,
                                                                   new[]
                                                                       {
                                                                           "if", "else", "do", "while", "return", "break",
                                                                           "continue", "const", "struct", "global", "for",
                                                                           "ref", "out", "InvokeSync", "InvokeAsync",
                                                                           "switch", "case", "default", "new", "delete", "this", "true", "false",
                                                                           "delegate", "value", "base"
                                                                       });

        public static GalaxyKeywords SystemExpressions = new GalaxyKeywords(Options.FontStyles.Keywords,
                                                                   new[]
                                                                       {
                                                                           "this", "true", "false", "null"
                                                                       });

        public static GalaxyKeywords OutMethodKeywords = new GalaxyKeywords(Options.FontStyles.Keywords,
                                                                   new[]
                                                                       {
                                                                           "include", "native", "const", "static",
                                                                           "struct", "inline", "namespace", "using",
                                                                           "Trigger", "ref", "out", "Initializer",
                                                                           "events", "conditions", "actions", "class",
                                                                           "delegate", "typedef", "get", "set", "enrich", 
                                                                           "public", "private", "protected", "operator", "enum"
                                                                       });
        
        public static GalaxyKeywords InitializerKeywords =
            new GalaxyKeywords(Options.FontStyles.Keywords,
                               new[]
                                   {
                                       "LibraryName", "LibraryVersion", "SupportedVersions", "RequiredLibraries"
                                   });

       



        public List<GalaxyKeyword> keywords = new List<GalaxyKeyword>();

        public string[] words
        {
            get
            {
                string[] returner = new string[keywords.Count];
                int i = 0;
                foreach (GalaxyKeyword keyword in keywords)
                {
                    returner[i] = keyword.Word;
                    i++;
                }
                return returner;
            }
        }
        public Options.FontStyles mod;

        public GalaxyKeywords(Options.FontStyles mod, string[] words)
        {
            this.mod = mod;
            foreach (string word in words)
            {
                keywords.Add(new GalaxyKeyword(){Word = word, InsertPostFix = word.StartsWith("Invoke") ? "<" : ""});
            }
        }

        public class GalaxyKeyword : SuggestionBoxItem
        {
            public string Word;
            public string InsertPostFix;

            public string DisplayText
            {
                get { return Word; }
            }

            public string InsertText
            {
                get { return Word + InsertPostFix; }
            }

            public string TooltipText
            {
                get { return null; }
            }

            public string Signature
            {
                get { return "K" + Word; }
            }

            public IDeclContainer ParentFile
            {
                get { throw new NotImplementedException(); }
            }

            public TextPoint Position
            {
                get { throw new NotImplementedException(); }
            }

            public string Comment
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }
        }
        
    }
}
