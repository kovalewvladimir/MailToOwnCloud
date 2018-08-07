using System;
using System.IO;

namespace MailToOwnCloud
{
    public static class AuthenticationHelper
    {
        public static bool Authentication(string nameAccessFile)
        {
            try
            {
                using (StreamReader sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + nameAccessFile))
                {
                    // Первый раз считываем загаловок
                    string line  = sr.ReadLine();
                    string[] row = new string[3];
                    while ((line = sr.ReadLine()) != null)
                    {
                        row = line.Split(',');
                        string user = row[0].ToUpper();
                        DateTime date = DateTime.Parse(row[1]);
                        if (Environment.UserName.ToUpper() == user && DateTime.Now < date)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBoxShow.Error(ex.Message, "", "Ошибка аутентификации");
                return false;
            }
        }
    }
}
