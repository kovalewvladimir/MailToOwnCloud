using System;
using System.Windows;

namespace MailToOwnCloud
{
    /// <summary>
    /// Обертка для MessageBox
    /// </summary>
    public sealed class MessageBoxShow
    {
        private const string _messageErrorDefault = "{0}Error{1}";

        public static string MessageErrorText = _messageErrorDefault;
        public static string MessageErrorTitle = _messageErrorDefault;

        /// <summary>
        /// Отображает окно ошибки
        /// </summary>
        /// <param name="afterMessageText">Текст после стандартного текста</param>
        /// <param name="afterMessageTitle">Заголовок после стандартного текста</param>
        /// <param name="beforeMessageText">Текст до стандартного текста</param>
        /// <param name="beforeMessageTitle">Заголовок до стандартного текста</param>
        /// <returns></returns>
        public static MessageBoxResult Error(string afterMessageText = "", string afterMessageTitle = "",
                                             string beforeMessageText = "", string beforeMessageTitle = "")
        {
            string message, title;
            try
            {
                message = String.Format(MessageErrorText, $"{beforeMessageText}\n", $"\n{afterMessageText}");
                title   = String.Format(MessageErrorTitle, $"{beforeMessageTitle}", $"{afterMessageTitle}");
            }
            catch (System.FormatException)
            {
                message = String.Format(_messageErrorDefault, beforeMessageText, afterMessageText);
                title   = String.Format(_messageErrorDefault, beforeMessageTitle, afterMessageTitle);
            }
            
            return MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
