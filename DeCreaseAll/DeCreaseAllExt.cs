namespace TDCPlugin10
{
    using System.Windows.Forms;

    using ACSG;

    using stdole;

    using TDCPlugin;

    using TDCScripting;

    using DeCreaseAll;

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
            DeCreaseAll.CreaseGroupRecursively(sceneGraph.ApplicationState.Scene, ref shapesCreasedCount);
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
    }
}