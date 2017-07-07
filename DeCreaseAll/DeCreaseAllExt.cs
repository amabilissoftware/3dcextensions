namespace TDCPlugin9
{
    using System.Windows.Forms;

    using ACSG;

    using stdole;

    using TDCPlugin;

    using TDCScripting;

    public class DeCreaseAllExt : Plugin
    {
        /// <summary>
        ///   Gets the author of the plug-in.
        /// </summary>
        public string Author => "Amabilis Software";

        /// <summary>
        ///   Gets cost of this plug-in. (Should always be CSGFunctionFree or some people won't be able to use your extension)
        /// </summary>
        public CSGFunctionCost Cost => CSGFunctionCost.CSGFunctionFree;

        /// <summary>
        ///   Gets the plug-in Icon. (currently not supported)
        /// </summary>
        public StdPicture Icon => null;

        /// <summary>
        ///   Gets the name of the plug-in. This will show up in a menu.
        /// </summary>
        /// 
        public string Name => "De-Crease All (Downloaded Extension)";

        /// <summary>
        ///   Gets the class of plug-ins that this plug-in belongs to. This will place it in the right menu location.
        /// </summary>
        public CSGPluginClass PluginClass => CSGPluginClass.CSGPluginUnclassified;

        public void Apply(CSScene obsolete, CSG sceneGraph, int deviceId, ref float[] userDataFloats, ref int[] userDataInts, ref string userDataString)
        {
            int shapesCreasedCount = 0;
            this.CreaseGroupRecursively(sceneGraph.ApplicationState.Scene, ref shapesCreasedCount);
            MessageBox.Show(string.Format("Shapes De-Creased: {0}", shapesCreasedCount));
        }

        public void GetValidation(
            CSGEntityType componentType,
            CSGSelectionType selectionType,
            ref int validMinComponents,
            ref int validMaxComponents,
            ref int validMinShapeSubSelection,
            ref int validMaxShapeSubSelection)
        {
            validMinComponents = 0;
            validMaxComponents = 999;
            validMinShapeSubSelection = 0;
            validMaxShapeSubSelection = 999;
        }

        private void CreaseGroupRecursively(CSGGroup group, ref int shapesCreasedCount)
        {
            CSGShapeArray childShapeList = group.GetShapes();
            for (int shapeIndex = 0; shapeIndex < childShapeList.GetSize(); shapeIndex++)
            {
                this.CreaseShape(childShapeList.GetElement(shapeIndex), ref shapesCreasedCount);
            }

            CSGGroupArray childGroupList = group.GetChildren();
            for (int groupIndex = 0; groupIndex < childGroupList.GetSize(); groupIndex++)
            {
                this.CreaseGroupRecursively(childGroupList.GetElement(groupIndex), ref shapesCreasedCount);
            }
        }

        private void CreaseShape(CSGShape shape, ref int shapesCreasedCount)
        {
            if (shape.RepresentativeEntityType == CSGEntityType.CSGEShape && shape.ShapeType == CSGShapeType.CSGShapeStandard && shape.GetFaceCount() > 0)
            {
                shape.Crease2(180);
                shapesCreasedCount++;
            }
        }
    }
}