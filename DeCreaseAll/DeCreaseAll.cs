namespace DeCreaseAll
{
    using System.Windows.Forms;

    using ACSG;

    using stdole;

    using TDCPlugin;

    using TDCScripting;

    internal static class DeCreaseAll
    {
        internal static void CreaseGroupRecursively(CSGGroup group, ref int shapesCreasedCount)
        {
            CSGShapeArray childShapeList = group.GetShapes();
            for (int shapeIndex = 0; shapeIndex < childShapeList.GetSize(); shapeIndex++)
            {
                CreaseShape(childShapeList.GetElement(shapeIndex), ref shapesCreasedCount);
            }

            CSGGroupArray childGroupList = group.GetChildren();
            for (int groupIndex = 0; groupIndex < childGroupList.GetSize(); groupIndex++)
            {
                CreaseGroupRecursively(childGroupList.GetElement(groupIndex), ref shapesCreasedCount);
            }
        }

        private static void CreaseShape(CSGShape shape, ref int shapesCreasedCount)
        {
            if (shape.RepresentativeEntityType == CSGEntityType.CSGEShape && shape.ShapeType == CSGShapeType.CSGShapeStandard && shape.GetFaceCount() > 0)
            {
                shape.Crease2(180);
                shapesCreasedCount++;
            }
        }
    }
}