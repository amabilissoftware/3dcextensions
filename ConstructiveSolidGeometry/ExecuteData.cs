namespace ACBO
{
    using System;

    [Serializable]
    public class ExecuteData
    {
        public int[] faceColorList { get; set; }

        public int[] faceColorList2 { get; set; }

        public int faceColorList2Count { get; set; }

        public int faceColorListCount { get; set; }

        public int[] FaceData { get; set; }

        public int[] FaceData2 { get; set; }

        public int FaceData2Count { get; set; }

        public int FaceDataCount { get; set; }

        public CBODataTypes.CSGVector[] NormalList { get; set; }

        public CBODataTypes.CSGVector[] NormalList2 { get; set; }

        public int NormalList2Count { get; set; }

        public int NormalListCount { get; set; }

        public CBODataTypes.CSGVector[] PointList { get; set; }

        public CBODataTypes.CSGVector[] PointList2 { get; set; }

        public int PointList2Count { get; set; }

        public int PointListCount { get; set; }

        public CBODataTypes.CSGUV[] TextureCoordinateList { get; set; }

        public CBODataTypes.CSGUV[] TextureCoordinateList2 { get; set; }

        public int TextureCoordinateList2Count { get; set; }

        public int TextureCoordinateListCount { get; set; }

        public CBODataTypes.CSGToolOperation udtOperation { get; set; }
    }
}