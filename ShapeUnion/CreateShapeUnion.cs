namespace TDCPlugin10
{
    using ACBO;
    using ACSG;
    using stdole;
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using TDCPlugin;

    using TDCScripting;

    public class CreateShapeUnion : Plugin
    {
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory([In, Out] CSGVector[] pdst, ACBO.CBODataTypes.CSGVector[] psrc, int cb);
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory([In, Out]  ACBO.CBODataTypes.CSGVector[] pdst, CSGVector[] psrc, int cb);

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory([In, Out] CSGUV[] pdst, ACBO.CBODataTypes.CSGUV[] psrc, int cb);
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory([In, Out]  ACBO.CBODataTypes.CSGUV[] pdst, CSGUV[] psrc, int cb);

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
        public string Name => "Create Shape Union (Downloaded Extension)";

        /// <summary>
        ///   Gets the class of plug-ins that this plug-in belongs to. This will place it in the right menu location.
        /// </summary>
        public CSGPluginClass PluginClass => CSGPluginClass.CSGPluginUnclassified;

        public void Apply(CSScene obsolete, CSG sceneGraph, int deviceId, ref float[] userDataFloats, ref int[] userDataInts, ref string userDataString)
        {
            var selectedShapes = sceneGraph.GetSelectedShapes(sceneGraph.ApplicationState.SceneDeviceID);
            CSGShape targetShape = selectedShapes.GetElement(0);
            CSGShape toolShape = selectedShapes.GetElement(1);
            var result = DoStuff(sceneGraph, targetShape, toolShape);
            targetShape.Parent.AddShape(result);
        }

        public void GetValidation(
            CSGEntityType componentType,
            CSGSelectionType selectionType,
            ref int validMinComponents,
            ref int validMaxComponents,
            ref int validMinShapeSubSelection,
            ref int validMaxShapeSubSelection)
        {
            if (selectionType == CSGSelectionType.CSGSelectShape)
            {
                validMinComponents = 2;
                validMaxComponents = 2;
            }
            else
            {
                validMaxComponents = 0;
            }
        }

        private CSGShape DoStuff(CSG sceneGraph, CSGShape targetShape, CSGShape toolShape)
        {
            var csgFunctions = new CSGFunctions();

            CSGVector[] PointList = null;
            int PointListCount = 0;
            CSGVector[] NormalList = null;
            int NormalListCount = 0;
                CSGUV[] TextureCoordinateList = null;
            int TextureCoordinateListCount = 0;
            int[] faceData = null;
            int faceDataCount = 0;
            int[] faceColorList = null;
            int faceColorListCount = 0;
            targetShape.x_GetGeometryBoolean(ref PointList, ref PointListCount, ref NormalList, ref NormalListCount 
            , ref TextureCoordinateList, ref TextureCoordinateListCount, ref faceData, ref faceDataCount, ref faceColorList, ref faceColorListCount);

            CSGVector[] transformedPointList = null;
            targetShape.Parent.TransformVectors(ref transformedPointList, ref PointList, targetShape.Parent.GetScene());

            CSGVector[] PointList2 = null;
            int PointList2Count = 0;
            CSGVector[] NormalList2 = null;
            int NormalList2Count = 0;
                CSGUV[] TextureCoordinateList2 = null;
            int TextureCoordinateList2Count = 0;
            int[] faceData2 = null;
            int faceDataCount2 = 0;
            int[] faceColorList2 = null;
            int faceColorList2Count = 0;
            toolShape.x_GetGeometryBoolean(ref PointList2, ref PointList2Count, ref NormalList2, ref NormalList2Count
            , ref TextureCoordinateList2, ref TextureCoordinateList2Count, ref faceData2, ref faceDataCount2, ref faceColorList2, ref faceColorList2Count);

            CSGVector[] transformedPointList2 = null;
            toolShape.Parent.TransformVectors(ref transformedPointList2, ref PointList2, toolShape.Parent.GetScene());


            Debugger.Break();
            CBODataTypes.CSGVector[] aPointList = new CBODataTypes.CSGVector[PointListCount];
            CopyMemory(aPointList, transformedPointList, aPointList.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(CBODataTypes.CSGVector)));
            Debugger.Break();

            CBODataTypes.CSGVector[] aNormalList = new CBODataTypes.CSGVector[NormalListCount];
            CopyMemory(aNormalList, NormalList, aNormalList.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(CBODataTypes.CSGVector)));

            CBODataTypes.CSGUV[] aTextureCoordinateList = new CBODataTypes.CSGUV[TextureCoordinateListCount];
            CopyMemory(aTextureCoordinateList, TextureCoordinateList, aTextureCoordinateList.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(CBODataTypes.CSGUV)));

            CBODataTypes.CSGVector[] aPointList2 = new CBODataTypes.CSGVector[PointList2Count];
            CopyMemory(aPointList2, transformedPointList2, aPointList2.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(CBODataTypes.CSGVector)));

            CBODataTypes.CSGVector[] aNormalList2 = new CBODataTypes.CSGVector[NormalList2Count];
            CopyMemory(aNormalList2, NormalList2, aNormalList2.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(CBODataTypes.CSGVector)));

            CBODataTypes.CSGUV[] aTextureCoordinateList2 = new CBODataTypes.CSGUV[TextureCoordinateList2Count];
            CopyMemory(aTextureCoordinateList2, TextureCoordinateList2, aTextureCoordinateList2.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(CBODataTypes.CSGUV)));

            Debugger.Break();

            int lFaceMax3 = 0;
            CBODataTypes.CSGVector[] acvaCompactedVertices3 = null;
            int[] laFaceTuples3 = null;
            bool[] faceDeleted3 = null;
            int[] faceColor3 = null;

            var cbo = new CBO();
            cbo.ProcessToolOperation(0
            , ref aPointList
            , PointListCount
            , ref aNormalList
            , NormalListCount
            , ref aTextureCoordinateList
            , TextureCoordinateListCount
            , ref faceData
            , faceDataCount
            , ref faceColorList
            , faceColorListCount
            , ref aPointList2
            , PointList2Count
            , ref aNormalList2
            , NormalList2Count
            , ref aTextureCoordinateList2
            , TextureCoordinateList2Count
            , ref faceData2
            , faceDataCount2
            , ref faceColorList2
            , faceColorList2Count
            , ref lFaceMax3, ref acvaCompactedVertices3
            , ref laFaceTuples3
            , ref faceDeleted3
            , ref faceColor3);


            CSGShape result = null;
            if (lFaceMax3 > 0)
            {
                CSGVector[] cvaCompactedVertices3 = new CSGVector[acvaCompactedVertices3.Length];
                CopyMemory(cvaCompactedVertices3, acvaCompactedVertices3, cvaCompactedVertices3.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(CSGVector)));

                Debugger.Break();
                result = csgFunctions.csCreateShapeFromCGMeshData(sceneGraph, lFaceMax3, cvaCompactedVertices3, laFaceTuples3, faceDeleted3, faceColor3);
                CSGShape thing = sceneGraph.CreateShape();
                Console.WriteLine("{0} {1} {2}", thing.GetFaceCount(), thing.GetPointCount(), thing.GetNormalCount());
                Debugger.Break();
            }
            return result;
        }
    }
}
