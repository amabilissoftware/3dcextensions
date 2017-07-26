namespace ACBO
{
    using SharpDX;
    using System;

    public class CBODataTypes
    {
        #region Enums

        public enum CSGFaceType
        {
            CSGFaceUnknown = 0, 

            CSGFaceInterior = 1, 

            CSGFaceExterior = 2
        }

        public enum CSGToolOperation
        {
            CSGToolOpUnion = 0, 

            CSGToolOpIntersection = 1, 

            CSGToolOpDifference = 2, 

            CSGToolOpBlend = 3, 

            CSGToolOpBridge = 4, 

            CSGToolOpMerge = 5, 

            CSGToolOpCut = 6
        }

        #endregion

        public struct CSGCGFace
        {
            #region Fields

            public bool bDeleted; // has the face been deleted

            public Vector3[] cvEdgeBoundingPlaneNormal; // the edge's bounding plane normal

            public Vector3[] cvEdgeBoundingPlanePoint; // point on the the edge's bounding plane

            public Vector3 cvNormal; // the face's normal

            // in a intersection is this unknown/interior/exterior
            public int lColor; // the face's color

            public int[] lEdgeProcessed;

            public int[] lVertex; // an index to the Shape's vertex list

            public CSGFaceType udtIntersection;

            #endregion

            #region Public Methods and Operators

            public static CSGCGFace CreateInstance()
            {
                var result = new CSGCGFace();
                return result;
            }

            #endregion
        }

        public struct CSGMinMaxVector
        {
            #region Fields

            public Vector3 Max;

            public Vector3 Min;

            #endregion

            #region Public Methods and Operators

            public static CSGMinMaxVector CreateInstance()
            {
                var result = new CSGMinMaxVector();
                return result;
            }

            #endregion
        }

        public struct CSGRay
        {
            #region Fields

            public Vector3 Direction;

            public Vector3 Position;

            #endregion

            #region Public Methods and Operators

            public static CSGRay CreateInstance()
            {
                var result = new CSGRay();
                return result;
            }

            #endregion
        }

        [Serializable]
        public struct CSGUV
        {
            #region Fields

            public float U;

            public float V;

            #endregion
        }

        [Serializable]
        public struct CSGVector
        {
            #region Fields

            public float X;

            public float Y;

            public float Z;

            #endregion
        }

        public struct i_CSGPoint
        {
            #region Fields

            public int D3DVertexListCount;

            public int[] D3DVertexListID;

            public int EdgeListCount;

            public int[] EdgeListID;

            public int FaceListCount;

            public int[] FaceListID;

            public int NextPoint;

            public Vector3 PointXYZ;

            public Vector3 RenderedPointXYZ;

            public float Weight;

            #endregion

            // used by hashing as a "linked list" of all items belonging to a hash
            #region Public Methods and Operators

            public static i_CSGPoint CreateInstance()
            {
                var result = new i_CSGPoint();
                return result;
            }

            #endregion
        }
    }
}