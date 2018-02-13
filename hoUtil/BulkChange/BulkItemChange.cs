﻿using System;
using System.Linq;
using EA;
using hoReverse.hoUtils.Diagrams;

namespace hoUtils.BulkChange
{
    public class BulkItemChange
    {
        /// <summary>
        /// Bulk change all Elements in package. It also supports recursion of package and elements
        /// </summary>
        /// <param name="bulkElement"></param>
        /// <param name="pkg"></param>
        /// <param name="pkgRecursive"></param>
        /// <param name="elRecursive"></param>
        public static void BulkChangePackage(BulkElementItem bulkElement, EA.Package pkg, bool pkgRecursive, bool elRecursive)
        {
            // All selected elements in tree
            foreach (EA.Element el in pkg.Elements)
            {
                BulkChangeElement(bulkElement, el, recursive:elRecursive);
            }

            if (pkgRecursive)
            {
                foreach (EA.Package pkgSub in pkg.Packages)
                {
                    BulkChangePackage(bulkElement, pkgSub, pkgRecursive: true, elRecursive: elRecursive);
                }
            }

        }

        /// <summary>
        /// Bulk change selected Diagram objects
        /// - Selected diagram objects
        /// - Selected tree selected elements
        /// - Selected package, recursive all elements
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="bulkElement"></param>
        public static void BulkChange(EA.Repository rep, BulkElementItem bulkElement)
        {
            var eaDia = new EaDiagram(rep);
            // all selected elements in diagram
            foreach (EA.DiagramObject diaObj in eaDia.SelObjects)
            {
                EA.Element el = rep.GetElementByID(diaObj.ElementID);
                BulkChangeElement(bulkElement, el);
            }
            // All selected elements in tree
            foreach (EA.Element el in eaDia.TreeSelectedElements)
            {
                BulkChangeElement(bulkElement, el);
            }

            if (eaDia.TreeSelectedPackage != null)
            {
                BulkChangePackage(bulkElement, eaDia.TreeSelectedPackage, pkgRecursive:false, elRecursive:true);
                
            }

        }
        /// <summary>
        /// Change an Element:
        /// - Stereotype
        /// - Property
        /// - Tagged Value
        /// </summary>
        /// <param name="bulkElement"></param>
        /// <param name="el"></param>
        /// <param name="recursive"></param>
        public static void BulkChangeElement(BulkElementItem bulkElement, Element el, bool recursive=false)
        {
            // Check if bulk change is to apply for the current element
            if (BulkChangeCheck(bulkElement, el))
            {
                // Apply changes to the current element
                // Stereotype
                el.StereotypeEx = String.Join(",", bulkElement.StereotypesApply);
                el.Update();

                // Tagged Values
                foreach (var tag in bulkElement.TaggedValuesApply)
                {
                    string name = tag.Name.Trim();
                    string value = tag.Value.Trim();
                    if (name == "") continue;
                    foreach (EA.TaggedValue tg in el.TaggedValues)
                    {
                        // Check complete name and name without namespace
                        char[] c = {':', ':'};
                        string nameWithoutNamespace = tg.FQName.Split(c).Last();
                        if (tg.FQName == name || nameWithoutNamespace == name)
                        {
                            tg.Value = value;
                        }

                        tg.Update();
                    }

                    el.TaggedValues.Refresh();
                }
                // Properties
                foreach (string s in bulkElement.PropertiesApply)
                {
                    var l = s.Split('=');
                    string propertyName = l[0];
                    string propertyValue = l[1];
                    switch (propertyName)
                    {
                        case "Priority":
                            el.Priority = propertyValue;
                            break;
                        case "Complexity":
                            el.Complexity = propertyValue;
                            break;
                        case "GenFile":
                            el.Genfile = propertyValue;
                            break;
                        case "Version":
                            el.Version = propertyValue;
                            break;
                        case "Phase":
                            el.Phase = propertyValue;
                            break;
                        case "Difficulty":
                            el.Difficulty = propertyValue;
                            break;
                        case "Alias":
                            el.Alias = propertyValue;
                            break;
                        case "Status":
                            el.Status = propertyValue;
                            break;
                        case "Tag":
                            el.Tag = propertyValue;
                            break;
                        case "Author":
                            el.Author = propertyValue;
                            break;
                        case "GenType":
                            el.Gentype = propertyValue;
                            break;
                        case "Multiplicity":
                            el.Multiplicity = propertyValue;
                            break;
                        case "Visibility":
                            el.Visibility = propertyValue;
                            break;
                        case "StyleEx":
                            el.StyleEx = propertyValue;
                            break;
                    
                    }

                }

                el.Update();
                
            }
            // check recursive
            if (recursive)
            {
                foreach (EA.Element elSub in el.Elements)
                {
                    BulkChangeElement(bulkElement, elSub, recursive: true);
                }
            }
        }
        /// <summary>
        /// Check of for current element an bulk change is to apply. It checks Stereotype and Type.
        /// </summary>
        /// <param name="bulkElement">Checking rules</param>
        /// <param name="el">Current EA element to check</param>
        /// <returns></returns>
        public static bool BulkChangeCheck(BulkElementItem bulkElement, EA.Element el)
        {
            // Check if for the current element type the change is to apply
            if (bulkElement.TypesCheck == null ||
                bulkElement.TypesCheck.Count == 0 ||
                (bulkElement.TypesCheck.Count == 1 && bulkElement.TypesCheck[0].Trim() == "") ||
                bulkElement.TypesCheck.Contains(el.Type))
            {
                // Check if for the current element stereotype the change is to apply
                if (bulkElement.StereotypesCheck == null || 
                    bulkElement.StereotypesCheck.Count == 0 ||
                    (bulkElement.StereotypesCheck.Count == 1 && bulkElement.StereotypesCheck[0].Trim() == "") ||
                    bulkElement.StereotypesCheck.Contains(el.Stereotype))
                    return true;
            }

            return false;
        }
    }
}
