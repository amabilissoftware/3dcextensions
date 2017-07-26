namespace ACBO
{
    using System;
    using System.Collections.Generic;

    using SharpDX;

    internal class ConstructiveSolidGeometryMesh
    {
        #region Fields

        public int[] PointHashTable;

        public int PointHashTableType;

        public Dictionary<string, int> colEdge;

        // edge information
        // udtaVertexHashTable() As CSGHashEntry
        public Vector3 cvCenter;

        public int lVertexCount;

        public float sBoundingSphere;

        public CBODataTypes.CSGCGFace[] udtBoundingFace; // BoundingPlanes for raypicking

        public CBODataTypes.CSGCGFace[] udtFace; // a face in the CGMesh

        public CBODataTypes.CSGMinMaxVector xdtBoundingBox; // bounding box for the Shape

        public CBODataTypes.i_CSGPoint[] xdtBoundingVertex; // bounding box vertex

        public CBODataTypes.i_CSGPoint[] xdtVertex; // the vertex list for the Shape

        #endregion

        // EdgeHashTable() As Long
        // EdgeHashTableType As Long
        #region Constructors and Destructors

        public ConstructiveSolidGeometryMesh()
        {
            this.xdtBoundingBox = CBODataTypes.CSGMinMaxVector.CreateInstance();
            this.colEdge = new Dictionary<string, int>();
        }

        #endregion

        #region Public Methods and Operators

        public void GetGeometry(
            ref double lFaceMax, 
            ref Vector3[] cvaCompactedVertices, 
            ref int[] laFaceTuples, 
            ref bool[] faceDeleted, 
            ref int[] faceColor)
        {
            int lTupleCount = 0;
            int lNewVertex = 0;

            // find the number of faces in the CGMesh
            lFaceMax = this.udtFace.Length - 1;

            // modCSG.SafeUBound(UpgradeSolution1Support.PInvoke.SafeNative.msvbvm60.VarPtrArray(ArraysHelper.CastArray<System.IntPtr[]>(this.udtFace)));

            // create the Shape
            // if there are some faces
            if (lFaceMax >= 0)
            {
                // run through the list of faces/vertices determining which are in
                // use
                faceDeleted = new bool[Convert.ToInt32(lFaceMax) + 1];
                faceColor = new int[Convert.ToInt32(lFaceMax) + 1];
                var laVerticesInUse = new int[this.xdtVertex.GetUpperBound(0) + 1, 3];
                for (int lFace = 0; lFace <= this.udtFace.GetUpperBound(0); lFace++)
                {
                    if (!this.udtFace[lFace].bDeleted)
                    {
                        foreach (int item in this.udtFace[lFace].lVertex)
                        {
                            laVerticesInUse[item, 0] = 1;
                        }
                    }

                    faceDeleted[lFace] = this.udtFace[lFace].bDeleted;
                    faceColor[lFace] = this.udtFace[lFace].lColor;
                }

                // now calculate the new vertex # for the vertex list
                // and create a compacted list while we are at it
                int lVertex;
                for (lVertex = 0; lVertex <= laVerticesInUse.GetUpperBound(0); lVertex++)
                {
                    if (laVerticesInUse[lVertex, 0] == 1)
                    {
                        laVerticesInUse[lVertex, 1] = lNewVertex;

                        // the compacted list
                        Array.Resize(ref cvaCompactedVertices, lNewVertex + 1);
                        cvaCompactedVertices[lNewVertex] = this.xdtVertex[lVertex].PointXYZ;
                        lNewVertex++;
                    }
                }

                // get the number of vertices in the CGMesh
                // run through the list of faces , adding them to the Shape
                for (int lFace = 0; lFace <= this.udtFace.GetUpperBound(0); lFace++)
                {
                    if (!this.udtFace[lFace].bDeleted)
                    {
                        // allocate some space in the list
                        Array.Resize(ref laFaceTuples, lTupleCount + 1 + this.udtFace[lFace].lVertex.GetUpperBound(0) * 3 + 2);
                        laFaceTuples[lTupleCount] = this.udtFace[lFace].lVertex.GetUpperBound(0) + 1;
                        lTupleCount++;
                        for (lVertex = 0; lVertex <= this.udtFace[lFace].lVertex.GetUpperBound(0); lVertex++)
                        {
                            laFaceTuples[lTupleCount] = laVerticesInUse[this.udtFace[lFace].lVertex[lVertex], 1];
                            lTupleCount += 3;
                        }
                    }
                }

                // udtCGMesh

                // terminate the tuple list
                Array.Resize(ref laFaceTuples, lTupleCount + 2);
                laFaceTuples[lTupleCount] = 0;

                // 'Reserve space for the Vertices, Normals and Faces
                // csCreateShapeFromCGMeshData.ReserveSpace UBound(cvaCompactedVertices) + 1, 0, UBound(this.udtFace)
                // add all of the faces to the Shape
            }
        }

        public List<CSGCGPick> RayPick(CBODataTypes.CSGRay xdtRay, bool bTestFlaggedExteriors, float sMaxRange)
        {
            var udtaPick = new List<CSGCGPick>();
            //var cvIntersectionPosition = new Vector3();
            var udtaPickUnsorted = new List<CSGCGPick>();

            float sMaxRangeSquared = sMaxRange * sMaxRange;
            // int udtaPickUnsorted.Count = 0;

            // int lPickCount = 0;
            var rayPosition = new Vector3(xdtRay.Position.X, xdtRay.Position.Y, xdtRay.Position.Z);
            var rayDirection = new Vector3(xdtRay.Direction.X, xdtRay.Direction.Y, xdtRay.Direction.Z);
            var ray = new Ray(rayPosition, rayDirection);

            var boundingBoxMinimum = new Vector3(this.xdtBoundingBox.Min.X, this.xdtBoundingBox.Min.Y, this.xdtBoundingBox.Min.Z);
            var boundingBoxMaximum = new Vector3(this.xdtBoundingBox.Max.X, this.xdtBoundingBox.Max.Y, this.xdtBoundingBox.Max.Z);
            var boundingBox = new BoundingBox(boundingBoxMinimum, boundingBoxMaximum);

            // 'Dim bSorted                As Boolean

            // Static lTotalJunk As Long
            // Static lSeconds As Long
            // Static lTotalPhase2Junk As Long
            // Dim lJunk As Long
            // Dim lJunk2 As Long
            // Dim lJunk3 As Long
            // lJunk = gcCHFGeneral.GetSystemTime
            // if we have a large CGMesh (>10 faces), let's pre-test against the bounding box
            if (ray.Intersects(boundingBox))
            {
                // go through faces until we find one that is intersected by the ray
                for (int lFace = 0; lFace <= this.udtFace.GetUpperBound(0); lFace++)
                {
                    // ignore deleted faces
                    if (!this.udtFace[lFace].bDeleted)
                    {
                        // optionally ignore intersections for faces that are flagged as exteriors
                        if (bTestFlaggedExteriors || this.udtFace[lFace].udtIntersection != CBODataTypes.CSGFaceType.CSGFaceExterior)
                        {
                            var point0 = this.xdtVertex[this.udtFace[lFace].lVertex[0]].PointXYZ;
                            var point1 = this.xdtVertex[this.udtFace[lFace].lVertex[1]].PointXYZ;
                            var point2 = this.xdtVertex[this.udtFace[lFace].lVertex[2]].PointXYZ;

                            // float intersectionPosition;
                            // var result = Collision.RayIntersectsTriangle(ref ray, ref point0, ref point1, ref point2, out intersectionPosition);
                            Vector3 intersectionPositionXYZ;
                            var wasTriangleIntersected = Collision.RayIntersectsTriangle(
                                ref ray, 
                                ref point0, 
                                ref point1, 
                                ref point2, 
                                out intersectionPositionXYZ);

                            float intersectionDistance = float.MaxValue;

                            if (wasTriangleIntersected)
                            {
                                Vector3 positionDifference = intersectionPositionXYZ - xdtRay.Position;
                                intersectionDistance = positionDifference.Length();

                                if (intersectionDistance > sMaxRange)
                                {
                                    const float myEpsilon = .00001f;
                                    ray.Position += myEpsilon;
                                    wasTriangleIntersected = Collision.RayIntersectsTriangle(
                                        ref ray, 
                                        ref point0, 
                                        ref point1, 
                                        ref point2, 
                                        out intersectionPositionXYZ);

                                    if (wasTriangleIntersected)
                                    {
                                        positionDifference = intersectionPositionXYZ - xdtRay.Position;
                                        intersectionDistance = positionDifference.Length();

                                        if (intersectionDistance > sMaxRange)
                                        {
                                            wasTriangleIntersected = false;
                                        }
                                    }
                                }
                            }

                            if (wasTriangleIntersected)
                            {
                                //cvIntersectionPosition = intersectionPositionXYZ;

                                // is the polygon convex?
                                bool bConvex = pbConvex(this.xdtVertex, this.udtFace[lFace]);

                                // which way does the polygon face
                                float sDotProductResult = Vector3.Dot(this.udtFace[lFace].cvNormal, xdtRay.Direction);

                                // add the pick to the list of picks
                                var pick = new CSGCGPick();
                                pick.lFace = lFace;
                                pick.cvPosition = intersectionPositionXYZ;
                                pick.sDistanceFromRayOrigin = intersectionDistance;
                                pick.bConvex = bConvex;
                                if (sDotProductResult < 0)
                                {
                                    pick.udtType = CBODataTypes.CSGFaceType.CSGFaceExterior;
                                }
                                else
                                {
                                    pick.udtType = CBODataTypes.CSGFaceType.CSGFaceInterior;
                                }

                                udtaPickUnsorted.Add(pick);

                                // udtaPickUnsorted.Count++;
                            }
                        }
                    }
                }
            }

            // now sort the unsorted intersection array
            // bSorted = False
            if (udtaPickUnsorted.Count > 1)
            {
                for (int lPick = 0; lPick <= udtaPickUnsorted.Count - 1; lPick++)
                {
                    float lPickMinimum = lPick;
                    for (int lPick2 = lPick + 1; lPick2 <= udtaPickUnsorted.Count - 1; lPick2++)
                    {
                        // if the same, try to get one that is convex
                        if (
                            Math.Abs(
                                udtaPickUnsorted[lPick2].sDistanceFromRayOrigin
                                - udtaPickUnsorted[Convert.ToInt32(lPickMinimum)].sDistanceFromRayOrigin) < Helper.CSG_sSmallEpsilon
                            && udtaPickUnsorted[lPick2].bConvex && !udtaPickUnsorted[Convert.ToInt32(lPickMinimum)].bConvex)
                        {
                            lPickMinimum = lPick2;
                        }
                        else if (udtaPickUnsorted[lPick2].sDistanceFromRayOrigin
                                 < udtaPickUnsorted[Convert.ToInt32(lPickMinimum)].sDistanceFromRayOrigin)
                        {
                            lPickMinimum = lPick2;
                        }
                    }

                    CSGCGPick udtPickSwap = udtaPickUnsorted[lPick];
                    udtaPickUnsorted[lPick] = udtaPickUnsorted[Convert.ToInt32(lPickMinimum)]; // copy minimum to pick
                    udtaPickUnsorted[Convert.ToInt32(lPickMinimum)] = udtPickSwap; // copy old over previous minimum
                }
            }

            // copy the unique sorted items to the sorted list
            if (udtaPickUnsorted.Count > 0)
            {
                int lPickCount = 1;
                udtaPick.Add(udtaPickUnsorted[0]);
                for (int lPick = 1; lPick <= udtaPickUnsorted.Count - 1; lPick++)
                {
                    // if not just a duplicate of something in the list already
                    if (Math.Abs(udtaPick[lPickCount - 1].sDistanceFromRayOrigin - udtaPickUnsorted[lPick].sDistanceFromRayOrigin)
                        > Helper.CSG_sSmallEpsilon)
                    {
                        udtaPick.Add(udtaPickUnsorted[lPick]);
                        lPickCount++;
                    }
                }
            }

            return udtaPick;
        }

        public bool bDividePolygon(int lFace, float sVertexX, float sVertexY, float sVertexZ)
        {
            bool result = false;
            var cvaFace = new Vector3[3];
            CBODataTypes.i_CSGPoint vertex = CBODataTypes.i_CSGPoint.CreateInstance();
            bool bExistingVertex = false;
            bool bFacetCoLinear = false;
            int lMatchedFace = 0;
            int lMatchedEdge = 0;
            int lColinearVertex = 0;
            var lEdgeSplit = new int[2];
            var lEdge = new int[2];

            // Dim lColinearVertexCount As Long
            // if the vertex we are adding is already a member of the Shape,
            // then we need not do anything (this could well be the case since
            // it would not be that uncommon for a pair of CGMeshes have intersection
            // points in common simply by chance)
            int lMaxVertex = this.xdtVertex.GetUpperBound(0); // track the current maximum
            vertex.PointXYZ.X = sVertexX;
            vertex.PointXYZ.Y = sVertexY;
            vertex.PointXYZ.Z = sVertexZ;

            // lNewVertex = gcCSGFunctions.getVertexCGMesh(udtCGMesh, xdtVertex, acPrecise) 'get the potentially new vertex
            // lNewVertex = gcCSGFunctions.getVertexIndex(xdtVertex, this.xdtVertex, this.lVertexCount, this.xdtBoundingBox.Min.x, this.xdtBoundingBox.Max.x, this.udtaVertexHashTable)
            int lNewVertex = Helper.lGetSGPointIndex(
                vertex, 
                ref this.xdtVertex, 
                ref this.lVertexCount, 
                this.PointHashTable, 
                this.PointHashTableType, 
                false, 
                true);

            // if we still have the same number of vertices then the vertex was an existing vertex
            if (this.xdtVertex.GetUpperBound(0) == lMaxVertex)
            {
                bExistingVertex = true;
            }

            // if a not an existing polygon vertex
            if (!bExistingVertex)
            {
                // see if it bi-sects an edge
                for (int lVertex = 0; lVertex <= this.udtFace[lFace].lVertex.GetUpperBound(0); lVertex++)
                {
                    CBO.GetEdgeCGMesh(this.udtFace[lFace], lVertex, lEdge);
                    cvaFace[0] = this.xdtVertex[lEdge[0]].PointXYZ;

                    // verify it isn't colinear
                    // add the new vertex between the two
                    cvaFace[1].X = sVertexX;
                    cvaFace[1].Y = sVertexY;
                    cvaFace[1].Z = sVertexZ;
                    cvaFace[2] = this.xdtVertex[lEdge[1]].PointXYZ;

                    // If bCoLinear(cvaFace) Then
                    if (pbCoLinearCoarse(cvaFace))
                    {
                        bFacetCoLinear = true;
                        lColinearVertex = lVertex;
                        break;
                    }
                }

                // get the edge split
                CBO.GetEdgeCGMesh(this.udtFace[lFace], lColinearVertex, lEdgeSplit);

                // split this polygon if not an edge split
                if (!bFacetCoLinear)
                {
                    this.SplitPolygon(lFace, lNewVertex);
                    result = true;
                }
                else
                {
                    // an edge split so split the edge
                    // add the vertex to the Shape
                    this.SplitEdge(lFace, lColinearVertex, lNewVertex, true, false);
                    result = true;

                    // if colinear then we have to also split the polygon that shares an edge
                    // find the polygon that shares an edge
                    if (this.bMatchEdgeCGMesh(lEdgeSplit, ref lMatchedFace, ref lMatchedEdge))
                    {
                        this.SplitEdge(lMatchedFace, lMatchedEdge, lNewVertex, true, false);

                        // Else
                        // 'msgbox "didn't match-this shouldn't happen"
                    }
                }
            }

            return result;
        }

        public bool bSplitIntersectorEdge(int lFace, int lEdge, float sVertexX, float sVertexY, float sVertexZ, bool bEdgeComplete)
        {
            bool result = false;
            CBODataTypes.i_CSGPoint vertex = CBODataTypes.i_CSGPoint.CreateInstance();
            int lMatchedFace = 0;
            int lMatchedEdge = 0;
            var lEdgeSplit = new int[2];
            var cvVector = new Vector3();
            bool bRepairable = false;
            int lAttempt = 0;

            // get the edge that will be split
            CBO.GetEdgeCGMesh(this.udtFace[lFace], lEdge, lEdgeSplit);

            // get the vertex that corresponds to the intersection position
            int lVertexMax = this.xdtVertex.GetUpperBound(0);
            vertex.PointXYZ.X = sVertexX;
            vertex.PointXYZ.Y = sVertexY;
            vertex.PointXYZ.Z = sVertexZ;

            // lVertexFound = gcCSGFunctions.getVertexCGMesh(udtCGMesh, xdtVertex, acPrecise)
            // lVertexFound = gcCSGFunctions.getVertexIndex(xdtVertex, this.xdtVertex, this.lVertexCount, this.xdtBoundingBox.Min.x, this.xdtBoundingBox.Max.x, this.udtaVertexHashTable)
            int lVertexFound = Helper.lGetSGPointIndex(
                vertex, 
                ref this.xdtVertex, 
                ref this.lVertexCount, 
                this.PointHashTable, 
                this.PointHashTableType, 
                false, 
                true);

            // since we are splitting an edge, an intersection position of either the
            // origin of the edge or end of the edge means we need not doing anything
            // futher
            if (lEdgeSplit[0] != lVertexFound && lEdgeSplit[1] != lVertexFound)
            {
                // if we still have the same number of vertices then we have a problem
                // the intersection has resulted in two edges that are identical due
                // to rounding. rather than giving up, let's try to fix it.
                // if we gradually shorten the edge length, we can get a point where
                // there isn't an existing vertex and this will do the trick
                // if we still have the same number of vertices then the vertex was an existing vertex
                bool bRepaired;
                if (this.xdtVertex.GetUpperBound(0) == lVertexMax)
                {
                    // Stop
                    // get the length of the vector beyond the intersection point
                    cvVector.X = this.xdtVertex[lEdgeSplit[1]].PointXYZ.X - sVertexX;
                    cvVector.Y = this.xdtVertex[lEdgeSplit[1]].PointXYZ.Y - sVertexY;
                    cvVector.Z = this.xdtVertex[lEdgeSplit[1]].PointXYZ.Z - sVertexZ;
                    float sLength = cvVector.Length();

                    // get the vector that describes the edge
                    cvVector.X = this.xdtVertex[lEdgeSplit[1]].PointXYZ.X - this.xdtVertex[lEdgeSplit[0]].PointXYZ.X;
                    cvVector.Y = this.xdtVertex[lEdgeSplit[1]].PointXYZ.Y - this.xdtVertex[lEdgeSplit[0]].PointXYZ.Y;
                    cvVector.Z = this.xdtVertex[lEdgeSplit[1]].PointXYZ.Z - this.xdtVertex[lEdgeSplit[0]].PointXYZ.Z;
                    cvVector.Normalize();
                    int lMaxAttempt = Convert.ToInt32(sLength * 1000) - 1;
                    bRepaired = false;
                    if (lMaxAttempt > 0)
                    {
                        bRepairable = true;
                    }

                    vertex.PointXYZ.X = sVertexX;
                    vertex.PointXYZ.Y = sVertexY;
                    vertex.PointXYZ.Z = sVertexZ;

                    while (!bRepaired && bRepairable)
                    {
                        lVertexMax = this.xdtVertex.GetUpperBound(0);

                        // subtract .01% of the intersected edge length until we get a
                        // new unique vertex
                        vertex.PointXYZ.X += cvVector.X / 1000;
                        vertex.PointXYZ.Y += cvVector.Y / 1000;
                        vertex.PointXYZ.Z += cvVector.Z / 1000;

                        // lVertexFound = gcCSGFunctions.getVertexCGMesh(udtCGMesh, xdtVertex, acPrecise)
                        // lVertexFound = gcCSGFunctions.getVertexIndex(xdtVertex, this.xdtVertex, this.lVertexCount, this.xdtBoundingBox.Min.x, this.xdtBoundingBox.Max.x, this.udtaVertexHashTable)
                        lVertexFound = Helper.lGetSGPointIndex(
                            vertex, 
                            ref this.xdtVertex, 
                            ref this.lVertexCount, 
                            this.PointHashTable, 
                            this.PointHashTableType, 
                            false, 
                            true);

                        // did we find the origin of the edge? if so we didn't fix things
                        if (lVertexFound == lEdgeSplit[1])
                        {
                            // Stop
                            bRepairable = false;
                        }
                        else if (lVertexFound == lEdgeSplit[0])
                        {
                            // now this is confusing - it should never happen
                            // Stop
                            bRepairable = false;
                        }
                        else if (this.xdtVertex.GetUpperBound(0) != lVertexMax)
                        {
                            // we fixed it
                            // Stop
                            bRepaired = true;
                        }
                        else if (lAttempt == lMaxAttempt)
                        {
                            // Stop
                            bRepairable = false;
                        }

                        lAttempt++;
                    }
                }
                else
                {
                    bRepaired = true; // it was never broken really
                }

                // if we created a unique vertex, split the intersector edge
                if (bRepaired)
                {
                    // if there are no further intersections we can mark the entire
                    // edge as done
                    if (bEdgeComplete)
                    {
                        this.SplitEdge(lFace, lEdge, lVertexFound, true, true);
                    }
                    else
                    {
                        this.SplitEdge(lFace, lEdge, lVertexFound, true, false);
                    }

                    result = true;

                    // if colinear then we have to also split the polygon that shares an edge
                    // find the polygon that shares an edge
                    if (this.bMatchEdgeCGMesh(lEdgeSplit, ref lMatchedFace, ref lMatchedEdge))
                    {
                        this.SplitEdge(lMatchedFace, lMatchedEdge, lVertexFound, true, true);

                        // Else
                        // 'msgbox "didn't match-this shouldn't happen"
                    }
                }
                else
                {
                    // we have a problem that we'll ignore
                    // Stop
                    // indicate that we have processed the edge
                    this.udtFace[lFace].lEdgeProcessed[lEdge] = 1;
                }
            }
            else
            {
                // indicate that we have processed the edge
                this.udtFace[lFace].lEdgeProcessed[lEdge] = 1;
            }

            return result;
        }

        #endregion

        #region Methods

        internal int AddFace(CBODataTypes.CSGCGFace face, bool bValidateFace)
        {
            int result;
            bool bValid;

            // ensure the face we are trying to add is valid
            if (bValidateFace)
            {
                bValid = this.IsFaceValid(face);
            }
            else
            {
                bValid = true;
            }

            if (bValid)
            {
                // get the face id to be added
                if (this.udtFace == null)
                {
                    result = 0;
                }
                else
                {
                    result = this.udtFace.Length;

                    // modCSG.SafeUBound(UpgradeSolution1Support.PInvoke.SafeNative.msvbvm60.VarPtrArray(ArraysHelper.CastArray<System.IntPtr[]>(this.udtFace))) + 1;
                }

                // create the new polygon
                Array.Resize(ref this.udtFace, result + 1);

                // copy the polygon to the CGMesh
                this.udtFace[result] = face;

                // calculate the normal information
                CreateNormal(this.xdtVertex, ref this.udtFace[result]);

                // calculate the bounding plane information
                CreateBoundingPlanes(this.xdtVertex, this.udtFace[result]);

                // calculate the edge information //udtCGMesh
                this.pCreateFaceEdgeData(result);
            }
            else
            {
                // otherwise halt with an error
                throw new CGIntersectionFailureException();
            }

            return result;
        }

        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************
        // optimizes while it works
        internal void CreateBoundingBox()
        {
            int lVertex;
            var cvVector = new Vector3();

            // calculate the bounding box
            for (lVertex = 0; lVertex <= this.xdtVertex.GetUpperBound(0); lVertex++)
            {
                // keep track of the minimums and maximums
                // the first vertex is both the maximums and minimums
                if (lVertex == 0)
                {
                    this.xdtBoundingBox.Min = this.xdtVertex[lVertex].PointXYZ;
                    this.xdtBoundingBox.Max = this.xdtVertex[lVertex].PointXYZ;
                }
                else
                {
                    if (this.xdtVertex[lVertex].PointXYZ.X < this.xdtBoundingBox.Min.X)
                    {
                        this.xdtBoundingBox.Min.X = this.xdtVertex[lVertex].PointXYZ.X;
                    }
                    else if (this.xdtVertex[lVertex].PointXYZ.X > this.xdtBoundingBox.Max.X)
                    {
                        this.xdtBoundingBox.Max.X = this.xdtVertex[lVertex].PointXYZ.X;
                    }

                    if (this.xdtVertex[lVertex].PointXYZ.Y < this.xdtBoundingBox.Min.Y)
                    {
                        this.xdtBoundingBox.Min.Y = this.xdtVertex[lVertex].PointXYZ.Y;
                    }
                    else if (this.xdtVertex[lVertex].PointXYZ.Y > this.xdtBoundingBox.Max.Y)
                    {
                        this.xdtBoundingBox.Max.Y = this.xdtVertex[lVertex].PointXYZ.Y;
                    }

                    if (this.xdtVertex[lVertex].PointXYZ.Z < this.xdtBoundingBox.Min.Z)
                    {
                        this.xdtBoundingBox.Min.Z = this.xdtVertex[lVertex].PointXYZ.Z;
                    }
                    else if (this.xdtVertex[lVertex].PointXYZ.Z > this.xdtBoundingBox.Max.Z)
                    {
                        this.xdtBoundingBox.Max.Z = this.xdtVertex[lVertex].PointXYZ.Z;
                    }
                }
            }

            // now create the bounding box vertices (8 of them
            this.xdtBoundingVertex = new CBODataTypes.i_CSGPoint[8];

            // now create the polygons for the bounding box (6 of them)
            this.udtBoundingFace = new CBODataTypes.CSGCGFace[6];

            // reserve space for the bounding box
            for (int lFace = 0; lFace <= this.udtBoundingFace.GetUpperBound(0); lFace++)
            {
                this.udtBoundingFace[lFace].lVertex = new int[4];
                this.udtBoundingFace[lFace].lEdgeProcessed = new int[4];
                this.udtBoundingFace[lFace].cvEdgeBoundingPlaneNormal = new Vector3[4];
                this.udtBoundingFace[lFace].cvEdgeBoundingPlanePoint = new Vector3[4];
            }

            // add the 4 bounding vertices on the minimum Y plane
            this.xdtBoundingVertex[0].PointXYZ = this.xdtBoundingBox.Min;
            this.xdtBoundingVertex[1].PointXYZ.X = this.xdtBoundingBox.Min.X;
            this.xdtBoundingVertex[1].PointXYZ.Y = this.xdtBoundingBox.Min.Y;
            this.xdtBoundingVertex[1].PointXYZ.Z = this.xdtBoundingBox.Max.Z;
            this.xdtBoundingVertex[2].PointXYZ.X = this.xdtBoundingBox.Max.X;
            this.xdtBoundingVertex[2].PointXYZ.Y = this.xdtBoundingBox.Min.Y;
            this.xdtBoundingVertex[2].PointXYZ.Z = this.xdtBoundingBox.Max.Z;
            this.xdtBoundingVertex[3].PointXYZ.X = this.xdtBoundingBox.Max.X;
            this.xdtBoundingVertex[3].PointXYZ.Y = this.xdtBoundingBox.Min.Y;
            this.xdtBoundingVertex[3].PointXYZ.Z = this.xdtBoundingBox.Min.Z;

            // add the 4 bounding vertices on the maximum Y plane
            this.xdtBoundingVertex[4].PointXYZ.X = this.xdtBoundingBox.Min.X;
            this.xdtBoundingVertex[4].PointXYZ.Y = this.xdtBoundingBox.Max.Y;
            this.xdtBoundingVertex[4].PointXYZ.Z = this.xdtBoundingBox.Min.Z;
            this.xdtBoundingVertex[5].PointXYZ.X = this.xdtBoundingBox.Min.X;
            this.xdtBoundingVertex[5].PointXYZ.Y = this.xdtBoundingBox.Max.Y;
            this.xdtBoundingVertex[5].PointXYZ.Z = this.xdtBoundingBox.Max.Z;
            this.xdtBoundingVertex[6].PointXYZ = this.xdtBoundingBox.Max;
            this.xdtBoundingVertex[7].PointXYZ.X = this.xdtBoundingBox.Max.X;
            this.xdtBoundingVertex[7].PointXYZ.Y = this.xdtBoundingBox.Max.Y;
            this.xdtBoundingVertex[7].PointXYZ.Z = this.xdtBoundingBox.Min.Z;

            // Top
            this.udtBoundingFace[0].lVertex[0] = 3;
            this.udtBoundingFace[0].lVertex[1] = 2;
            this.udtBoundingFace[0].lVertex[2] = 1;
            this.udtBoundingFace[0].lVertex[3] = 0;

            // Bottom
            this.udtBoundingFace[1].lVertex[0] = 4;
            this.udtBoundingFace[1].lVertex[1] = 5;
            this.udtBoundingFace[1].lVertex[2] = 6;
            this.udtBoundingFace[1].lVertex[3] = 7;

            // Front
            this.udtBoundingFace[2].lVertex[0] = 0;
            this.udtBoundingFace[2].lVertex[1] = 4;
            this.udtBoundingFace[2].lVertex[2] = 7;
            this.udtBoundingFace[2].lVertex[3] = 3;

            // Back
            this.udtBoundingFace[3].lVertex[0] = 2;
            this.udtBoundingFace[3].lVertex[1] = 6;
            this.udtBoundingFace[3].lVertex[2] = 5;
            this.udtBoundingFace[3].lVertex[3] = 1;

            // Left
            this.udtBoundingFace[4].lVertex[0] = 5;
            this.udtBoundingFace[4].lVertex[1] = 4;
            this.udtBoundingFace[4].lVertex[2] = 0;
            this.udtBoundingFace[4].lVertex[3] = 1;

            // Right
            this.udtBoundingFace[5].lVertex[0] = 7;
            this.udtBoundingFace[5].lVertex[1] = 6;
            this.udtBoundingFace[5].lVertex[2] = 2;
            this.udtBoundingFace[5].lVertex[3] = 3;

            // create the bounding planes for the bounding box
            // run through all of the faces
            for (int lFace = 0; lFace <= this.udtBoundingFace.GetUpperBound(0); lFace++)
            {
                // calculate the face normal
                CreateNormal(this.xdtBoundingVertex, ref this.udtBoundingFace[lFace]);

                // create the polygon's bounding planes
                CreateBoundingPlanes(this.xdtBoundingVertex, this.udtBoundingFace[lFace]);
            }

            // udtCGMesh

            // now determine the center of the CGMesh
            this.cvCenter.X = (this.xdtBoundingBox.Max.X + this.xdtBoundingBox.Min.X) / 2;
            this.cvCenter.Y = (this.xdtBoundingBox.Max.Y + this.xdtBoundingBox.Min.Y) / 2;
            this.cvCenter.Z = (this.xdtBoundingBox.Max.Z + this.xdtBoundingBox.Min.Z) / 2;

            // now calculate the bounding sphere
            this.sBoundingSphere = -1; // impossible value
            for (lVertex = 0; lVertex <= this.xdtVertex.GetUpperBound(0); lVertex++)
            {
                cvVector.X = this.xdtVertex[lVertex].PointXYZ.X - this.cvCenter.X;
                cvVector.Y = this.xdtVertex[lVertex].PointXYZ.Y - this.cvCenter.Y;
                cvVector.Z = this.xdtVertex[lVertex].PointXYZ.Z - this.cvCenter.Z;
                float sRadius = cvVector.Length();
                if (sRadius > this.sBoundingSphere)
                {
                    this.sBoundingSphere = sRadius;
                }
            }

            // udtCGMesh
        }

        internal void DeleteFace(int lFace)
        {
            this.udtFace[lFace].bDeleted = true;
            this.pDeleteFaceEdgeData(lFace);
        }

        // *********************************************************************************
        // Private Methods
        // *********************************************************************************
        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************
        internal void Detessellate(bool bQuickDetessellate)
        {
            int lFace = 0;
            var lEdge = new int[2];
            int lMatchedFace = 0;
            int lMatchedEdge = 0;

            int lFaceMax = this.udtFace.Length - 1;

            // modCSG.SafeUBound(UpgradeSolution1Support.PInvoke.SafeNative.msvbvm60.VarPtrArray(ArraysHelper.CastArray<System.IntPtr[]>(this.udtFace)));
            if (lFaceMax >= 0)
            {
                while (lFace <= this.udtFace.GetUpperBound(0))
                {
                    bool bReprocessFace = false;
                    if (!this.udtFace[lFace].bDeleted)
                    {
                        for (int lVertex = 0; lVertex <= this.udtFace[lFace].lVertex.GetUpperBound(0); lVertex++)
                        {
                            // get the edge
                            CBO.GetEdgeCGMesh(this.udtFace[lFace], lVertex, lEdge);

                            // get a matching edge
                            if (this.bMatchEdgeCGMesh(lEdge, ref lMatchedFace, ref lMatchedEdge))
                            {
                                if (bQuickDetessellate)
                                {
                                    if (lMatchedFace == lFace + 1)
                                    {
                                        bReprocessFace = this.pbDeTessellateProcessQuick(lFace, lVertex, lMatchedFace, lMatchedEdge);
                                    }
                                    else
                                    {
                                        bReprocessFace = false;
                                    }
                                }
                                else
                                {
                                    bReprocessFace = this.pbDeTessellateProcess(lFace, lVertex, lMatchedFace, lMatchedEdge);
                                }

                                if (bReprocessFace)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    if (!bReprocessFace)
                    {
                        lFace++;
                    }
                }
            }
        }

        internal void InitializeFromGeometry(
            Vector3[] PointList, 
            int PointListCount, 
            Vector3[] NormalList, 
            int NormalListCount, 
            CBODataTypes.CSGUV[] TextureCoordinateList, 
            int TextureCoordinateListCount, 
            int[] FaceData, 
            int FaceDataCount, 
            int[] faceColorList, 
            int faceColorListCount, 
            bool bValidateFace)
        {
            CBODataTypes.CSGCGFace face = CBODataTypes.CSGCGFace.CreateInstance();
            CBODataTypes.i_CSGPoint udtPoint = CBODataTypes.i_CSGPoint.CreateInstance();

            // Dim lNewFace             As Long
            // UPGRADE_TODO: (1065) Error handling statement (On Error Goto) could not be converted. More Information: http://www.vbtonet.com/ewis/ewi1065.aspx
            // UpgradeHelpers.Helpers.NotUpgradedHelper.NotifyNotUpgradedElement("On Error Goto Label (0)");
            // get the vertices from the Shape
            // csShape.GetPointsXYZ cvaShapeVertexList, lPointListCount
            // need to allocate space for the point hash table
            if (PointListCount > 4000)
            {
                this.PointHashTableType = 1;
                this.PointHashTable = new int[65536];
            }
            else
            {
                this.PointHashTableType = 0;
                this.PointHashTable = new int[256];
            }

            // 'get its bounding box
            // xdtBoundingBox  =csShape.GetBoundingBox
            // determine the hash table sizes etc.
            // ReDim this.udtaVertexHashTable(CInt(csShape.GetPointCount ^ 0.5)) As CSGHashEntry
            // now create the vertex list in the CGMesh (it may not match that of
            // the Shape exactly. rounding may cause some vertices to be
            // merged (effectively) in the CGMesh
            var lCGMeshVertexList = new int[PointListCount];

            // MUST initialize the vertex list since lGetSGPointIndex expects it
            this.xdtVertex = new CBODataTypes.i_CSGPoint[PointListCount];
            for (int lVertex = 0; lVertex <= PointListCount - 1; lVertex++)
            {
                udtPoint.PointXYZ = PointList[lVertex]; // cvaShapeVertexList(lVertex)
                lCGMeshVertexList[lVertex] = Helper.lGetSGPointIndex(
                    udtPoint, 
                    ref this.xdtVertex, 
                    ref this.lVertexCount, 
                    this.PointHashTable, 
                    this.PointHashTableType, 
                    false, 
                    true);
            }

            int lFaceMax = FaceData.Length - 1;

            // FaceData.GetUpperBound; modCSG.SafeUBound(UpgradeSolution1Support.PInvoke.SafeNative.msvbvm60.VarPtrArray(ArraysHelper.CastArray<System.IntPtr[]>(FaceData)));
            int lElement = 0;
            int lFace = 0;

            while (lElement < lFaceMax)
            {
                int vertexCount = FaceData[lElement];
                lElement++;

                CBO.CreateFace(ref face, vertexCount - 1);

                // get each vertex
                int lLastVertex = -1;
                int lFirstVertex = -1;
                int lActualVertexCount = 0;
                for (int lVertex = 0; lVertex <= vertexCount - 1; lVertex++)
                {
                    int lOptimizedVertex = FaceData[lElement];

                    // track the first vertex
                    if (lFirstVertex == -1)
                    {
                        lFirstVertex = lOptimizedVertex;
                    }

                    // don't add duplicate vertices
                    if (lOptimizedVertex != lLastVertex)
                    {
                        // add the vertex to the face
                        face.lVertex[lActualVertexCount] = lOptimizedVertex;

                        // track the last vertex added
                        lLastVertex = lOptimizedVertex;

                        // track the actual vertices added
                        lActualVertexCount++;
                    }

                    // ReDim Preserve laNewFaceData(lNewFaceDataCount + 9) As Long
                    // laNewFaceData(lNewFaceDataCount) = 3 '3 vertices coming
                    // 'each triangle starts at lelement
                    // laNewFaceData(lNewFaceDataCount + 1) = laFaceData(lElement)
                    // laNewFaceData(lNewFaceDataCount + 2) = laFaceData(lElement + 1)
                    // laNewFaceData(lNewFaceDataCount + 3) = laFaceData(lElement + 2)
                    // laNewFaceData(lNewFaceDataCount + 4) = laFaceData(lElement + lVertex * 3 + 3)
                    // laNewFaceData(lNewFaceDataCount + 5) = laFaceData(lElement + lVertex * 3 + 3 + 1)
                    // laNewFaceData(lNewFaceDataCount + 6) = laFaceData(lElement + lVertex * 3 + 3 + 2)
                    // laNewFaceData(lNewFaceDataCount + 7) = laFaceData(lElement + lVertex * 3 + 6)
                    // laNewFaceData(lNewFaceDataCount + 8) = laFaceData(lElement + lVertex * 3 + 6 + 1)
                    // laNewFaceData(lNewFaceDataCount + 9) = laFaceData(lElement + lVertex * 3 + 6 + 2)
                    // lNewFaceDataCount = lNewFaceDataCount + 10
                    lElement += 3;
                }

                // if the first vertex equal the last decrement to remove the last
                if (lLastVertex == lFirstVertex)
                {
                    lActualVertexCount--;
                }

                // resize the face vectors to match the actual number of vertices
                if (lActualVertexCount > 0)
                {
                    Array.Resize(ref face.lVertex, lActualVertexCount);
                    Array.Resize(ref face.lEdgeProcessed, lActualVertexCount);
                    Array.Resize(ref face.cvEdgeBoundingPlaneNormal, lActualVertexCount);
                    Array.Resize(ref face.cvEdgeBoundingPlanePoint, lActualVertexCount);
                }

                // get the color
                face.lColor = faceColorList[lFace]; // csShape.fGetFaceMaterial(lFace).x_Color

                // add the face to the CGMesh (if we fail-we don't fail-we ignore it)
                // UPGRADE_TODO: (1065) Error handling statement (On Error Resume Next) could not be converted. More Information: http://www.vbtonet.com/ewis/ewi1065.aspx
                // UpgradeHelpers.Helpers.NotUpgradedHelper.NotifyNotUpgradedElement("On Error Resume Next");
                if (lActualVertexCount > 2)
                {
                    this.AddFace(face, bValidateFace);
                }

                // UPGRADE_TODO: (1065) Error handling statement (On Error Goto) could not be converted. More Information: http://www.vbtonet.com/ewis/ewi1065.aspx
                // UpgradeHelpers.Helpers.NotUpgradedHelper.NotifyNotUpgradedElement("On Error Goto Label (0)");

                // lElement = lElement + lVertexCount * 3
                lFace++;
            }

            // 'run through all of the faces
            // For lFace = 0 To csShape.GetFaceCount - 1
            // 'create a face
            // CreateFace udtFace, csShape.fGetFacePointCount(lFace) - 1
            // 'get each vertex
            // lLastVertex = -1
            // lFirstVertex = -1
            // lActualVertexCount = 0
            // For lVertex = 0 To csShape.fGetFacePointCount(lFace) - 1
            // lOptimizedVertex = lCGMeshVertexList(csShape.fGetFacePointID(lFace, lVertex))
            // 'track the first vertex
            // If lFirstVertex = -1 Then
            // lFirstVertex = lOptimizedVertex
            // End If
            // 'don't add duplicate vertices
            // If lOptimizedVertex <> lLastVertex Then
            // 'add the vertex to the face
            // udtFace.lVertex(lActualVertexCount) = lOptimizedVertex
            // 'track the last vertex added
            // lLastVertex = lOptimizedVertex
            // 'track the actual vertices added
            // lActualVertexCount = lActualVertexCount + 1
            // End If
            // Next lVertex
            // 'if the first vertex equal the last decrement to remove the last
            // If lLastVertex = lFirstVertex Then
            // lActualVertexCount = lActualVertexCount - 1
            // End If
            // 'resize the face vectors to match the actual number of vertices
            // If lActualVertexCount > 0 Then
            // With udtFace
            // ReDim Preserve .lVertex(lActualVertexCount - 1)
            // ReDim Preserve .lEdgeProcessed(lActualVertexCount - 1)
            // ReDim Preserve .cvEdgeBoundingPlaneNormal(lActualVertexCount - 1)
            // ReDim Preserve .cvEdgeBoundingPlanePoint(lActualVertexCount - 1)
            // End With 'udtFace
            // End If
            // 'get the color
            // udtFace.lColor = csShape.fGetFaceMaterial(lFace).x_Color
            // 'add the face to the CGMesh (if we fail-we don't fail-we ignore it)
            // On Error Resume Next
            // If lActualVertexCount > 2 Then
            // lAddCGMeshFace udtCGMesh, udtFace, bValidateFace
            // End If
            // On Error GoTo 0
            // Next lFace
            // create the bounding box
            // Console.WriteLine(this.udtFace.Length);
            this.CreateBoundingBox();
        }

        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************
        internal bool IsFaceValid(CBODataTypes.CSGCGFace face)
        {
            var lEdge = new int[2];

            // 'Dim lEdgeData As Long
            // UPGRADE_TODO: (1065) Error handling statement (On Error Goto) could not be converted. More Information: http://www.vbtonet.com/ewis/ewi1065.aspx
            // UpgradeHelpers.Helpers.NotUpgradedHelper.NotifyNotUpgradedElement("On Error Goto Label (0)");
            // check that the edges don't already exist
            bool result = true;
            for (int lVertex = 0; lVertex <= face.lVertex.GetUpperBound(0); lVertex++)
            {
                CBO.GetEdgeCGMesh(face, lVertex, lEdge);

                // if we don't have a null edge ( two points in the same location)
                if (lEdge[0] != lEdge[1])
                {
                    // ensure that the edge is insertable
                    // UPGRADE_TODO: (1065) Error handling statement (On Error Resume Next) could not be converted. More Information: http://www.vbtonet.com/ewis/ewi1065.aspx
                    // UpgradeHelpers.Helpers.NotUpgradedHelper.NotifyNotUpgradedElement("On Error Resume Next");
                    int lEdgeData;
                    if (this.colEdge.TryGetValue(lEdge[0].ToString() + "|" + lEdge[1].ToString(), out lEdgeData))
                    {
                        result = false;
                        break;
                    }

                    // lEdgeData = Convert.ToInt32(this.colEdge[lEdge[0].ToString() + "|" + lEdge[1].ToString()]);
                    ////this should not be here, so no error means we failed
                    ////UPGRADE_WARNING: (2081) Err.Number has a new behavior. More Information: http://www.vbtonet.com/ewis/ewi2081.aspx
                    // if (Information.Err().Number == 0)
                    // {
                    // result = false;
                    // break;
                    // }
                }
            }

            return result;
        }

        internal bool bMatchEdgeCGMesh(int[] lEdge, ref int lMatchedFace, ref int lMatchedEdge)
        {
            // UPGRADE_TODO: (1065) Error handling statement (On Error Goto) could not be converted. More Information: http://www.vbtonet.com/ewis/ewi1065.aspx
            bool result = false;

            // UpgradeHelpers.Helpers.NotUpgradedHelper.NotifyNotUpgradedElement("On Error Goto Label (0)");
            // the matching edge must be here by definition (since we are closed)
            // insert the edges into the collection
            // UPGRADE_TODO: (1069) Error handling statement (On Error Resume Next) was converted to a pattern that might have a different behavior. More Information: http://www.vbtonet.com/ewis/ewi1069.aspx
            try
            {
                int lFaceEdge;
                if (this.colEdge.TryGetValue(lEdge[1].ToString() + "|" + lEdge[0].ToString(), out lFaceEdge))
                {
                    // int lFaceEdge = Convert.ToInt32(this.colEdge[lEdge[1].ToString() + "|" + lEdge[0].ToString()]);
                    ////UPGRADE_WARNING: (2081) Err.Number has a new behavior. More Information: http://www.vbtonet.com/ewis/ewi2081.aspx
                    // if (Information.Err().Number == 0)
                    // {
                    lMatchedFace = lFaceEdge / 10000;
                    lMatchedEdge = lFaceEdge % 10000;
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception)
            {
                // NotUpgradedHelper.NotifyNotUpgradedElement("Resume in On-Error-Resume-Next Block");
            }

            return result;
        }

        private static void CreateBoundingPlanes(CBODataTypes.i_CSGPoint[] xdtVertex, CBODataTypes.CSGCGFace udtFace)
        {
            var cvaEdge = new Vector3[2];
            var cvaBoundingPolygon = new Vector3[3];
            var lEdge = new int[2];

            // take each edge within the polygon and create a bounding polygon
            // get the bounding planes
            for (int lVertex = 0; lVertex <= udtFace.lVertex.GetUpperBound(0); lVertex++)
            {
                CBO.GetEdgeCGMesh(udtFace, lVertex, lEdge);
                cvaEdge[0] = xdtVertex[lEdge[0]].PointXYZ;
                cvaEdge[1] = xdtVertex[lEdge[1]].PointXYZ;

                // create the three vertices for the determination of the plane normal
                cvaBoundingPolygon[0] = cvaEdge[1];
                cvaBoundingPolygon[1] = cvaEdge[0];
                cvaBoundingPolygon[2].X = cvaEdge[0].X + udtFace.cvNormal.X;
                cvaBoundingPolygon[2].Y = cvaEdge[0].Y + udtFace.cvNormal.Y;
                cvaBoundingPolygon[2].Z = cvaEdge[0].Z + udtFace.cvNormal.Z;

                // point on the plane //cvaBoundingPolygon(2&)
                udtFace.cvEdgeBoundingPlanePoint[lVertex] = cvaEdge[0];

                // plane normal
                udtFace.cvEdgeBoundingPlaneNormal[lVertex] = Helper.cvGetNormal(cvaBoundingPolygon);
            }
        }

        private static void CreateNormal(CBODataTypes.i_CSGPoint[] xdtVertex, ref CBODataTypes.CSGCGFace udtFace)
        {
            udtFace.cvNormal = pcvGetNormal(xdtVertex, udtFace);
        }

        private static void pGetFacet(
            CBODataTypes.i_CSGPoint[] xdtVertex, 
            CBODataTypes.CSGCGFace udtPolygon, 
            int lFacet, 
            Vector3[] cvFacet)
        {
            // get first vertex of facet
            // exception for the first facet since the first vertex of the facet is the last
            // vertex of the polygon
            if (lFacet == 0)
            {
                cvFacet[0] = xdtVertex[udtPolygon.lVertex[udtPolygon.lVertex.GetUpperBound(0)]].PointXYZ;
            }
            else
            {
                // normal
                cvFacet[0] = xdtVertex[udtPolygon.lVertex[lFacet - 1]].PointXYZ;
            }

            // get second vertex of facet
            cvFacet[1] = xdtVertex[udtPolygon.lVertex[lFacet]].PointXYZ;

            // get final vertex
            // exception for final vertex since the final vertex of the facet is the
            // first vertex of the polygon
            if (lFacet == udtPolygon.lVertex.GetUpperBound(0))
            {
                cvFacet[2] = xdtVertex[udtPolygon.lVertex[0]].PointXYZ;
            }
            else
            {
                cvFacet[2] = xdtVertex[udtPolygon.lVertex[lFacet + 1]].PointXYZ;
            }
        }

        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************

        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************
        private static bool pbCoLinearCoarse(Vector3[] cvaTriangleFacet)
        {
            bool result = false;
            var cvVector = new Vector3[2];

            cvVector[0].X = cvaTriangleFacet[0].X - cvaTriangleFacet[1].X;
            cvVector[0].Y = cvaTriangleFacet[0].Y - cvaTriangleFacet[1].Y;
            cvVector[0].Z = cvaTriangleFacet[0].Z - cvaTriangleFacet[1].Z; // cvVector(0)
            cvVector[0].Normalize();
            cvVector[1].X = cvaTriangleFacet[2].X - cvaTriangleFacet[1].X;
            cvVector[1].Y = cvaTriangleFacet[2].Y - cvaTriangleFacet[1].Y;
            cvVector[1].Z = cvaTriangleFacet[2].Z - cvaTriangleFacet[1].Z; // cvVector(1)
            cvVector[1].Normalize();
            float sAngle = Vector3.Dot(cvVector[0], cvVector[1]);

            // is it colinear? (Angle is 180 degrees (cos(180) = -1))
            if (Math.Abs(sAngle + 1) < Helper.CSG_sSmallEpsilon * 10)
            {
                result = true;
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
        private static bool pbConvex(CBODataTypes.i_CSGPoint[] xdtVertex, CBODataTypes.CSGCGFace udtPolygon)
        {
            bool result = false;
            var cvaFacet = new Vector3[3];

            // get the polygon normal
            Vector3 cvPolygonNormal = pcvGetNormaConcave(xdtVertex, udtPolygon);

            // look at each vertex. if any are concave then we don't have a convex polygon
            for (int lVertex = 0; lVertex <= udtPolygon.lVertex.GetUpperBound(0); lVertex++)
            {
                // GetFacet cvaPolygon, lVertex, cvaFacet
                pGetFacet(xdtVertex, udtPolygon, lVertex, cvaFacet);

                // get the vertex normal at lvertex
                Vector3 cvVertexNormal = Helper.cvGetNormal(cvaFacet);

                // determine if convex at lvertex i.e. same normal as the polygon as a whole
                // If Abs(cvVertexNormal.X - cvPolygonNormal.X) > CSG_sSmallEpsilon
                // '        Or Abs(cvVertexNormal.Y - cvPolygonNormal.Y) > CSG_sSmallEpsilon
                // '        Or Abs(cvVertexNormal.Z - cvPolygonNormal.Z) > CSG_sSmallEpsilon Then
                if (Math.Abs(cvVertexNormal.X - cvPolygonNormal.X) > 0.001d || Math.Abs(cvVertexNormal.Y - cvPolygonNormal.Y) > 0.001d
                    || Math.Abs(cvVertexNormal.Z - cvPolygonNormal.Z) > 0.001d)
                {
                    return false; // the easy way out
                }
            }

            // we made it this far, so we are convex unless the normal of the polygon is zero
            if (!Helper.bVectorEqual(cvPolygonNormal.X, cvPolygonNormal.Y, cvPolygonNormal.Z, 0f, 0f, 0f))
            {
                result = true;
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
        private static Vector3 pcvGetNormaConcave(CBODataTypes.i_CSGPoint[] xdtVertex, CBODataTypes.CSGCGFace udtFace)
        {
            var result = new Vector3();
            int lVertex;
            var cvNormal = new Vector3();

            int lVertexPrevious = udtFace.lVertex.GetUpperBound(0);
            for (lVertex = 0; lVertex <= udtFace.lVertex.GetUpperBound(0); lVertex++)
            {
                // *rwb this looks wrong. why don't we subtract an origin?
                // Is it because the effect of doing ALL of the vertices of a polygon
                // corrects the obvious problem?
                // time for the cross product calculation
                cvNormal = Vector3.Cross(xdtVertex[udtFace.lVertex[lVertexPrevious]].PointXYZ, 
                    xdtVertex[udtFace.lVertex[lVertex]].PointXYZ);

                // sum things up
                result.X += cvNormal.X;
                result.Y += cvNormal.Y;
                result.Z += cvNormal.Z;

                // keep track of the previous vertex //pcvGetNormaConcave
                lVertexPrevious = lVertex;
            }

            // udtFace
            if (result.X != 0 || result.Y != 0 || result.Z != 0)
            {
                result.Normalize();
            }

            return result;
        }

        private static Vector3 pcvGetNormal(CBODataTypes.i_CSGPoint[] xdtVertex, CBODataTypes.CSGCGFace udtFace)
        {
            var result = new Vector3();
            var cvVector1 = new Vector3();
            var cvVector2 = new Vector3();

            // calculate face normal
            cvVector1.X = xdtVertex[udtFace.lVertex[1]].PointXYZ.X - xdtVertex[udtFace.lVertex[0]].PointXYZ.X;
            cvVector1.Y = xdtVertex[udtFace.lVertex[1]].PointXYZ.Y - xdtVertex[udtFace.lVertex[0]].PointXYZ.Y;
            cvVector1.Z = xdtVertex[udtFace.lVertex[1]].PointXYZ.Z - xdtVertex[udtFace.lVertex[0]].PointXYZ.Z; // cvVector1
            cvVector2.X = xdtVertex[udtFace.lVertex[2]].PointXYZ.X - xdtVertex[udtFace.lVertex[1]].PointXYZ.X;
            cvVector2.Y = xdtVertex[udtFace.lVertex[2]].PointXYZ.Y - xdtVertex[udtFace.lVertex[1]].PointXYZ.Y;
            cvVector2.Z = xdtVertex[udtFace.lVertex[2]].PointXYZ.Z - xdtVertex[udtFace.lVertex[1]].PointXYZ.Z; // cvVector2
            result = Vector3.Cross(cvVector1, cvVector2);
            if (result.X != 0 || result.Y != 0 || result.Z != 0)
            {
                result.Normalize();
            }

            return result;
        }

        private void SplitEdge(int lFace, int lEdge, int lNewVertex, bool bFlagProcessed, bool bFlagSecondSplit)
        {
            // Dim lNewFace       As Long

            // allocate the new faces
            var face = new CBODataTypes.CSGCGFace[this.udtFace[lFace].lVertex.GetUpperBound(0)];
            for (int lVertexCounter = 0; lVertexCounter <= this.udtFace[lFace].lVertex.GetUpperBound(0); lVertexCounter++)
            {
                // have to ignore the edge (i.e. vertex) that we are splitting
                if (lVertexCounter != lEdge)
                {
                    // we are skipping a vertex at lEdge, so the
                    // vertex we are working on above lEdge is actually one less
                    // (confusing but true)
                    int lVertex;
                    if (lVertexCounter > lEdge)
                    {
                        lVertex = lVertexCounter - 1;
                    }
                    else
                    {
                        lVertex = lVertexCounter;
                    }

                    // create the new face
                    CBO.CreateFace(ref face[lVertex], 2);

                    // set the face's color
                    face[lVertex].lColor = this.udtFace[lFace].lColor;

                    // always add vertex and the next vertex as the first two of three
                    face[lVertex].lVertex[0] = this.udtFace[lFace].lVertex[lVertexCounter];
                    face[lVertex].lEdgeProcessed[0] = this.udtFace[lFace].lEdgeProcessed[lVertexCounter]; // udtFace(lVertex)
                    if (lVertexCounter < this.udtFace[lFace].lVertex.GetUpperBound(0))
                    {
                        face[lVertex].lVertex[1] = this.udtFace[lFace].lVertex[lVertexCounter + 1];
                    }
                    else
                    {
                        // a special case for the last vertex (which of course needs to have the first vertex)
                        face[lVertex].lVertex[1] = this.udtFace[lFace].lVertex[0];
                    }

                    // add the newly created vertex
                    face[lVertex].lVertex[2] = lNewVertex;

                    // update the processed status
                    if (bFlagProcessed)
                    {
                        // indicate that the edge is processed (vertex 1 of the edge immediately before lEdge is processed)
                        // If lEdge = 0 Then
                        // If lVertexCounter = UBound(this.udtFace(lFace).lVertex) Then
                        face[lVertex].lEdgeProcessed[1] = 1;

                        // End If
                        // Else
                        // If lVertexCounter = lEdge - 1 Then
                        // udtFace(lVertex).lEdgeProcessed(1) = 1
                        // End If
                        // End If
                        // indicate that the second part of the edge is processed (vertex 2 of the edge immediately after lEdge is processed)
                        if (bFlagSecondSplit)
                        {
                            if (lEdge == this.udtFace[lFace].lVertex.GetUpperBound(0))
                            {
                                if (lVertexCounter == 0)
                                {
                                    face[lVertex].lEdgeProcessed[2] = 1;
                                }
                            }
                            else
                            {
                                if (lVertexCounter == lEdge + 1)
                                {
                                    face[lVertex].lEdgeProcessed[2] = 1;
                                }
                            }
                        }
                    }
                }
            }

            // make a copy of the old face (so we can re-add if there is an error)
            CBODataTypes.CSGCGFace udtFaceCopy = this.udtFace[lFace];

            // delete the old face
            this.DeleteFace(lFace);

            // first ensure that all faces created are valid
            for (int lFaceCounter = 0; lFaceCounter <= face.GetUpperBound(0); lFaceCounter++)
            {
                if (!this.IsFaceValid(face[lFaceCounter]))
                {
                    // re-add the deleted face
                    this.AddFace(udtFaceCopy, true);
                    throw new CGIntersectionFailureException();
                }
            }

            // add the new faces to the CGMesh
            for (int lFaceCounter = 0; lFaceCounter <= face.GetUpperBound(0); lFaceCounter++)
            {
                // If Not pbConvex(this.xdtVertex, udtFace(lFaceCounter)) Then
                // 'Stop
                // Else
                // 'Stop
                // End If
                // add the face to the CGMesh
                this.AddFace(face[lFaceCounter], true);
            }
        }

        private void SplitPolygon(int lFace, int lNewVertex)
        {
            int lVertex;

            // Dim lNewFace     As Long

            // allocate the new faces
            var face = new CBODataTypes.CSGCGFace[this.udtFace[lFace].lVertex.GetUpperBound(0) + 1];
            for (lVertex = 0; lVertex <= this.udtFace[lFace].lVertex.GetUpperBound(0); lVertex++)
            {
                // create the new face
                CBO.CreateFace(ref face[lVertex], 2);

                // always add vertex and the next vertex as the first two of three
                face[lVertex].lVertex[0] = this.udtFace[lFace].lVertex[lVertex];
                face[lVertex].lEdgeProcessed[0] = this.udtFace[lFace].lEdgeProcessed[lVertex];
                if (lVertex < this.udtFace[lFace].lVertex.GetUpperBound(0))
                {
                    face[lVertex].lVertex[1] = this.udtFace[lFace].lVertex[lVertex + 1];
                }
                else
                {
                    // a special case for the last vertex (which of course needs to have the first vertex)
                    face[lVertex].lVertex[1] = this.udtFace[lFace].lVertex[0];
                }

                // add the newly created vertex
                face[lVertex].lVertex[2] = lNewVertex;

                // indicate that the edge prior to the new vertex is processed
                face[lVertex].lEdgeProcessed[1] = 1;

                // set the face's color
                face[lVertex].lColor = this.udtFace[lFace].lColor; // udtFace(lVertex)
            }

            // make a copy of the old face (so we can re-add if there is an error)
            CBODataTypes.CSGCGFace udtFaceCopy = this.udtFace[lFace];

            // delete the old face
            this.DeleteFace(lFace);

            // first ensure that all faces created are valid
            for (int lFaceCounter = 0; lFaceCounter <= face.GetUpperBound(0); lFaceCounter++)
            {
                if (!this.IsFaceValid(face[lFaceCounter]))
                {
                    // re-add the deleted face
                    this.AddFace(udtFaceCopy, true);
                    throw new CGIntersectionFailureException();
                }
            }

            // add the new faces to the CGMesh
            for (int lFaceCounter = 0; lFaceCounter <= face.GetUpperBound(0); lFaceCounter++)
            {
                // If Not pbConvex(this.xdtVertex, udtFace(lFaceCounter)) Then
                // 'Stop
                // Else
                // 'Stop
                // End If
                // add the face to the CGMesh
                this.AddFace(face[lFaceCounter], true);
            }
        }

        private void pCreateFaceEdgeData(int lFace)
        {
            var lEdge = new int[2];

            for (int lVertex = 0; lVertex <= this.udtFace[lFace].lVertex.GetUpperBound(0); lVertex++)
            {
                CBO.GetEdgeCGMesh(this.udtFace[lFace], lVertex, lEdge);

                // insert the edges into the collection]
                this.colEdge.Add(lEdge[0].ToString() + "|" + lEdge[1].ToString(), lFace * 10000 + lVertex);
            }

            // udtCGMesh
        }

        private void pDeleteFaceEdgeData(int lFace)
        {
            var lEdge = new int[2];

            // UPGRADE_TODO: (1065) Error handling statement (On Error Goto) could not be converted. More Information: http://www.vbtonet.com/ewis/ewi1065.aspx
            // UpgradeHelpers.Helpers.NotUpgradedHelper.NotifyNotUpgradedElement("On Error Goto Label (0)");
            for (int lVertex = 0; lVertex <= this.udtFace[lFace].lVertex.GetUpperBound(0); lVertex++)
            {
                CBO.GetEdgeCGMesh(this.udtFace[lFace], lVertex, lEdge);

                // remove the edge from the collection
                // ignore errors so that a second delete works just fine
                try
                {
                    this.colEdge.Remove(lEdge[0].ToString() + "|" + lEdge[1].ToString());
                }
                catch
                {
                }
            }
        }

        private void pMergeFacesOnEdge(
            int lFace0, 
            int lEdge0, 
            int lFace1, 
            int lEdge1, 
            ref Vector3[] cvaVertices, 
            ref int[] laVertices)
        {
            int lActualVertex;

            // Dim cvNormal As CSGVector
            // reserve some space for the vertices
            cvaVertices =
                new Vector3[this.udtFace[lFace0].lVertex.GetUpperBound(0) + this.udtFace[lFace1].lVertex.GetUpperBound(0)];
            laVertices = new int[this.udtFace[lFace0].lVertex.GetUpperBound(0) + this.udtFace[lFace1].lVertex.GetUpperBound(0)];

            // run through the two faces adding vertices
            // this is gonna be confusing
            for (int lVertexCounter = 0; lVertexCounter <= this.udtFace[lFace0].lVertex.GetUpperBound(0) - 1; lVertexCounter++)
            {
                // have to calculate an "actual" vertex since lVertexCounter is really
                // just a count variable
                lActualVertex = lVertexCounter + lEdge0 + 1;
                if (lActualVertex > this.udtFace[lFace0].lVertex.GetUpperBound(0))
                {
                    lActualVertex = lActualVertex - this.udtFace[lFace0].lVertex.GetUpperBound(0) - 1;
                }

                // now get the vertex
                cvaVertices[lVertexCounter] = this.xdtVertex[this.udtFace[lFace0].lVertex[lActualVertex]].PointXYZ;
                laVertices[lVertexCounter] = this.udtFace[lFace0].lVertex[lActualVertex];

                // 'debug.print "vertex: " & lVertexCounter, laVertices(lVertexCounter)
            }

            // now for the second face
            for (int lVertexCounter = 0; lVertexCounter <= this.udtFace[lFace1].lVertex.GetUpperBound(0) - 1; lVertexCounter++)
            {
                // have to calculate an "actual" vertex since lVertexCounter is really
                // just a count variable
                lActualVertex = lVertexCounter + lEdge1 + 1;
                if (lActualVertex > this.udtFace[lFace1].lVertex.GetUpperBound(0))
                {
                    lActualVertex = lActualVertex - this.udtFace[lFace1].lVertex.GetUpperBound(0) - 1;
                }

                // now get the vertex
                cvaVertices[lVertexCounter + this.udtFace[lFace0].lVertex.GetUpperBound(0)] =
                    this.xdtVertex[this.udtFace[lFace1].lVertex[lActualVertex]].PointXYZ;
                laVertices[lVertexCounter + this.udtFace[lFace0].lVertex.GetUpperBound(0)] = this.udtFace[lFace1].lVertex[lActualVertex];

                // 'debug.print "vertex: " & lVertexCounter, laVertices(lVertexCounter + cfFace0.GetPointCount - 1)
            }
        }

        // *********************************************************************************
        // Private Methods
        // *********************************************************************************
        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************
        private bool pbDeTessellateProcess(int lFace0, int lEdge0, int lFace1, int lEdge1)
        {
            bool result = false;
            Vector3[] cvaVertices = null;
            int[] laVertices = null;
            var cvaFacet = new Vector3[3];
            int lColinearVertex = 0;
            var laCompanionFaces = new int[2];
            var laCompanionEdges = new int[2];
            var laMatchingEdges = new int[2];
            int lMatchedEdge = 0;
            int lMatchedFace = 0;
            CBODataTypes.CSGCGFace face = CBODataTypes.CSGCGFace.CreateInstance();
            var laColinearFacet = new int[3];
            var laFacet = new int[3];
            var laEdge = new int[2];
            int lColinearFacet = 0;

            // Dim lFaceMax            As Long

            // merge the faces (as an array)
            this.pMergeFacesOnEdge(lFace0, lEdge0, lFace1, lEdge1, ref cvaVertices, ref laVertices);

            // see if we have any facets that are colinear
            int lColinearFacetCount = 0;
            for (int lVertex = 0; lVertex <= cvaVertices.GetUpperBound(0); lVertex++)
            {
                Helper.GetFacet(cvaVertices, lVertex, cvaFacet);

                // are we co-linear and convex
                if (Helper.bCoLinear(cvaFacet))
                {
                    // And bConvex(cvaFacet) Then
                    lColinearFacetCount++;
                    lColinearVertex = laVertices[lVertex];
                    lColinearFacet = lVertex;
                    Helper.GetFacet2(laVertices, lVertex, laColinearFacet);
                }

                // if we have more than one colinear facet then no need to look further
                if (lColinearFacetCount > 1)
                {
                    break;
                }
            }

            // if we have only one colinear facet we can be pretty sure that
            // we can merge the colinear facet into a single edge
            if (lColinearFacetCount == 1)
            {
                // if we are removing a facet, there must be two
                // companion polygons that share this facet. these companion
                // polygons will have to be merged on the facet also.
                // note that we don't check for the faces being co-planar because
                // we are ASSUMMING that we are detessellating because we have
                // TRIANGLES and two triangles that can be merged such that
                // they produce a colinear edge, must have been co-planar.
                // get the facet that is colinear
                Helper.GetFacet2(laVertices, lColinearFacet, laFacet);

                // find companion faces
                bool bCompanionsFound = false;
                laEdge[0] = laFacet[0];
                laEdge[1] = laFacet[1];
                if (this.bMatchEdgeCGMesh(laEdge, ref laCompanionFaces[0], ref laCompanionEdges[0]))
                {
                    laEdge[0] = laFacet[1];
                    laEdge[1] = laFacet[2];
                    if (this.bMatchEdgeCGMesh(laEdge, ref laCompanionFaces[1], ref laCompanionEdges[1]))
                    {
                        // see if the companion faces join on an edge. If so, then we have
                        // companions that will likely create a convex polygon when merged
                        // the easiest way is to run through the edges of one, finding the
                        // matching edges. if one of the matching edges is part of the
                        // other companion then we found it
                        for (int lVertex = 0; lVertex <= this.udtFace[laCompanionFaces[0]].lVertex.GetUpperBound(0); lVertex++)
                        {
                            CBO.GetEdgeCGMesh(this.udtFace[laCompanionFaces[0]], lVertex, laEdge);

                            // have matched an edge
                            if (this.bMatchEdgeCGMesh(laEdge, ref lMatchedFace, ref lMatchedEdge))
                            {
                                // have we found it
                                if (lMatchedFace == laCompanionFaces[1])
                                {
                                    // found it
                                    bCompanionsFound = true;
                                    laMatchingEdges[0] = lVertex;
                                    laMatchingEdges[1] = lMatchedEdge;

                                    // ensure that the new potential new edge doesn't already exist within the CGMesh
                                    // this is really a hack - in a truly closed shape this
                                    // shouldn't happen, but sometimes we get so odd arrangements
                                    // when we hack and shape closed
                                    laEdge[0] = laColinearFacet[0];
                                    laEdge[1] = laColinearFacet[2];
                                    if (this.bMatchEdgeCGMesh(laEdge, ref lMatchedFace, ref lMatchedEdge) && lMatchedFace != lFace0
                                        && lMatchedFace != lFace1)
                                    {
                                        // didn't find it
                                        bCompanionsFound = false;
                                    }

                                    // Stop 'found it
                                }
                            }
                        }
                    }
                }

                if (bCompanionsFound)
                {
                    // test that the merged faces will be convex
                    // the test merge
                    // and ensure it is convex
                    var cvaTestMerge = new Vector3[cvaVertices.GetUpperBound(0)];
                    int lActualVertex = 0;
                    for (int lVertex = 0; lVertex <= laVertices.GetUpperBound(0); lVertex++)
                    {
                        if (laVertices[lVertex] != lColinearVertex)
                        {
                            cvaTestMerge[lActualVertex] = cvaVertices[lVertex];
                            lActualVertex++;
                        }
                    }

                    if (Helper.bConvex(cvaTestMerge, false))
                    {
                        // merge the companion faces (as an array)
                        this.pMergeFacesOnEdge(
                            laCompanionFaces[0], 
                            laMatchingEdges[0], 
                            laCompanionFaces[1], 
                            laMatchingEdges[1], 
                            ref cvaVertices, 
                            ref laVertices);

                        // the test merging
                        cvaTestMerge = new Vector3[cvaVertices.GetUpperBound(0)];
                        lActualVertex = 0;
                        for (int lVertex = 0; lVertex <= laVertices.GetUpperBound(0); lVertex++)
                        {
                            if (laVertices[lVertex] != lColinearVertex)
                            {
                                cvaTestMerge[lActualVertex] = cvaVertices[lVertex];
                                lActualVertex++;
                            }
                        }

                        if (Helper.bConvex(cvaTestMerge, false))
                        {
                            // merge the faces (as an array)
                            this.pMergeFacesOnEdge(lFace0, lEdge0, lFace1, lEdge1, ref cvaVertices, ref laVertices);

                            // merge the two faces (for real)
                            // create the new face
                            CBO.CreateFace(ref face, laVertices.GetUpperBound(0) - 1);

                            // set the color (just use the first face's color)
                            face.lColor = this.udtFace[lFace0].lColor;

                            // set the interior/exterior (just use the first face's)
                            face.udtIntersection = this.udtFace[lFace0].udtIntersection;

                            // the merging
                            lActualVertex = 0;
                            for (int lVertex = 0; lVertex <= laVertices.GetUpperBound(0); lVertex++)
                            {
                                if (laVertices[lVertex] != lColinearVertex)
                                {
                                    // this.udtFace(lFaceMax).lVertex(lActualVertex) = laVertices(lVertex)
                                    face.lVertex[lActualVertex] = laVertices[lVertex];
                                    lActualVertex++;
                                }
                            }

                            // remove old polygons
                            this.DeleteFace(lFace0);
                            this.DeleteFace(lFace1);

                            // add the merged face to the CGMesh
                            this.AddFace(face, false);

                            // merge the companion faces (as an array)
                            this.pMergeFacesOnEdge(
                                laCompanionFaces[0], 
                                laMatchingEdges[0], 
                                laCompanionFaces[1], 
                                laMatchingEdges[1], 
                                ref cvaVertices, 
                                ref laVertices);

                            // merge the two faces (for real)
                            // create the new face
                            CBO.CreateFace(ref face, laVertices.GetUpperBound(0) - 1);

                            // set the color (just use the first face's color)
                            face.lColor = this.udtFace[laCompanionFaces[0]].lColor;

                            // set the interior/exterior (just use the first face's)
                            face.udtIntersection = this.udtFace[laCompanionFaces[0]].udtIntersection;

                            // the merging
                            lActualVertex = 0;
                            for (int lVertex = 0; lVertex <= laVertices.GetUpperBound(0); lVertex++)
                            {
                                if (laVertices[lVertex] != lColinearVertex)
                                {
                                    // this.udtFace(lFaceMax).lVertex(lActualVertex) = laVertices(lVertex)
                                    face.lVertex[lActualVertex] = laVertices[lVertex];
                                    lActualVertex++;
                                }
                            }

                            // remove old polygons
                            this.DeleteFace(laCompanionFaces[0]);
                            this.DeleteFace(laCompanionFaces[1]);

                            // add the face to the CGMesh
                            this.AddFace(face, false);

                            // yes we did change things
                            result = true;
                        }
                    }
                }

                // if we have none, and the polygon is convex then the merge is OK
            }
            else if (lColinearFacetCount == 0)
            {
                if (Helper.bConvex(cvaVertices, false))
                {
                    // merge the two faces (for real)
                    // create the new face
                    CBO.CreateFace(ref face, laVertices.GetUpperBound(0));

                    // set the color (just use the first face's color)
                    face.lColor = this.udtFace[lFace0].lColor;

                    // set the interior/exterior (just use the first face's)
                    face.udtIntersection = this.udtFace[lFace0].udtIntersection;

                    // the merging
                    for (int lVertex = 0; lVertex <= laVertices.GetUpperBound(0); lVertex++)
                    {
                        face.lVertex[lVertex] = laVertices[lVertex];
                    }

                    // remove old polygons
                    this.DeleteFace(lFace0);
                    this.DeleteFace(lFace1);

                    // add the face to the CGMesh
                    this.AddFace(face, false);

                    // yes we did change things
                    result = true;
                }
            }

            return result;
        }

        // *********************************************************************************
        // Purpose:
        // Parameters:
        // *********************************************************************************
        private bool pbDeTessellateProcessQuick(int lFace0, int lEdge0, int lFace1, int lEdge1)
        {
            bool result = false;
            Vector3[] cvaVertices = null;
            int[] laVertices = null;
            CBODataTypes.CSGCGFace face = CBODataTypes.CSGCGFace.CreateInstance();

            // Dim lFaceMax      As Long

            // merge the faces (as an array)
            this.pMergeFacesOnEdge(lFace0, lEdge0, lFace1, lEdge1, ref cvaVertices, ref laVertices);
            if (Helper.bConvex(cvaVertices, true))
            {
                // merge the two faces (for real)
                // create the new face
                CBO.CreateFace(ref face, laVertices.GetUpperBound(0));

                // set the color (just use the first face's color)
                face.lColor = this.udtFace[lFace0].lColor;

                // set the interior/exterior (just use the first face's)
                face.udtIntersection = this.udtFace[lFace0].udtIntersection;

                // the merging
                for (int lVertex = 0; lVertex <= laVertices.GetUpperBound(0); lVertex++)
                {
                    face.lVertex[lVertex] = laVertices[lVertex];
                }

                // remove old polygons
                this.DeleteFace(lFace0);
                this.DeleteFace(lFace1);

                // add the face to the CGMesh
                this.AddFace(face, false);

                // yes we did change things
                result = true;
            }

            return result;
        }

        #endregion

        public struct CSGCGPick
        {
            #region Fields

            public bool bConvex; // polygon tests as convex

            public Vector3 cvPosition; // pick position

            public int lFace; // pick face

            public float sDistanceFromRayOrigin; // distance from ray origin

            public CBODataTypes.CSGFaceType udtType; // intersection type

            #endregion
        }

        // #endregion
    }
}