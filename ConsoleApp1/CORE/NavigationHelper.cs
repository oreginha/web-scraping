using OpenQA.Selenium;
using System;
//namespace EventScraperBackend.Core
//{
//     public static class NavigationHelper
//    {
//        public static void OpenNewTabAndNavigate(IWebDriver driver, string url)
//        {
//           string originalWindow = driver.CurrentWindowHandle;
//            driver.SwitchTo().NewWindow(WindowType.Tab);
//           driver.Navigate().GoToUrl(url);
//          }
//         public static void CloseCurrentTabAndSwitchBack(IWebDriver driver, string originalWindow)
//            {
//                driver.Close();
//                driver.SwitchTo().Window(originalWindow);
//            }
//    }
//}



namespace EventScraperBackend.Core
{
    public static class NavigationHelper
    {
        public static void OpenNewTabAndNavigate(IWebDriver driver, string url)
        {
            string originalWindow = driver.CurrentWindowHandle;
            driver.SwitchTo().NewWindow(WindowType.Tab);
            driver.Navigate().GoToUrl(url);
        }
        public static void CloseCurrentTabAndSwitchBack(IWebDriver driver, string originalWindow)
        {
            driver.Close();
            driver.SwitchTo().Window(originalWindow);
        }
    }
}