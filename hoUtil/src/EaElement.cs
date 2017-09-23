using System.Collections.Generic;
using EA;


// ReSharper disable once CheckNamespace
namespace hoReverse.hoUtils
{
    /// <summary>
    /// Item to delete an EA item which is part of a collection
    /// </summary>
    public class EmbeddedItem
    {
        public string Name { get; }
        public short Pos { get; }
        public string Stereotype { get; }
        public string Type { get; }

        public EmbeddedItem(short pos, string name, string type, string stereotype)
        {
            Name = name;
            Stereotype = stereotype;
            Type = type;
            Pos = pos;
        }
    }
    public static class EaElement
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="pkg"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="stereotype"></param>
        /// <returns></returns>
        public static EA.Element CreateElement(Repository rep, Package pkg, string name, string type, string stereotype)
        {
            var el = CallOperationAction.GetElementFromName(rep, name, type);

            if (el == null)
            {
                el = (EA.Element)pkg.Elements.AddNew(name, type);
                pkg.Elements.Refresh();
            }
            if (stereotype != "")
            {
                if (el.Stereotype != stereotype)
                {
                    el.Stereotype = stereotype;
                    el.Update();
                }
            }
            return el;
        }
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static EA.Element CreatePortWithInterface(EA.Element elSource, EA.Element elInterface, string ifType ="RequiredInterface")
        {
            foreach (EA.Element p in elSource.EmbeddedElements)
            {
                if (p.Name == elInterface.Name) return null; //interface exists
            }
            // create a port
            var port = (EA.Element)elSource.EmbeddedElements.AddNew(elInterface.Name, "Port");
            // add interface
            var interf = (EA.Element)port.EmbeddedElements.AddNew(elInterface.Name, ifType);
            // set classifier
            interf.ClassfierID = elInterface.ElementID;
            interf.Update();
            return interf;
        }


        /// <summary>
        /// Get List of Embedded Elements
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public static List<EmbeddedItem> GetEmbeddedElements(EA.Element el) {
            var embeddedElements = new List<EmbeddedItem>();
            for (int i = el.EmbeddedElements.Count - 1; i >= 0; i--)
            {
                short iShort = (short) i;
                EA.Element elEmbedded = (EA.Element)el.EmbeddedElements.GetAt(iShort);
                string name = elEmbedded.Name;
                embeddedElements.Add(new EmbeddedItem(iShort, elEmbedded.Name, elEmbedded.Type, elEmbedded.Stereotype ));
            }
            return embeddedElements;
        }
    }


   
}
