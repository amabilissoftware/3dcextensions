//using UpgradeHelpers.Helpers;

namespace ACBO
{
    using System;

    using SharpDX;

    internal static class Helper
    {
        public const float CSG_sLargeEpsilon = 100000000000000f;

        public const float CSG_sSmallEpsilon = 0.0001f;

        //// *********************************************************************************
        //// Purpose:
        //// Parameters:
        //// *********************************************************************************
        // internal static void VectorCrossProduct(
        // ref CBODataTypes.CSGVector Result, 
        // CBODataTypes.CSGVector Vector1, 
        // CBODataTypes.CSGVector Vector2)
        // {
        // var vector3_1 = new Vector3(Vector1.X, Vector1.Y, Vector1.Z);
        // var vector3_2 = new Vector3(Vector2.X, Vector2.Y, Vector2.Z);

        // var result3 = Vector3.Cross(vector3_1, vector3_2);

        // Result.X = result3.X;
        // Result.Y = result3.Y;
        // Result.Z = result3.Z;
        // }

        // internal static float VectorDotProduct(CBODataTypes.CSGVector Vector1, CBODataTypes.CSGVector Vector2)
        // {
        // var vector3_1 = new Vector3(Vector1.X, Vector1.Y, Vector1.Z);
        // var vector3_2 = new Vector3(Vector2.X, Vector2.Y, Vector2.Z);

        // return Vector3.Dot(vector3_1, vector3_2);
        // }

        // internal static float VectorModulus(CBODataTypes.CSGVector Vector)
        // {
        // var vector3 = new Vector3(Vector.X, Vector.Y, Vector.Z);
        // return vector3.Length();

        // // return gcrCRetainedMode.VectorModulus(Vector.X, Vector.Y, Vector.Z);
        // }

        // internal static void VectorNormalize(ref CBODataTypes.CSGVector Result, CBODataTypes.CSGVector Vector)
        // {
        // var vector3 = new Vector3(Vector.X, Vector.Y, Vector.Z);
        // vector3.Normalize();

        // Result.X = vector3.X;
        // Result.Y = vector3.Y;
        // Result.Z = vector3.Z;
        // }
        internal static bool bCoLinear(Vector3[] cvTriangleFacet)
        {
            bool result = false;
            var cvVector = new Vector3[2];

            cvVector[0].X = cvTriangleFacet[0].X - cvTriangleFacet[1].X;
            cvVector[0].Y = cvTriangleFacet[0].Y - cvTriangleFacet[1].Y;
            cvVector[0].Z = cvTriangleFacet[0].Z - cvTriangleFacet[1].Z; // cvVector(0)
            cvVector[0].Normalize();
            cvVector[1].X = cvTriangleFacet[2].X - cvTriangleFacet[1].X;
            cvVector[1].Y = cvTriangleFacet[2].Y - cvTriangleFacet[1].Y;
            cvVector[1].Z = cvTriangleFacet[2].Z - cvTriangleFacet[1].Z; // cvVector(1)
            cvVector[1].Normalize();
            float sAngle = Vector3.Dot(cvVector[0], cvVector[1]);

            // is it colinear? (Angle is 180 degrees (cos(180) = -1))
            if (Math.Abs(sAngle + 1) < CSG_sSmallEpsilon)
            {
                result = true;
            }

            return result;
        }

        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************
        internal static bool bConvex(Vector3[] cvaPolygon, bool bCoarseCheck)
        {
            var cvaFacet = new Vector3[3];
            float sComparisonValue;

            if (bCoarseCheck)
            {
                sComparisonValue = 0.01f;
            }
            else
            {
                sComparisonValue = 0.001f;
            }

            // get the polygon normal
            Vector3 cvPolygonNormal = cvGetNormalConcave(cvaPolygon);

            // look at each vertex. if any are concave then we don't have a convex polygon
            for (int lVertex = 0; lVertex <= cvaPolygon.GetUpperBound(0); lVertex++)
            {
                GetFacet(cvaPolygon, lVertex, cvaFacet);

                // get the vertex normal at lvertex
                Vector3 cvVertexNormal = cvGetNormal(cvaFacet);

                // determine if convex at lvertex i.e. same normal as the polygon as a whole
                if (Math.Abs(cvVertexNormal.X - cvPolygonNormal.X) > sComparisonValue || Math.Abs(cvVertexNormal.Y - cvPolygonNormal.Y) > sComparisonValue
                    || Math.Abs(cvVertexNormal.Z - cvPolygonNormal.Z) > sComparisonValue)
                {
                    return false; // the easy way out
                }
            }

            // we made it this far, so we are convex
            return true;
        }

        internal static bool bVectorEqual(float sVector0X, float sVector0Y, float sVector0Z, float sVector1X, float sVector1Y, float sVector1z)
        {
            bool result = Math.Abs(sVector0X - sVector1X) < CSG_sSmallEpsilon && Math.Abs(sVector0Y - sVector1Y) < CSG_sSmallEpsilon
                          && Math.Abs(sVector0Z - sVector1z) < CSG_sSmallEpsilon;

            return result;
        }

        internal static bool bVectorEqual2(Vector3 cvVector0, Vector3 cvVector1)
        {
            // faster version
            bool result = false;
            if (Math.Abs(cvVector0.X - cvVector1.X) < CSG_sSmallEpsilon)
            {
                if (Math.Abs(cvVector0.Y - cvVector1.Y) < CSG_sSmallEpsilon)
                {
                    if (Math.Abs(cvVector0.Z - cvVector1.Z) < CSG_sSmallEpsilon)
                    {
                        result = true;
                    }
                }
            }

            // cvVector0
            return result;
        }

        internal static Vector3 cvGetNormal(Vector3[] cvaSourceFace)
        {
            var result = new Vector3();
            var cvVector1 = new Vector3();
            var cvVector2 = new Vector3();

            // Dim lVertex As Long
            // calculate face normal
            cvVector1.X = cvaSourceFace[1].X - cvaSourceFace[0].X;
            cvVector1.Y = cvaSourceFace[1].Y - cvaSourceFace[0].Y;
            cvVector1.Z = cvaSourceFace[1].Z - cvaSourceFace[0].Z; // cvVector1
            cvVector2.X = cvaSourceFace[2].X - cvaSourceFace[1].X;
            cvVector2.Y = cvaSourceFace[2].Y - cvaSourceFace[1].Y;
            cvVector2.Z = cvaSourceFace[2].Z - cvaSourceFace[1].Z; // cvVector2
            result = Vector3.Cross(cvVector1, cvVector2);
            if (result.X != 0 || result.Y != 0 || result.Z != 0)
            {
                result.Normalize();
            }

            // 'begin testing
            // Dim csWork As CSGShape
            // Dim cfFace As CSGFace
            // Dim xdttestnormal As CSGVector
            // 'determine the source face's normal
            // 'create a shape with the supplied face since it is much
            // 'easier to find the face normal using
            // 'built in RM features
            // gset csWork = cCSG.CreateShape
            // 'create the face
            // csWork,cfFace
            // 'add the vertices to the face
            // For lVertex = 0 To 2 ' UBound(cvaSourceFace) ' 0 To UBound(cvaSourceFace)
            // cfFace.AddPointXYZUV gcCSGFunctions.createvector(cvaSourceFace(lVertex).x, cvaSourceFace(lVertex).y, cvaSourceFace(lVertex).z
            // Next lVertex
            // cfFace.fGetFaceNormal xdttestnormal
            // Set cfFace = Nothing
            // Set csWork = Nothing
            // If Abs(xdttestnormal.x - pcvGetNormal.x) > 0.001
            // '    Or Abs(xdttestnormal.y - pcvGetNormal.y) > 0.001
            // '    Or Abs(xdttestnormal.z - pcvGetNormal.z) > 0.001 Then
            // 'MsgBox "pcvGetNormal failed"
            // End If
            // end testing
            return result;
        }

        internal static Vector3 cvGetNormalConcave(Vector3[] cvaSourceFace)
        {
            var result = new Vector3();
            var cvNormal = new Vector3();
            var cvVector1 = new Vector3();
            var cvVector2 = new Vector3();

            int lVertexPrevious = cvaSourceFace.GetUpperBound(0);
            for (int lVertex = 0; lVertex <= cvaSourceFace.GetUpperBound(0); lVertex++)
            {
                // *rwb this looks wrong. why don't we subtract an origin?
                // Is it because the effect of doing ALL of the vertices of a polygon
                // corrects the obvious problem?
                // a special case for the last vertex since it connects to the first
                cvVector1.X = cvaSourceFace[lVertexPrevious].X; // - cvaSourceFace(UBound(cvaSourceFace)).X
                cvVector1.Y = cvaSourceFace[lVertexPrevious].Y; // - cvaSourceFace(UBound(cvaSourceFace)).Y
                cvVector1.Z = cvaSourceFace[lVertexPrevious].Z; // - cvaSourceFace(UBound(cvaSourceFace)).z //cvVector1
                cvVector2.X = cvaSourceFace[lVertex].X; // - cvaSourceFace(lVertex).X
                cvVector2.Y = cvaSourceFace[lVertex].Y; // - cvaSourceFace(lVertex).Y
                cvVector2.Z = cvaSourceFace[lVertex].Z; // - cvaSourceFace(lVertex).z

                // time for the cross product calculation //cvVector2
                cvNormal = Vector3.Cross(cvVector1, cvVector2);

                // sum things up
                result.X = result.X + cvNormal.X;
                result.Y = result.Y + cvNormal.Y;
                result.Z = result.Z + cvNormal.Z;

                // keep track of the previous vertex //cvGetNormalConcave
                lVertexPrevious = lVertex;
            }

            if (result.X != 0 || result.Y != 0 || result.Z != 0)
            {
                result.Normalize();
            }

            // 'being testing
            // Dim cvNormalTest As CSGVector
            // cvNormalTest = cvGetNormalConcave2(cvaSourceFace)
            // '    'use the results of cvNormaltest
            // '    cvGetNormalConcave = cvNormalTest
            // '
            // If Abs(cvGetNormalConcave.X - cvNormalTest.X) > CSG_sSmallEpsilon
            // '    Or Abs(cvGetNormalConcave.Y - cvNormalTest.Y) > CSG_sSmallEpsilon
            // '    Or Abs(cvGetNormalConcave.Z - cvNormalTest.Z) > CSG_sSmallEpsilon Then
            // 'debug.print "cvGetNormalConcave2 doesn't agree with cvGetNormalConcave"
            // 'debug.print CSGSnapTo(cvGetNormalConcave.X, 0.001) & "," & CSGSnapTo(cvNormalTest.X, 0.001)
            // 'debug.print CSGSnapTo(cvGetNormalConcave.Y, 0.001) & "," & CSGSnapTo(cvNormalTest.Y, 0.001)
            // 'debug.print CSGSnapTo(cvGetNormalConcave.Z, 0.001) & "," & CSGSnapTo(cvNormalTest.Z, 0.001)
            // 'msgbox "cvGetNormalConcave2 doesn't agree with cvGetNormalConcave"
            // End If
            // 'end testing
            return result;
        }

        internal static void GetFacet(Vector3[] cvaFace, int lFacet, Vector3[] cvaFacet)
        {
            // get first vertex of facet
            // exception for the first facet since the first vertex of the facet is the last
            // vertex of the polygon
            if (lFacet == 0)
            {
                cvaFacet[0] = cvaFace[cvaFace.GetUpperBound(0)];
            }
            else
            {
                // normal
                cvaFacet[0] = cvaFace[lFacet - 1];
            }

            // get second vertex of facet
            cvaFacet[1] = cvaFace[lFacet];

            // get final vertex
            // exception for final vertex since the final vertex of the facet is the
            // first vertex of the polygon
            if (lFacet == cvaFace.GetUpperBound(0))
            {
                cvaFacet[2] = cvaFace[0];
            }
            else
            {
                cvaFacet[2] = cvaFace[lFacet + 1];
            }
        }

        internal static void GetFacet2(int[] laPolygon, int lFacet, int[] laFacet)
        {
            // get first vertex of facet
            // exception for the first facet since the first vertex of the facet is the last
            // vertex of the polygon
            if (lFacet == 0)
            {
                laFacet[0] = laPolygon[laPolygon.GetUpperBound(0)];
            }
            else
            {
                // normal
                laFacet[0] = laPolygon[lFacet - 1];
            }

            // get second vertex of facet
            laFacet[1] = laPolygon[lFacet];

            // get final vertex
            // exception for final vertex since the final vertex of the facet is the
            // first vertex of the polygon
            if (lFacet == laPolygon.GetUpperBound(0))
            {
                laFacet[2] = laPolygon[0];
            }
            else
            {
                laFacet[2] = laPolygon[lFacet + 1];
            }
        }

        internal static int lGetSGPointIndex(
            CBODataTypes.i_CSGPoint udtPoint,
            ref CBODataTypes.i_CSGPoint[] cvaPointList,
            ref int lPointListCount,
            int[] laHashTable,
            int lHashTableType,
            bool bTestOnly,
            bool bOptimizePoints)
        {
            int result = 0;

            // byte[] bytHash = new byte[4];
            bool bFound = false;
            int lPreviousPoint = 0;
            float sPointSum = 0;

            // this seems a reasonable alternate choice - maybe faster
            // sPointSum = udtPoint.PointXYZ.X + udtPoint.PointXYZ.Y + udtPoint.PointXYZ.Z
            // CopyMemory bytHash(0), sPointSum, 4
            // lHashPosition = CLng(bytHash(1)) * 256 * lHashTableType + CLng(bytHash(2))
            if (Math.Abs(udtPoint.PointXYZ.X) > CSG_sSmallEpsilon)
            {
                sPointSum += udtPoint.PointXYZ.X;
            }

            if (Math.Abs(udtPoint.PointXYZ.Y) > CSG_sSmallEpsilon)
            {
                sPointSum += udtPoint.PointXYZ.Y;
            }

            if (Math.Abs(udtPoint.PointXYZ.Z) > CSG_sSmallEpsilon)
            {
                sPointSum += udtPoint.PointXYZ.Z;
            }

            sPointSum = float.Parse(sPointSum.ToString("0.0000E+00"));

            var bytHash = BitConverter.GetBytes(sPointSum);

            // get the "bytes" of the Vector
            // UpgradeSolution1Support.PInvoke.SafeNative.kernel32.CopyMemory(ref bytHash[0], ref sPointSum, 4);
            // make a hash
            int lHashPosition = bytHash[1] * 256 * lHashTableType + bytHash[2];

            // run through the list at this hash position to determine
            // an appropriate location
            if (bOptimizePoints && laHashTable[lHashPosition] != 0)
            {
                result = laHashTable[lHashPosition] - 1;

                while (result >= 0)
                {
                    lPreviousPoint = result;
                    if (Math.Abs(cvaPointList[result].PointXYZ.X - udtPoint.PointXYZ.X) < CSG_sSmallEpsilon)
                    {
                        if (Math.Abs(cvaPointList[result].PointXYZ.Y - udtPoint.PointXYZ.Y) < CSG_sSmallEpsilon)
                        {
                            if (Math.Abs(cvaPointList[result].PointXYZ.Z - udtPoint.PointXYZ.Z) < CSG_sSmallEpsilon)
                            {
                                bFound = true;
                                break; // found one - easy way out
                            }
                        }
                    }

                    result = cvaPointList[result].NextPoint - 1;
                }
            }

            if (bTestOnly)
            {
                if (!bFound)
                {
                    result = -1;
                }
            }
            else
            {
                // if we didn't find it
                if (!bFound)
                {
                    // store the point in the point list
                    if (lPointListCount == 0)
                    {
                        cvaPointList = new CBODataTypes.i_CSGPoint[1];
                    }
                    else if (cvaPointList.GetUpperBound(0) < lPointListCount)
                    {
                        Array.Resize(ref cvaPointList, lPointListCount + 1);

                        // cvaPointList = ArraysHelper.RedimPreserve(cvaPointList, new int[]{lPointListCount + 1});
                    }

                    cvaPointList[lPointListCount] = udtPoint;

                    // set the index found
                    result = lPointListCount;

                    // increment the number of points
                    lPointListCount++;
                }

                // if the hash entry is zero then there is no "previous point"
                if (laHashTable[lHashPosition] == 0)
                {
                    laHashTable[lHashPosition] = result + 1;

                    // update the previous point to point to this entry
                }
                else if (!bFound)
                {
                    cvaPointList[lPreviousPoint].NextPoint = result + 1;
                }
            }

            return result;
        }

        // Purpose:

        // *********************************************************************************
        // Parameters:
        // *********************************************************************************
    }
}