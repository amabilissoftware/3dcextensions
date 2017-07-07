namespace AutoDeskFBXSDK
{
    public class Import
    {
        // Coming Soon! - This is what I'll probably be using as a base:


        // Option Explicit
        // '*********************************************************************************
        // '
        // ' 3DCrafter (tm)
        // ' Copyright (c)1998-2011 Amabilis Software
        // ' Copyright (c)1998-2011 Richard Borsheim
        // ' All Rights Reserved.
        // '
        // '*********************************************************************************
        // '*********************************************************************************
        // ' Module Level Constants
        // '*********************************************************************************
        // Private Const mt_Module As String = "modImportAT3D"

        // '*********************************************************************************
        // ' Public Methods
        // '*********************************************************************************
        // '*********************************************************************************
        // ' Purpose:
        // '
        // ' Parameters:
        // '
        // '*********************************************************************************
        // Public Function lImportAccuTrans3D(stFileName As String, _
        // udtImportType As At3d_InType, _
        // cgParentGroup As CSGGroup, _
        // cCSG As CSG) As Long

        // Dim lReturn   As Long
        // Dim oAT3D     As clsAT3DData
        // Dim Data()    As Data_3D
        // Dim DataCount As Long
        // Dim lShape    As Long

        // 'Dim udtGridOrigin As CSGVector
        // 'Dim sGridSize     As Single
        // 'Dim sGridInterval As Single
        // 'Dim udtPosition   As CSGVector
        // On Error GoTo 0
        // Set oAT3D = New clsAT3DData
        // 'start up AT3D
        // If Not gbAT3DStarted Then
        // lReturn = At3d_Start()
        // gbAT3DStarted = True
        // End If
        // 'indicate that we don't want faces removed
        // At3d_WriteFlag LIBFLAG_REMOVE_DUPLICATE_FACES, 0
        // 'indicate that we don't want to prompt for textures
        // At3d_WriteFlag LIBFLAG_TEXTURESEARCH_PROMPT, 0
        // 'load the file
        // Dim byteFileName(1000) As Byte
        // gcCHFFormatting.StringToBytes byteFileName, stFileName
        // lReturn = At3d_Read(udtImportType, byteFileName(0), CDbl(1))
        // If lReturn Then
        // oAT3D.GetData Data, DataCount
        // oAT3D.MassageData Data, DataCount, True, True
        // 'dummy stuff for now
        // 'ReDim xdtaNormalList(0)
        // 'ReDim xdtaUVWList(0)
        // 'translate and add the AT3D data
        // For lShape = 0 To UBound(Data)
        // pAddShape cCSG, Data(lShape), cgParentGroup
        // Next lShape
        // 'tidy up
        // At3d_Clear
        // 'return the number of files exported (actually the maximum number exported)
        // lImportAccuTrans3D = UBound(Data) + 1
        // Else
        // MsgBox "file load failed", vbExclamation, "3DCrafter"
        // End If

        // End Function

        // '*********************************************************************************
        // ' Private Methods
        // '*********************************************************************************
        // '*********************************************************************************
        // ' Purpose:
        // '
        // ' Parameters:
        // '
        // '*********************************************************************************
        // Private Sub pAddShape(cCSG As CSG, _
        // xdtData As Data_3D, _
        // cgParentGroup As CSGGroup)

        // Dim laTriangleData()   As Long
        // Dim lTriangleDataCount As Long
        // Dim udtaPointList()    As CSGVector
        // Dim lPointListCount    As Long
        // Dim udtaNormalList()   As CSGVector
        // Dim udtaUVList()       As CSGUV
        // Dim lTriangle          As Long
        // Dim lNormal            As Long
        // Dim lPoint             As Long
        // Dim csShape            As CSGShape
        // Dim cmMaterial         As CSGMaterial
        // Dim udtBoundingBox     As CSGMinMaxVector
        // Dim cgGroup            As CSGGroup
        // Dim lTextureWidth      As Long
        // Dim lTextureHeight     As Long
        // Dim lUV                As Long

        // On Error GoTo 0
        // 'move the data from the AT3D data to 3DCrafter data
        // With xdtData
        // 'first the points (can't copy memory since AT3D uses double data)
        // lPointListCount = .PCount3D
        // ReDim udtaPointList(lPointListCount - 1) As CSGVector
        // For lPoint = 0 To lPointListCount - 1
        // udtaPointList(lPoint).X = CSng(.xyz3D(lPoint).X)
        // udtaPointList(lPoint).Y = CSng(.xyz3D(lPoint).z)
        // udtaPointList(lPoint).z = CSng(.xyz3D(lPoint).Y)
        // Next lPoint
        // 'then the normals
        // If.vnormCount > 0 Then
        // ReDim udtaNormalList(.vnormCount - 1) As CSGVector
        // For lNormal = 0 To.vnormCount - 1
        // udtaNormalList(lNormal).X = CSng(.vnorm(lNormal).X)
        // udtaNormalList(lNormal).Y = CSng(.vnorm(lNormal).z)
        // udtaNormalList(lNormal).z = CSng(.vnorm(lNormal).Y)
        // Next lNormal
        // 'lNormalListCount = .vnormcount
        // Else
        // ReDim udtaNormalList(0) As CSGVector
        // 'lNormalListCount = 1
        // End If
        // 'then the uv's
        // If.uvCount > 0 Then
        // ReDim udtaUVList(.uvCount - 1) As CSGUV
        // For lUV = 0 To.uvCount - 1
        // udtaUVList(lUV).u = CSng(.uv(lUV).u)
        // udtaUVList(lUV).v = -CSng(.uv(lUV).v)
        // Next lUV
        // 'lUVListCount = .uvcount
        // Else
        // ReDim udtaUVList(0) As CSGUV
        // 'lUVListCount = 1
        // End If
        // 'finally the face data
        // ReDim laTriangleData(.TCount* 10) As Long
        // For lTriangle = 0 To.TCount - 1
        // 'the point count
        // laTriangleData(lTriangleDataCount) = 3 'a triangle
        // 'the first point
        // laTriangleData(lTriangleDataCount + 1) = .xyzIndex(lTriangle).p0
        // If.vnormCount > 0 Then
        // laTriangleData(lTriangleDataCount + 2) = .vnormIndex(lTriangle).p0
        // End If
        // If.uvCount > 0 Then
        // laTriangleData(lTriangleDataCount + 3) = .uvIndex(lTriangle).p0
        // End If
        // 'the second point
        // laTriangleData(lTriangleDataCount + 4) = .xyzIndex(lTriangle).p1
        // If.vnormCount > 0 Then
        // laTriangleData(lTriangleDataCount + 5) = .vnormIndex(lTriangle).p1
        // End If
        // If.uvCount > 0 Then
        // laTriangleData(lTriangleDataCount + 6) = .uvIndex(lTriangle).p1
        // End If
        // 'the third point
        // laTriangleData(lTriangleDataCount + 7) = .xyzIndex(lTriangle).p2
        // If.vnormCount > 0 Then
        // laTriangleData(lTriangleDataCount + 8) = .vnormIndex(lTriangle).p2
        // End If
        // If.uvCount > 0 Then
        // laTriangleData(lTriangleDataCount + 9) = .uvIndex(lTriangle).p2
        // End If
        // lTriangleDataCount = lTriangleDataCount + 10
        // Next lTriangle
        // 'terminate the triangle list
        // laTriangleData(lTriangleDataCount) = 0
        // Set csShape = cCSG.CreateShape
        // csShape.AddFacesXYZUV udtaPointList, udtaNormalList, udtaUVList, laTriangleData, False, False
        // csShape.name = .name
        // 'if there are no normals then crease it
        // If.vnormCount = 0 Then
        // csShape.Crease

        // End If
        // 'set the material

        // Set cmMaterial = csShape.GetFace(0).Material

        // cmMaterial.x_SetColorRGBA.diffuseRGB.red, .diffuseRGB.green, .diffuseRGB.blue, 1 - .transparencyRGB.blue
        // If .ambientRGB.red = 0 And.ambientRGB.green = 0 And.ambientRGB.blue = 0 Then
        // .ambientRGB.red = .diffuseRGB.red * 0.6
        // .ambientRGB.green = .diffuseRGB.green * 0.6
        // .ambientRGB.blue = .diffuseRGB.blue * 0.6

        // End If

        // cmMaterial.x_SetAmbient.ambientRGB.red, .ambientRGB.green, .ambientRGB.blue
        // If .emissiveRGB.red = 0 And.emissiveRGB.green = 0 And.emissiveRGB.blue = 0 Then
        // 'we could just set this to an arbitrary number, but it is better
        // 'that we do this cleverly and make sure things are "bright"
        // .emissiveRGB.red = 0.8 * .diffuseRGB.red - .ambientRGB.red
        // .emissiveRGB.green = 0.8 * .diffuseRGB.green - .ambientRGB.green
        // .emissiveRGB.blue = 0.8 * .diffuseRGB.blue - .ambientRGB.blue

        // End If

        // cmMaterial.x_SetEmissive.emissiveRGB.red, .emissiveRGB.green, .emissiveRGB.blue
        // If LenB(.texturePathName) Then
        // On Error Resume Next
        // Set cmMaterial.Texture = cCSG.CreateTexture(.texturePathName, False, "", "", "", "", "")
        // Err.Clear
        // End If
        // cmMaterial.power = 250! - (.shininess* 100) * 2.49!
        // cmMaterial.x_SetSpecular.specularRGB.red, .specularRGB.green, .specularRGB.blue
        // '        .CreaseAngle = csShape.GetCreasingFactor
        // If Not cmMaterial.Texture Is Nothing Then
        // .texturePathName = cmMaterial.Texture.Texture0Name
        // End If
        // End With
        // 'center the shape on its group
        // udtBoundingBox = csShape.GetBoundingBox
        // csShape.Translate gcCSG.x_Functions.CreateVector(-(udtBoundingBox.Max.X + udtBoundingBox.Min.X) / 2, -(udtBoundingBox.Max.Y + udtBoundingBox.Min.Y) / 2, -(udtBoundingBox.Max.z + udtBoundingBox.Min.z) / 2)
        // 'add the shape
        // Set cgGroup = cCSG.CreateGroup(cgParentGroup)
        // With cgGroup
        // .name = "Group"
        // .AddShape csShape
        // 'center the shape on its group
        // .AddTranslation CSGCombineAfter, gcCSG.x_Functions.CreateVector((udtBoundingBox.Max.X + udtBoundingBox.Min.X) / 2, (udtBoundingBox.Max.Y + udtBoundingBox.Min.Y) / 2, (udtBoundingBox.Max.z + udtBoundingBox.Min.z) / 2)
        // 'tidy up
        // End With 'cgGroup
        // Set csShape = Nothing
        // Set cgGroup = Nothing

        // End Sub

        // ''*********************************************************************************
        // '' Purpose:
        // ''
        // '' Parameters:
        // ''
        // ''*********************************************************************************
        // '
        // 'Private Sub pGetFaceEntries(sToken As String
        // '', lVertexIndex As Long
        // '', lTextureIndex As Long
        // '', bTextureIndex As Boolean
        // '', lNormalIndex As Long
        // '', bNormalIndex As Boolean)
        // '    On Error GoTo 0
        // '
        // '    Dim lSlashPosition0 As Long
        // '    Dim lSlashPosition1 As Long
        // '
        // '    bTextureIndex = False
        // '    bNormalIndex = False
        // '
        // '    'determine what type we have
        // '    lSlashPosition0 = InStr(1, sToken, "/")
        // '    If lSlashPosition0 > 0 Then
        // '        lVertexIndex = CLng(mID(sToken, 1, lSlashPosition0 - 1))
        // '
        // '        'the texture index
        // '        lSlashPosition1 = InStr(lSlashPosition0 + 1, sToken, "/")
        // '        If lSlashPosition1 > lSlashPosition0 + 1 Then
        // '            lTextureIndex = CLng(mID(sToken, lSlashPosition0 + 1, lSlashPosition1 - 1 - lSlashPosition0))
        // '            bTextureIndex = True
        // '        'only one slash-requires a texture index
        // '        ElseIf lSlashPosition1 = 0 Then
        // '            On Error Resume Next
        // '            lTextureIndex = -1
        // '            lTextureIndex = CLng(mID(sToken, lSlashPosition0 + 1, Len(sToken) - lSlashPosition0))
        // '            On Error GoTo 0
        // '            If lTextureIndex <> -1 Then
        // '                bTextureIndex = True
        // '            Else
        // '                'no texture index
        // '                lTextureIndex = 0
        // '            End If
        // '            lSlashPosition1 = Len(sToken)
        // '        End If
        // '
        // '        'is there a normal index
        // '        If lSlashPosition1 <> Len(sToken) Then
        // '            lNormalIndex = CLng(mID(sToken, lSlashPosition1 + 1, Len(sToken) - lSlashPosition1))
        // '            bNormalIndex = True
        // '        End If
        // '    'a simple vertex only
        // '    Else
        // '        lVertexIndex = CLng(sToken)
        // '    End If
        // '
        // '    Exit Sub
        // '
        // 'ErrorxHandler:
        // '    Err.Raise Err.Number, mt_Module, Err.Description
        // 'End Sub
        // '
        // }
    }
}