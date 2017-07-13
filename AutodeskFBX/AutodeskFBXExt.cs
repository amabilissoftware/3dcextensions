namespace TDCPluginImportExport10
{
    #region

    using ACSG;

    using AutoDeskFBXSDK;

    using stdole;

    using TDCPlugin;

    #endregion

    /// <summary>
    /// The implementation of a Autodesk FBX Import/Export plug-in. 
    /// If you change the class name, change the assembly name to match
    /// </summary>
    public class AutodeskFBXExt : PluginImportExport
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
        ///   Gets file type exported by this plug-in.
        /// </summary>
        public string FileType => "fbx";

        /// <summary>
        ///   Gets the description of the file type exported by this plug-in.
        /// </summary>
        public string FileTypeDescription => "Autodesk FBX (Downloaded Extension)";

        /// <summary>
        ///   Gets the plug-in Icon. (currently not supported)
        /// </summary>
        public StdPicture Icon => null;

        /// <summary>
        ///   Gets the class of plug-ins that this plug-in belongs to.  This will place it in the right menu location.
        /// </summary>
        public CSGPluginImportExportClass PluginClass => CSGPluginImportExportClass.CSGPluginImportExportUnclassified;

        /// <summary>
        ///   Gets a value indicating whether exports are supported.
        /// </summary>
        public bool SupportExport => true;

        /// <summary>
        ///   Gets a value indicating whether imports are supported.
        /// </summary>
        public bool SupportImport => false;

        public void Export(
            string filename,
            CSGGroup group,
            CSG sceneGraph,
            CSGApplicationState applicationState,
            ref float[] userDataFloats,
            ref int[] userDataInts,
            ref string userDataString)
        {
            Exporter exporter = new Exporter();
            exporter.Export(filename, sceneGraph, group);
        }

        public void Import(
            string importFileName,
            CSGGroup importGroup,
            CSG sceneGraph,
            CSGApplicationState applicationState,
            ref float[] userDataFloats,
            ref int[] userDataInts,
            ref string userDataString)
        {
            // if importing, set SupportImport to return true and add your import code
        }
    }
}