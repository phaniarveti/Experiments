using System;

using System.Xml;
using System.Xml.XPath;
using System.Reflection;
using Umbraco.Core.IO;
using umbraco.BusinessLogic;
using umbraco.BasePages;

namespace umbraco.presentation.create
{
    /// <summary>
    /// Summary description for dialogHandler_temp.
    /// </summary>
    public class dialogHandler_temp
    {
        public dialogHandler_temp()
        {
        }

        public static void Delete(string NodeType, int NodeId)
        {
            Delete(NodeType, NodeId, "");
        }
        public static void Delete(string NodeType, int NodeId, string Text)
        {
            // Load task settings
            XmlDocument createDef = GetXmlDoc();

            // Create an instance of the type by loading it from the assembly
            XmlNode def = createDef.SelectSingleNode("//nodeType [@alias = '" + NodeType + "']");
            string taskAssembly = def.SelectSingleNode("./tasks/delete").Attributes.GetNamedItem("assembly").Value;
            string taskType = def.SelectSingleNode("./tasks/delete").Attributes.GetNamedItem("type").Value;

            Assembly assembly = Assembly.LoadFrom( IOHelper.MapPath(SystemDirectories.Bin + "/" + taskAssembly + ".dll"));
            Type type = assembly.GetType(taskAssembly + "." + taskType);
            interfaces.ITask typeInstance = Activator.CreateInstance(type) as interfaces.ITask;
            if (typeInstance != null)
            {
                typeInstance.ParentID = NodeId;
                typeInstance.Alias = Text;
                typeInstance.Delete();
            }
        }

        public static string Create(string NodeType, int NodeId, string Text)
        {
            return Create(NodeType, 0, NodeId, Text);
        }

        public static string Create(string NodeType, int TypeId, int NodeId, string Text)
        {

            // Load task settings
            XmlDocument createDef = GetXmlDoc();

            // Create an instance of the type by loading it from the assembly
            XmlNode def = createDef.SelectSingleNode("//nodeType [@alias = '" + NodeType + "']");
            string taskAssembly = def.SelectSingleNode("./tasks/create").Attributes.GetNamedItem("assembly").Value;
            string taskType = def.SelectSingleNode("./tasks/create").Attributes.GetNamedItem("type").Value;

            Assembly assembly = Assembly.LoadFrom( IOHelper.MapPath(SystemDirectories.Bin + "/" + taskAssembly + ".dll"));
            Type type = assembly.GetType(taskAssembly + "." + taskType);
            var typeInstance = Activator.CreateInstance(type) as interfaces.ITask;
            if (typeInstance != null)
            {
                typeInstance.TypeID = TypeId;
                typeInstance.ParentID = NodeId;
                typeInstance.Alias = Text;
                typeInstance.UserId = BasePages.BasePage.GetUserId(BasePages.BasePage.umbracoUserContextID);
                typeInstance.Save();

                // check for returning url
                try
                {
                    return ((interfaces.ITaskReturnUrl)typeInstance).ReturnUrl;
                }
                catch
                {
                    return "";
                }
            }

            return "";
        }

        private static XmlDocument GetXmlDoc()
        {
            // Load task settings
            XmlDocument createDef = new XmlDocument();
            XmlTextReader defReader = new XmlTextReader(IOHelper.MapPath(SystemFiles.CreateUiXml));
            createDef.Load(defReader);
            defReader.Close();
            return createDef;
        }

    }
}
