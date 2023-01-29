using FlatRedBall;
using FlatRedBall.Gui;

namespace NarfoxGameTools.Extensions
{
    public static class CursorExtensions
    {
        /// <summary>
        /// This is often needed when using a gamepad-controlled cursor.
        /// 
        /// Usually called when moving from gameplay to showing a menu to
        /// ensure the cursor isn't offscreen.
        /// </summary>
        /// <param name="cursor">The FRB cursor to move</param>
        public static void CenterOnScreen(this Cursor cursor)
        {
            cursor.ScreenX = (int)(Camera.Main.DestinationRectangle.Width / 2.0f);
            cursor.ScreenY = (int)(Camera.Main.DestinationRectangle.Height / 2.0f);
        }
    }
}
