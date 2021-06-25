using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MoonSharp.Interpreter;
namespace LunarBind.Documentation
{
    /// <summary>
    /// Provides documentation for Script Bindings
    /// </summary>
    public static class BindingDocumentation
    {
        public static int MaxRecursion { get; set; } = 32;

        /// <summary>
        /// Create global script bindings documentation
        /// </summary>
        /// <param name="typesToDocument"></param>
        /// <returns></returns>
        public static DocItem CreateDocumentation()
        {
            DocItem doc = new DocItem(DocItemType.Table, null, "Root", "Root", "");
            var bi = GlobalScriptBindings.bindItems.Values;

            foreach (var item in bi)
            {
                if (item is BindTable bTable)
                {
                    doc.SubDocs.Add(DocumentTable(bTable, "", 0));
                }
                else if(item is BindFunc func)
                {
                    doc.SubDocs.Add(DocumentFunction(func, ""));
                }
                else if (item is BindUserType bUserType)
                {
                    doc.SubDocs.Add(DocumentType(bUserType, "", 0));
                }
                else if (item is BindUserObject bUserObj)
                {
                    doc.SubDocs.Add(DocumentObject(bUserObj, "", 0));
                }
                else if (item is BindEnum bEnum)
                {
                    doc.SubDocs.Add(DocumentEnum(bEnum, ""));
                }
            }

            doc.Sort();

            return doc;
        }


        public static DocItem CreateDocumentation(ScriptBindings bindings)
        {
            DocItem doc = new DocItem(DocItemType.Table) { };
            var bi = bindings.bindItems.Values;

            foreach (var item in bi)
            {
                if (item is BindTable bTable)
                {
                    doc.SubDocs.Add(DocumentTable(bTable, "", 0));
                }
                else if (item is BindFunc func)
                {
                    doc.SubDocs.Add(DocumentFunction(func, ""));
                }
                else if (item is BindUserType bUserType)
                {
                    doc.SubDocs.Add(DocumentType(bUserType, "", 0));
                }
                else if (item is BindUserObject bUserObj)
                {
                    doc.SubDocs.Add(DocumentObject(bUserObj, "", 0));
                }
                else if (item is BindEnum bEnum)
                {
                    doc.SubDocs.Add(DocumentEnum(bEnum, ""));
                }
            }

            doc.Sort();

            return doc;
        }

        private static DocItem DocumentTable(BindTable table, string prefix, int curLevel)
        {
            DocItem doc = new DocItem(DocItemType.Table, typeof(Table), table.Name, prefix + table.Name, prefix + table.Name, prefix + table.Name);
            if(curLevel > MaxRecursion)
            {
                doc.SubDocs.Add(new DocItem(DocItemType.None, typeof(void), "Max Recursion Reached", "Max Recursion Reached", "Max Recursion Reached", "", "", ""));
                return doc;
            }
            var nextPrefix = prefix + table.Name + ".";
            //var items = table.GetAllItems();

            //Functions
            foreach (var item in table.bindFunctions.Values)
            {
                var docItem = DocumentFunction(item, nextPrefix);
                if (docItem != null)
                {
                    doc.SubDocs.Add(docItem);
                }
            }

            //Types
            foreach (var item in table.bindTypes.Values)
            {
                var docItem = DocumentType(item, nextPrefix, curLevel + 1);
                if (docItem != null)
                {
                    doc.SubDocs.Add(docItem);
                }
            }

            //Enums
            foreach (var item in table.bindEnums.Values)
            {
                var docItem = DocumentEnum(item, nextPrefix);
                if (docItem != null)
                {
                    doc.SubDocs.Add(docItem);
                }
            }

            //User Objects
            foreach (var item in table.bindObjects.Values)
            {
                var docItem = DocumentObject(item, nextPrefix, curLevel + 1);
                if (docItem != null)
                {
                    doc.SubDocs.Add(docItem);
                }
            }

            //Tables last
            foreach (var item in table.bindTables.Values)
            {
                //Add to prefix
                var documentTable = DocumentTable(item, nextPrefix, curLevel + 1);
                if (documentTable != null) 
                {
                    doc.SubDocs.Add(documentTable);
                }
            }

            return doc;
        }

        private static DocItem DocumentFunction(BindFunc func, string prefix)
        {
            string name = func.Name;
            string fullName = prefix + name;
            string documentation = func.Documentation ?? "";
            string example = func.Example ?? "";


            var mi = func.Callback.Method;
            var parameterInfo = mi.GetParameters();
            string parameterTypes = "";
            string parameterNames = "";
            if (parameterInfo.Length > 0)
            {
                parameterTypes = GetParamString(parameterInfo);
                parameterNames = GetParamNamesOnlyString(parameterInfo);
            }

            var returnType = mi.ReturnType;
            string returnTypeName = returnType.IsGenericType ? GetGenericString(returnType) : returnType.Name;
            //string tooltip = $"Return Type:\n- {(returnType.IsGenericType ? GetGenericString(returnType, true) : returnType.FullName)}{(parameterFullNames != null ? $"\nParameter Types:\n{parameterFullNames}" : " ")}";

            string definition = $"{returnTypeName} {fullName}({parameterTypes})";
            string copy = $"{fullName}()";

            return new DocItem(DocItemType.Function, func.Callback.Method.DeclaringType, name, fullName, definition, copy, documentation, example);

        }

        //For user type
        private static DocItem DocumentFunction(MethodInfo mi, string prefix)
        {
            string name = mi.Name;
            string fullName = prefix + name;
            
            var parameterInfo = mi.GetParameters();
            string parameterTypes = "";
            string parameterNames = "";

            if (parameterInfo.Length > 0)
            {
                parameterTypes = GetParamString(parameterInfo);
                parameterNames = GetParamNamesOnlyString(parameterInfo);
            }
            string documentation = "";
            string example = "";

            var docAttrib = mi.GetCustomAttribute<LunarBindDocumentationAttribute>();
            if (docAttrib != null)
            {
                documentation = docAttrib.Data;
            }
            var exampleAttrib = mi.GetCustomAttribute<LunarBindExampleAttribute>();
            if (exampleAttrib != null)
            {
                example = exampleAttrib.Data;
            }

            var returnType = mi.ReturnType;
            string returnTypeName = returnType.IsGenericType ? GetGenericString(returnType) : returnType.Name;
            //string tooltip = $"Return Type:\n- {(returnType.IsGenericType ? GetGenericString(returnType, true) : returnType.FullName)}{(parameterFullNames != null ? $"\nParameter Types:\n{parameterFullNames}" : " ")}";

            string definition = $"{returnTypeName} {fullName}({parameterTypes})";
            string copy = $"{fullName}()";

            return new DocItem(DocItemType.Function, mi.DeclaringType, name, fullName, definition, copy, documentation, example);
        }


        private static DocItem DocumentType(BindUserType userType, string prefix, int curLevel)
        {

            //Get attributes
            string documentation = "";
            string example = "";

            var docAttrib = userType.UserType.GetCustomAttribute<LunarBindDocumentationAttribute>();
            if (docAttrib != null)
            {
                documentation = docAttrib.Data;
            }
            var exampleAttrib = userType.UserType.GetCustomAttribute<LunarBindExampleAttribute>();
            if (exampleAttrib != null)
            {
                example = exampleAttrib.Data;
            }

            DocItem doc = new DocItem(DocItemType.StaticType, userType.UserType, userType.Name, prefix + userType.Name, "", documentation, example);
            var type = userType.UserType;
            var nextPrefix = prefix + userType.Name + ".";
            //STATIC
            var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public).Where(a => !a.IsSpecialName && a.Name != "ToString" && a.Name != "GetHashCode" && a.Name != "GetType" && a.Name != "Equals" && a.Name != "GetTypeCode")
                .Where(x => !x.CustomAttributes.Any(y => y.AttributeType == typeof(MoonSharpHiddenAttribute) || y.AttributeType == typeof(MoonSharpHideMemberAttribute) || y.AttributeType == typeof(LunarBindHideAttribute)));
            var properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public).Where(a => !a.IsSpecialName && a.Name != "ToString" && a.Name != "GetHashCode" && a.Name != "GetType" && a.Name != "Equals" && a.Name != "GetTypeCode")
                .Where(x => !x.CustomAttributes.Any(y => y.AttributeType == typeof(MoonSharpHiddenAttribute) || y.AttributeType == typeof(MoonSharpHideMemberAttribute) || y.AttributeType == typeof(LunarBindHideAttribute)));
            var funcs = type.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(a => !a.IsSpecialName && a.Name != "ToString" && a.Name != "GetHashCode" && a.Name != "GetType" && a.Name != "Equals" && a.Name != "GetTypeCode")
                .Where(x => !x.CustomAttributes.Any(y => y.AttributeType == typeof(MoonSharpHiddenAttribute) || y.AttributeType == typeof(MoonSharpHideMemberAttribute) || y.AttributeType == typeof(LunarBindHideAttribute)));
            //TODO: events
            //var events = type.GetEvents(BindingFlags.Static | BindingFlags.Public)
            //    .Where(x => !x.CustomAttributes.Any(y => y.AttributeType == typeof(MoonSharpHiddenAttribute) || y.AttributeType == typeof(MoonSharpHideMemberAttribute) || y.AttributeType == typeof(LunarBindHideAttribute)));

            foreach (var field in fields)
            {
                string docu = "";
                string ex = "";

                var docAttrib2 = field.GetCustomAttribute<LunarBindDocumentationAttribute>();
                if (docAttrib2 != null)
                {
                    docu = docAttrib2.Data;
                }
                var exampleAttrib2 = field.GetCustomAttribute<LunarBindExampleAttribute>();
                if (exampleAttrib2 != null)
                {
                    ex = exampleAttrib2.Data;
                }

                var fdoc = DocumentInstanceObject(field.FieldType, field.Name, nextPrefix, curLevel + 1, type, docu, ex);
                if(fdoc != null)
                {
                    doc.SubDocs.Add(fdoc);
                }
            }

            foreach (var property in properties)
            {
                string docu = "";
                string ex = "";

                var docAttrib2 = property.GetCustomAttribute<LunarBindDocumentationAttribute>();
                if (docAttrib2 != null)
                {
                    docu = docAttrib2.Data;
                }
                var exampleAttrib2 = property.GetCustomAttribute<LunarBindExampleAttribute>();
                if (exampleAttrib2 != null)
                {
                    ex = exampleAttrib2.Data;
                }

                var pdoc = DocumentInstanceObject(property.PropertyType, property.Name, nextPrefix, curLevel + 1, type, docu, ex);
                if (pdoc != null)
                {
                    doc.SubDocs.Add(pdoc);
                }
            }

            foreach (var func in funcs)
            {
                var pdoc = DocumentFunction(func, nextPrefix);
                if (pdoc != null)
                {
                    doc.SubDocs.Add(pdoc);
                }
            }

            return doc;
        }

        /// <summary>
        /// CAN RETURN NULL
        /// </summary>
        private static DocItem DocumentInstanceObject(Type type, string name, string prefix, int curLevel, Type declType = null, string documentation = "", string example = "")
        {
            string fullName = prefix + name;
            var nextPrefix = prefix + name + ".";

            if (type.IsPrimitive || type == typeof(string))
            {
                //Stop recursing
                return new DocItem(DocItemType.InstanceObject, declType ?? type, name, fullName, fullName, fullName);
            }
            else if (!UserData.IsTypeRegistered(type))
            {
                return null;
            }

            DocItem doc = new DocItem(DocItemType.InstanceObject, type, name, fullName, "");

            if (curLevel > MaxRecursion)
            {
                doc.SubDocs.Add(new DocItem(DocItemType.None, typeof(void), "Max Recursion Reached", "Max Recursion Reached", "Max Recursion Reached", "", "", ""));
                return doc;
            }


            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public).Where(a => !a.IsSpecialName && a.Name != "ToString" && a.Name != "GetHashCode" && a.Name != "GetType" && a.Name != "Equals" && a.Name != "GetTypeCode")
                .Where(x => !x.CustomAttributes.Any(y => y.AttributeType == typeof(MoonSharpHiddenAttribute) || y.AttributeType == typeof(MoonSharpHideMemberAttribute) || y.AttributeType == typeof(LunarBindHideAttribute)));
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(a => !a.IsSpecialName && a.Name != "ToString" && a.Name != "GetHashCode" && a.Name != "GetType" && a.Name != "Equals" && a.Name != "GetTypeCode")
                .Where(x => !x.CustomAttributes.Any(y => y.AttributeType == typeof(MoonSharpHiddenAttribute) || y.AttributeType == typeof(MoonSharpHideMemberAttribute) || y.AttributeType == typeof(LunarBindHideAttribute)));
            var funcs = type.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(a => !a.IsSpecialName && a.Name != "ToString" && a.Name != "GetHashCode" && a.Name != "GetType" && a.Name != "Equals" && a.Name != "GetTypeCode")
                .Where(x => !x.CustomAttributes.Any(y => y.AttributeType == typeof(MoonSharpHiddenAttribute) || y.AttributeType == typeof(MoonSharpHideMemberAttribute) || y.AttributeType == typeof(LunarBindHideAttribute)));
            //TODO: events
            //var events = type.GetEvents(BindingFlags.Instance | BindingFlags.Public)
            //    .Where(x => !x.CustomAttributes.Any(y => y.AttributeType == typeof(MoonSharpHiddenAttribute) || y.AttributeType == typeof(MoonSharpHideMemberAttribute) || y.AttributeType == typeof(LunarBindHideAttribute)));

            foreach (var field in fields)
            {
                string docu = "";
                string ex = "";

                var docAttrib = field.GetCustomAttribute<LunarBindDocumentationAttribute>();
                if (docAttrib != null)
                {
                    docu = docAttrib.Data;
                }
                var exampleAttrib = field.GetCustomAttribute<LunarBindExampleAttribute>();
                if (exampleAttrib != null)
                {
                    ex = exampleAttrib.Data;
                }

                var fdoc = DocumentInstanceObject(field.FieldType, field.Name, nextPrefix, curLevel + 1, type, docu, ex);
                if (fdoc != null)
                {
                    doc.SubDocs.Add(fdoc);
                }
            }

            foreach (var property in properties)
            {

                string docu = "";
                string ex = "";

                var docAttrib = property.GetCustomAttribute<LunarBindDocumentationAttribute>();
                if (docAttrib != null)
                {
                    docu = docAttrib.Data;
                }
                var exampleAttrib = property.GetCustomAttribute<LunarBindExampleAttribute>();
                if (exampleAttrib != null)
                {
                    ex = exampleAttrib.Data;
                }

                var pdoc = DocumentInstanceObject(property.PropertyType, property.Name, nextPrefix, curLevel + 1, type, docu, ex);
                if (pdoc != null)
                {
                    doc.SubDocs.Add(pdoc);
                }
            }

            foreach (var func in funcs)
            {
                var pdoc = DocumentFunction(func, nextPrefix);
                if (pdoc != null)
                {
                    doc.SubDocs.Add(pdoc);
                }
            }

            return doc;
        }

        private static DocItem DocumentEnum(BindEnum enu, string prefix)
        {
            string name = enu.Name;
            string fullName = prefix + name;
            string definition = fullName;
            string copy = definition;
            var nextPrefix = prefix + name + ".";

            var doc = new DocItem(DocItemType.Enum, enu.EnumType, name, fullName, definition, copy);
            var fields = enu.EnumType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            foreach (var field in fields)
            {
                var hidden = (LunarBindHideAttribute)Attribute.GetCustomAttribute(field, typeof(LunarBindHideAttribute)) != null ||
                    (MoonSharpHiddenAttribute)Attribute.GetCustomAttribute(field, typeof(MoonSharpHiddenAttribute)) != null;

                if (!hidden)
                {
                    doc.SubDocs.Add(DocumentInstanceObject(typeof(int), field.Name, nextPrefix, 0, enu.EnumType));
                }
            }
            return doc;
        }

        private static DocItem DocumentObject(BindUserObject obj, string prefix, int curLevel)
        {
            return DocumentInstanceObject(obj.UserObject.GetType(), obj.Name, prefix, curLevel, obj.UserObject.GetType());
        }


        private static string GetGenericString(Type genericType, bool full = false)
        {
            string genericStringRes = full ? $"{genericType.FullName.Split('`')[0]}<" : $"{genericType.Name.Split('`')[0]}<";

            var genericArgs = genericType.GenericTypeArguments;
            for (int i = 0; i < genericArgs.Length; i++)
            {

                if (genericArgs[i].IsGenericType)
                {
                    genericStringRes += GetGenericString(genericArgs[i], full);
                }
                else
                {
                    genericStringRes += full ? genericArgs[i].FullName : genericArgs[i].Name;
                }
                if (i < genericArgs.Length - 1)
                {
                    genericStringRes += ", ";
                }
            }
            return genericStringRes + ">";
        }

        private static string GetParamString(ParameterInfo[] parameterInfo, bool full = false)
        {
            List<string> paramSubList = new List<string>();

            for (int l = 0; l < parameterInfo.Length; l++)
            {
                var paramType = parameterInfo[l].ParameterType;
                if (paramType.IsGenericType)
                {
                    string str = GetGenericString(paramType, full) + " " + parameterInfo[l].Name;
                    if (parameterInfo[l].HasDefaultValue)
                    {
                        str += " = " + (parameterInfo[l].DefaultValue?.ToString() ?? "null");
                    }
                    paramSubList.Add(str);
                }
                else
                {
                    string str = (full ? paramType.FullName : paramType.Name) + " " + parameterInfo[l].Name;
                    if (parameterInfo[l].HasDefaultValue)
                    {
                        str += " = " + (parameterInfo[l].DefaultValue?.ToString() ?? "null");
                    }
                    paramSubList.Add(str);
                }
            }
            return string.Join(", ", paramSubList);
        }

        private static string GetParamNamesOnlyString(ParameterInfo[] parameterInfo)
        {
            List<string> paramSubList = new List<string>();
            for (int l = 0; l < parameterInfo.Length; l++)
            {
                paramSubList.Add(parameterInfo[l].Name);
            }
            return string.Join(", ", paramSubList);
        }

    }

    //Todo: move into separate files and add to other projects

    [Flags]
    public enum DocItemType
    {
        None = 0,
        Function = 1,
        InstanceObject = 2,
        Enum = 4,
        StaticType = 8,
        All = Function | Enum | StaticType | InstanceObject,
        Table = 16,
    }


    public class DocItem 
    {
        //Accessable for sorting
        public string Name { get; private set; }
        public string FullName { get; private set; }
        public List<DocItem> SubDocs { get; private set; } = new List<DocItem>();
        public DocItemType ItemType { get; internal set; } = DocItemType.None;

        /// <summary>
        /// The definition, complete with types
        /// </summary>
        public string DefinitionString { get; internal set; }
        /// <summary>
        /// The string to copy with a copy function
        /// </summary>
        public string CopyString { get; internal set; }
        public string DocumentationString { get; internal set; }
        public string Example { get; internal set; }
        public Type DeclaringType { get; internal set; }

        internal DocItem(DocItemType itemType) : this(itemType, null, "", "", "") { }

        internal DocItem(DocItemType itemType, Type declaringType, string name, string fullname, string definition, string copyString = "", string documentation = "", string example = "")
        {
            ItemType = itemType;
            Name = name;
            FullName = fullname;
            DeclaringType = declaringType;
            DefinitionString = definition;
            CopyString = copyString;
            DocumentationString = documentation;
            Example = example;
        }

        public void Sort()
        {
            SubDocs.Sort(new DocComparer());
            foreach (var doc in SubDocs)
            {
                doc.Sort();
            }
        }

        public override string ToString()
        {
            return Name;
        }
        //Sort tables lower than others

    }

    public class DocComparer : IComparer<DocItem>
    {
        public int Compare(DocItem x, DocItem y)
        {
            if(x.ItemType != DocItemType.InstanceObject 
                && x.ItemType != DocItemType.Enum
                && x.ItemType == y.ItemType)
            {
                return string.Compare(x.Name, y.Name);
            }
            else if(x.ItemType == DocItemType.InstanceObject)
            {
                if (y.ItemType == DocItemType.InstanceObject) {
                    //Intentional, check if conditions are equal
                    if (x.SubDocs.Count > 0 == y.SubDocs.Count > 0)
                    {
                        //Compare by str
                        return string.Compare(x.Name, y.Name);
                    }
                    else if(x.SubDocs.Count > 0)
                    {
                        return -1; //X above
                    }
                    else
                    {
                        return 1; //X below
                    }
                }
                else if(y.ItemType == DocItemType.Enum)
                {
                    return x.SubDocs.Count == 0 ? -1 : 1;
                }
                //use default
            }
            else if(x.ItemType == DocItemType.Enum)
            {
                if (y.ItemType == DocItemType.Enum)
                {
                    return string.Compare(x.Name, y.Name);
                }
                else if (y.ItemType == DocItemType.InstanceObject)
                {
                    return y.SubDocs.Count > 0 ? -1 : 1;
                }
                else
                {
                    return (int)x.ItemType < (int)y.ItemType ? -1 : 1;
                }
                //use default
            }

            //Default
            return (int)x.ItemType < (int)y.ItemType ? -1 : 1;
        }
    }


}
