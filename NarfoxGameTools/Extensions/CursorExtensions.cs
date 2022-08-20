using FlatRedBall;
using FlatRedBall.Gui;

namespace NarfoxGameTools.Extensions
{
    public static class CursorExtensions
    {
        public static void CenterOnScreen(this Cursor cursor)
        {
            cursor.ScreenX = (int)(Camera.Main.DestinationRectangle.Width / 2.0f);
            cursor.ScreenY = (int)(Camera.Main.DestinationRectangle.Height / 2.0f);
        }
    }
}
