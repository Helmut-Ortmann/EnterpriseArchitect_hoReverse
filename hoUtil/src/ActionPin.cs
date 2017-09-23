﻿using System;
using EA;

// ReSharper disable once CheckNamespace
namespace hoReverse.hoUtils.ActionPins
{
    //--------------------------------------------------------------------------------------------
    // Update Action Pins from Operation
    //--------------------------------------------------------------------------------------------

    public static class ActionPin
    {

        //---------------------------------------------------------------------------------------------
        // updateActionParameter(EA.Repository rep, EA.Element actionPin)
        //---------------------------------------------------------------------------------------------
        public static bool UpdateActionPinParameter(Repository rep, EA.Element action)
        {
            foreach (EA.Element actionPin in action.EmbeddedElements)
            {
                // pin target for the return type of the action
                if (actionPin.Name == "target")
                {
                    //// return type
                    //Int32 parTypeID = Util.getTypeID(rep, m.ReturnType);
                    //if (parTypeID != 0)
                    //{
                    //    //pin.Name = par.
                    //    pin.ClassfierID = parTypeID;
                    //    EA.Element el = rep.GetElementByID(parTypeID);
                    //    pin.Update(); // do it before update table
                    //    Util.setElementPDATA1(rep, pin, el.ElementGUID);// set PDATA1

                    //}
                }
                else
                {
                    // get type of synchronized parameter
                    // if parameter isn't synchronized it will not work
                    string type = HoUtil.GetParameterType(rep, actionPin.ElementGUID);
                    if (type == "")
                    {
                        string txt = "No type is available for action:'" + action.Name + "'";
                        rep.WriteOutput("hoReverse", txt, 0);
                    }
                    else
                    {
                        Int32 parTypeId = HoUtil.GetTypeId(rep, type);
                        if (parTypeId != 0)
                        {
                            //pin.Name = par.
                            EA.Element el = rep.GetElementByID(parTypeId);
                            HoUtil.SetElementPdata1(rep, actionPin, el.ElementGUID);// set PDATA1 
                        }
                    }
                }
            }

            return true;
        }

        //public static bool updateActionPin(EA.Repository rep, EA.Element el) {
        //    // get classifier (operation)
        //    EA.Method m = Util.getOperationFromCallAction(rep, el);

        //    // update action pins
        //    if (m != null)
        //    {
        //        foreach (EA.Parameter par in m.Parameters)
        //        {
        //            updateActionPinParameter(rep, m, el, par);
        //        }
        //    }
        //    return true;
        //}

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static bool UpdateActionPinForElement(Repository rep, EA.Element el1)
        {
            if (el1.Type == "Action")
            {
                UpdateActionPinParameter(rep, el1);
                return true;
            }
            if (el1.Type == "Class" | el1.Type == "Interface")
            {
                return true;
            }
            foreach (EA.Element el in el1.Elements)
            {   // update parameter
                if (el.Type == "Action")
                {
                    UpdateActionPinParameter(rep, el);

                }
                if (el.Type == "Activity")
                {
                    UpdateActionPinForElement(rep, el);
                }
            }

            return true;
        }
        //----------------------------------------------------------------------------
        // updateActionPinForPackage(EA.Repository rep, EA.Package pkg)
        //----------------------------------------------------------------------------

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static bool UpdateActionPinForPackage(Repository rep, Package pkg)
        {
            foreach (EA.Element el in pkg.Elements)
            {   // update parameter
                if (el.Type == "Action")
                {
                    UpdateActionPinParameter(rep, el);
                    rep.RefreshModelView(pkg.PackageID); // reload package
                }
                if (el.Type == "Activity")
                {
                    foreach (EA.Element elSub in el.Elements)
                    {
                        UpdateActionPinForElement(rep, elSub);
                    }
                }
            }
            foreach (Package pkgSub in pkg.Packages)
            {
                // update all packages
                UpdateActionPinForPackage(rep, pkgSub);
            }
            return true;

        }
    }
}
