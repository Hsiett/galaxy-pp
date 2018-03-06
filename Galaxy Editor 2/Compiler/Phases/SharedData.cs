using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.NotGenerated;

namespace Galaxy_Editor_2.Compiler.Phases
{
    class SharedData
    {
        public bool IsLiteCompile;
        public static SharedData LastCreated;
        public bool AllowPrintouts;

        public SharedData()
        {
            LastCreated = this;
        }

        public List<KeyValuePair<string, int>> BankPreloads = new List<KeyValuePair<string, int>>();


        public LibraryData Libraries;
        public AMethodDecl DeobfuscateMethod;
        public Dictionary<AStringConstExp, AFieldDecl> ObfuscatedStrings = new Dictionary<AStringConstExp, AFieldDecl>();
        public Dictionary<AStringConstExp, AFieldDecl> UnobfuscatedStrings = new Dictionary<AStringConstExp, AFieldDecl>();
        public List<AFieldDecl> ObfuscationFields = new List<AFieldDecl>();
        public Dictionary<AASourceFile, int> LineCounts = new Dictionary<AASourceFile, int>();

        public Dictionary<APropertyDecl, AALocalDecl[]> ArrayPropertyLocals = new Dictionary<APropertyDecl, AALocalDecl[]>();
        public Dictionary<AStructDecl, bool> Enums = new Dictionary<AStructDecl, bool>();//true = int type

        //Enviroment Building
        public struct DeclItem<T> where T : PDecl
        {
            public T Decl;
            public AASourceFile File;

            public DeclItem(AASourceFile file, T decl)
            {
                File = file;
                Decl = decl;
            }
        }
        public List<ATypedefDecl> Typedefs = new List<ATypedefDecl>();
        public List<DeclItem<AMethodDecl>> Methods = new List<DeclItem<AMethodDecl>>();
        public List<AMethodDecl> UserMethods = new List<AMethodDecl>();
        public List<DeclItem<AMethodDecl>> Delegates = new List<DeclItem<AMethodDecl>>();
        public List<ATriggerDecl> Triggers = new List<ATriggerDecl>();
        public Dictionary<AABlock, List<AALocalDecl>> Locals = new Dictionary<AABlock, List<AALocalDecl>>();
        public List<AALocalDecl> UserLocals = new List<AALocalDecl>();
        public List<DeclItem<AFieldDecl>> Fields = new List<DeclItem<AFieldDecl>>();
        public List<AFieldDecl> UserFields = new List<AFieldDecl>();
        public List<DeclItem<APropertyDecl>> Properties = new List<DeclItem<APropertyDecl>>();
        public List<DeclItem<AStructDecl>> Structs = new List<DeclItem<AStructDecl>>();
        public List<AEnrichmentDecl> Enrichments = new List<AEnrichmentDecl>();
        public Dictionary<AStructDecl, List<AMethodDecl>> StructMethods = new Dictionary<AStructDecl, List<AMethodDecl>>();
        public Dictionary<AStructDecl, List<AConstructorDecl>> StructConstructors = new Dictionary<AStructDecl, List<AConstructorDecl>>();
        public Dictionary<AStructDecl, ADeconstructorDecl> StructDeconstructor = new Dictionary<AStructDecl, ADeconstructorDecl>();
        public Dictionary<AStructDecl, List<AALocalDecl>> StructFields = new Dictionary<AStructDecl, List<AALocalDecl>>();
        public Dictionary<AStructDecl, List<APropertyDecl>> StructProperties = new Dictionary<AStructDecl, List<APropertyDecl>>();
        public Dictionary<AMethodDecl, List<TStringLiteral>> TriggerDeclarations = new Dictionary<AMethodDecl, List<TStringLiteral>>();
        public bool HasUnknownTrigger;
        public Dictionary<AMethodDecl, List<InvokeStm>> Invokes = new Dictionary<AMethodDecl, List<InvokeStm>>();
        public List<AMethodDecl> InitializerMethods = new List<AMethodDecl>();

        public List<AMethodDecl> InvokeOnIniti = new List<AMethodDecl>();

        //Generics
        public Dictionary<ANamedType, TIdentifier> GenericLinks = new Dictionary<ANamedType, TIdentifier>();
        public Dictionary<ANamedType, TIdentifier> GenericMethodLinks = new Dictionary<ANamedType, TIdentifier>();
        //public Dictionary<AStructDecl, Dictionary<string, PType>> GenericsMap = new Dictionary<AStructDecl, Dictionary<string, PType>>();

        //Enheritance
        public Dictionary<AALocalDecl, AALocalDecl> EnheritanceLocalMap = new Dictionary<AALocalDecl, AALocalDecl>();

        //Typelinking
        public Dictionary<ANamedType, AStructDecl> StructTypeLinks = new Dictionary<ANamedType, AStructDecl>();
        public Dictionary<ANamedType, AMethodDecl> DelegateTypeLinks = new Dictionary<ANamedType, AMethodDecl>();
        public Dictionary<ANamedType, ATypedefDecl> TypeDefLinks = new Dictionary<ANamedType, ATypedefDecl>();
        public Dictionary<AFieldLvalue, AFieldDecl> FieldLinks = new Dictionary<AFieldLvalue, AFieldDecl>();
        public Dictionary<ATypeLvalue, AStructDecl> StaticLinks = new Dictionary<ATypeLvalue, AStructDecl>();
        public Dictionary<APropertyLvalue, APropertyDecl> PropertyLinks = new Dictionary<APropertyLvalue, APropertyDecl>();
        public Dictionary<ALocalLvalue, AALocalDecl> LocalLinks = new Dictionary<ALocalLvalue, AALocalDecl>();
        public Dictionary<AStructFieldLvalue, AALocalDecl> StructMethodFieldLinks = new Dictionary<AStructFieldLvalue, AALocalDecl>();
        public Dictionary<AStructLvalue, AALocalDecl> StructFieldLinks = new Dictionary<AStructLvalue, AALocalDecl>();
        public Dictionary<AStructFieldLvalue, APropertyDecl> StructMethodPropertyLinks = new Dictionary<AStructFieldLvalue, APropertyDecl>();
        public Dictionary<AStructLvalue, APropertyDecl> StructPropertyLinks = new Dictionary<AStructLvalue, APropertyDecl>();
        public Dictionary<ASimpleInvokeExp, string> SimpleNamespaceInvokes = new Dictionary<ASimpleInvokeExp, string>();
        public Dictionary<PType, AEnrichmentDecl> EnrichmentTypeLinks = new Dictionary<PType, AEnrichmentDecl>();
        public Dictionary<AArrayLvalue, Util.Pair<APropertyDecl, bool>> ArrayPropertyLinks = new Dictionary<AArrayLvalue, Util.Pair<APropertyDecl, bool>>();
        

        //Type checking
        public Dictionary<ASimpleInvokeExp, AMethodDecl> SimpleMethodLinks = new Dictionary<ASimpleInvokeExp, AMethodDecl>();
        public Dictionary<AConstructorDecl, AConstructorDecl> ConstructorBaseLinks = new Dictionary<AConstructorDecl, AConstructorDecl>();
        public Dictionary<ANonstaticInvokeExp, AMethodDecl> StructMethodLinks = new Dictionary<ANonstaticInvokeExp, AMethodDecl>();
        public Dictionary<ANewExp, AConstructorDecl> ConstructorLinks = new Dictionary<ANewExp, AConstructorDecl>();
        //public Dictionary<AStructLvalue, AStructDecl> StructLinks = new Dictionary<AStructLvalue, AStructDecl>();
        public Dictionary<PLvalue, PType> LvalueTypes = new Dictionary<PLvalue, PType>();
        public Dictionary<PExp, PType> ExpTypes = new Dictionary<PExp, PType>();
        public Dictionary<ADelegateExp, AMethodDecl> DelegateCreationMethod = new Dictionary<ADelegateExp, AMethodDecl>();
        public Dictionary<ADelegateExp, APointerLvalue> DelegateRecieveres = new Dictionary<ADelegateExp, APointerLvalue>();
        public List<AFieldDecl> FieldsToInitInMapInit = new List<AFieldDecl>();
        public Dictionary<AArrayLengthLvalue, AArrayTempType> ArrayLengthTypes = new Dictionary<AArrayLengthLvalue, AArrayTempType>();


        //Final changes
        public List<AALocalDecl> GeneratedVariables = new List<AALocalDecl>();
        public Dictionary<AConstructorDecl, AMethodDecl> ConstructorMap = new Dictionary<AConstructorDecl, AMethodDecl>();
        public Dictionary<ADeconstructorDecl, AMethodDecl> DeconstructorMap = new Dictionary<ADeconstructorDecl, AMethodDecl>();
        public List<ASimpleInvokeExp> BulkCopyProcessedInvokes = new List<ASimpleInvokeExp>();
        public Dictionary<APropertyDecl, AMethodDecl> Getters = new Dictionary<APropertyDecl, AMethodDecl>();
        public Dictionary<APropertyDecl, AMethodDecl> Setters = new Dictionary<APropertyDecl, AMethodDecl>();
        public List<AStringConstExp> StringsDontJoinRight = new List<AStringConstExp>();
        public Dictionary<AStructDecl, AALocalDecl> StructTypeField = new Dictionary<AStructDecl, AALocalDecl>();
    }
}
