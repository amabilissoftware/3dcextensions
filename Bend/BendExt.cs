namespace TDCOperation10
{
    using System;

    using ACSG;

    using ExtensionHelpers;

    using stdole;

    using TDCApplier;

    using TDCOperation10.Properties;

    // *********************************************************************************
    // This sample operation is an "operation control" based operation. It implements
    // a "Bend" operation.
    // Operation Control based operations are the simplest to implement since a
    // generic implementation of many of the functions are included in 3DC.
    // They also offer a lot of potential for new and innovative features.
    // This type of operation is "volume" based. 3DC determins which points are
    // in the volume and all you, as the programmer, need do is manipulate the points
    // provided. You neednt worry about the direction of the operation since 3DC
    // transforms the object such that to you it is always up (Y = 1).
    // If this is a "directional" function you will also be provided with the "strength"
    // of the effect on each point. This "strength" is between 0 and 1. This is the
    // distance from the root of the operation volume.
    // *********************************************************************************

    // *********************************************************************************
    // Note1 : Operations are a very restricted plug-in class. Operations have to be
    // "reapplyable" in order to support layer changes and animation. As a result
    // changes to the scene structure cant be supported.For example if you created
    // a new object and new group a reapply would result in the object and group
    // being re-created without the object and group from the previous application
    // being deleted. This would result in multiple objects and groups being created
    // even though the operation only intended to create one object and group.
    // Operations can only operate on the object sent. They cant affect its group
    // in any way. The only case where an operation affects group location is when
    // applied to a hierarchy or group of objects. In this very specific case
    // it is 3DC that is making the change and only in a very specific manner.
    // *********************************************************************************

    // *********************************************************************************
    // Note2: The API used here is not the same as documented in the 3DC help.
    // It is similar. No effort will be made to document the API since it will
    // be merged with the documented API in future releases.
    // *********************************************************************************1
    public class BendExt : Operation
    {
        // *********************************************************************************
        // Purpose:  Returns the author of the operation.
        // *********************************************************************************
        public string Author => "Amabilis Software";

        // *********************************************************************************
        // Purpose: Note that this only works for Amabilis created plug-ins.
        // All user-created operations will automatically be free only
        // *********************************************************************************
        public CSGFunctionCost Cost => CSGFunctionCost.CSGFunctionFree;

        // *********************************************************************************
        // Purpose: Returns the picture for the operation button
        // *********************************************************************************
        public StdPicture Icon => (StdPicture)IconConverter.ImageToPictureDisp(Resources.OperationBend);

        // *********************************************************************************
        // Purpose:  Returns whether the operation is destructive or not (changes the number
        // of points or faces). An operation is non-destructive if it just moves
        // points around.
        // *********************************************************************************
        public bool IsDestructive => false;

        // *********************************************************************************
        // Purpose:  Returns whether this is a "standard" feature. That is does it
        // automatically appear in the list of available operations or does
        // it need to be enabled via tools->options
        // *********************************************************************************
        public bool IsStandardFeature => true;

        // *********************************************************************************
        // Purpose:  Returns the class of the operation. 3DC uses this information
        // to decide where to put the operation in the list of available
        // operations.
        // *********************************************************************************
        public CSGLayerClass LayerClass => CSGLayerClass.CSGLayerOperationDeform;

        // *********************************************************************************
        // Purpose: Returns who is responsible for multi-shape application
        // If 3DC is then 3DC will merege selected shapes and provide it to the 
        // operation
        // *********************************************************************************
        public CSGOperationMultiShapeApplyMethod MultiShapeApplyMethod => CSGOperationMultiShapeApplyMethod.CSGOperationMultiShapeOperationDefined;

        // *********************************************************************************
        // Purpose: Returns the name of the operation as shown in the Operations
        // Layers List
        // *********************************************************************************
        public string Name => "Bend Extension";

        // *********************************************************************************
        // Purpose: Returns the tooltip help for the operation button
        // *********************************************************************************
        public string ToolTipText => "Bend Shape (Downloaded Extension)";

        // *********************************************************************************
        // Purpose:  Returns the operation OperationVersion number. When the operation is applied
        // via Modify the plug-in can tell what OperationVersion was originally applied
        // and re-apply in the same way.
        // *********************************************************************************
        public int Version => 100;

        // *********************************************************************************
        // Purpose: The actual implementation of the operation
        // Parameters (significant only)
        // OperationVersion:         Operations need to be re-applied. If you change your operation
        // you need to ensure that applications with a previous version
        // still reapply the same way. So, you will be supplied with the
        // version this data was applied with and you need to re-apply identically
        // ApplyMode:                What mode is this operation being applied - original application/
        // Reapply/Move/Rotate etc.
        // ApplyModeParameters:      Delta X/Y/Z/U/V for the Move/Rotate/Scale. These values are provided by
        // the edit control while using the "Operation Adjust" tool.
        // Texture:                  Texture to be applied (only supplied for material operations)
        // UserDataSingles:          The float parameter data - This will be the parameters you define
        // below and any items you want to add manually.
        // If the a parameter is "OperationControl" then the data locations 0-12
        // starting at ".DataSinglesEntry" are reserved for the operation
        // control data.
        // Usage for "operationcontrol" operations:
        // 0=the volume type. Usually not relevant for these types of
        // operations. since all we care is what points are selected
        // 0 = box 1 = cylinder 2 = sphere (sphere only applies
        // to non-directional operations.
        // 1=x origin
        // 2=y origin
        // 3=z origin
        // 4=euler x orientation (in radians)
        // 5=euler y orientation (in radians)
        // 6=euler z orientation (in radians)
        // 7=cube x size
        // 8=cube y size
        // 9=cube z size
        // 10=cylinder radius
        // 11=cylinder height
        // 12=sphere radius
        // UserDataInts:             The int parameter data - this is entirely available for your use
        // whatever you store here will be returned the next time the operation
        // is reapplied.
        // UserDataString            this is entirely available for your use
        // whatever you store here will be returned the next time the operation
        // is reapplied.
        // *********************************************************************************
        public bool Apply(
            CSG sceneGraph,
            int deviceId,
            int operationVersion,
            CSGOperationApplyMode applyMode,
            ref float[] applyModeParameters,
            ref float[] userDataSingles,
            ref int[] userDataInts,
            ref string userDataString,
            CSGTexture texture)
        {
            CSGVector point2 = new CSGVector();
            CSGVector[] pointList = null;
            int pointListCount = 0;
            var functions = new CSGFunctions();

            // we only have something to do if there is an actual bend angle
            if (userDataSingles[13] != 0)
            {
                var selectedShape = sceneGraph.GetSelectedShape(deviceId, 0);

                // determine the size of the "bend circle"
                // a box
                float effectLength;
                if (userDataSingles[0] == 0)
                {
                    effectLength = userDataSingles[8];
                }
                else
                {
                    // a cylinder
                    effectLength = userDataSingles[11];
                }

                // the arc of the bend circle must be the same length as the effect length
                // so figure it out using plain old math
                var radius = effectLength / (userDataSingles[13] / 360) / 2 / (float)Math.PI;

                // the axis is always the Z axis. The user can rotate the control if they want something else
                CSGVector axis;
                axis.X = 0;
                axis.Y = 0;
                axis.Z = -1;

                // get the object's points
                selectedShape.GetPointsXYZ(ref pointList, ref pointListCount);

                // run through the points and move 'em
                // not too shabby really only a dozen lines of code for the actual bend!
                // the hard stuff (operation control stuff) is all handled by 3DC forutunately.
                // SelectionItemsEffect is the "power" of the effect for each affected point. For a
                // directional operation this starts at the "base" (0) of the control and reaches 1 at
                // the top (and beyond)
                var selection = selectedShape.GetSubSelection(deviceId);
                for (int pointId = 0; pointId < selection.ItemListCount; pointId++)
                {
                    var primary = selection.ItemList[pointId].Primary;
                    CSGVector point;
                    point.X = -(radius - pointList[primary].X);
                    point.Y = pointList[primary].Y - effectLength * selection.ItemList[pointId].OperationControlEffect;
                    point.Z = pointList[primary].Z;
                    var vectorLength = functions.VectorModulus(ref point);
                    functions.VectorRotate(ref point2, ref point, ref axis, selection.ItemList[pointId].OperationControlEffect * userDataSingles[13] / 180 * (float)Math.PI);
                    functions.VectorScale(ref point, ref point2, vectorLength);
                    pointList[primary].X = point.X + radius;
                    pointList[primary].Y = point.Y;
                    pointList[primary].Z = point.Z;
                }

                // set the shape's points
                selectedShape.SetPointsXYZ(pointList);
            }

            return true;
        }

        // *********************************************************************************
        // Purpose:  Returns the parameters used by this operation.
        // *********************************************************************************
        public void GetParameters(CSG sceneGraph, int version, CSGSelectionType selectionType, ref CSGParameter[] parameters)
        {
            parameters = new CSGParameter[2];

            // Operation Control
            // note that if you have paramater of type "CSGOperationControlDirectional" or
            // "CSGOperationControl" a total of 13 (0-12) UserDataSingles are dedicated to it for internal use
            // this means your next available parameter must be the CSGOperationControl// s UserDataSinglesEntry+13
            // as is the Bend angle below
            // note that we don't have to add the "dropdownlistentries" since 3DC will do that for us
            // since it KNOWS what applies to what
            parameters[0].Type = CSGParameterType.CSGOperationControlDirectional;
            parameters[0].Title = "Control";
            parameters[0].DataSinglesEntry = 0;
            parameters[0].DropDownListEntries = null;
            parameters[0].NumberDecimalPlaces = 0;
            parameters[0].Default = 0;
            parameters[0].NumberMinimum = 0;
            parameters[0].NumberMaximum = 2;
            // Parameters[0].UserDefault = GetSetting(SceneGraph.ApplicationState.ApplicationRegistryKey, App.Title, Parameters[0].Title, Parameters[0].Default);
            parameters[0].UserDefault = parameters[0].Default; // user default relies on registry storage so it doesn't work in .NET operations

            // Bend Angle
            parameters[1].Type = CSGParameterType.CSGNumber;
            parameters[1].Title = "Angle";
            parameters[1].DataSinglesEntry = 13;
            parameters[1].DropDownListEntries = null;
            parameters[1].NumberDecimalPlaces = 3;
            parameters[1].Default = 45;
            parameters[1].NumberMinimum = -99999999;
            parameters[1].NumberMaximum = 99999999;
            parameters[1].Increment = 10;
            // Parameters[1].UserDefault = GetSetting(SceneGraph.ApplicationState.ApplicationRegistryKey, App.Title, Parameters[1].Title, Parameters[1].Default);
            parameters[1].UserDefault = parameters[1].Default; // user default relies on registry storage so it doesn't work in .NET operations

        }

        // *********************************************************************************
        // Purpose:  Displays and permits the user to enter options for the operation. This
        // is now mostly obsolete. Options are usually declared below. This
        // permits 3DC to create the entry forms automatically. But, there are
        // operations, such as the Particle operation, that need their own form
        // since they are too complex for the declaritive method.
        // Mode indicates whether 3DC is in Modelling Mode or Animation Mode.
        // *********************************************************************************
        public void ShowParameters(CSG sceneGraph, int version, CSGSelectionType selectionType)
        {
        }

        // *********************************************************************************
        // Purpose: Returns whether 3DC supports a particular operation function
        // This sort of overlaps with Operation_SupportSelectionType above, but
        // provides more details as to specifically what is supported for what types
        // of functions.
        // *********************************************************************************
        public bool SupportApplyMode(int version, CSGOperationApplyMode applyMode, CSGSelectionType selectionType)
        {
            switch (applyMode)
            {
                case CSGOperationApplyMode.CSGOperationApplyCreate:
                case CSGOperationApplyMode.CSGOperationApplyReapply:
                    {
                        return true;
                    }

                default:
                    {
                        return false;
                    }
            }
        }

        // *********************************************************************************
        // Purpose:  Returns whether a selection type is supported or not.
        // *********************************************************************************
        public bool SupportSelectionType(CSGSelectionType selectionType)
        {
            if (selectionType == CSGSelectionType.CSGSelectShape)
            {
                return true;
            }

            return false;
        }
    }
}