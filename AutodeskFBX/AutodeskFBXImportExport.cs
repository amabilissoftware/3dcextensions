namespace TDCPluginImportExport10
{
    #region

    using System;
    using System.IO;

    using ACHF;

    using ACSG;

    #endregion

    /// <summary>
    /// Exports a 3DCrafter group to a file in Autodesk FBX format.
    /// Import is not implemented in this sample.
    /// </summary>
    public class AutodeskFBXImportExport
    {
        /// <summary>
        /// The unique group names list.
        /// </summary>
        private static string[] uniqueNamesList;

        /// <summary>
        /// The unique group names list count.
        /// </summary>
        private static int uniqueNamesListCount;

        /// <summary>
        /// The Autodesk FBX export. 
        /// Note that the OBJ format also includes the export of material in a separate MTL file.
        /// </summary>
        /// <param name="outputPath">
        /// The output path. 
        /// </param>
        /// <param name="sceneGraph">
        /// The 3DCrafter Scene Graph. 
        /// </param>
        /// <param name="exportGroup">
        /// The export group. 
        /// </param>
        public static void ExportFBX(string outputPath, CSG sceneGraph, CSGGroup exportGroup)
        {
            try
            {
                string output = null; // the assembled geometry to be exported 
                string outputMaterial = null; // the assembled material to be exported
                string materialFile = Path.GetFileNameWithoutExtension(outputPath) + ".MTL";
                int shapeStartPointIndex = 0; // the index of the first point output for the current shape
                int shapeStartUVIndex = 0; // the index of the first uv output for the current shape
                int shapeStartNormalIndex = 0; // the index of the first normal output for the current shape

                // set the material library within the geometry (OBJ) export
                output = "mtllib " + materialFile + "\r\n";

                // assemble the export (geometry and materials)
                AssembleExport(
                    exportGroup,
                    ref output,
                    ref outputMaterial,
                    ref shapeStartPointIndex,
                    ref shapeStartUVIndex,
                    ref shapeStartNormalIndex);

                // write the geometry to file
                using (StreamWriter outputFile = new StreamWriter(outputPath))
                {
                    outputFile.Write(output);
                }

                // write the material library
                using (StreamWriter outputMaterialFile =
                    new StreamWriter(Path.GetDirectoryName(outputPath) + "\\" + materialFile))
                {
                    outputMaterialFile.Write(outputMaterial);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Assemble the export.
        /// </summary>
        /// <param name="exportGroup">
        /// The export group. 
        /// </param>
        /// <param name="output">
        /// the assembled geometry to be exported.
        /// </param>
        /// <param name="outputMaterial">
        /// the assembled material to be exported.
        /// </param>
        /// <param name="shapeStartPointIndex">
        /// the index of the first point output for the current shape. 
        /// </param>
        /// <param name="shapeStartUVIndex">
        /// the index of the first uv output for the current shape
        /// </param>
        /// <param name="shapeStartNormalIndex">
        /// the index of the first normal output for the current shape
        /// </param>
        private static void AssembleExport(
            CSGGroup exportGroup,
            ref string output,
            ref string outputMaterial,
            ref int shapeStartPointIndex,
            ref int shapeStartUVIndex,
            ref int shapeStartNormalIndex)
        {
            try
            {
                // recursively run through the groups to process all the shapes in the export group
                CSGGroupArray childGroupList = exportGroup.GetChildren();
                for (int groupIndex = 0; groupIndex < childGroupList.GetSize(); groupIndex++)
                {
                    AssembleExport(
                        childGroupList.GetElement(groupIndex),
                        ref output,
                        ref outputMaterial,
                        ref shapeStartPointIndex,
                        ref shapeStartUVIndex,
                        ref shapeStartNormalIndex);
                }

                // process the shapes of the current group
                CSGShapeArray childShapeList = exportGroup.GetShapes();
                for (int shapeIndex = 0; shapeIndex < childShapeList.GetSize(); shapeIndex++)
                {
                    AssembleShape(
                        childShapeList.GetElement(shapeIndex),
                        ref output,
                        ref outputMaterial,
                        ref shapeStartPointIndex,
                        ref shapeStartUVIndex,
                        ref shapeStartNormalIndex);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Assemble the shape.
        /// </summary>
        /// <param name="shape">
        /// The shape. 
        /// </param>
        /// <param name="output">
        /// the assembled geometry to be exported.
        /// </param>
        /// <param name="outputMaterial">
        /// the assembled material to be exported.
        /// </param>
        /// <param name="shapeStartPointIndex">
        /// the index of the first point output for the current shape. 
        /// </param>
        /// <param name="shapeStartUVIndex">
        /// the index of the first uv output for the current shape
        /// </param>
        /// <param name="shapeStartNormalIndex">
        /// the index of the first normal output for the current shape
        /// </param>
        private static void AssembleShape(
            CSGShape shape,
            ref string output,
            ref string outputMaterial,
            ref int shapeStartPointIndex,
            ref int shapeStartUVIndex,
            ref int shapeStartNormalIndex)
        {
            try
            {
                CHFGeneral helperFunctionsGeneral = new CHFGeneral();
                CHFFormatting helperFunctionsFormatting = new CHFFormatting();

                CSGMaterialFaceList[] materialFaceList = null;
                CSGVector[] pointList = null;
                int pointListCount = 0;
                CSGUV[] uvList = null;
                int uvListCount = 0;
                CSGVector[] normalList = null;
                int normalListCount = 0;

                int materialFaceListCount = 0;
                bool exportUVs = false;

                // export only actual shapes, not representation of lights etc.
                if (shape.RepresentativeEntityType == CSGEntityType.CSGEShape)
                {
                    // get the name of the Object (it might not be there)
                    string shapeName = shape.Name;
                    if (shapeName == string.Empty)
                    {
                        shapeName = "Shape";
                    }

                    // get a unique name
                    helperFunctionsGeneral.MakeNameUnique(ref shapeName, ref uniqueNamesList, ref uniqueNamesListCount);

                    // transform the shape to world coordinates
                    shape.Transform(shape.Parent);

                    // get the geometry sorted by material
                    shape.GetGeometryMaterialSorted(
                        true,
                        false,
                        true,
                        ref pointList,
                        ref pointListCount,
                        ref normalList,
                        ref normalListCount,
                        ref uvList,
                        ref uvListCount,
                        ref materialFaceList,
                        ref materialFaceListCount);

                    // restore the shape local coordinates
                    shape.InverseTransform(shape.Parent);

                    AssembleShapePointList(pointList, pointListCount, helperFunctionsFormatting, ref output);

                    exportUVs = AssembleShapeUVList(
                        uvList,
                        uvListCount,
                        materialFaceList,
                        materialFaceListCount,
                        helperFunctionsFormatting,
                        ref output);

                    AssembleShapeNormalList(normalList, normalListCount, helperFunctionsFormatting, ref output);

                    // output the group header
                    output = output + "g " + shapeName + "\r\n";

                    // output the materials/faces
                    for (int materialIndex = 0; materialIndex < materialFaceListCount; materialIndex++)
                    {
                        // Assemble the material
                        AssembleShapeMaterial(
                            materialFaceList[materialIndex].Material,
                            shapeName,
                            materialIndex,
                            ref outputMaterial);

                        // indicate the material to use
                        output = output + "usemtl mat_" + shapeName + "_" + materialIndex.ToString() + "\r\n";

                        AssembleShapeFaceList(
                            materialFaceList[materialIndex],
                            shapeStartPointIndex,
                            shapeStartUVIndex,
                            shapeStartNormalIndex,
                            exportUVs,
                            ref output);
                    }

                    shapeStartPointIndex = shapeStartPointIndex + pointListCount;
                    if (exportUVs)
                    {
                        shapeStartUVIndex = shapeStartUVIndex + uvListCount;
                    }

                    shapeStartNormalIndex = shapeStartNormalIndex + normalListCount;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// The Assemble the shape's face list.
        /// </summary>
        /// <param name="faceList">
        /// The face list. 
        /// </param>
        /// <param name="shapeStartPointIndex">
        /// the index of the first point output for the current shape.
        /// </param>
        /// <param name="shapeStartUVIndex">
        /// the index of the first uv output for the current shape
        /// </param>
        /// <param name="shapeStartNormalIndex">
        /// the index of the first normal output for the current shape
        /// </param>
        /// <param name="exportUVs">
        /// are UVs to be exported. 
        /// </param>
        /// <param name="output">
        /// the assembled geometry to be exported.
        /// </param>
        private static void AssembleShapeFaceList(
            CSGMaterialFaceList faceList,
            int shapeStartPointIndex,
            int shapeStartUVIndex,
            int shapeStartNormalIndex,
            bool exportUVs,
            ref string output)
        {
            for (int faceIndex = 0; faceIndex < faceList.FaceListCount; faceIndex++)
            {
                // the vertex count
                output = output + "f";

                // run through the vertices/normals of the face (inverting as we go)
                for (int pointIndex = faceList.FaceList[faceIndex].PointListCount - 1; pointIndex >= 0; pointIndex--)
                {
                    // the point
                    output = output + " " + (shapeStartPointIndex
                                             + faceList.FaceList[faceIndex].PointList[pointIndex].PointID + 1);

                    // the UV marker
                    output = output + "/";

                    // if there is a texture coordinate
                    if (exportUVs)
                    {
                        output = output + (shapeStartUVIndex
                                           + faceList.FaceList[faceIndex].PointList[pointIndex]
                                               .TextureCoordinateListID[0] + 1);
                    }

                    // the normal
                    output = output + "/" + (shapeStartNormalIndex
                                             + faceList.FaceList[faceIndex].PointList[pointIndex].NormalID + 1);
                }

                output = output + "\r\n";
            }
        }

        /// <summary>
        /// Assemble the shape's materials.
        /// </summary>
        /// <param name="material">
        /// The material. 
        /// </param>
        /// <param name="shapeName">
        /// The shape name. 
        /// </param>
        /// <param name="materialIndex">
        /// The current material index. 
        /// </param>
        /// <param name="output">
        /// the assembled geometry to be exported.
        /// </param>
        private static void AssembleShapeMaterial(
            CSGMaterial material,
            string shapeName,
            int materialIndex,
            ref string output)
        {
            float colorRed = 0;
            float colorGreen = 0;
            float colorBlue = 0;
            float colorAlpha = 0;
            CHFFormatting helperFunctionsFormatting = new CHFFormatting();

            output = output + "newmtl mat_" + shapeName + "_" + materialIndex.ToString() + "\r\n";

            material.x_GetEmissive(ref colorRed, ref colorGreen, ref colorBlue);
            output = output + "Ka " + helperFunctionsFormatting.FormatExport(colorRed) + " "
                     + helperFunctionsFormatting.FormatExport(colorGreen) + " "
                     + helperFunctionsFormatting.FormatExport(colorBlue) + "\r\n";

            material.x_GetColorRGBA(ref colorRed, ref colorGreen, ref colorBlue, ref colorAlpha);
            output = output + "Kd " + helperFunctionsFormatting.FormatExport(colorRed) + " "
                     + helperFunctionsFormatting.FormatExport(colorGreen) + " "
                     + helperFunctionsFormatting.FormatExport(colorBlue) + "\r\n";

            material.x_GetSpecular(ref colorRed, ref colorGreen, ref colorBlue);
            output = output + "Ks " + helperFunctionsFormatting.FormatExport(colorRed) + " "
                     + helperFunctionsFormatting.FormatExport(colorGreen) + " "
                     + helperFunctionsFormatting.FormatExport(colorBlue) + "\r\n";

            output = output + "illum 2" + "\r\n";
            output = output + "Ns " + helperFunctionsFormatting.FormatExport((material.Power / 5) + 1) + "\r\n";
            if (material.Texture != null)
            {
                output = output + "map_Kd " + Path.GetFileName(material.Texture.TextureName) + "\r\n";
            }

            output = output + "\r\n";
        }

        /// <summary>
        /// Assemble the shape's normal list.
        /// </summary>
        /// <param name="normalList">
        /// The normal list. 
        /// </param>
        /// <param name="normalListCount">
        /// The normal list count. 
        /// </param>
        /// <param name="helperFunctionsFormatting">
        /// Helper functions for formatting. 
        /// </param>
        /// <param name="output">
        /// the assembled geometry to be exported.
        /// </param>
        private static void AssembleShapeNormalList(
            CSGVector[] normalList,
            int normalListCount,
            CHFFormatting helperFunctionsFormatting,
            ref string output)
        {
            // output the normals
            for (int normalIndex = 0; normalIndex < normalListCount; normalIndex++)
            {
                output = output + "vn " + helperFunctionsFormatting.FormatExport(normalList[normalIndex].X) + " "
                         + helperFunctionsFormatting.FormatExport(normalList[normalIndex].Z) + " "
                         + helperFunctionsFormatting.FormatExport(normalList[normalIndex].Y) + "\r\n";
            }
        }

        /// <summary>
        /// Assemble the shape's point list.
        /// </summary>
        /// <param name="pointList">
        /// The point list. 
        /// </param>
        /// <param name="pointListCount">
        /// The point list count. 
        /// </param>
        /// <param name="helperFunctionsFormatting">
        /// Helper functions for formatting. 
        /// </param>
        /// <param name="output">
        /// the assembled geometry to be exported.
        /// </param>
        private static void AssembleShapePointList(
            CSGVector[] pointList,
            int pointListCount,
            CHFFormatting helperFunctionsFormatting,
            ref string output)
        {
            // output the points
            for (int pointIndex = 0; pointIndex < pointListCount; pointIndex++)
            {
                output = output + "v " + helperFunctionsFormatting.FormatExport(pointList[pointIndex].X) + " "
                         + helperFunctionsFormatting.FormatExport(pointList[pointIndex].Z) + " "
                         + helperFunctionsFormatting.FormatExport(pointList[pointIndex].Y) + "\r\n";
            }
        }

        /// <summary>
        /// Assemble the shape's UV list.
        /// </summary>
        /// <param name="uvList">
        /// The uv list. 
        /// </param>
        /// <param name="uvListCount">
        /// The uv list count. 
        /// </param>
        /// <param name="materialFaceList">
        /// The material face list. 
        /// </param>
        /// <param name="materialFaceListCount">
        /// The material face list count. 
        /// </param>
        /// <param name="helperFunctionsFormatting">
        /// Helper functions for formatting. 
        /// </param>
        /// <param name="output">
        /// the assembled geometry to be exported.
        /// </param>
        /// <returns>
        /// The Assemble shape uv list. 
        /// </returns>
        private static bool AssembleShapeUVList(
            CSGUV[] uvList,
            int uvListCount,
            CSGMaterialFaceList[] materialFaceList,
            int materialFaceListCount,
            CHFFormatting helperFunctionsFormatting,
            ref string output)
        {
            bool exportUVs = false;

            // determine if we have to export texture coordinates
            if (materialFaceListCount > 0)
            {
                for (int materialIndex = 0; materialIndex < materialFaceListCount; materialIndex++)
                {
                    if (materialFaceList[materialIndex].Material.Texture != null)
                    {
                        exportUVs = true;
                        break;
                    }
                }
            }

            // output the UVs
            if (exportUVs)
            {
                for (int uvIndex = 0; uvIndex < uvListCount; uvIndex++)
                {
                    output = output + "vt " + helperFunctionsFormatting.FormatExport(uvList[uvIndex].U) + " "
                             + helperFunctionsFormatting.FormatExport(-uvList[uvIndex].V) + "\r\n";
                }
            }

            return exportUVs;
        }
    }
}