using System.Drawing;
using System.Windows.Forms;

namespace ExtensionHelpers
{
    public class IconConverter : AxHost
    {
        private IconConverter() : base("")
        {
        }

        static public stdole.IPictureDisp ImageToPictureDisp(Image image)

        {
            return (stdole.IPictureDisp)GetIPictureDispFromPicture(image);
        }

        static public Image PictureDispToImage(stdole.IPictureDisp pictureDisp)

        {
            return GetPictureFromIPicture(pictureDisp);
        }
    }
}