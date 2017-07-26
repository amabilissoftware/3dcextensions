//using UpgradeHelpers.Helpers;
namespace ACBO
{
    using SharpDX;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters.Binary;

    public class CGIntersectionFailureException : Exception
    {
        #region Constructors and Destructors

        public CGIntersectionFailureException()
        {
        }

        public CGIntersectionFailureException(string message)
            : base(message)
        {
        }

        public CGIntersectionFailureException(string message, Exception inner)
            : base(message, inner)
        {
        }

        #endregion
    }

    public class CBO
    {
        // ****************************************************************************
        // Copyright (c) 1998-2011 Ambient Software
        // All Rights Reserved.
        // ****************************************************************************
        // *********************************************************************************
        // Module Level Constants
        // *********************************************************************************
        #region Constants

        #endregion

        // *********************************************************************************
        // Private Type Definitions
        // *********************************************************************************
        // a picked faces
        #region Public Methods and Operators

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory([In, Out] Vector3[] pdst, ACBO.CBODataTypes.CSGVector[] psrc, int cb);
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory([In, Out]  ACBO.CBODataTypes.CSGVector[] pdst, Vector3[] psrc, int cb);

        //public void Test(Vector3[] PointList)
        //{
        //    Vector3[] PointList2 = new Vector3[PointList.Length];
        //    Console.WriteLine(System.Runtime.InteropServices.Marshal.SizeOf(typeof(ACBO.Vector3)));
        //    CopyMemory(PointList2, PointList, PointList.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(ACBO.Vector3)));
        //    Debugger.Break();
        //}

        public CBODataTypes.CSGVector GetPointOnLine(ref CBODataTypes.CSGVector linePoint0, ref CBODataTypes.CSGVector linePoint1, ref CBODataTypes.CSGVector point)
        {
            Vector3 linePointDX0 = new Vector3(linePoint0.X, linePoint0.Y, linePoint0.Z);
            Vector3 linePointDX1 = new Vector3(linePoint1.X, linePoint1.Y, linePoint1.Z);
            Vector3 pointDX = new Vector3(point.X, point.Y, point.Z);
            
            Vector3 p1 = new Vector3(linePointDX0.X, linePointDX0.Y, linePointDX0.Z);
            Vector3 p2 = new Vector3(linePointDX1.X, linePointDX1.Y, linePointDX1.Z);
            Vector3 q = new Vector3(pointDX.X, pointDX.Y, pointDX.Z);

            Vector3 u2 = p2 - p1;
            Vector3 pq2 = q - p1;
            Vector3 w2 = pq2 - Vector3.Multiply(u2, Vector3.Dot(pq2, u2) / u2.LengthSquared());

            Vector3 pointOnLineDX = q - w2;

            CBODataTypes.CSGVector pointOnLine = new CBODataTypes.CSGVector();
            pointOnLine.X = pointOnLineDX.X;
            pointOnLine.Y = pointOnLineDX.Y;
            pointOnLine.Z = pointOnLineDX.Z;
            return pointOnLine;
        }

        public float GetDistanceFromLine(ref CBODataTypes.CSGVector linePoint0, ref CBODataTypes.CSGVector linePoint1, ref CBODataTypes.CSGVector point)
        {
            Vector3 linePointDX0 = new Vector3(linePoint0.X, linePoint0.Y, linePoint0.Z);
            Vector3 linePointDX1 = new Vector3(linePoint1.X, linePoint1.Y, linePoint1.Z);
            Vector3 pointDX = new Vector3(point.X, point.Y, point.Z);

            Vector3 u = new Vector3(linePointDX1.X - linePointDX0.X, linePointDX1.Y - linePointDX0.Y, linePointDX1.Z - linePointDX0.Z);
            Vector3 pq = new Vector3(pointDX.X - linePointDX0.X, pointDX.Y - linePointDX0.Y, pointDX.Z - linePointDX0.Z);

            float distance = Vector3.Cross(pq, u).Length() / u.Length();
            return distance;
        }

        public bool RayIntersectsBox(ref CBODataTypes.CSGVector rayPosition, ref CBODataTypes.CSGVector rayDirection, ref CBODataTypes.CSGVector boxMinimum, ref CBODataTypes.CSGVector boxMaximum, ref CBODataTypes.CSGVector intersectionPosition)
        {
            Ray raySDX = new Ray(new Vector3(rayPosition.X, rayPosition.Y, rayPosition.Z), new Vector3(rayDirection.X, rayDirection.Y, rayDirection.Z));
            Vector3 boxMinimumSDX = new Vector3(boxMinimum.X, boxMinimum.Y, boxMinimum.Z);
            Vector3 boxMaximumSDX = new Vector3(boxMaximum.X, boxMaximum.Y, boxMaximum.Z);
            BoundingBox boxSDX = new BoundingBox(boxMinimumSDX, boxMaximumSDX);
            Vector3 intersectionPositionSDX;

            var wasBoxIntersected = Collision.RayIntersectsBox(ref raySDX, ref boxSDX, out intersectionPositionSDX);

            intersectionPosition = new CBODataTypes.CSGVector();
            intersectionPosition.X = intersectionPositionSDX.X;
            intersectionPosition.Y = intersectionPositionSDX.Y;
            intersectionPosition.Z = intersectionPositionSDX.Z;
            return wasBoxIntersected;
        }


        public bool RayIntersectsTriangle(ref CBODataTypes.CSGVector rayPosition, ref CBODataTypes.CSGVector rayDirection, ref CBODataTypes.CSGVector vertex0, ref CBODataTypes.CSGVector vertex1, ref CBODataTypes.CSGVector vertex2, ref CBODataTypes.CSGVector intersectionPosition)
        {
            Ray raySDX = new Ray(new Vector3(rayPosition.X, rayPosition.Y, rayPosition.Z), new Vector3(rayDirection.X, rayDirection.Y, rayDirection.Z));
            Vector3 vertexSDX0 = new Vector3(vertex0.X, vertex0.Y, vertex0.Z);
            Vector3 vertexSDX1 = new Vector3(vertex1.X, vertex1.Y, vertex1.Z);
            Vector3 vertexSDX2 = new Vector3(vertex2.X, vertex2.Y, vertex2.Z);
            Vector3 intersectionPositionSDX;

            var wasTriangleIntersected = Collision.RayIntersectsTriangle(
                ref raySDX, 
                ref vertexSDX0,
                ref vertexSDX1,
                ref vertexSDX2,
                out intersectionPositionSDX);

            intersectionPosition = new CBODataTypes.CSGVector();
            intersectionPosition.X = intersectionPositionSDX.X;
            intersectionPosition.Y = intersectionPositionSDX.Y;
            intersectionPosition.Z = intersectionPositionSDX.Z;
            return wasTriangleIntersected;
        }

        public void ProcessToolOperation(
            int operation,
            ref CBODataTypes.CSGVector[] PointList,
            int PointListCount,
            ref CBODataTypes.CSGVector[] NormalList,
            int NormalListCount,
            ref CBODataTypes.CSGUV[] TextureCoordinateList,
            int TextureCoordinateListCount,
            ref int[] FaceData,
            int FaceDataCount,
            ref int[] faceColorList,
            int faceColorListCount,
            ref CBODataTypes.CSGVector[] PointList2,
            int PointList2Count,
            ref CBODataTypes.CSGVector[] NormalList2,
            int NormalList2Count,
            ref CBODataTypes.CSGUV[] TextureCoordinateList2,
            int TextureCoordinateList2Count,
            ref int[] faceData2,
            int faceData2Count,
            ref int[] faceColorList2,
            int faceColorList2Count,
            ref int lFaceMax,
            ref CBODataTypes.CSGVector[] cvaCompactedVertices,
            ref int[] laFaceTuples,
            ref bool[] faceDeleted,
            ref int[] faceColor)
        {
            CBODataTypes.CSGToolOperation udtOperation = (CBODataTypes.CSGToolOperation)operation;

            Vector3[] PointListA = new Vector3[PointList.Length];
            CopyMemory(PointListA, PointList, PointList.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(ACBO.CBODataTypes.CSGVector)));

            Vector3[] NormalListA = new Vector3[NormalList.Length];
            CopyMemory(NormalListA, NormalList, NormalList.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(ACBO.CBODataTypes.CSGVector)));

            Vector3[] PointListA2 = new Vector3[PointList2.Length];
            CopyMemory(PointListA2, PointList2, PointList2.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(ACBO.CBODataTypes.CSGVector)));

            Vector3[] NormalListA2 = new Vector3[NormalList2.Length];
            CopyMemory(NormalListA2, NormalList2, NormalList2.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(ACBO.CBODataTypes.CSGVector)));

            Vector3[] cvaCompactedVerticesA = new Vector3[0];

            ProcessToolOperation2(udtOperation,
            ref PointListA,
            PointListCount,
            ref NormalListA,
            NormalListCount,
            ref TextureCoordinateList,
            TextureCoordinateListCount,
            ref FaceData,
            FaceDataCount,
            ref faceColorList,
            faceColorListCount,
            ref PointListA2,
            PointList2Count,
            ref NormalListA2,
            NormalList2Count,
            ref TextureCoordinateList2,
            TextureCoordinateList2Count,
            ref faceData2,
            faceData2Count,
            ref faceColorList2,
            faceColorList2Count,
            ref lFaceMax,
            ref cvaCompactedVerticesA,
            ref laFaceTuples,
            ref faceDeleted,
            ref faceColor);

            //foreach (var item in cvaCompactedVerticesA)
            //{
            //    if (
            //        float.IsInfinity(item.X) || float.IsNaN(item.X) || float.IsNegativeInfinity(item.X) || float.IsPositiveInfinity(item.X) ||
            //        float.IsInfinity(item.Y) || float.IsNaN(item.Y) || float.IsNegativeInfinity(item.Y) || float.IsPositiveInfinity(item.Y) ||
            //        float.IsInfinity(item.Z) || float.IsNaN(item.Z) || float.IsNegativeInfinity(item.Z) || float.IsPositiveInfinity(item.Z))
            //    {
            //        Debugger.Break();
            //    }
            //}

            cvaCompactedVertices = new CBODataTypes.CSGVector[cvaCompactedVerticesA.Length];
            CopyMemory(cvaCompactedVertices, cvaCompactedVerticesA, cvaCompactedVerticesA.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(ACBO.CBODataTypes.CSGVector)));

            //foreach (var item in cvaCompactedVertices)
            //{
            //    if (
            //        float.IsInfinity(item.X) || float.IsNaN(item.X) || float.IsNegativeInfinity(item.X) || float.IsPositiveInfinity(item.X) ||
            //        float.IsInfinity(item.Y) || float.IsNaN(item.Y) || float.IsNegativeInfinity(item.Y) || float.IsPositiveInfinity(item.Y) ||
            //        float.IsInfinity(item.Z) || float.IsNaN(item.Z) || float.IsNegativeInfinity(item.Z) || float.IsPositiveInfinity(item.Z))
            //    {
            //        Debugger.Break();
            //    }
            //}

        }

        private void ProcessToolOperation2(
            CBODataTypes.CSGToolOperation udtOperation, 
            ref Vector3[] PointList, 
            int PointListCount, 
            ref Vector3[] NormalList, 
            int NormalListCount, 
            ref CBODataTypes.CSGUV[] TextureCoordinateList, 
            int TextureCoordinateListCount, 
            ref int[] FaceData, 
            int FaceDataCount, 
            ref int[] faceColorList, 
            int faceColorListCount, 
            ref Vector3[] PointList2, 
            int PointList2Count, 
            ref Vector3[] NormalList2, 
            int NormalList2Count, 
            ref CBODataTypes.CSGUV[] TextureCoordinateList2, 
            int TextureCoordinateList2Count, 
            ref int[] faceData2, 
            int faceData2Count, 
            ref int[] faceColorList2, 
            int faceColorList2Count, 
            ref int lFaceMax, 
            ref Vector3[] cvaCompactedVertices, 
            ref int[] laFaceTuples, 
            ref bool[] faceDeleted, 
            ref int[] faceColor)
        {
            var udtToolCGMesh = new ConstructiveSolidGeometryMesh();
            var udtTargetCGMesh = new ConstructiveSolidGeometryMesh();
            var udtResultCGMesh = new ConstructiveSolidGeometryMesh();

            udtTargetCGMesh.InitializeFromGeometry(
                PointList, 
                PointListCount, 
                NormalList, 
                NormalListCount, 
                TextureCoordinateList, 
                TextureCoordinateListCount, 
                FaceData, 
                FaceDataCount, 
                faceColorList, 
                faceColorListCount, 
                true);

            udtToolCGMesh.InitializeFromGeometry(
                PointList2, 
                PointList2Count, 
                NormalList2, 
                NormalList2Count, 
                TextureCoordinateList2, 
                TextureCoordinateList2Count, 
                faceData2, 
                faceData2Count, 
                faceColorList2, 
                faceColorList2Count, 
                true);

            ProcessToolOperationEx(udtOperation, ref udtTargetCGMesh, ref udtToolCGMesh, ref udtResultCGMesh);

            double tempRefParam = lFaceMax;
            udtResultCGMesh.GetGeometry(ref tempRefParam, ref cvaCompactedVertices, ref laFaceTuples, ref faceDeleted, ref faceColor);
            lFaceMax = Convert.ToInt32(tempRefParam);

            // Console.WriteLine(udtResultCGMesh.udtFace.Length.ToString());

            // End If
            // If bShowProgress Then
            // UpdateProgressBar False '15
            // End If
            // create the accumulated Shape
        }

        public void ProcessToolOperationMakeBin(
            CBODataTypes.CSGToolOperation udtOperation,
            ref CBODataTypes.CSGVector[] PointList, 
            int PointListCount,
            ref CBODataTypes.CSGVector[] NormalList, 
            int NormalListCount, 
            ref CBODataTypes.CSGUV[] TextureCoordinateList, 
            int TextureCoordinateListCount, 
            ref int[] FaceData, 
            int FaceDataCount, 
            ref int[] faceColorList, 
            int faceColorListCount,
            ref CBODataTypes.CSGVector[] PointList2, 
            int PointList2Count,
            ref CBODataTypes.CSGVector[] NormalList2, 
            int NormalList2Count, 
            ref CBODataTypes.CSGUV[] TextureCoordinateList2, 
            int TextureCoordinateList2Count, 
            ref int[] faceData2, 
            int faceData2Count, 
            ref int[] faceColorList2, 
            int faceColorList2Count, 
            ref int lFaceMax,
            ref CBODataTypes.CSGVector[] cvaCompactedVertices, 
            ref int[] laFaceTuples, 
            ref bool[] faceDeleted, 
            ref int[] faceColor)
        {
            var executeData = new ExecuteData();
            executeData.PointList = PointList;
            executeData.PointListCount = PointListCount;
            executeData.NormalList = NormalList;
            executeData.NormalListCount = NormalListCount;
            executeData.TextureCoordinateList = TextureCoordinateList;
            executeData.TextureCoordinateListCount = TextureCoordinateListCount;
            executeData.FaceData = FaceData;
            executeData.FaceDataCount = FaceDataCount;
            executeData.faceColorList = faceColorList;
            executeData.faceColorListCount = faceColorListCount;

            executeData.PointList2 = PointList2;
            executeData.PointList2Count = PointList2Count;
            executeData.NormalList2 = NormalList2;
            executeData.NormalList2Count = NormalList2Count;
            executeData.TextureCoordinateList2 = TextureCoordinateList2;
            executeData.TextureCoordinateList2Count = TextureCoordinateList2Count;
            executeData.FaceData2 = faceData2;
            executeData.FaceData2Count = faceData2Count;
            executeData.faceColorList2 = faceColorList2;
            executeData.faceColorList2Count = faceColorList2Count;

            // byte[] result;
            using (var stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, executeData);
                stream.Flush();
                var bytes = stream.ToArray();
                File.WriteAllBytes("D:\\Users\\Richard\\Documents\\TestData.bin", bytes);

                // MessageBox.Show(result2.Length.ToString());
                // using (var fileStream = new FileStream("D:\\Users\\Richard\\Documents\\TestData.bin", FileMode.CreateNew))
                // {
                // stream.CopyTo(fileStream);
                // }
                // stream.CopyTo()
                // stream.Flush();
                // result = stream.ToArray();
            }
        }

        #endregion

        #region Methods

        internal static int CreateFace(ref CBODataTypes.CSGCGFace udtFace, int lVertexMax)
        {
            // reserve space for the face vertices and bounding plane information
            udtFace.lVertex = new int[lVertexMax + 1];
            udtFace.lEdgeProcessed = new int[lVertexMax + 1];
            udtFace.cvEdgeBoundingPlaneNormal = new Vector3[lVertexMax + 1];
            udtFace.cvEdgeBoundingPlanePoint = new Vector3[lVertexMax + 1];

            // set to a default color of white
            udtFace.lColor = -1;

            return 0;
        }

        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************
        internal static void GetEdgeCGMesh(CBODataTypes.CSGCGFace udtFace, int lVertex, int[] lEdge)
        {
            // get all of the necessary information about the edge
            lEdge[0] = udtFace.lVertex[lVertex];
            if (lVertex < udtFace.lVertex.GetUpperBound(0))
            {
                lEdge[1] = udtFace.lVertex[lVertex + 1];
            }
            else
            {
                // special case for the last vertex
                lEdge[1] = udtFace.lVertex[0];
            }
        }

        internal static int GetIntersectionPosition(
            float fltPlanePointX, 
            float fltPlanePointY, 
            float fltPlanePointZ, 
            float fltPlaneNormalX, 
            float fltPlaneNormalY, 
            float fltPlaneNormalZ, 
            float fltRaydvPosX, 
            float fltRaydvPosY, 
            float fltRaydvPosZ, 
            float fltRaydvDirX, 
            float fltRaydvDirY, 
            float fltRaydvDirZ, 
            ref float fltPlaneIntersectionPositionX, 
            ref float fltPlaneIntersectionPositionY, 
            ref float fltPlaneIntersectionPositionZ)
        {
            int intReturnValue;

            float fltTScalerDenominator = fltPlaneNormalX * fltRaydvDirX + fltPlaneNormalY * fltRaydvDirY + fltPlaneNormalZ * fltRaydvDirZ;

            // if fltTDenominator is not zero then there is something for
            if (fltTScalerDenominator >= 0.00001 || fltTScalerDenominator <= -0.00001)
            {
                // a small epsilon
                // single version of calculations
                // calculate scaler D i.e. solving the equation of a plane for D (ax + by + cz + d = 0)
                float fltDScaler = -(fltPlanePointX * fltPlaneNormalX + fltPlanePointY * fltPlaneNormalY + fltPlanePointZ * fltPlaneNormalZ);

                // 'the magic "d" scaler

                // calculate the scaler T
                float fltTScalerNumerator =
                    -(fltPlaneNormalX * fltRaydvPosX + fltPlaneNormalY * fltRaydvPosY + fltPlaneNormalZ * fltRaydvPosZ + fltDScaler);

                float fltTScaler = fltTScalerNumerator / fltTScalerDenominator; // 'the magic "t" scaler

                // if we are REALLY close to zero then make it zero
                if (fltTScaler < 0.00001 && fltTScaler > -0.00001)
                {
                    // a small epsilon
                    fltTScaler = 0;
                }

                // if fltTScaler is greater than or equal to zero then there is something for
                // us to do. otherwise, thre is no intersection
                if (fltTScaler >= 0)
                {
                    fltPlaneIntersectionPositionX = fltRaydvPosX + fltRaydvDirX * fltTScaler;
                    fltPlaneIntersectionPositionY = fltRaydvPosY + fltRaydvDirY * fltTScaler;
                    fltPlaneIntersectionPositionZ = fltRaydvPosZ + fltRaydvDirZ * fltTScaler;

                    intReturnValue = 1;
                }
                else
                {
                    // we don't have an intersection
                    intReturnValue = 0;
                }
            }
            else
            {
                // denominator is zero so they are parallel
                intReturnValue = 0;
            }

            return intReturnValue;
        }

        private static void ProcessToolOperationEx(
            CBODataTypes.CSGToolOperation udtOperation, 
            ref ConstructiveSolidGeometryMesh udtTargetCGMesh, 
            ref ConstructiveSolidGeometryMesh udtToolCGMesh, 
            ref ConstructiveSolidGeometryMesh udtResultCGMesh)
        {
            bool bIntersectionFailure = false;
            int lAttempt = 0;

            int lOriginalPointSum = udtTargetCGMesh.lVertexCount + udtToolCGMesh.lVertexCount;

            // If bShowProgress Then
            // UpdateProgressBar False '5
            // End If
            // debug.print "Before Target:", UBound(udtTargetCGMesh.udtFace) + 1, UBound(udtTargetCGMesh.xdtVertex) + 1
            // debug.print "Before Tool  :", UBound(udtToolCGMesh.udtFace) + 1, UBound(udtToolCGMesh.xdtVertex) + 1
            // Detect the exterior faces that are clearly outside and
            // never could be intersected (via bounding box)
            pPreDetectExteriors(ref udtTargetCGMesh, ref udtToolCGMesh);

            // If bShowProgress Then
            // UpdateProgressBar False '6
            // End If
            pPreDetectExteriors(ref udtToolCGMesh, ref udtTargetCGMesh);

            // If bShowProgress Then
            // UpdateProgressBar False '7
            // End If
            int lLastTargetFaces = 0;
            int lLastToolFaces = 0;
            int lCurrentTargetFaces = udtTargetCGMesh.udtFace.GetUpperBound(0) + 1;
            int lCurrentToolFaces = udtToolCGMesh.udtFace.GetUpperBound(0) + 1;

            // you would think that all we have to do is intersect each
            // CGMesh with the other, but the reality is that
            // simply because we have intersected in both directions
            // doesn't mean that everything is resolved, so let's keep
            // trying until no changes occur in the CGMesh.
            // debug.print "to before intersection: " & gcCHFGeneral.GetSystemTime - lJunk
            // we don't actually split with merges
            if (udtOperation != CBODataTypes.CSGToolOperation.CSGToolOpMerge)
            {
                // Dim lAttempt As Long
                // lTest = gcCHFGeneral.GetSystemTime
                while ((lCurrentTargetFaces != lLastTargetFaces || lCurrentToolFaces != lLastToolFaces) && !bIntersectionFailure
                       && lAttempt < 10)
                {
                    lLastTargetFaces = lCurrentTargetFaces;
                    lLastToolFaces = lCurrentToolFaces;
                    try
                    {
                        pIntersectProcessCGMesh(ref udtToolCGMesh, ref udtTargetCGMesh);
                        try
                        {
                            pIntersectProcessCGMesh(ref udtTargetCGMesh, ref udtToolCGMesh);
                            lCurrentTargetFaces = udtTargetCGMesh.udtFace.GetUpperBound(0) + 1;
                            lCurrentToolFaces = udtToolCGMesh.udtFace.GetUpperBound(0) + 1;
                        }
                        catch (CGIntersectionFailureException)
                        {
                            bIntersectionFailure = true;
                        }
                    }
                    catch (CGIntersectionFailureException)
                    {
                        bIntersectionFailure = true;
                    }

                    lAttempt++;
                }
            }

            // If bShowProgress Then
            // UpdateProgressBar False '8
            // End If
            // debug.print "to intersection completion before detect of exteriors: " & gcCHFGeneral.GetSystemTime - lJunk
            // ltest = gcCHFGeneral.GetSystemTime
            if (udtOperation == CBODataTypes.CSGToolOperation.CSGToolOpIntersection)
            {
                pDetectExteriors(ref udtTargetCGMesh, ref udtToolCGMesh, true, true);

                // ElseIf udtOperation = CSGToolOpCut Then
                // pDetectExteriorsCut udtTargetCGMesh, udtToolCGMesh
            }
            else if (udtOperation != CBODataTypes.CSGToolOperation.CSGToolOpMerge)
            {
                pDetectExteriors(ref udtTargetCGMesh, ref udtToolCGMesh, true, false);
            }
            else
            {
                // CSGToolOpMerge requires that we call everything an exterior that has a single vertex outside of the other
                pDetectExteriorsMerge(ref udtTargetCGMesh, ref udtToolCGMesh, true, false);
            }

            // MsgBox "detect: " & gcCHFGeneral.GetSystemTime - ltest
            // If bShowProgress Then
            // UpdateProgressBar False '9
            // End If
            // ltest = gcCHFGeneral.GetSystemTime
            // If udtOperation <> CSGToolOpCut Then
            if (udtOperation == CBODataTypes.CSGToolOperation.CSGToolOpIntersection)
            {
                pDetectExteriors(ref udtToolCGMesh, ref udtTargetCGMesh, false, true);
            }
            else if (udtOperation != CBODataTypes.CSGToolOperation.CSGToolOpMerge)
            {
                pDetectExteriors(ref udtToolCGMesh, ref udtTargetCGMesh, false, false);
            }
            else
            {
                // CSGToolOpMerge requires that we call everything an exterior that has a single vertex outside of the other
                pDetectExteriorsMerge(ref udtToolCGMesh, ref udtTargetCGMesh, false, false);
            }

            // End If
            // MsgBox "detect: " & gcCHFGeneral.GetSystemTime - ltest
            // If bShowProgress Then
            // UpdateProgressBar False '10
            // End If
            // set up the result CGMesh
            // first we need a min and max x
            if (udtTargetCGMesh.xdtBoundingBox.Min.X < udtToolCGMesh.xdtBoundingBox.Min.X)
            {
                udtResultCGMesh.xdtBoundingBox.Min.X = udtTargetCGMesh.xdtBoundingBox.Min.X;
            }
            else
            {
                udtResultCGMesh.xdtBoundingBox.Min.X = udtToolCGMesh.xdtBoundingBox.Min.X;
            }

            if (udtTargetCGMesh.xdtBoundingBox.Max.X > udtToolCGMesh.xdtBoundingBox.Max.X)
            {
                udtResultCGMesh.xdtBoundingBox.Max.X = udtTargetCGMesh.xdtBoundingBox.Max.X;
            }
            else
            {
                udtResultCGMesh.xdtBoundingBox.Max.X = udtToolCGMesh.xdtBoundingBox.Max.X;
            }

            // and a hash table
            // need to allocate space for the point hash table
            if (lOriginalPointSum > 4000)
            {
                udtResultCGMesh.PointHashTableType = 1;
                udtResultCGMesh.PointHashTable = new int[65536];
            }
            else
            {
                udtResultCGMesh.PointHashTableType = 0;
                udtResultCGMesh.PointHashTable = new int[256];
            }

            // and allocate space for points since lGetSGPointIndex requires this
            udtResultCGMesh.xdtVertex = new CBODataTypes.i_CSGPoint[lOriginalPointSum + 1];

            // time to do the actual tool operation
            // start with our work on the Target
            for (int lFace = 0; lFace <= udtTargetCGMesh.udtFace.GetUpperBound(0); lFace++)
            {
                // only need to deal with non-deleted faces
                if (!udtTargetCGMesh.udtFace[lFace].bDeleted)
                {
                    // decide to include or not in the new Shape
                    // if an exterior polygon
                    if (udtTargetCGMesh.udtFace[lFace].udtIntersection == CBODataTypes.CSGFaceType.CSGFaceExterior)
                    {
                        // if we are doing a union or a difference, we are applying it to
                        // A, so the external polygons are always included
                        if (udtOperation == CBODataTypes.CSGToolOperation.CSGToolOpCut
                            || udtOperation == CBODataTypes.CSGToolOperation.CSGToolOpUnion
                            || udtOperation == CBODataTypes.CSGToolOperation.CSGToolOpDifference
                            || udtOperation == CBODataTypes.CSGToolOperation.CSGToolOpBlend
                            || udtOperation == CBODataTypes.CSGToolOperation.CSGToolOpBridge
                            || udtOperation == CBODataTypes.CSGToolOperation.CSGToolOpMerge)
                        {
                            // ignore any errors caused by the add of a face
                            try
                            {
                                pCopyFaceToCGMesh(ref udtResultCGMesh, ref udtTargetCGMesh, lFace, false);
                            }
                            catch
                            {
                                // call this an exterior (used later for filling holes)
                                udtResultCGMesh.udtFace[udtResultCGMesh.udtFace.GetUpperBound(0)].udtIntersection =
                                    CBODataTypes.CSGFaceType.CSGFaceExterior;
                            }
                        }
                    }
                    else
                    {
                        // an interior polygon (0)
                        // if we are doing an intersection then we included
                        // the interior polygons
                        if (udtOperation == CBODataTypes.CSGToolOperation.CSGToolOpIntersection)
                        {
                            // ignore any errors caused by the add of a face
                            try
                            {
                                pCopyFaceToCGMesh(ref udtResultCGMesh, ref udtTargetCGMesh, lFace, false);
                            }
                            catch
                            {
                                // call this an exterior (used later for filling holes)
                                udtResultCGMesh.udtFace[udtResultCGMesh.udtFace.GetUpperBound(0)].udtIntersection =
                                    CBODataTypes.CSGFaceType.CSGFaceExterior;
                            }
                        }
                    }

                    // If udtTargetCGMesh.udtFace(lFace).udtIntersection = CSGFaceUnknown Then
                    // 'Stop
                    // End If
                }
            }

            // udtTargetCGMesh
            if (udtOperation != CBODataTypes.CSGToolOperation.CSGToolOpCut)
            {
                for (int lFace = 0; lFace <= udtToolCGMesh.udtFace.GetUpperBound(0); lFace++)
                {
                    // only need to deal with non-deleted faces
                    if (!udtToolCGMesh.udtFace[lFace].bDeleted)
                    {
                        // decide to include or not in the new Shape
                        // if an exterior polygon
                        // If bToolExteriorFace(lFace) Then
                        if (udtToolCGMesh.udtFace[lFace].udtIntersection == CBODataTypes.CSGFaceType.CSGFaceExterior)
                        {
                            // only if we are doing a union
                            if (udtOperation == CBODataTypes.CSGToolOperation.CSGToolOpUnion
                                || udtOperation == CBODataTypes.CSGToolOperation.CSGToolOpBlend
                                || udtOperation == CBODataTypes.CSGToolOperation.CSGToolOpBridge
                                || udtOperation == CBODataTypes.CSGToolOperation.CSGToolOpMerge)
                            {
                                // ignore any errors caused by the add of a face
                                try
                                {
                                    pCopyFaceToCGMesh(ref udtResultCGMesh, ref udtToolCGMesh, lFace, false);
                                }
                                catch
                                {
                                    // call this an interior (used later for filling holes)
                                    udtResultCGMesh.udtFace[udtResultCGMesh.udtFace.GetUpperBound(0)].udtIntersection =
                                        CBODataTypes.CSGFaceType.CSGFaceInterior;
                                }
                            }
                        }
                        else
                        {
                            // an interior polygon (0)
                            // if we are doing an intersection or difference then we included
                            // the interior polygons
                            if (udtOperation == CBODataTypes.CSGToolOperation.CSGToolOpIntersection
                                || udtOperation == CBODataTypes.CSGToolOperation.CSGToolOpDifference)
                            {
                                if (udtOperation == CBODataTypes.CSGToolOperation.CSGToolOpDifference)
                                {
                                    // inverted for difference
                                    // ignore any errors caused by the add of a face
                                    try
                                    {
                                        pCopyFaceToCGMesh(ref udtResultCGMesh, ref udtToolCGMesh, lFace, true);
                                    }
                                    catch
                                    {
                                        udtResultCGMesh.udtFace[udtResultCGMesh.udtFace.GetUpperBound(0)].udtIntersection =
                                            CBODataTypes.CSGFaceType.CSGFaceInterior;
                                    }
                                }
                                else
                                {
                                    // CSGToolOpIntersection
                                    // ignore any errors caused by the add of a face
                                    try
                                    {
                                        pCopyFaceToCGMesh(ref udtResultCGMesh, ref udtToolCGMesh, lFace, false);
                                    }
                                    catch
                                    {
                                        udtResultCGMesh.udtFace[udtResultCGMesh.udtFace.GetUpperBound(0)].udtIntersection =
                                            CBODataTypes.CSGFaceType.CSGFaceInterior;
                                    }
                                }
                            }
                        }

                        // If udtToolCGMesh.udtFace(lFace).udtIntersection = CSGFaceUnknown Then
                        // 'Stop
                        // End If
                    }
                }
            }

            // MsgBox "time (processing): " & gcCHFGeneral.GetSystemTime - ltest
            // If bShowProgress Then
            // UpdateProgressBar False '12
            // End If
            // 'debug.print "to closing tests: " & gcCHFGeneral.GetSystemTime - lJunk
            // If bShowProgress Then
            // UpdateProgressBar False '13
            // End If
            // if all is well, detesselate it (tidy it up a bit)
            // If udtOperation <> CSGToolOpCut Then
            // Console.WriteLine(udtResultCGMesh.udtFace.Length.ToString());
            udtResultCGMesh.Detessellate(true);
            udtResultCGMesh.Detessellate(false);
        }

        private static void pCopyFaceToCGMesh(
            ref ConstructiveSolidGeometryMesh udtDestinationCGMesh, 
            ref ConstructiveSolidGeometryMesh udtSourceCGMesh, 
            int lSourceFace, 
            bool bInvertFace)
        {
            int lVertex;
            CBODataTypes.CSGCGFace udtFace = CBODataTypes.CSGCGFace.CreateInstance();

            // create the face
            CreateFace(ref udtFace, udtSourceCGMesh.udtFace[lSourceFace].lVertex.GetUpperBound(0));

            // set the color
            udtFace.lColor = udtSourceCGMesh.udtFace[lSourceFace].lColor;

            // copy the vertices
            for (lVertex = 0; lVertex <= udtSourceCGMesh.udtFace[lSourceFace].lVertex.GetUpperBound(0); lVertex++)
            {
                CBODataTypes.i_CSGPoint xdtVertex;
                if (!bInvertFace)
                {
                    xdtVertex = udtSourceCGMesh.xdtVertex[udtSourceCGMesh.udtFace[lSourceFace].lVertex[lVertex]];
                    xdtVertex.NextPoint = 0;
                }
                else
                {
                    // invert the face
                    xdtVertex =
                        udtSourceCGMesh.xdtVertex[
                            udtSourceCGMesh.udtFace[lSourceFace].lVertex[
                                udtSourceCGMesh.udtFace[lSourceFace].lVertex.GetUpperBound(0) - lVertex]];
                    xdtVertex.NextPoint = 0;
                }

                // lNewVertex = gcCSGFunctions.getVertexCGMesh(udtDestinationCGMesh, xdtVertex, acPrecise)
                // lNewVertex = gcCSGFunctions.getVertexIndex(xdtVertex, udtDestinationCGMesh.xdtVertex, udtDestinationCGMesh.lVertexCount, udtDestinationCGMesh.xdtBoundingBox.Min.x, udtDestinationCGMesh.xdtBoundingBox.Max.x, udtDestinationCGMesh.udtaVertexHashTable)
                int lNewVertex = Helper.lGetSGPointIndex(
                    xdtVertex, 
                    ref udtDestinationCGMesh.xdtVertex, 
                    ref udtDestinationCGMesh.lVertexCount, 
                    udtDestinationCGMesh.PointHashTable, 
                    udtDestinationCGMesh.PointHashTableType, 
                    false, 
                    true);
                udtFace.lVertex[lVertex] = lNewVertex;
            }

            // udtSourceCGMesh

            // verify that the face is a face, not a line or a point
            if (pbFace(udtFace))
            {
                // add the face to the CGMesh
                int lResultFace = udtDestinationCGMesh.AddFace(udtFace, true);

                // copy the intersection data
                udtDestinationCGMesh.udtFace[lResultFace].udtIntersection = udtSourceCGMesh.udtFace[lSourceFace].udtIntersection;
            }
        }

        private static void pDetectExteriors(
            ref ConstructiveSolidGeometryMesh udtCGMeshA, 
            ref ConstructiveSolidGeometryMesh udtCGMeshB, 
            bool bCoplanarExterior, 
            bool bCoplanarOppositeExterior)
        {
            CBODataTypes.CSGRay xdtRay = CBODataTypes.CSGRay.CreateInstance();

            // now run through all of the faces in B to see which ones are in A
            for (int lFace = 0; lFace <= udtCGMeshB.udtFace.GetUpperBound(0); lFace++)
            {
                if (!udtCGMeshB.udtFace[lFace].bDeleted)
                {
                    // only need to detect if we don't already know for sure
                    if (udtCGMeshB.udtFace[lFace].udtIntersection == CBODataTypes.CSGFaceType.CSGFaceUnknown)
                    {
                        // send the normal off as a ray from the center of the polygon in both directions
                        xdtRay.Position = pcvGetFaceInteriorPoint(udtCGMeshB.xdtVertex, udtCGMeshB.udtFace[lFace]);

                        // need the normal as the direction
                        xdtRay.Direction = udtCGMeshB.udtFace[lFace].cvNormal;

                        // testing
                        if (Math.Abs(xdtRay.Direction.X) < Helper.CSG_sSmallEpsilon
                            && Math.Abs(xdtRay.Direction.Y) < Helper.CSG_sSmallEpsilon
                            && Math.Abs(xdtRay.Direction.Z) < Helper.CSG_sSmallEpsilon)
                        {
                            // MsgBox "Hmmmm. normal is 0"
                            // just set it to something arbitrary
                            xdtRay.Direction.X = 0;
                            xdtRay.Direction.Y = 0;
                            xdtRay.Direction.Z = 1;
                        }

                        // xdtRay

                        // test for interior/exterior
                        if (pbExteriorFace(xdtRay, ref udtCGMeshA, bCoplanarExterior, bCoplanarOppositeExterior))
                        {
                            udtCGMeshB.udtFace[lFace].udtIntersection = CBODataTypes.CSGFaceType.CSGFaceExterior;
                        }
                        else
                        {
                            udtCGMeshB.udtFace[lFace].udtIntersection = CBODataTypes.CSGFaceType.CSGFaceInterior;
                        }
                    }
                }
            }

            // udtCGMeshB
        }

        // UPGRADE_NOTE: (7001) The following declaration (pDetectExteriorsCut) seems to be dead code More Information: http://www.vbtonet.com/ewis/ewi7001.aspx
        // private void pDetectExteriorsCut(CBO.CSGCGMesh udtCGMeshA, CBO.CSGCGMesh udtCGMeshB)
        // {
        // CBODataTypes.CSGRay xdtRay = CBODataTypes.CSGRay.CreateInstance();
        // now run through all of the faces in B to see which ones are in A
        // for (int lFace = 0; lFace <= udtCGMeshA.udtFace.GetUpperBound(0); lFace++)
        // {
        // if (!udtCGMeshA.udtFace[lFace].bDeleted)
        // {
        // udtCGMeshA.udtFace[lFace].udtIntersection = CBODataTypes.CSGFaceType.CSGFaceExterior;
        // }
        // } //udtCGMeshB
        // for (int lFace = 0; lFace <= udtCGMeshB.udtFace.GetUpperBound(0); lFace++)
        // {
        // if (!udtCGMeshB.udtFace[lFace].bDeleted)
        // {
        // udtCGMeshB.udtFace[lFace].udtIntersection = CBODataTypes.CSGFaceType.CSGFaceInterior;
        // }
        // } //udtCGMeshB
        // }

        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************
        private static void pDetectExteriorsMerge(
            ref ConstructiveSolidGeometryMesh udtCGMeshA, 
            ref ConstructiveSolidGeometryMesh udtCGMeshB, 
            bool bCoplanarExterior, 
            bool bCoplanarOppositeExterior)
        {
            CBODataTypes.CSGRay xdtRay = CBODataTypes.CSGRay.CreateInstance();

            // now run through all of the faces in B to see which ones are in A
            for (int lFace = 0; lFace <= udtCGMeshB.udtFace.GetUpperBound(0); lFace++)
            {
                if (!udtCGMeshB.udtFace[lFace].bDeleted)
                {
                    // only need to detect if we don't already know for sure
                    if (udtCGMeshB.udtFace[lFace].udtIntersection == CBODataTypes.CSGFaceType.CSGFaceUnknown)
                    {
                        // initially assume that it is an interior face
                        udtCGMeshB.udtFace[lFace].udtIntersection = CBODataTypes.CSGFaceType.CSGFaceInterior;

                        // check each vertex and if even 1 indicates outside, then
                        // it is an exterior
                        foreach (int item in udtCGMeshB.udtFace[lFace].lVertex)
                        {
                            // send the normal off as a ray from the center of the polygon in both directions
                            xdtRay.Position = udtCGMeshB.xdtVertex[item].PointXYZ;

                            // need the normal as the direction
                            xdtRay.Direction = udtCGMeshB.udtFace[lFace].cvNormal;

                            // testing
                            if (Math.Abs(xdtRay.Direction.X) < Helper.CSG_sSmallEpsilon
                                && Math.Abs(xdtRay.Direction.Y) < Helper.CSG_sSmallEpsilon
                                && Math.Abs(xdtRay.Direction.Z) < Helper.CSG_sSmallEpsilon)
                            {
                                // MsgBox "Hmmmm. normal is 0"
                                // just set it to something arbitrary
                                xdtRay.Direction.X = 0;
                                xdtRay.Direction.Y = 0;
                                xdtRay.Direction.Z = 1;
                            }

                            // test for interior/exterior
                            if (pbExteriorFace(xdtRay, ref udtCGMeshA, bCoplanarExterior, bCoplanarOppositeExterior))
                            {
                                udtCGMeshB.udtFace[lFace].udtIntersection = CBODataTypes.CSGFaceType.CSGFaceExterior;
                                break;
                            }
                        }
                    }
                }
            }
        }

        // *********************************************************************************
        // Purpose:
        // Assumptions:  they are both in world coordinates
        // Parameters:
        // *********************************************************************************
        private static void pIntersectProcessCGMesh(
            ref ConstructiveSolidGeometryMesh udtIntersectee, 
            ref ConstructiveSolidGeometryMesh udtIntersector)
        {
            int lFace = 0;
            var lEdge = new int[2];

            // Dim lProcessedEdge As Long
            // Dim lProcessedState As Variant
            // run through all the polygons in the intersector and see which ones intersect with
            // those in intersectee
            // if there is an intersection, then split the intersectee
            // only process those faces that were there when we started this run through
            int lFaceCount = udtIntersector.udtFace.GetUpperBound(0);

            // While lFace <= UBound(udtIntersector.udtFace)
            while (lFace <= lFaceCount)
            {
                if (lFace > 1000)
                {
                    // Stop
                }

                // only need to process faces that haven't been deleted
                if (!udtIntersector.udtFace[lFace].bDeleted)
                {
                    // only need to process those faces that aren't classified
                    if (udtIntersector.udtFace[lFace].udtIntersection != CBODataTypes.CSGFaceType.CSGFaceExterior)
                    {
                        bool bReProcessFaceIntersector = false;

                        // get each "ray" in succession (meaning each edge of the polygon)
                        int lVertex = 0;
                        int lVertexCount = udtIntersector.udtFace[lFace].lVertex.GetUpperBound(0);

                        while (lVertex <= lVertexCount && !bReProcessFaceIntersector)
                        {
                            // get the edge to be processed
                            GetEdgeCGMesh(udtIntersector.udtFace[lFace], lVertex, lEdge);
                            bool bEdgeProcessed = udtIntersector.udtFace[lFace].lEdgeProcessed[lVertex] == 1;

                            // if the edge hasn't been processed, process it
                            if (!bEdgeProcessed)
                            {
                                bReProcessFaceIntersector = pbIntersectProcessVertex(ref udtIntersectee, ref udtIntersector, lFace, lVertex);
                            }

                            lVertex++;
                        }

                        // Else
                        // 'msgbox "processsed"
                    }
                }

                // if we modified the "intersector" then the face we were
                // working on is no longer the current face, but has been
                // moved to the end, so the "current" face must be repeated
                // since it is really a "new" face. confusing?
                // If Not bReProcessFaceIntersector Then
                lFace++;

                // End If
            }
        }

        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************
        private static void pPreDetectExteriors(ref ConstructiveSolidGeometryMesh udtCGMeshA, ref ConstructiveSolidGeometryMesh udtCGMeshB)
        {
            // Dim xdtRay As CSGRay
            // Dim lFaceIntersected As Long
            // Dim bFaceIntersectedExterior As Boolean
            // now run through all of the faces in B to see which ones are in A
            for (int lFace = 0; lFace <= udtCGMeshB.udtFace.GetUpperBound(0); lFace++)
            {
                // only need to detect faces that haven't been deleted
                if (!udtCGMeshB.udtFace[lFace].bDeleted)
                {
                    // only need to detect if we don't already know for sure
                    if (udtCGMeshB.udtFace[lFace].udtIntersection == CBODataTypes.CSGFaceType.CSGFaceUnknown)
                    {
                        // detect if it is an exterior
                        if (pbPreDetectExteriorFace(udtCGMeshB.xdtVertex, udtCGMeshB.udtFace[lFace], ref udtCGMeshA))
                        {
                            udtCGMeshB.udtFace[lFace].udtIntersection = CBODataTypes.CSGFaceType.CSGFaceExterior;

                            // 'if it isn't an exterior see if it is an interior (expensive test)
                            // ElseIf pbPreDetectInteriorFace(udtCGMeshB.xdtVertex, udtCGMeshB.udtFace(lFace), udtCGMeshA) Then
                            // udtCGMeshB.udtFace(lFace).udtIntersection = CSGFaceInterior
                        }
                    }
                }
            }
        }

        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************
        // Private Sub pOptimizeCGMesh(udtCGMesh as CSGCGMesh)
        // Dim udtResultCGMesh As CSGCGMesh
        // Dim lFace As Long
        // 'time to do the actual tool operation
        // 'start with our work on the Target
        // For lFace = 0 To UBound(udtCGMesh.udtFace)
        // 'only need to deal with non-deleted faces
        // If Not udtCGMesh.udtFace(lFace).bDeleted Then
        // pCopyFaceToCGMesh udtResultCGMesh, udtCGMesh, lFace, False
        // End If
        // Next lFace
        // 'create the bounding box
        // CreateBoundingBox udtResultCGMesh
        // 'copy it back to the CGMesh
        // udtCGMesh = udtResultCGMesh
        // Exit Sub
        // ErrorxHandler:
        // If lHandleError(Err.Number, mt_Module, Err.Description, False) = CSGResume Then
        // Resume
        // Else
        // Err.Raise Err.Number, mt_Module, Err.Description
        // End If
        // End Sub
        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************
        // Private Function pbCGMeshClosed(udtCGMesh as CSGCGMesh) As Boolean
        // Dim colEdgeFailures As New Collection
        // GetEdgeFailures udtCGMesh, colEdgeFailures
        // If colEdgeFailures.Count > 0 Then
        // pbCGMeshClosed = False
        // Else
        // pbCGMeshClosed = True
        // End If
        // Exit Function
        // ErrorxHandler:
        // If lHandleError(Err.Number, mt_Module, Err.Description, False) = CSGResume Then
        // Resume
        // Else
        // Err.Raise Err.Number, mt_Module, Err.Description
        // End If
        // End Function

        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************
        private static bool pbExteriorFace(
            CBODataTypes.CSGRay xdtRay, 
            ref ConstructiveSolidGeometryMesh udtExterior, 
            bool bCoplanarExterior, 
            bool bCoplanarOppositeExterior)
        {
            // 'Dim lExteriorFace                  As Long
            // 'Dim lInteriorFace                  As Long
            var cvExteriorIntersectionPosition = new Vector3();
            var cvInteriorIntersectionPosition = new Vector3();
            float sDistanceExterior = 0;
            float sDistanceInterior = 0;

            // CBO.actPick[] udtaPick = null;
            var udtaPick = udtExterior.RayPick(xdtRay, true, Helper.CSG_sLargeEpsilon);

            var lPickMax = udtaPick.Count - 1;

            // int lPickMax = udtaPick.Length - 1; // modCSG.SafeUBound(UpgradeSolution1Support.PInvoke.SafeNative.msvbvm60.VarPtrArray(ArraysHelper.CastArray<System.IntPtr[]>(udtaPick)));
            // see if we have a pick
            bool bExteriorPick = false;
            bool bInteriorPick = false;
            if (lPickMax >= 0)
            {
                // get the first exterior and interior
                foreach (ConstructiveSolidGeometryMesh.CSGCGPick udtaPick_item in udtaPick)
                {
                    if (udtaPick_item.udtType == CBODataTypes.CSGFaceType.CSGFaceExterior && !bExteriorPick)
                    {
                        bExteriorPick = true;

                        // lExteriorFace = udtaPick(lPick).lFace
                        cvExteriorIntersectionPosition = udtaPick_item.cvPosition;
                        sDistanceExterior = udtaPick_item.sDistanceFromRayOrigin;
                    }
                    else if (udtaPick_item.udtType == CBODataTypes.CSGFaceType.CSGFaceInterior && !bInteriorPick)
                    {
                        bInteriorPick = true;

                        // lInteriorFace = .lFace
                        cvInteriorIntersectionPosition = udtaPick_item.cvPosition;
                        sDistanceInterior = udtaPick_item.sDistanceFromRayOrigin; // udtaPick(lPick)
                    }
                }
            }

            // if the picked position equals the ray origin, then they are co-planar
            // in this case they lie in opposite directions
            // so we make it an exterior or an interior based on bCoplanarOppositeExterior
            if (bExteriorPick
                && Helper.bVectorEqual(
                    xdtRay.Position.X, 
                    xdtRay.Position.Y, 
                    xdtRay.Position.Z, 
                    cvExteriorIntersectionPosition.X, 
                    cvExteriorIntersectionPosition.Y, 
                    cvExteriorIntersectionPosition.Z))
            {
                return bCoplanarOppositeExterior;
            }
            else
            {
                // if the picked position equals the ray origin, then they are co-planar
                // in this case they lie in the same direction
                // so we default based on bCoplanarExterior
                if (bInteriorPick
                    && Helper.bVectorEqual(
                        xdtRay.Position.X, 
                        xdtRay.Position.Y, 
                        xdtRay.Position.Z, 
                        cvInteriorIntersectionPosition.X, 
                        cvInteriorIntersectionPosition.Y, 
                        cvInteriorIntersectionPosition.Z))
                {
                    return bCoplanarExterior;
                }
                else
                {
                    // if neither were picked then we are obvious an exterior
                    if (!bExteriorPick && !bInteriorPick)
                    {
                        return true;

                        // if the exterior was picked and not the interior then something is wrong
                    }
                    else if (bExteriorPick && !bInteriorPick)
                    {
                        // call it an exterior
                        return true;

                        // if the interior was picked and not the exterior then we have an interior
                    }
                    else if (bInteriorPick && !bExteriorPick)
                    {
                        return false;
                    }
                    else
                    {
                        // both picked
                        // if the exterior is closer than the interior the we have an Exterior
                        return sDistanceExterior <= sDistanceInterior;
                    }
                }
            }
        }

        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************
        // any repeated vertices means failure
        private static bool pbFace(CBODataTypes.CSGCGFace udtFace)
        {
            bool result = true;
            for (int lVertex = 0; lVertex <= udtFace.lVertex.GetUpperBound(0) - 1; lVertex++)
            {
                for (int lVertex2 = lVertex + 1; lVertex2 <= udtFace.lVertex.GetUpperBound(0); lVertex2++)
                {
                    if (udtFace.lVertex[lVertex] == udtFace.lVertex[lVertex2])
                    {
                        result = false;
                        break;
                    }
                }
            }

            return result;
        }

        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************

        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************
        private static bool pbIntersectProcessVertex(
            ref ConstructiveSolidGeometryMesh udtIntersectee, 
            ref ConstructiveSolidGeometryMesh udtIntersector, 
            int lFace, 
            int lVertex)
        {
            bool result = false;
            CBODataTypes.CSGRay xdtRay = CBODataTypes.CSGRay.CreateInstance();
            var cvaEdgeVertex = new Vector3[2];
            var cvVector = new Vector3();
            int lPickedFace = 0;
            var cvPickedPosition = new Vector3();
            var lEdge = new int[2];
            bool bEdgeComplete = false;

            // CBO.actPick[] udtaPick = null;
            // int lPickMax = 0;
            int lPick = 0;

            // get all of the necessary information about the edge
            GetEdgeCGMesh(udtIntersector.udtFace[lFace], lVertex, lEdge);
            cvaEdgeVertex[0] = udtIntersector.xdtVertex[lEdge[0]].PointXYZ;
            cvaEdgeVertex[1] = udtIntersector.xdtVertex[lEdge[1]].PointXYZ;

            // the position //udtIntersector
            xdtRay.Position = cvaEdgeVertex[0];

            // the direction
            xdtRay.Direction.X = cvaEdgeVertex[1].X - cvaEdgeVertex[0].X;
            xdtRay.Direction.Y = cvaEdgeVertex[1].Y - cvaEdgeVertex[0].Y;
            xdtRay.Direction.Z = cvaEdgeVertex[1].Z - cvaEdgeVertex[0].Z;
            xdtRay.Direction.Normalize();
            cvVector.X = cvaEdgeVertex[0].X - cvaEdgeVertex[1].X;
            cvVector.Y = cvaEdgeVertex[0].Y - cvaEdgeVertex[1].Y;
            cvVector.Z = cvaEdgeVertex[0].Z - cvaEdgeVertex[1].Z; // cvVector
            float sEdgeLength = cvVector.Length();

            // only need to process if the edge is of some length
            if (sEdgeLength >= Helper.CSG_sSmallEpsilon * 10)
            {
                var udtaPick = udtIntersectee.RayPick(xdtRay, false, sEdgeLength + Helper.CSG_sSmallEpsilon * 2);

                var lPickMax = udtaPick.Count - 1;

                // see if we have a pick
                bool bPick = false;
                if (lPickMax >= 0)
                {
                    // get the first non-zero pick
                    foreach (ConstructiveSolidGeometryMesh.CSGCGPick udtaPick_item in udtaPick)
                    {
                        if (udtaPick_item.sDistanceFromRayOrigin >= Helper.CSG_sSmallEpsilon
                            && udtaPick_item.sDistanceFromRayOrigin <= sEdgeLength + (Helper.CSG_sSmallEpsilon / 2))
                        {
                            lPickedFace = udtaPick_item.lFace;
                            cvPickedPosition = udtaPick_item.cvPosition;
                            bPick = true;
                            break;
                        }
                    }
                }

                if (bPick)
                {
                    // if the first valid pick is the last valid pick then we
                    // have no further intersections - so we can call the entire
                    // edge done
                    if (lPick == udtaPick.Count - 1)
                    {
                        bEdgeComplete = true;
                    }

                    // another circumstance of completion is if the other intersections
                    // are very close to the original intersection (< CSG_sSmallEpsilon)
                    if (!bEdgeComplete)
                    {
                        bEdgeComplete = true;
                        for (int lPickTest = lPick + 1; lPickTest < udtaPick.Count; lPickTest++)
                        {
                            // if a point on the edge intersecting
                            if (udtaPick[lPickTest].sDistanceFromRayOrigin <= sEdgeLength + (Helper.CSG_sSmallEpsilon / 2))
                            {
                                // if the point doesn't match then we failed
                                if (!Helper.bVectorEqual2(udtaPick[lPick].cvPosition, udtaPick[lPickTest].cvPosition))
                                {
                                    bEdgeComplete = false;
                                    break;
                                }
                            }
                        }

                        if (bEdgeComplete)
                        {
                            // Stop
                        }
                    }

                    // only divide the polygon and split the intersector if the polygon
                    // is convex. Tiny polygons (which appear to not be convex)
                    // should not be split since this almost guarantees an infinite loop
                    if (udtaPick[lPick].bConvex)
                    {
                        udtIntersectee.bDividePolygon(lPickedFace, cvPickedPosition.X, cvPickedPosition.Y, cvPickedPosition.Z);

                        // split the intersector
                        result = udtIntersector.bSplitIntersectorEdge(
                            lFace, 
                            lVertex, 
                            cvPickedPosition.X, 
                            cvPickedPosition.Y, 
                            cvPickedPosition.Z, 
                            bEdgeComplete);
                    }
                    else
                    {
                        // indicate that we have processed the edge
                        udtIntersector.udtFace[lFace].lEdgeProcessed[lVertex] = 1;
                    }
                }
                else
                {
                    // indicate that we have processed the edge
                    udtIntersector.udtFace[lFace].lEdgeProcessed[lVertex] = 1;
                }
            }
            else
            {
                // indicate that we have processed the edge
                udtIntersector.udtFace[lFace].lEdgeProcessed[lVertex] = 1;
            }

            return result;
        }

        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************
        private static bool pbPreDetectExteriorFace(
            CBODataTypes.i_CSGPoint[] xdtVertex, 
            CBODataTypes.CSGCGFace udtFace, 
            ref ConstructiveSolidGeometryMesh udtCGMesh)
        {
            bool result = false;
            var cvIntersectionPosition = new Vector3();
            CBODataTypes.CSGRay xdtRay = CBODataTypes.CSGRay.CreateInstance();

            // Dim cvVector As Vector3
            // Dim sRadius As Single
            // 'look at each bounding plane
            // 'if all of the face's vertices are outside of a plane
            // 'then the face is outside
            // pbPreDetectExteriorFace = True
            // 'check the bounding sphere first. If all of the vertices are outside
            // 'of the bounding sphere, then we know it is outside of the bounding sphere
            // For lVertex = 0 To UBound(udtFace.lVertex)
            // cvVector.X = xdtVertex(udtFace.lVertex(lVertex)).X - udtCGMesh.cvCenter.X
            // cvVector.Y = xdtVertex(udtFace.lVertex(lVertex)).Y - udtCGMesh.cvCenter.Y
            // cvVector.Z = xdtVertex(udtFace.lVertex(lVertex)).Z - udtCGMesh.cvCenter.Z
            // sRadius = modCSG.vectorModulus(cvVector)
            // 'if inside the bounding sphere then we aren't an exterior
            // If sRadius <= udtCGMesh.sBoundingSphere Then
            // pbPreDetectExteriorFace = False
            // Exit For 'the quick way out
            // End If
            // Next lVertex
            // If pbPreDetectExteriorFace Then
            // 'Stop
            // End If
            // 'look further if necessary
            // If Not pbPreDetectExteriorFace Then
            // This really does work so don't mess with it!
            foreach (CBODataTypes.CSGCGFace udtCGMesh_udtBoundingFace_item in udtCGMesh.udtBoundingFace)
            {
                // check each bounding plane
                // assume that the vertex is an exterior vertex
                bool bExteriorFace = true;
                for (int lVertex = 0; lVertex <= udtFace.lVertex.GetUpperBound(0); lVertex++)
                {
                    // send a ray along the bounding plane normal, with an origin
                    // of the vertex.
                    // the ray
                    xdtRay.Direction = udtCGMesh_udtBoundingFace_item.cvNormal;
                    xdtRay.Position = xdtVertex[udtFace.lVertex[lVertex]].PointXYZ;

                    // if there is an intersection, then the vertex is
                    // not an exterior and as a result, the face is not an exterior
                    if (GetIntersectionPosition(
                        udtCGMesh.xdtBoundingVertex[udtCGMesh_udtBoundingFace_item.lVertex[0]].PointXYZ.X, 
                        udtCGMesh.xdtBoundingVertex[udtCGMesh_udtBoundingFace_item.lVertex[0]].PointXYZ.Y, 
                        udtCGMesh.xdtBoundingVertex[udtCGMesh_udtBoundingFace_item.lVertex[0]].PointXYZ.Z, 
                        udtCGMesh_udtBoundingFace_item.cvNormal.X, 
                        udtCGMesh_udtBoundingFace_item.cvNormal.Y, 
                        udtCGMesh_udtBoundingFace_item.cvNormal.Z, 
                        xdtRay.Position.X, 
                        xdtRay.Position.Y, 
                        xdtRay.Position.Z, 
                        xdtRay.Direction.X, 
                        xdtRay.Direction.Y, 
                        xdtRay.Direction.Z, 
                        ref cvIntersectionPosition.X, 
                        ref cvIntersectionPosition.Y, 
                        ref cvIntersectionPosition.Z) == 1)
                    {
                        // If pbgcCHFGeneral.GetIntersectionPosition(udtCGMesh.xdtBoundingVertex(udtCGMesh.udtBoundingFace(lBoundingPlane).lVertex(0)), udtCGMesh.udtBoundingFace(lBoundingPlane).cvNormal, xdtRay, cvIntersectionPosition) Then
                        bExteriorFace = false;

                        // the easy way out
                        break;
                    }
                }

                // the easy way out - if all vertices lie outside of the
                // bounding plane that we have an exterior
                if (bExteriorFace)
                {
                    result = true;
                    break;
                }
            }

            // End If
            // 'testing
            // If pbPreDetectExteriorFace Then
            // udtFace.lColor = gcrCRetainedMode.createcolorRGB(1, 0, 0)
            // End If
            return result;
        }

        // '*********************************************************************************
        // ' Purpose:
        // '
        // ' Parameters:
        // '
        // '*********************************************************************************
        // Private Function pcvGetFaceEdgePoint(cvVertex() As Vector3, udtFace As actFace, lVertex As Long) As Vector3
        // Dim lEdge(1) As Long
        // GetEdgeCGMesh udtFace, lVertex, lEdge
        // pcvGetFaceEdgePoint.X = (cvVertex(lEdge(0)).X + cvVertex(lEdge(1)).X) / 2
        // pcvGetFaceEdgePoint.Y = (cvVertex(lEdge(0)).Y + cvVertex(lEdge(1)).Y) / 2
        // pcvGetFaceEdgePoint.Z = (cvVertex(lEdge(0)).Z + cvVertex(lEdge(1)).Z) / 2
        // Exit Function
        // ErrorxHandler:
        // Err.Raise Err.Number, mt_Module, Err.Description
        // End Function
        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************

        // '*********************************************************************************
        // ' Purpose:
        // '
        // ' Parameters:
        // '
        // '*********************************************************************************
        // Private Sub pCreateBridge(csShape1 As CSGShape, _
        // '                          csShape2 As CSGShape, _
        // '                          sRange As Single)
        // Dim lVertex1         As Long
        // Dim lVertex2         As Long
        // Dim sDistance        As Single
        // Dim lNearestPair1    As Long
        // Dim lNearestPair2    As Long
        // Dim sNearestDistance As Single
        // Dim cvVector         As Vector3
        // Dim cvDeltaDistance  As Vector3
        // Dim cvaVertices1()   As Vector3
        // Dim cvaVertices2()   As Vector3
        // Dim lPointListCount  As Long
        // 'Dim sDeformResult    As Single
        // 'get the vertices for the Shapes
        // csShape1.GetPointsXYZ cvaVertices1, lPointListCount
        // csShape2.GetPointsXYZ cvaVertices2, lPointListCount
        // 'initialize to some large distance
        // sNearestDistance = CSG_sLargeEpsilon
        // 'determine the pair of vertices that are the closest to each other
        // For lVertex1 = 0 To csShape1.GetPointCount - 1
        // For lVertex2 = 0 To csShape2.GetPointCount - 1
        // 'calculate the distance apart
        // With cvVector
        // .X = cvaVertices2(lVertex2).X - cvaVertices1(lVertex1).X
        // .Y = cvaVertices2(lVertex2).Y - cvaVertices1(lVertex1).Y
        // .Z = cvaVertices2(lVertex2).Z - cvaVertices1(lVertex1).Z
        // End With 'cvVector
        // sDistance = modCSG.VectorModulus(cvVector)
        // 'see if it is closer than the nearest so far
        // If sDistance < sNearestDistance Then
        // sNearestDistance = sDistance
        // lNearestPair1 = lVertex1
        // lNearestPair2 = lVertex2
        // End If
        // Next lVertex2
        // Next lVertex1
        // 'deform each CGMesh towards the other
        // With cvDeltaDistance
        // .X = (cvaVertices2(lNearestPair2).X - cvaVertices1(lNearestPair1).X) / sRange
        // .Y = (cvaVertices2(lNearestPair2).Y - cvaVertices1(lNearestPair1).Y) / sRange
        // .Z = (cvaVertices2(lNearestPair2).Z - cvaVertices1(lNearestPair1).Z) / sRange
        // End With 'cvDeltaDistance
        // sDeformShape csShape1, sRange, cvaVertices1(lNearestPair1), cvDeltaDistance
        // 'deform each CGMesh towards the other
        // With cvDeltaDistance
        // .X = (cvaVertices1(lNearestPair1).X - cvaVertices2(lNearestPair2).X) / sRange
        // .Y = (cvaVertices1(lNearestPair1).Y - cvaVertices2(lNearestPair2).Y) / sRange
        // .Z = (cvaVertices1(lNearestPair1).Z - cvaVertices2(lNearestPair2).Z) / sRange
        // End With 'cvDeltaDistance
        // sDeformShape csShape2, sRange, cvaVertices2(lNearestPair2), cvDeltaDistance
        // 'MsgBox "distance: " & sNearestDistance
        // End Sub

        // '*********************************************************************************
        // ' Purpose:
        // '
        // ' Parameters:
        // '
        // '*********************************************************************************
        // Private Sub pPostDetectExteriors(udtCGMeshA As CSGCGMesh, udtCGMeshB As CSGCGMesh, cCSG As CSG)
        // Dim lFace As Long
        // 'Dim xdtRay As CSGRay
        // 'Dim lFaceIntersected As Long
        // 'Dim bFaceIntersectedExterior As Boolean
        // 'now run through all of the faces in B to see which ones are in A
        // For lFace = 0 To UBound(udtCGMeshB.udtFace)
        // 'only need to detect faces that haven't been deleted
        // If Not udtCGMeshB.udtFace(lFace).bDeleted Then
        // 'only need to detect if we don't already know for sure
        // If udtCGMeshB.udtFace(lFace).udtIntersection = CSGFaceUnknown Then
        // 'detect if it is an exterior
        // If pbPostDetectExteriorFace(udtCGMeshB.xdtVertex, udtCGMeshB.udtFace(lFace), udtCGMeshA, cCSG) Then
        // udtCGMeshB.udtFace(lFace).udtIntersection = CSGFaceExterior
        // End If
        // End If
        // End If
        // Next lFace
        // Exit Sub
        // ErrorxHandler:
        // Err.Raise Err.Number, mt_Module, Err.Description
        // End Sub
        // '*********************************************************************************
        // ' Purpose:
        // '
        // ' Parameters:
        // '
        // '*********************************************************************************
        // Private Function pbPostDetectExteriorFace(xdtVertex() As i_CSGPoint, udtFace As actFace, udtCGMesh As CSGCGMesh, cCSG As CSG) As Boolean
        // Dim lVertex As Long
        // 'Dim lBoundingPlane As Long
        // 'Dim cvIntersectionPosition As Vector3
        // 'Dim xdtRay As CSGRay
        // 'Dim bExteriorFace As Boolean
        // Dim cvVector As Vector3
        // Dim sRadius As Single
        // pbPostDetectExteriorFace = False
        // 'check the bounding sphere first. If ANY vertex lies outside
        // 'the face is an exterior
        // For lVertex = 0 To UBound(udtFace.lVertex)
        // cvVector.X = xdtVertex(udtFace.lVertex(lVertex)).PointXYZ.X - udtCGMesh.cvCenter.X
        // cvVector.Y = xdtVertex(udtFace.lVertex(lVertex)).PointXYZ.Y - udtCGMesh.cvCenter.Y
        // cvVector.Z = xdtVertex(udtFace.lVertex(lVertex)).PointXYZ.Z - udtCGMesh.cvCenter.Z
        // sRadius = modCSG.VectorModulus(cvVector)
        // 'if outside the bounding sphere then we are an exterior
        // If sRadius > udtCGMesh.sBoundingSphere + CSG_sSmallEpsilon * 10 Then
        // pbPostDetectExteriorFace = True
        // Exit For 'the quick way out
        // End If
        // Next lVertex
        // 'now check the bounding box if necessary
        // If Not pbPostDetectExteriorFace Then
        // 'if any point lies outside the bounding box then we have an exterior
        // For lVertex = 0 To UBound(udtFace.lVertex)
        // If xdtVertex(udtFace.lVertex(lVertex)).PointXYZ.X < udtCGMesh.xdtBoundingBox.Min.X - CSG_sSmallEpsilon * 10
        // '            Or xdtVertex(udtFace.lVertex(lVertex)).PointXYZ.Y < udtCGMesh.xdtBoundingBox.Min.Y - CSG_sSmallEpsilon * 10
        // '            Or xdtVertex(udtFace.lVertex(lVertex)).PointXYZ.Z < udtCGMesh.xdtBoundingBox.Min.Z - CSG_sSmallEpsilon * 10
        // '            Or xdtVertex(udtFace.lVertex(lVertex)).PointXYZ.X > udtCGMesh.xdtBoundingBox.Max.X + CSG_sSmallEpsilon * 10
        // '            Or xdtVertex(udtFace.lVertex(lVertex)).PointXYZ.Y > udtCGMesh.xdtBoundingBox.Max.Y + CSG_sSmallEpsilon * 10
        // '            Or xdtVertex(udtFace.lVertex(lVertex)).PointXYZ.Z > udtCGMesh.xdtBoundingBox.Max.Z + CSG_sSmallEpsilon * 10 Then
        // pbPostDetectExteriorFace = True
        // Exit For 'the quick way out
        // End If
        // Next lVertex
        // End If
        // '    'testing
        // '    If pbPostDetectExteriorFace Then
        // '        udtFace.lColor = gcrCRetainedMode.createcolorRGB(0, 0, 1)
        // '    End If
        // Exit Function
        // ErrorxHandler:
        // Err.Raise Err.Number, mt_Module, Err.Description
        // End Function
        // '*********************************************************************************
        // ' Purpose:
        // '
        // ' Parameters:
        // '
        // '*********************************************************************************
        // Private Function pbPreDetectInteriorFace(cvaVertex() As Vector3, udtFace As actFace, udtIntersectee As CSGCGMesh, cCSG As CSG) As Boolean
        // Dim lEdge As Long
        // Dim lEdgeVertex(1) As Long
        // Dim cvaEdgeVertex(1) As Vector3
        // Dim xdtRay As CSGRay
        // Dim lPick As Long
        // Dim bIntersection As Boolean
        // Dim cvVector As Vector3
        // Dim sEdgeLength As Single
        // Dim udtaPick() As actPick
        // Dim lPickMax As Long
        // 'assume an interior
        // pbPreDetectInteriorFace = True
        // For lEdge = 0 To UBound(udtFace.lVertex)
        // 'get all of the necessary information about the edge
        // GetEdgeCGMesh udtFace, lEdge, lEdgeVertex
        // cvaEdgeVertex(0) = cvaVertex(lEdgeVertex(0))
        // cvaEdgeVertex(1) = cvaVertex(lEdgeVertex(1))
        // 'the position
        // xdtRay.Position = cvaEdgeVertex(0)
        // 'the direction
        // xdtRay.Direction.X = cvaEdgeVertex(1).X - cvaEdgeVertex(0).X
        // xdtRay.Direction.Y = cvaEdgeVertex(1).Y - cvaEdgeVertex(0).Y
        // xdtRay.Direction.Z = cvaEdgeVertex(1).Z - cvaEdgeVertex(0).Z
        // modCSG.VectorNormalize xdtRay.Direction, xdtRay.Direction
        // cvVector.X = cvaEdgeVertex(0).X - cvaEdgeVertex(1).X
        // cvVector.Y = cvaEdgeVertex(0).Y - cvaEdgeVertex(1).Y
        // cvVector.Z = cvaEdgeVertex(0).Z - cvaEdgeVertex(1).Z
        // sEdgeLength = modCSG.VectorModulus(cvVector)
        // 'only need to process if the edge is of some length
        // If sEdgeLength >= CSG_sSmallEpsilon * 10 Then
        // pRayPickCGMesh udtIntersectee, xdtRay, True, udtaPick, CSG_sLargeEpsilon, cCSG
        // 'determine the UBound of the picks
        // On Error Resume Next
        // lPickMax = UBound(udtaPick)
        // If Err.Number <> 0 Then
        // lPickMax = -1
        // End If
        // 'see if we have a pick
        // bIntersection = False
        // If lPickMax >= 0 Then
        // If udtaPick(0).sDistanceFromRayOrigin <= sEdgeLength + (CSG_sSmallEpsilon / 2) Then
        // bIntersection = True
        // End If
        // End If
        // 'if not an intersection then we can mark the edge as complete
        // If Not bIntersection Then
        // udtFace.lEdgeProcessed(lEdge) = 1
        // End If
        // If Not bIntersection Then
        // If lPickMax >= 0 Then
        // If udtaPick(0).udtType <> CSGFaceInterior Then
        // pbPreDetectInteriorFace = False
        // Exit For 'the easy way out
        // End If
        // Else 'no intersection at all so it isn't an interior
        // pbPreDetectInteriorFace = False
        // Exit For 'the easy way out
        // End If
        // Else 'intersection so we can't say what it is
        // pbPreDetectInteriorFace = False
        // Exit For 'the easy way out
        // End If
        // Else
        // pbPreDetectInteriorFace = False
        // Exit For 'the easy way out
        // End If
        // Next lEdge
        // '    'testing
        // '    If pbPreDetectInteriorFace Then
        // '        udtFace.lColor = gcrCRetainedMode.createcolorRGB(0, 1, 0)
        // '    End If
        // 'tidy up
        // Erase udtaPick
        // Exit Function
        // ErrorxHandler:
        // Err.Raise Err.Number, mt_Module, Err.Description
        // End Function
        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************
        private static Vector3 pcvGetFaceInteriorPoint(CBODataTypes.i_CSGPoint[] xdtVertex, CBODataTypes.CSGCGFace udtFace)
        {
            // get a point in the interior of the polygon 'the center of the polygon
            var result = new Vector3();
            for (int lVertex = 0; lVertex <= 2; lVertex++)
            {
                // UBound(udtFace.lVertex)
                result.X += xdtVertex[udtFace.lVertex[lVertex]].PointXYZ.X;
                result.Y += xdtVertex[udtFace.lVertex[lVertex]].PointXYZ.Y;
                result.Z += xdtVertex[udtFace.lVertex[lVertex]].PointXYZ.Z; // pcvGetFaceInteriorPoint
            }

            // calculate the interior point as the average of the three vertices
            result.X /= 3;
            result.Y /= 3;
            result.Z /= 3;

            return result;
        }

        #endregion

        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************

        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************
    }
}