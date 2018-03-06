using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Galaxy_Editor_2.Dialog_Creator.Enums;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
namespace Galaxy_Editor_2.Dialog_Creator.Fonts
{
    class FontParser
    {
        public static Dictionary<string, FontData> Fonts = new Dictionary<string, FontData>();

        static FontParser()
        {
            Dictionary<string, string> constants = new Dictionary<string, string>();
            List<string> parseLater = new List<string>();
            do
            {

                XmlReader reader = XmlReader.Create("Fonts\\FontStyles.SC2Style");

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            switch (reader.Name)
                            {
                                case "Constant":
                                    if (!constants.ContainsKey(reader.GetAttribute("name")))
                                        constants.Add(reader.GetAttribute("name"),
                                                      ParseForConstant(reader.GetAttribute("val"), constants));
                                    break;
                                case "Style":
                                    string template = ParseForConstant(reader.GetAttribute("template"), constants);
                                    string name = ParseForConstant(reader.GetAttribute("name"), constants);
                                    
                                    if (template != null && !Fonts.ContainsKey(template))
                                    {
                                        if (!parseLater.Contains(name))
                                            parseLater.Add(name);
                                        break;
                                    }
                                    if (parseLater.Contains(name))
                                        parseLater.Remove(name);
                                    if (Fonts.ContainsKey(name))
                                        break;

                                    FontData data = template == null ? new FontData() : Fonts[template].GetClone();
                                    data.Name = name;
                                    //if (name == "TeamResourceTradingNotAllowedTime"||name== "TeamResourceTradingNotAllowedTime_Zerg")
                                    //    Console.WriteLine("");
                                    reader.MoveToFirstAttribute();
                                    do
                                    {
                                        if (reader.Name == "template" || reader.Name == "name")
                                            continue;
                                        string value = ParseForConstant(reader.Value, constants);
                                        switch (reader.Name.ToLower())
                                        {
                                            case "font":
                                                data.FontRef = value;
                                                break;
                                            case "styleflags":
                                                if (value == "Shadow")
                                                    data.StyleFlags = StyleFlags.Shadow;
                                                else if (value == "!Shadow")
                                                    data.StyleFlags -= StyleFlags.Shadow;
                                                else
                                                    throw new Exception("font parser: Invalid styleflags - " + value);
                                                break;
                                            case "shadowoffset":
                                                data.ShadowOffset = float.Parse(value);
                                                break;
                                            case "height":
                                                data.Size = (int)(int.Parse(value)* 0.8f);
                                                break;
                                            case "vjustify":
                                                switch (value)
                                                {
                                                    case "Top":
                                                        if (data.Anchor == Anchor.TopLeft || data.Anchor == Anchor.Left ||
                                                            data.Anchor == Anchor.BottomLeft)
                                                            data.Anchor = Anchor.TopLeft;
                                                        else if (data.Anchor == Anchor.Top ||
                                                                 data.Anchor == Anchor.Center ||
                                                                 data.Anchor == Anchor.Bottom)
                                                            data.Anchor = Anchor.Top;
                                                        else if (data.Anchor == Anchor.TopRight ||
                                                                 data.Anchor == Anchor.Right ||
                                                                 data.Anchor == Anchor.BottomRight)
                                                            data.Anchor = Anchor.TopRight;
                                                        break;
                                                    case "Middle":
                                                        if (data.Anchor == Anchor.TopLeft || data.Anchor == Anchor.Left ||
                                                            data.Anchor == Anchor.BottomLeft)
                                                            data.Anchor = Anchor.Left;
                                                        else if (data.Anchor == Anchor.Top ||
                                                                 data.Anchor == Anchor.Center ||
                                                                 data.Anchor == Anchor.Bottom)
                                                            data.Anchor = Anchor.Center;
                                                        else if (data.Anchor == Anchor.TopRight ||
                                                                 data.Anchor == Anchor.Right ||
                                                                 data.Anchor == Anchor.BottomRight)
                                                            data.Anchor = Anchor.Right;
                                                        break;
                                                    case "Bottom":
                                                        if (data.Anchor == Anchor.TopLeft || data.Anchor == Anchor.Left ||
                                                            data.Anchor == Anchor.BottomLeft)
                                                            data.Anchor = Anchor.BottomLeft;
                                                        else if (data.Anchor == Anchor.Top ||
                                                                 data.Anchor == Anchor.Center ||
                                                                 data.Anchor == Anchor.Bottom)
                                                            data.Anchor = Anchor.Bottom;
                                                        else if (data.Anchor == Anchor.TopRight ||
                                                                 data.Anchor == Anchor.Right ||
                                                                 data.Anchor == Anchor.BottomRight)
                                                            data.Anchor = Anchor.BottomRight;
                                                        break;
                                                    default:
                                                        throw new Exception("font parser: Invalid vjustify - " + value);
                                                }
                                                break;
                                            case "hjustify":
                                                switch (value)
                                                {
                                                    case "Left":
                                                        if (data.Anchor == Anchor.TopLeft || data.Anchor == Anchor.Top ||
                                                            data.Anchor == Anchor.TopRight)
                                                            data.Anchor = Anchor.TopLeft;
                                                        else if (data.Anchor == Anchor.Left ||
                                                                 data.Anchor == Anchor.Center ||
                                                                 data.Anchor == Anchor.Right)
                                                            data.Anchor = Anchor.Left;
                                                        else if (data.Anchor == Anchor.BottomLeft ||
                                                                 data.Anchor == Anchor.Bottom ||
                                                                 data.Anchor == Anchor.BottomRight)
                                                            data.Anchor = Anchor.BottomLeft;
                                                        break;
                                                    case "Center":
                                                        if (data.Anchor == Anchor.TopLeft || data.Anchor == Anchor.Top ||
                                                            data.Anchor == Anchor.TopRight)
                                                            data.Anchor = Anchor.Top;
                                                        else if (data.Anchor == Anchor.Left ||
                                                                 data.Anchor == Anchor.Center ||
                                                                 data.Anchor == Anchor.Right)
                                                            data.Anchor = Anchor.Center;
                                                        else if (data.Anchor == Anchor.BottomLeft ||
                                                                 data.Anchor == Anchor.Bottom ||
                                                                 data.Anchor == Anchor.BottomRight)
                                                            data.Anchor = Anchor.Bottom;
                                                        break;
                                                    case "Right":
                                                        if (data.Anchor == Anchor.TopLeft || data.Anchor == Anchor.Top ||
                                                            data.Anchor == Anchor.TopRight)
                                                            data.Anchor = Anchor.TopRight;
                                                        else if (data.Anchor == Anchor.Left ||
                                                                 data.Anchor == Anchor.Center ||
                                                                 data.Anchor == Anchor.Right)
                                                            data.Anchor = Anchor.Right;
                                                        else if (data.Anchor == Anchor.BottomLeft ||
                                                                 data.Anchor == Anchor.Bottom ||
                                                                 data.Anchor == Anchor.BottomRight)
                                                            data.Anchor = Anchor.BottomRight;
                                                        break;
                                                    default:
                                                        throw new Exception("font parser: Invalid hjustify - " + value);
                                                }
                                                break;
                                            case "textcolor":
                                                data.TextColor = ParseColor(value);
                                                break;
                                            case "disabledcolor":
                                                data.DisabledColor = ParseColor(value);
                                                break;
                                            case "highlightcolor":
                                                data.HighLightColor = ParseColor(value);
                                                break;
                                            case "hotkeycolor":
                                                data.HotKeyColor = ParseColor(value);
                                                break;
                                            case "hyperlinkcolor":
                                                data.HyperlinkColor = ParseColor(value);
                                                break;
                                                /*case "shadowcolor":
                                                data.s = ParseColor(value);
                                                break;*/
                                            default:
                                                throw new Exception("font parser: unable to handle " + reader.Name);

                                        }
                                    } while (reader.MoveToNextAttribute());
                                    Fonts.Add(data.Name, data);
                                    break;
                            }
                            break;
                    }
                }
                reader.Close();

            } while (parseLater.Count != 0);
        }

        private static string ParseForConstant(string value, Dictionary<string, string> constants)
        {
            if (value != null && value.Length > 0 && value[0] == '#')
            {
                return constants[value.Substring(1)];
            }
            return value;
        }

        private static Color ParseColor(string s)
        {
            if(s.Contains(","))
            {
                //255, 255, 255
                string[] colors = s.Split(',');
                return new Color(byte.Parse(colors[0].Trim()), byte.Parse(colors[1].Trim()), byte.Parse(colors[2].Trim()));
            }
            else
            {
                //ffffff
                int i = int.Parse(s, NumberStyles.HexNumber);
                byte b = (byte)(i % 256);
                i /= 256;
                byte g = (byte)(i % 256);
                i /= 256;
                byte r = (byte)(i % 256);
                return new Color(r, g, b);
            }
        }
    }
}
