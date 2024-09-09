using FlatRedBall.Gui;
using FlatRedBall.Screens;
using NarfoxGameTools.Extensions;
using NarfoxGameTools.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NarfoxGameTools.Services
{
    public interface IShowableMenu
    {
        /// <summary>
        /// Event intended to be fired when
        /// the menu's Show method is called
        /// </summary>
        event EventHandler OnShow;

        /// <summary>
        /// Event intended to be fired when
        /// the menu's Hide method is called
        /// </summary>
        event EventHandler OnHide;

        /// <summary>
        /// Event intended to be fired when
        /// the menu's Close button is clicked.
        /// 
        /// Allows UI Service to manage close
        /// menu behavior without the screen needing
        /// to implement behavior for every menu
        /// </summary>
        event Action<IShowableMenu> CloseCommand;

        /// <summary>
        /// Should return whether the menu is
        /// being shown or not. Generally a wrapper
        /// for the menu's unique visibility state
        /// </summary>
        bool IsShown { get; }

        /// <summary>
        /// A method that can be called to show
        /// the menu
        /// </summary>
        void Show();

        /// <summary>
        /// A method that can be called to hide
        /// the menu
        /// </summary>
        /// <param name="immediate"></param>
        void Hide(bool immediate = false);
    }


    /// <summary>
    /// This service makes it easy to manage menu states. An IShowableMenu
    /// interface ensures that menus have Show and Hide methods, and some
    /// type of close button that executes a command. Screens can register 
    /// and unregister their menus and this service will make sure that only
    /// one menu can show at a time and keep track of the dominant window
    /// </summary>
    public class UIService
    {
        public event EventHandler MenuShown;
        public event EventHandler MenuHidden;
        
        private static UIService instance;
        private static readonly Object padlock = new object();
        private List<IShowableMenu> menus;
        private ILogger log;

        
        public bool PauseScreenOnShowMenu { get; set; } = true;
        public IShowableMenu CurrentMenu => menus.FirstOrDefault(m => m.IsShown);
        public bool IsMenuShowing => CurrentMenu != null;

        public static UIService Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new InvalidOperationException("This service has not been initialized! Call Initialize before using.");
                }
                return instance;
            }
        }


        private UIService(ILogger logger)
        {
            menus = new List<IShowableMenu>();
            this.log = logger;
        }

        /// <summary>
        /// Sets up the list of menus and prepares the
        /// service to be used. Should be called before
        /// using any other methods
        /// </summary>
        public static void Initialize(ILogger logger)
        {
            lock(padlock)
            {
                if(instance == null)
                {
                    instance = new UIService(logger);
                }
            }

        }

        /// <summary>
        /// Tries to show the provided menu. Will fail
        /// silently if another menu is already showing
        /// </summary>
        /// <param name="menu">The menu to show</param>
        /// <returns>A bool indicating success or failure</returns>
        public bool TryShowMenu(IShowableMenu menu)
        {
            if (menus.Contains(menu) == false)
            {
                throw new Exception("Tried to show a menu that was never registered.");
            }

            // EARLY OUT: a menu is already showing and it's not this menu
            if (IsMenuShowing && menu.IsShown == false)
            {
                return false;
            }

            if (PauseScreenOnShowMenu && !ScreenManager.CurrentScreen.IsPaused)
            {
                ScreenManager.CurrentScreen.PauseThisScreen();
            }

            // make sure this menu is not already showing
            if (menu.IsShown == false)
            {
                menu.Show();
            }

            return true;
        }

        /// <summary>
        /// Tries to hide the provided menu. Will fail if the menu is not showing.
        /// </summary>
        /// <param name="menu">The menu to hide</param>
        /// <param name="immediate">Whether to skip animations and immediately hide the menu</param>
        /// <returns>A bool indicating success</returns>
        public bool TryHideMenu(IShowableMenu menu, bool immediate = false)
        {
            if (menus.Contains(menu) == false)
            {
                throw new Exception("Tried to hide a menu that was never registered.");
            }

            // EARLY OUT: menu is already hidden
            if (menu.IsShown == false)
            {
                return false;
            }

            // auto pause screen if set
            if (PauseScreenOnShowMenu && ScreenManager.CurrentScreen.IsPaused)
            {
                ScreenManager.CurrentScreen.UnpauseThisScreen();
            }

            menu.Hide(immediate);
            return true;
        }

        /// <summary>
        /// An overload that will hide ANY menu that is showing. Can be
        /// called before showing a priority menu, such as a pause menu
        /// </summary>
        /// <param name="immediate">Whether to hide all menus immediately without animations</param>
        /// <returns>bool indicating success</returns>
        public bool TryHideMenu(bool immediate = false)
        {
            // EARLY OUT: no menu was showing
            if (!IsMenuShowing)
            {
                return false;
            }

            if (PauseScreenOnShowMenu && ScreenManager.CurrentScreen.IsPaused)
            {
                ScreenManager.CurrentScreen.UnpauseThisScreen();
            }

            menus.ForEach(m =>
            {
                if (m.IsShown)
                {
                    m.Hide(immediate);
                }
            });
            return true;
        }

        /// <summary>
        /// Registers a menu for management by this service. Game screens should
        /// use this to register all menus they want managed by the service.
        /// 
        /// Hides the registered menu immediately by default
        /// </summary>
        /// <param name="menu">The menu to register</param>
        public void RegisterMenu(IShowableMenu menu)
        {
            menus.Add(menu);
            menu.Hide(true);
            menu.OnShow += Menu_OnShow;
            menu.OnHide += Menu_OnHide;
            menu.CloseCommand += Menu_CloseCommand;
        }

        /// <summary>
        /// Unregisters a menu with this service. Game screens should call this
        /// for any managed menu before they unload or references to the menu
        /// will be held by this service, preventing it from being destroyed!
        /// </summary>
        /// <param name="menu">The menu to unregister</param>
        public void UnregisterMenu(IShowableMenu menu)
        {
            menu.OnShow -= Menu_OnShow;
            menu.OnHide -= Menu_OnHide;
            menu.CloseCommand -= Menu_CloseCommand;
            menus.Remove(menu);
        }

        /// <summary>
        /// Unregisters all registered menus. This is a convenience method that
        /// screens can call while unloading to make sure all menus are unregistered.
        /// </summary>
        public void UnregisterAllMenus()
        {
            for (var i = menus.Count - 1; i > -1; i--)
            {
                UnregisterMenu(menus[i]);
            }
        }

        /// <summary>
        /// Event handler applied to all menu's OnShow methods and
        /// cascades, raising the service's MenuShown method
        /// </summary>
        /// <param name="sender">The calling object, which should be an IShowable menu</param>
        /// <param name="e">Event arguments</param>
        private void Menu_OnShow(object sender, EventArgs e)
        {
            var window = (IWindow)sender;
            GuiManager.AddDominantWindow(window);
            GuiManager.Cursor.CenterOnScreen();

            // cascade event to subscribers (usually an FRB screen)
            MenuShown?.Invoke(sender as IShowableMenu, e);
        }

        /// <summary>
        /// Event handler applied to all menu's OnHide methods and
        /// cascades, raising the service's MenuHidden
        /// </summary>
        /// <param name="sender">The calling object, which should be an IShowable menu</param>
        /// <param name="e">Event arguments</param>
        private void Menu_OnHide(object sender, EventArgs e)
        {
            var window = (IWindow)sender;
            GuiManager.MakeRegularWindow(window);

            // cascade event to subscribers (usually an FRB screen)
            MenuHidden?.Invoke(sender, e);
        }

        /// <summary>
        /// Event handler applied to all menus' CloseCommand methods.
        /// Hides the menu so close buttons don't have to be manually bound.
        /// </summary>
        /// <param name="menu">The menu that triggered this command</param>
        private void Menu_CloseCommand(IShowableMenu menu)
        {
            TryHideMenu(menu);
        }
    }
}
