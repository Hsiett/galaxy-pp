using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Editor_control;

namespace Galaxy_Editor_2.Compiler.Contents
{
    
    class VariableDescription : SuggestionBoxItem
    {
        public enum VariableTypes
        {
            LocalVariable,
            Parameter,
            Field,
            StructVariable
        }

        public string Name;
        public string Type;
        public string PlacementPrefix;
        public VariableTypes VariableType;
        public PExp init;
        public string initStr;
        public bool Const;
        public bool IsStatic;
        public PVisibilityModifier Visibility = new APublicVisibilityModifier();
        public PType realType;
        public int Line;
        public bool IsArrayProperty;
        public TextPoint Position { get; private set; }

        public VariableDescription(AALocalDecl localDecl, VariableTypes type)
        {
            Name = localDecl.GetName().Text;
            Type = Util.TypeToString(localDecl.GetType());
            switch (type)
            {
                case VariableTypes.LocalVariable:
                    PlacementPrefix = "Local";
                    break;
                case VariableTypes.Parameter:
                    PlacementPrefix = "Parameter";
                    break;
                case VariableTypes.StructVariable:
                    PlacementPrefix = "Struct field";
                    break;
                default:
                    PlacementPrefix = "";
                    break;
            }
            VariableType = type;
            Const = localDecl.GetConst() != null;
            IsStatic = localDecl.GetStatic() != null;
            Visibility = localDecl.GetVisibilityModifier();
            realType = (PType) localDecl.GetType().Clone();
            init = localDecl.GetInit();
            Line = localDecl.GetName().Line;
            Position = TextPoint.FromCompilerCoords(localDecl.GetName());
        }

        public VariableDescription(AFieldDecl fieldDecl)
        {
            Name = fieldDecl.GetName().Text;
            Type = Util.TypeToString(fieldDecl.GetType());
            PlacementPrefix = "Field";
            VariableType = VariableTypes.Field;
            Const = fieldDecl.GetConst() != null;
            IsStatic = fieldDecl.GetStatic() != null;
            Visibility = fieldDecl.GetVisibilityModifier();
            realType = (PType)fieldDecl.GetType().Clone();
            init = fieldDecl.GetInit();
            Line = fieldDecl.GetName().Line;
            Position = TextPoint.FromCompilerCoords(fieldDecl.GetName());
        }

        public VariableDescription(ATriggerDecl triggerDecl)
        {
            Name = triggerDecl.GetName().Text;
            Type = "trigger";
            PlacementPrefix = "Field";
            VariableType = VariableTypes.Field;
            Const = false;
            IsStatic = false;
            realType = new ANamedType(new TIdentifier("trigger"), null);
            Visibility = (PVisibilityModifier)triggerDecl.GetVisibilityModifier().Clone();
            Line = triggerDecl.GetName().Line;
            Position = TextPoint.FromCompilerCoords(triggerDecl.GetActionsToken());
        }

        public VariableDescription(APropertyDecl property)
        {
            Name = property.GetName().Text;
            IsArrayProperty = Name == "array property";
            if (IsArrayProperty)
                Name = "";
            Type = Util.TypeToString(property.GetType());
            PlacementPrefix = "Property";
            VariableType = VariableTypes.Field;
            Const = false;
            IsStatic = property.GetStatic() != null;
            Visibility = property.GetVisibilityModifier();
            realType = (PType)property.GetType().Clone();
            Line = property.GetName().Line;
            Position = TextPoint.FromCompilerCoords(property.GetName());
        }

        public override bool Equals(object obj)
        {
            if (!(obj is VariableDescription)) return false;
            VariableDescription other = (VariableDescription)obj;
            if (Name != other.Name ||
                Type != other.Type ||
                Line != other.Line ||
                PlacementPrefix != other.PlacementPrefix ||
                Visibility.GetType() != other.Visibility.GetType())
                return false;
            return true;
        }

        public string DisplayText
        {
            get { return Name; }
        }

        public string InsertText
        {
            get { return Name; }
        }

        public string TooltipText
        {//Could make fullname here for fields (specify file aswell)
            get { return PlacementPrefix + ": " + (Const ? "const " : "") + Type + " " + Name + (initStr != null ? " = " + initStr + ";" : ""); }
        }

        public string Signature
        {//Must include methodname
            get
            {
                return (VariableType == VariableTypes.Field ? "F" : VariableType == VariableTypes.StructVariable ? "SF" : "V") +
                       Type + ":" + Name;
            }
        }

        public IDeclContainer ParentFile { get; set; }

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
