namespace AutoDeskFBXSDK
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using ArcManagedFBX;
    using ArcManagedFBX.IO;

    // this is converted from the Autodesk SDK
    // there are lots of items commented out. I'll either be uncommenting the soon, or removing them completely

    class Common
    {
        public static void InitializeSdkObjects(string sceneName, out FBXManager pManager, out FBXScene pScene)
        {
            // The first thing to do is to create the FBX Manager which is the object allocator for almost all the classes in the SDK
            pManager = FBXManager.Create();
            if (pManager == null)
            {
                Debug.WriteLine("Error: Unable to create FBX Manager!");
                Environment.Exit(1);
            }
            else
            {
                Debug.WriteLine("Autodesk FBX SDK version {0}", FBXManager.GetVersion(true));
            }

            // Create an IOSettings object. This object holds all import/export settings.
            FBXIOSettings ios = FBXIOSettings.Create(pManager, "IOSRoot");
            pManager.SetIOSettings(ios);

            // Load plugins from the executable directory (optional)
            pManager.LoadPluginsDirectory(Environment.CurrentDirectory, string.Empty);

            // Create an FBX scene. This object holds most objects imported/exported from/to files.
            pScene = FBXScene.Create(pManager, sceneName);
            if (pScene == null)
            {
                Debug.WriteLine("Error: Unable to create FBX scene!\n");
                Environment.Exit(1);
            }
        }

        public static bool LoadScene(FBXManager pManager, FBXDocument pScene, string pFilename)
        {
            int sdkMajor = 0;
            int sdkMinor = 0;
            int sdkRevision = 0;

            // int lFileFormat = -1;
            string lPassword = new string(new char[1024]);

            // Get the file version number generate by the FBX SDK.
            FBXManager.GetFileFormatVersion(ref sdkMajor, ref sdkMinor, ref sdkRevision);

            // Create an importer.
            FBXImporter lImporter = FBXImporter.Create(pManager, string.Empty);

            // Initialize the importer by providing a filename.
            bool lImportStatus = lImporter.Initialize(pFilename, -1, pManager.GetIOSettings());

            // lImporter.GetFileVersion(ref lFileMajor, ref lFileMinor, ref lFileRevision);
            if (!lImportStatus)
            {
                var error = lImporter.GetStatus().GetErrorString();
                Debug.WriteLine("Call to FBXImporter::Initialize() failed");
                Debug.WriteLine("Error returned: {0}", error);

                // if (lImporter.GetStatus().GetCode() == FBXStatus.eInvalidFileVersion)
                // {
                // Debug.WriteLine("FBX file format version for this FBX SDK is %d.%d.%d\n", lSDKMajor, lSDKMinor, lSDKRevision);
                // Debug.WriteLine("FBX file format version for file '%s' is %d.%d.%d\n\n", pFilename, lFileMajor, lFileMinor, lFileRevision);
                // }
                return false;
            }

            Debug.WriteLine("FBX file format version for this FBX SDK is {0}.{1}.{2}", sdkMajor, sdkMinor, sdkRevision);

            //if (lImporter.IsFBX())
            //{
            //    // Debug.WriteLine("FBX file format version for file '%s' is %d.%d.%d\n\n", pFilename, lFileMajor, lFileMinor, lFileRevision);

            //    // From this point, it is possible to access animation stack information without
            //    // the expense of loading the entire file.
            //    Debug.WriteLine("Animation Stack Information");

            //    var lAnimStackCount = lImporter.GetAnimStackCount();

            //    Debug.WriteLine("    Number of Animation Stacks: {0}", lAnimStackCount);
            //    Debug.WriteLine("    Current Animation Stack: {0}", lImporter.GetActiveAnimStackName());

            //    int i;
            //    for (i = 0; i < lAnimStackCount; i++)
            //    {
            //        // FbxTakeInfo lTakeInfo = lImporter.GetTakeInfo(i);

            //        // Debug.WriteLine("    Animation Stack %d\n", i);
            //        // Debug.WriteLine("         Name: \"%s\"\n", lTakeInfo.mName.Buffer());
            //        // Debug.WriteLine("         Description: \"%s\"\n", lTakeInfo.mDescription.Buffer());

            //        //// Change the value of the import name if the animation stack should be imported
            //        //// under a different name.
            //        // Debug.WriteLine("         Import Name: \"%s\"\n", lTakeInfo.mImportName.Buffer());

            //        //// Set the value of the import state to false if the animation stack should be not
            //        //// be imported.
            //        // Debug.WriteLine("         Import State: %s\n", lTakeInfo.mSelect ? "true" : "false");
            //        // Debug.WriteLine("\n");
            //    }

            //    //// Set the import states. By default, the import states are always set to
            //    //// true. The code below shows how to change these states.
            //    // (*(pManager.GetIOSettings())).SetBoolProp(IMP_FBX_MATERIAL, true);
            //    // (*(pManager.GetIOSettings())).SetBoolProp(IMP_FBX_TEXTURE, true);
            //    // (*(pManager.GetIOSettings())).SetBoolProp(IMP_FBX_LINK, true);
            //    // (*(pManager.GetIOSettings())).SetBoolProp(IMP_FBX_SHAPE, true);
            //    // (*(pManager.GetIOSettings())).SetBoolProp(IMP_FBX_GOBO, true);
            //    // (*(pManager.GetIOSettings())).SetBoolProp(IMP_FBX_ANIMATION, true);
            //    // (*(pManager.GetIOSettings())).SetBoolProp(IMP_FBX_GLOBAL_SETTINGS, true);
            //}

            // Import the scene.
            var lStatus = lImporter.Import(pScene);

            // if (lStatus == false && lImporter.GetStatus().GetCode() == FbxStatus.ePasswordError)
            // {
            // Debug.WriteLine("Please enter password: ");

            // lPassword = StringFunctions.ChangeCharacter(lPassword, 0, '\0');

            // FBXSDK_CRT_SECURE_NO_WARNING_BEGIN scanf = new FBXSDK_CRT_SECURE_NO_WARNING_BEGIN("%s", lPassword);
            // FBXSDK_CRT_SECURE_NO_WARNING_END FbxString lString(lPassword);

            // (*(pManager.GetIOSettings())).SetStringProp(IMP_FBX_PASSWORD, lString);
            // (*(pManager.GetIOSettings())).SetBoolProp(IMP_FBX_PASSWORD_ENABLE, true);

            // lStatus = lImporter.Import(pScene);

            // if (lStatus == false && lImporter.GetStatus().GetCode() == FbxStatus.ePasswordError)
            // {
            // Debug.WriteLine("\nPassword is wrong, import aborted.\n");
            // }
            // }
            return lStatus;
        }

        public static bool SaveScene(FBXManager pManager, FBXDocument pScene, string pFilename, int pFileFormat = -1, bool pEmbedMedia = false)
        {
            int lMajor = 0;
            int lMinor = 0;
            int lRevision = 0;
            bool lStatus = true;

            // Create an exporter.
            FBXExporter lExporter = FBXExporter.Create(pManager, string.Empty);
            var ioSettings = lExporter.GetIOSettings();

            // if (pFileFormat < 0 || pFileFormat >= pManager.GetIOPluginRegistry().GetWriterFormatCount())
            // {
            // // Write in fall back format in less no ASCII format found
            // pFileFormat = pManager.GetIOPluginRegistry().GetNativeWriterFormat();

            // //Try to export in ASCII if possible
            // int lFormatIndex;
            // int lFormatCount = pManager.GetIOPluginRegistry().GetWriterFormatCount();

            // for (lFormatIndex = 0; lFormatIndex < lFormatCount; lFormatIndex++)
            // {
            // if (pManager.GetIOPluginRegistry().WriterIsFBX(lFormatIndex))
            // {
            // FbxString lDesc = pManager.GetIOPluginRegistry().GetWriterFormatDescription(lFormatIndex);
            // string lASCII = "ascii";
            // if (lDesc.Find(lASCII) >= 0)
            // {
            // pFileFormat = lFormatIndex;
            // break;
            // }
            // }
            // }
            // }

            //// Set the export states. By default, the export states are always set to
            //// true except for the option eEXPORT_TEXTURE_AS_EMBEDDED. The code below
            //// shows how to change these states.
            // (*(pManager.GetIOSettings())).SetBoolProp(EXP_FBX_MATERIAL, true);
            // (*(pManager.GetIOSettings())).SetBoolProp(EXP_FBX_TEXTURE, true);
            // (*(pManager.GetIOSettings())).SetBoolProp(EXP_FBX_EMBEDDED, pEmbedMedia);
            // (*(pManager.GetIOSettings())).SetBoolProp(EXP_FBX_SHAPE, true);
            // (*(pManager.GetIOSettings())).SetBoolProp(EXP_FBX_GOBO, true);
            // (*(pManager.GetIOSettings())).SetBoolProp(EXP_FBX_ANIMATION, true);
            // (*(pManager.GetIOSettings())).SetBoolProp(EXP_FBX_GLOBAL_SETTINGS, true);

            // Initialize the exporter by providing a filename.
            if (lExporter.Initialize(pFilename, pFileFormat, pManager.GetIOSettings()) == false)
            {
                Debug.WriteLine("Call to FBXExporter::Initialize() failed.");

                // Debug.WriteLine("Error returned: {0}", lExporter.GetStatus().GetErrorString());
                return false;
            }

            FBXManager.GetFileFormatVersion(ref lMajor, ref lMinor, ref lRevision);
            Debug.WriteLine("FBX file format version {0}.{1}.{2}", lMajor, lMinor, lRevision);

            // Export the scene.
            Directory.SetCurrentDirectory(Path.GetDirectoryName(pFilename));
            lStatus = lExporter.Export(pScene, false);

            // Destroy the exporter.
            lExporter.Destroy();
            return lStatus;
        }
    }
}