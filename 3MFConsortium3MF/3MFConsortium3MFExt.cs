namespace TDCPluginImportExport10
{
    #region

    using ACSG;

    using AutoDeskFBXSDK;

    using stdole;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using TDCPlugin;
    using Windows.Storage;

    using System.IO;
    using System.Numerics;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Xml.Linq;
    using Windows.Foundation;
    using Windows.Graphics.Printing3D;
    using Windows.Security.Cryptography;
    using Windows.Storage.Pickers;
    using Windows.Storage.Streams;
    using Windows.UI;
    using Windows.UI.Xaml.Controls;

    using Windows.System;
    using Windows.UI.Xaml;
    using System.Diagnostics;
    using ACHF;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// The implementation of a 3MF Consortium 3MF Import/Export plug-in. 
    /// If you change the class name, change the assembly name to match
    /// </summary>
    public class TMFConsortium3MFExt : PluginImportExport
    {
        private struct Component
        {
            public CSGShape shape;
            public CSGGroup group;
            public Printing3DMesh mesh;
        }

        private Printing3D3MFPackage currentPackage;

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
        public string FileType => "3mf";

        /// <summary>
        ///   Gets the description of the file type exported by this plug-in.
        /// </summary>
        public string FileTypeDescription => "3MF Consortium 3MF (Downloaded Extension)";

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
            var sceneComponents = this.CollectSceneComponents(group);
            this.CreateProgrammatically(sceneComponents);
            SavePackage(filename);
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
            Importer importer = new Importer();
            importer.Import(importFileName, sceneGraph, importGroup);
        }

        private void CreateProgrammatically(List<Component> sceneComponents)
        {
            var packagingTask = CreatePackageAsync(sceneComponents);
            packagingTask.Wait();
            currentPackage = packagingTask.Result;
        }

        #region Creating a package programmatically
        private static async Task<Printing3D3MFPackage> CreatePackageAsync(List<Component> sceneComponents)
        {
            var package = new Printing3D3MFPackage();

            var model = new Printing3DModel();
            model.Unit = Printing3DModelUnit.Millimeter;

            // Material indices start at 1. (0 is reserved.)

            #region Color materials
            // Create color materials.
            var colorMaterial1 = new Printing3DColorMaterial { Color = Color.FromArgb(255, 20, 20, 90) };
            var colorMaterial2 = new Printing3DColorMaterial { Color = Color.FromArgb(255, 250, 120, 45) };

            // Create a color group with id 1 and add the colors to it.
            var colorGroup = new Printing3DColorMaterialGroup(1);
            colorGroup.Colors.Add(colorMaterial1);
            colorGroup.Colors.Add(colorMaterial2);

            // add the color group to the model.
            model.Material.ColorGroups.Add(colorGroup);
            #endregion

            #region Base materials
            // Create base materials.
            var material1 = new Printing3DBaseMaterial { Name = Printing3DBaseMaterial.Pla, Color = colorMaterial1 };
            var material2 = new Printing3DBaseMaterial { Name = Printing3DBaseMaterial.Abs, Color = colorMaterial2 };

            // Create a new base material group with id 2 and add the base materials to it.
            var materialGroup = new Printing3DBaseMaterialGroup(2);
            materialGroup.Bases.Add(material1);
            materialGroup.Bases.Add(material2);

            // Add the material group to the base groups on the model.
            model.Material.BaseGroups.Add(materialGroup);

            #endregion

            #region Composite material groups
            // Create a composite material group with id 3.
            var compositeMaterialGroup = new Printing3DCompositeMaterialGroup(3);

            // Add it to the metadata.
            // The key is the string "composite" followed by the composite material group id.
            // The value specifies that the default base material group to use is id 2.
            model.Metadata.Add("composite3", "2");

            // The indices are relative to the material indices that will be set by SetMaterialIndicesAsync.
            compositeMaterialGroup.MaterialIndices.Add(0);
            compositeMaterialGroup.MaterialIndices.Add(1);

            // Create new composite materials.
            // The Values correspond to the materials added in the compositeMaterialGroup,
            // and they must sum to 1.0.

            // Composite material 1 consists of 20% material1 and 80% material2.
            var compositeMaterial1 = new Printing3DCompositeMaterial();
            compositeMaterial1.Values.Add(0.2);
            compositeMaterial1.Values.Add(0.8);

            // Composite material 2 consists of 50% material1 and 50% material2.
            var compositeMaterial2 = new Printing3DCompositeMaterial();
            compositeMaterial2.Values.Add(0.5);
            compositeMaterial2.Values.Add(0.5);

            // Composite material 3 consists of 80% material1 and 20% material2.
            var compositeMaterial3 = new Printing3DCompositeMaterial();
            compositeMaterial3.Values.Add(0.8);
            compositeMaterial3.Values.Add(0.2);

            // Composite material 4 consists of 40% material1 and 60% material2.
            var compositeMaterial4 = new Printing3DCompositeMaterial();
            compositeMaterial4.Values.Add(0.4);
            compositeMaterial4.Values.Add(0.6);

            // Add the composite materials to the compositeMaterialGroup.
            compositeMaterialGroup.Composites.Add(compositeMaterial1);
            compositeMaterialGroup.Composites.Add(compositeMaterial2);
            compositeMaterialGroup.Composites.Add(compositeMaterial3);
            compositeMaterialGroup.Composites.Add(compositeMaterial4);

            // Add the composite material group to the model.
            model.Material.CompositeGroups.Add(compositeMaterialGroup);
            #endregion

            #region Texture resource
            Printing3DTextureResource textureResource = new Printing3DTextureResource();

            // Set the texture path in the 3MF file.
            //textureResource.Name = "msLogo.png";
            textureResource.Name = "/3D/Texture/msLogo.png";

            // Load the texture from our package.
            StorageFile file = await StorageFile.GetFileFromPathAsync("C:\\Users\\richa\\Documents\\3D Crafter 10.2\\Extensions\\msLogo.png");
            //StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/msLogo.png"));
            textureResource.TextureData = await file.OpenReadAsync();

            package.Textures.Add(textureResource);
            #endregion

            #region 2D texture materials
            // Create a texture material group with id 4.
            var texture2CoordGroup = new Printing3DTexture2CoordMaterialGroup(4);

            // Add it to the metadata.
            // The key is the string "tex" followed by the texture material group id.
            // The value is the name of the texture resource (see textureResource.Name above).
            //model.Metadata.Add("tex4", "msLogo.png");
            model.Metadata.Add("tex4", "/3D/Texture/msLogo.png");

            // Create texture materials and add them to the group.
            texture2CoordGroup.Texture2Coords.Add(new Printing3DTexture2CoordMaterial() { U = 0.0, V = 1.0 });
            texture2CoordGroup.Texture2Coords.Add(new Printing3DTexture2CoordMaterial() { U = 1.0, V = 1.0 });
            texture2CoordGroup.Texture2Coords.Add(new Printing3DTexture2CoordMaterial() { U = 0.0, V = 0.0 });
            texture2CoordGroup.Texture2Coords.Add(new Printing3DTexture2CoordMaterial() { U = 1.0, V = 0.0 });

            // Add the texture material group to the model.
            model.Material.Texture2CoordGroups.Add(texture2CoordGroup);
            #endregion

            #region Mesh
            for (int itemId = 0; itemId < sceneComponents.Count; itemId++)
            {
                var mesh2 = sceneComponents[itemId].mesh;

                CSGVector[] pointList = new CSGVector[0];
                int pointListCount = 0;

                CSGVector[] normalList = new CSGVector[0];
                int normalListCount = 0;

                CSGUV[] textureCoordinateList = new CSGUV[0];
                int textureCoordinateListCount = 0;

                CSGMaterialFaceList[] materialFaceList = new CSGMaterialFaceList[0];
                int materialFaceListCount = 0;

                sceneComponents[itemId].shape.GetGeometryMaterialSorted(true, true, true, ref pointList, ref pointListCount, ref normalList, ref normalListCount, ref textureCoordinateList, ref textureCoordinateListCount, ref materialFaceList, ref materialFaceListCount);

                await SetVerticesAsync2(mesh2, pointList);
                await SetTriangleIndicesAsync2(mesh2, materialFaceList);

                model.Meshes.Add(mesh2);
            }

            var mesh = new Printing3DMesh();

            await SetVerticesAsync(mesh);
            await SetTriangleIndicesAsync(mesh);
            //await SetMaterialIndicesAsync(mesh);

            // add the mesh to the model
            model.Meshes.Add(mesh);
            #endregion

            for (int itemId = 0; itemId < sceneComponents.Count; itemId++)
            {
                var item = sceneComponents[itemId];

                Printing3DComponent component2 = new Printing3DComponent();
                component2.Mesh = item.mesh;

                // Add the component to the model. A model can have multiple components.
                model.Components.Add(component2);

                CSGMatrix transformLH = new CSGMatrix();
                item.group.GetTransform(item.group.GetScene(), -1, ref transformLH);
                var transformRH = transformLH; // ConvertTransformLeftHandedToRightHandedAndBack_ThisLittleBitTookFourDaysToFigureOut(transformLH);

                Matrix4x4 matrix = new Matrix4x4();
                matrix.M11 = transformRH.m11;
                matrix.M12 = transformRH.m12;
                matrix.M13 = transformRH.m13;
                matrix.M14 = transformRH.m14;
                matrix.M21 = transformRH.m21;
                matrix.M22 = transformRH.m22;
                matrix.M23 = transformRH.m23;
                matrix.M24 = transformRH.m24;
                matrix.M31 = transformRH.m31;
                matrix.M32 = transformRH.m32;
                matrix.M33 = transformRH.m33;
                matrix.M34 = transformRH.m34;
                matrix.M41 = transformRH.m41;
                matrix.M42 = transformRH.m42;
                matrix.M43 = transformRH.m43;
                matrix.M44 = transformRH.m44;
                var componentWithMatrix2 = new Printing3DComponentWithMatrix() { Component = component2, Matrix = matrix };

                model.Build.Components.Add(componentWithMatrix2);
            }

            #region Adding a component to the build
            // create a component.
            Printing3DComponent component = new Printing3DComponent();

            // assign the mesh to the component's mesh.
            component.Mesh = mesh;

            // Add the component to the model. A model can have multiple components.
            model.Components.Add(component);

            // The world matrix for the component is the identity matrix.
            var componentWithMatrix = new Printing3DComponentWithMatrix() { Component = component, Matrix = Matrix4x4.Identity };

            // add the componentWithMatrix to the build.
            // The build defines what is to be printed from within a Printing3DModel.
            // If you leave a mesh out of the build, it will not be printed.
            model.Build.Components.Add(componentWithMatrix);
            #endregion

            // Save the completed model into a package.
            await package.SaveModelToPackageAsync(model);

            // fix any textures in the model file.
            await FixTextureContentTypeAsync(package);

            return package;
        }

        public static CSGMatrix ConvertTransformLeftHandedToRightHandedAndBack_ThisLittleBitTookFourDaysToFigureOut(CSGMatrix transform)
        {
            // mirror each base vector's z component
            transform.m13 *= -1;
            transform.m23 *= -1;
            transform.m33 *= -1;
            transform.m43 *= -1;

            // and now invert the Z base vector to keep the matrix decomposable
            // will come out just fine when the meshes are locally mirrored as well
            transform.m31 *= -1;
            transform.m32 *= -1;
            transform.m33 *= -1; // NOTE: this is inverted twice. Not sure why, but if there is a bug, this is probably it.
            transform.m34 *= -1;
            return transform;
        }

        #region Mesh buffers

        // Create the buffer for vertex positions.
        private static async Task SetVerticesAsync(Printing3DMesh mesh)
        {
            Printing3DBufferDescription description;
            description.Format = Printing3DBufferFormat.Printing3DDouble;
            description.Stride = 3; // Three values per vertex (x, y, z).
            mesh.VertexPositionsDescription = description;
            mesh.VertexCount = 8; // 8 total vertices in the cube

            // Create the buffer into which we will write the vertex positions.
            mesh.CreateVertexPositions(sizeof(double) * description.Stride * mesh.VertexCount);

            // Fill the buffer with vertex coordinates.
            using (var stream = mesh.GetVertexPositions().AsStream())
            {
                double[] vertices =
                {
                    0, 0, 0,
                    10, 0, 0,
                    0, 10, 0,
                    10, 10, 0,
                    0, 0, 10,
                    10, 0, 10,
                    0, 10, 10,
                    10, 10, 10,
                };

                byte[] vertexData = vertices.SelectMany(v => BitConverter.GetBytes(v)).ToArray();

                await stream.WriteAsync(vertexData, 0, vertexData.Length);
            }
        }

        // Create the buffer for vertex positions.
        private static async Task SetVerticesAsync2(Printing3DMesh mesh, CSGVector[] pointList)
        {
            Printing3DBufferDescription description;
            description.Format = Printing3DBufferFormat.Printing3DDouble;
            description.Stride = 3; // Three values per vertex (x, y, z).
            mesh.VertexPositionsDescription = description;
            mesh.VertexCount = (uint)pointList.Length;

            // Create the buffer into which we will write the vertex positions.
            mesh.CreateVertexPositions(sizeof(double) * description.Stride * mesh.VertexCount);

            // Fill the buffer with vertex coordinates.
            using (var stream = mesh.GetVertexPositions().AsStream())
            {
                double[] vertices = new double[pointList.Length * 3];
                int verticesLocation = 0;
                foreach (var point in pointList)
                {
                    vertices[verticesLocation] = point.X;
                    verticesLocation++;
                    vertices[verticesLocation] = point.Y;
                    verticesLocation++;
                    vertices[verticesLocation] = point.Z;
                    verticesLocation++;
                }

                byte[] vertexData = vertices.SelectMany(v => BitConverter.GetBytes(v)).ToArray();

                await stream.WriteAsync(vertexData, 0, vertexData.Length);
            }
        }

        // Create the buffer for triangle indices.
        private static async Task SetTriangleIndicesAsync(Printing3DMesh mesh)
        {
            Printing3DBufferDescription description;
            description.Format = Printing3DBufferFormat.Printing3DUInt;
            description.Stride = 3; // 3 vertex position indices per triangle
            mesh.IndexCount = 12; // 12 triangles in the cube
            mesh.TriangleIndicesDescription = description;

            // Create the buffer into which we will write the triangle vertex indices.
            mesh.CreateTriangleIndices(sizeof(UInt32) * description.Stride * mesh.IndexCount);

            // Fill the buffer with triangle vertex indices.
            using (var stream = mesh.GetTriangleIndices().AsStream())
            {
                UInt32[] indices =
                {
                    1, 0, 2,
                    1, 2, 3,
                    0, 1, 5,
                    0, 5, 4,
                    1, 3, 7,
                    1, 7, 5,
                    2, 7, 3,
                    2, 6, 7,
                    0, 6, 2,
                    0, 4, 6,
                    6, 5, 7,
                    4, 5, 6,
                };

                var vertexData = indices.SelectMany(v => BitConverter.GetBytes(v)).ToArray();
                await stream.WriteAsync(vertexData, 0, vertexData.Length);
            }
        }

        // Create the buffer for triangle indices.
        private static async Task SetTriangleIndicesAsync2(Printing3DMesh mesh, CSGMaterialFaceList[] materialFaceList)
        {
            uint triangleCount = 0;
            foreach (var material in materialFaceList)
            {
                triangleCount += (uint)material.FaceListCount;
            }

            Printing3DBufferDescription description;
            description.Format = Printing3DBufferFormat.Printing3DUInt;
            description.Stride = 3; // 3 vertex position indices per triangle
            mesh.IndexCount = triangleCount;
            mesh.TriangleIndicesDescription = description;

            // Create the buffer into which we will write the triangle vertex indices.
            mesh.CreateTriangleIndices(sizeof(UInt32) * description.Stride * mesh.IndexCount);

            // Fill the buffer with triangle vertex indices.
            using (var stream = mesh.GetTriangleIndices().AsStream())
            {
                UInt32[] indices = new UInt32[triangleCount * 3];

                int indicesLocation = 0;
                foreach (var material in materialFaceList)
                {
                    foreach (var face in material.FaceList)
                    {
                        indices[indicesLocation] = (uint)face.PointList[0].PointID;
                        indicesLocation++;
                        indices[indicesLocation] = (uint)face.PointList[1].PointID;
                        indicesLocation++;
                        indices[indicesLocation] = (uint)face.PointList[2].PointID;
                        indicesLocation++;
                    }
                }

                var vertexData = indices.SelectMany(v => BitConverter.GetBytes(v)).ToArray();
                await stream.WriteAsync(vertexData, 0, vertexData.Length);
            }
        }

        // Create the buffer for material  indices.
        private static async Task SetMaterialIndicesAsync(Printing3DMesh mesh)
        {
            Printing3DBufferDescription description;
            description.Format = Printing3DBufferFormat.Printing3DUInt;
            description.Stride = 4; // 4 indices per material
            mesh.IndexCount = 12; // 12 triangles with material
            mesh.TriangleMaterialIndicesDescription = description;

            // Create the buffer into which we will write the material indices.
            mesh.CreateTriangleMaterialIndices(sizeof(UInt32) * description.Stride * mesh.IndexCount);

            // Fill the buffer with material indices.
            using (var stream = mesh.GetTriangleMaterialIndices().AsStream())
            {
                UInt32[] indices =
                {
                    // base materials:
                    // in  the BaseMaterialGroup, the BaseMaterial with id=0 will be applied to these triangle vertices
                    2, 0, 0, 0,
                    2, 0, 0, 0,
                    // color materials:
                    // in the ColorMaterialGroup, the ColorMaterials with these ids will be applied to these triangle vertices
                    1, 1, 1, 1,
                    1, 1, 1, 1,
                    1, 0, 0, 0,
                    1, 0, 0, 0,
                    1, 0, 1, 0,
                    1, 0, 1, 1,
                    // composite materials:
                    // in the CompositeMaterialGroup, the CompositeMaterial with id=0 will be applied to these triangles
                    3,0,0,0,
                    3,0,0,0,
                    // texture materials:
                    // in the Texture2CoordMaterialGroup, each texture coordinate is mapped to the appropriate vertex on these
                    // two adjacent triangle faces, so that the square face they create displays the original rectangular image
                    4, 0, 3, 1,
                    4, 2, 3, 0,
                };

                var vertexData = indices.SelectMany(v => BitConverter.GetBytes(v)).ToArray();
                await stream.WriteAsync(vertexData, 0, vertexData.Length);
            }
        }
        #endregion
        #endregion

        private async void SavePackage(string path)
        {
            //var storageFile = new IStorageFile();

            //rootPage.NotifyUser("", NotifyType.StatusMessage);

            //FileSavePicker savePicker = new FileSavePicker();
            //savePicker.DefaultFileExtension = ".3mf";
            //savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            //savePicker.FileTypeChoices.Add("3MF File", new[] { ".3mf" });
            //var storageFile = await savePicker.PickSaveFileAsync();
            //if (storageFile == null)
            //{
            //    return;
            //}

            //Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.lo new StorageFolder(); // Windows.Storage.ApplicationData.Current.LocalFolder;
            //Windows.Storage.StorageFile storageFile =
            //    await storageFolder.CreateFileAsync("test.3mf",
            //        Windows.Storage.CreationCollisionOption.ReplaceExisting);

            using (var stream = await currentPackage.SaveAsync())
            {
                stream.Seek(0);
                using (var dataReader = new DataReader(stream))
                {
                    await dataReader.LoadAsync((uint)stream.Size);
                    var buffer = dataReader.ReadBuffer((uint)stream.Size);

                    byte[] bytes = new byte[buffer.Length];
                    buffer.CopyTo(bytes);

                    ByteArrayToFile(path, bytes);
                }
            }

            //rootPage.NotifyUser("Saved", NotifyType.StatusMessage);
        }

        public bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
                return false;
            }
        }

        private async void PrintPackage()
        {
            // register the PrintTaskRequest callback to get the Printing3D3MFPackage.
            var print3DManager = Print3DManager.GetForCurrentView();
            print3DManager.TaskRequested += OnTaskRequested;

            // show the PrintUI
            var result = await Print3DManager.ShowPrintUIAsync();

            // Remove the print task request after the dialog is complete.
            print3DManager.TaskRequested -= OnTaskRequested;
        }

        private void OnTaskRequested(Print3DManager sender, Print3DTaskRequestedEventArgs args)
        {
            Print3DTask print3dTask = args.Request.CreateTask("Sample Model", "Default", (e) => e.SetSource(currentPackage));

            // Optional notification handlers.
            //print3dTask.Completed += (s, e) => rootPage.NotifyUser("Printing completed.", NotifyType.StatusMessage);
            //print3dTask.Submitting += (s, e) => rootPage.NotifyUser("Submitting print job.", NotifyType.StatusMessage);
        }

        private static async Task<IRandomAccessStream> StringToStreamAsync(string data)
        {
            var buffer = CryptographicBuffer.ConvertStringToBinary(data, BinaryStringEncoding.Utf8);
            var outputStream = new InMemoryRandomAccessStream();
            await outputStream.WriteAsync(buffer);
            return outputStream;
        }

        // Works around an issue with Printing3DTextureResource: It does not respect
        // the TextureData.ContentType property, and always encodes the content type as "".
        // Repair the XML by assuming that a path that ends with ".png" is a PNG file,
        // and everything else is a JPG file.
        private static async Task FixTextureContentTypeAsync(Printing3D3MFPackage package)
        {
            XDocument xmldoc = XDocument.Load(package.ModelPart.AsStreamForRead());
            XNamespace ns3d = "http://schemas.microsoft.com/3dmanufacturing/material/2015/02";

            foreach (var node in xmldoc.Descendants(ns3d + "texture2d"))
            {
                var contentType = node.Attribute("contenttype");
                if (contentType.Value == "")
                {
                    if (node.Attribute("path").Value.EndsWith(".png"))
                    {
                        contentType.Value = "image/png";
                    }
                    else
                    {
                        contentType.Value = "image/jpg";
                    }
                }
            }

            // Update the model with the repaired XML.
            package.ModelPart = await StringToStreamAsync(xmldoc.ToString());
        }

        private readonly CHFGeneral helperFunctionsGeneral = new CHFGeneral();

        private readonly Dictionary<string, int> textureIdList = new Dictionary<string, int>();

        //private readonly List<FBXTexture> textureList = new List<FBXTexture>();

        private string[] uniqueNamesList = new string[0];

        private int uniqueNamesListCount;

        private List<Component> CollectSceneComponents(CSGGroup scene)
        {
            //Common.InitializeSdkObjects(group.Name, out FBXManager managerInstance, out FBXScene fbxScene);

            //FBXAnimStack myAnimStack = FBXAnimStack.Create(fbxScene, group.Name + " Animation Stack");
            //FBXAnimLayer myAnimBaseLayer = FBXAnimLayer.Create(fbxScene, "Layer0");
            //myAnimStack.AddMember(myAnimBaseLayer);

            //var node = fbxScene.GetRootNode();

            var shapeList = new List<Component>();
            this.RenameMeCollectGroupShapesRecursively(scene, shapeList);

            //int fileFormat = -1;
            //if (string.Equals(Path.GetExtension(fileName), ".fbx", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    fileFormat = 1; // force text export for fbx
            //}

            //Common.SaveScene(managerInstance, fbxScene, fileName, fileFormat);
            return shapeList;
        }

        private static void ConvertTextureCoordinate(CSGUV[] textureCoordinateList, CSGVectorLong point)
        {
            if (textureCoordinateList[point.Z].V > 1)
            {
                textureCoordinateList[point.Z].V *= -1;
                textureCoordinateList[point.Z].V += 1;
            }

            textureCoordinateList[point.Z].V *= -1;
            textureCoordinateList[point.Z].V += 1;
        }

        private void RenameMeCollectGroupShapesRecursively(CSGGroup group, List<Component> sceneShapeList)
        {
            this.RenameMeCollectGroupShapes(group, sceneShapeList);

            CSGGroupArray childGroupList = group.GetChildren();
            for (int groupIndex = 0; groupIndex < childGroupList.GetSize(); groupIndex++)
            {
                this.RenameMeCollectGroupShapesRecursively(childGroupList.GetElement(groupIndex), sceneShapeList);
            }
        }

        private void RenameMeCollectGroupShapes(CSGGroup group, List<Component> sceneShapeList)
        {
            CSGShapeArray childShapeList = group.GetShapes();
            for (int shapeIndex = 0; shapeIndex < childShapeList.GetSize(); shapeIndex++)
            {
                var shape = childShapeList.GetElement(shapeIndex);
                if (shape.RepresentativeEntityType == CSGEntityType.CSGEShape && shape.ShapeType == CSGShapeType.CSGShapeStandard && shape.GetFaceCount() > 0)
                {
                    var component = new Component();
                    component.shape = shape;
                    component.group = group;
                    component.mesh = new Printing3DMesh();
                    sceneShapeList.Add(component);
                }
            }
        }

//        private void ExportMaterial(FBXScene fbxScene, FBXNode fbxNode, CSGMaterial material)
//        {
//            string materialName = this.GetUniqueName("material", "material");

//            FBXSurfacePhong fbxMaterial = FBXSurfacePhong.Create(fbxScene, materialName);

//            var diffuse = material.get_Diffuse();
//            fbxMaterial.SetDiffuse(diffuse.r, diffuse.g, diffuse.b);
//            fbxMaterial.SetTransparencyFactor(diffuse.a);

//            var ambient = material.get_Ambient();
//            fbxMaterial.SetAmbient(ambient.r, ambient.g, ambient.b);

//            var emissive = material.get_Emissive();
//            fbxMaterial.SetEmissive(emissive.r, emissive.g, emissive.b);

//            var specular = material.get_Specular();
//            fbxMaterial.SetSpecular(specular.r, specular.g, specular.b);
//            fbxMaterial.SetSpecularFactor(.1);

//            fbxMaterial.SetShadingModel("Phong");
//            fbxMaterial.SetShininess(0.0);

//            fbxNode.AddMaterial(fbxMaterial);

//            if (material.Texture != null)
//            {
//#if IS_3DC9
//                var textureFileName = Path.GetFileName(material.Texture.TextureName);
//#else
//                var textureFileName = Path.GetFileName(material.Texture.Texture0Name);
//#endif
//                if (!string.IsNullOrWhiteSpace(textureFileName))
//                {
//                    if (!this.textureIdList.TryGetValue(textureFileName, out int textureId))
//                    {
//                        textureId = this.textureIdList.Count;

//                        this.textureIdList.Add(textureFileName, textureId);

//                        string textureName = this.GetUniqueName(Path.GetFileNameWithoutExtension(textureFileName), "texture");

//                        FBXFileTexture fbxFileTexture = FBXFileTexture.Create(fbxScene, textureName);
//                        fbxFileTexture.SetFileName(textureFileName);
//                        fbxFileTexture.SetTextureUse(ArcManagedFBX.Types.ETextureUse.eStandard);
//                        fbxFileTexture.SetMappingType(ArcManagedFBX.Types.EMappingType.eUV);
//                        fbxFileTexture.SetMaterialUse(ArcManagedFBX.Types.EMaterialUse.eModelMaterial);
//                        fbxFileTexture.SetSwapUV(false);
//                        fbxFileTexture.SetTranslation(0.0, 0.0);
//                        fbxFileTexture.SetScale(1.0, 1.0);
//                        fbxFileTexture.SetRotation(0.0, 0.0);

//                        this.textureList.Add(fbxFileTexture);
//                    }

//                    fbxMaterial.DiffuseConnectSrcObjectHelper(this.textureList[textureId]);
//                }
//            }
//        }

//        private void ExportShape(FBXScene fbxScene, FBXNode parentNode, CSGShape shape)
//        {
//            Dictionary<long, int> fbxPointIdList = new Dictionary<long, int>();
//            List<CSGVectorLong> fbxPointList = new List<CSGVectorLong>();

//            CSGVector[] pointList = null;
//            int pointListCount = 0;
//            CSGVector[] normalList = null;
//            int normalListCount = 0;
//            CSGUV[] textureCoordinateList = null;
//            int textureCoordinateListCount = 0;
//            CSGMaterialFaceList[] materialFaceList = null;
//            int materialFaceListCount = 0;
//            shape.GetGeometryMaterialSorted(
//                true,
//                false,
//                true,
//                ref pointList,
//                ref pointListCount,
//                ref normalList,
//                ref normalListCount,
//                ref textureCoordinateList,
//                ref textureCoordinateListCount,
//                ref materialFaceList,
//                ref materialFaceListCount);

//            foreach (var material in materialFaceList)
//            {
//                this.ExportMaterial(fbxScene, parentNode, material.Material);
//                foreach (var face in material.FaceList)
//                {
//                    foreach (var facePoint in face.PointList)
//                    {
//                        long fbxPointKey = ((long)facePoint.PointID << 42) | ((long)facePoint.NormalID << 21) | (long)facePoint.TextureCoordinateListID[0];
//                        CSGVectorLong fbxPoint;
//                        if (fbxPointIdList.TryGetValue(fbxPointKey, out int fbxPointId))
//                        {
//                            fbxPoint = fbxPointList[fbxPointId];
//                        }
//                        else
//                        {
//                            fbxPoint = new CSGVectorLong() { X = facePoint.PointID, Y = facePoint.NormalID, Z = facePoint.TextureCoordinateListID[0] };
//                            fbxPointId = fbxPointList.Count;

//                            fbxPointList.Add(fbxPoint);

//                            fbxPointIdList.Add(fbxPointKey, fbxPointId);
//                        }
//                    }
//                }
//            }

//            string shapeName = this.GetUniqueName(shape.Name, "shape");

//            FBXMesh fbxMesh = FBXMesh.Create(fbxScene, shapeName);
//            parentNode.AddNodeAttribute(fbxMesh);

//            fbxMesh.InitControlPoints(fbxPointIdList.Count);
//            fbxMesh.InitMaterialIndices(ArcManagedFBX.Types.EMappingMode.eByPolygon);
//            fbxMesh.InitNormals(fbxPointIdList.Count);
//            fbxMesh.InitTextureUV(0);
//            fbxMesh.InitTextureUVIndices(ArcManagedFBX.Types.EMappingMode.eByControlPoint);

//            int id = 0;
//            foreach (var point in fbxPointList)
//            {
//                FBXVector controlPoint = new FBXVector(pointList[point.X].X, pointList[point.X].Y, -pointList[point.X].Z);
//                fbxMesh.SetControlPointAt(controlPoint, id);

//                FBXVector normal = new FBXVector(normalList[point.Y].X, normalList[point.Y].Y, -normalList[point.Y].Z);
//                fbxMesh.SetControlPointNormalAt(normal, id);

//                ConvertTextureCoordinate(textureCoordinateList, point);

//                fbxMesh.AddTextureUV(new FBXVector2(textureCoordinateList[point.Z].U, textureCoordinateList[point.Z].V));

//                id++;
//            }

//            int materialId = 0;
//            foreach (var material in materialFaceList)
//            {
//                foreach (var face in material.FaceList)
//                {
//                    fbxMesh.BeginPolygon(materialId, -1, -1, true);
//                    foreach (var facePoint in face.PointList.Reverse())
//                    {
//                        long fbxPointKey = ((long)facePoint.PointID << 42) | ((long)facePoint.NormalID << 21) | (long)facePoint.TextureCoordinateListID[0];
//                        if (fbxPointIdList.TryGetValue(fbxPointKey, out int fbxPointId))
//                        {
//                            fbxMesh.AddPolygon(fbxPointId, fbxPointId);
//                        }
//                        else
//                        {
//                            // should never happen
//                            Debug.WriteLine("what to do for the impossible?");
//                            fbxMesh.AddPolygon(0, 0);
//                        }
//                    }

//                    fbxMesh.EndPolygon();
//                }

//                materialId++;
//            }
//        }

//        private FBXVector GetEulerXYZ(CSGGroup group, float time)
//        {
//            CSGMatrix transformLH = new CSGMatrix();
//            group.GetTransform(group.Parent, time, ref transformLH);

//            var transformRH = Common.ConvertTransformLeftHandedToRightHandedAndBack_ThisLittleBitTookFourDaysToFigureOut(transformLH);

//            FBXMatrix matrix = new FBXMatrix(
//                transformRH.m11,
//                transformRH.m12,
//                transformRH.m13,
//                transformRH.m14,
//                transformRH.m21,
//                transformRH.m22,
//                transformRH.m23,
//                transformRH.m24,
//                transformRH.m31,
//                transformRH.m32,
//                transformRH.m33,
//                transformRH.m34,
//                transformRH.m41,
//                transformRH.m42,
//                transformRH.m43,
//                transformRH.m44);
//            FBXQuaternion fbxQuaternionRH = matrix.GetQuaternion();

//            var eulerXYZRH = fbxQuaternionRH.DecomposeSphericalXYZ();
//            return eulerXYZRH;
//        }

//        private string GetUniqueName(string name, string defaultName)
//        {
//            string uniqueName = name;
//            if (string.IsNullOrWhiteSpace(uniqueName))
//            {
//                uniqueName = defaultName;
//            }

//            this.helperFunctionsGeneral.MakeNameUnique(ref uniqueName, ref this.uniqueNamesList, ref this.uniqueNamesListCount);
//            return uniqueName;
//        }

//        private void SetTransform(CSGGroup group, FBXNode node)
//        {
//            CSGVector position = new CSGVector();
//            group.GetPosition(group.Parent, -1, ref position);

//            position.Z *= -1;
//            node.LclTranslationSet(new FBXVector(position.X, position.Y, position.Z));

//            FBXVector eulerXYZRH = this.GetEulerXYZ(group, -1);

//            node.LclRotationSet(new FBXVector(eulerXYZRH.x, eulerXYZRH.y, eulerXYZRH.z));
//        }
    }
}