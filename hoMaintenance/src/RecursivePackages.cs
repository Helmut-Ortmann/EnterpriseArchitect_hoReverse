using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hoMaintenance
{
    /// <summary>
    /// Recursive to execute functions for Package, Diagram, Element. You can pass an string[] to give parameters.
    /// <para>- setPackage(EA.Repository rep, EA.Package pkg, string[] s) </para>
    /// <para>- setElement(EA.Repository rep, EA.Element el, string[] s) </para>
    /// <para>- setDiagram(EA.Repository rep, EA.Diagram dia, string[] s)</para>
    /// </summary>
    public class RecursivePackages
    {
        public delegate void setPackage(EA.Repository rep, EA.Package pkg, string[] s);
        public delegate void setElement(EA.Repository rep, EA.Element el, string[] s);
        public delegate void setDiagram(EA.Repository rep, EA.Diagram dia, string[] s);

        /// <summary>
        /// Recursive over package and elements with delegate for:
        /// <para>- Package</para>
        /// <para>- Element</para>
        /// <para>- Diagram</para>
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="pkg"></param>
        /// <param name="setPkg"></param>
        /// <param name="setEl"></param>
        /// <param name="setDia"></param>
        /// <param name="s"></param>
        public static void doRecursivePkg(EA.Repository rep, EA.Package pkg, setPackage setPkg, setElement setEl, setDiagram setDia, string[] s)
        {
            // perform package
            if (setPkg != null) setPkg(rep, pkg, s);

            // perform diagram
            if (setDia != null) {
                // perform diagrams of package
                for (short idx = (short) (pkg.Diagrams.Count -1); idx >=0; idx--)
                {
                    setDia(rep, (EA.Diagram)pkg.Diagrams.GetAt(idx), s);
                }
            }
            // perform element
            if (setEl != null | setDia != null) {
                // run elements of package
                for (short idx = (short)(pkg.Elements.Count - 1); idx >= 0; idx--)
                {
                    doRecursiveEl(rep, (EA.Element)pkg.Elements.GetAt(idx), setEl, setDia, s);
                }
            }

            // run packages of package
            for (short idx = (short)(pkg.Packages.Count - 1); idx >= 0; idx--)
            {
                doRecursivePkg(rep, (EA.Package)pkg.Packages.GetAt(idx), setPkg, setEl, setDia, s);
            }
            return;
        }
        /// <summary>
        /// Recursive over elements with delegate for:
        /// <para>- Element</para>
        /// <para>- Diagram</para>
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="pkg"></param>
        /// <param name="setPkg"></param>
        /// <param name="setEl"></param>
        /// <param name="setDia"></param>
        /// <param name="s"></param>
        public static void doRecursiveEl(EA.Repository rep, EA.Element el, setElement setEl, setDiagram setDia, string[] s)
        {
            // performel
            if (setEl != null) setEl(rep, el, s);
            //run all elements
            if (el.Elements.Count != 0)
            {
                for (short idx = (short)(el.Elements.Count - 1); idx >= 0; idx--)
                {
                    doRecursiveEl(rep, (EA.Element)el.Elements.GetAt(idx), setEl, setDia, s);
                }
            }
            return;
        }
    }

}
