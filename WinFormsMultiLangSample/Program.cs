using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsMultiLangSample
{
    internal static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 保存済みの言語設定を読み込む
            string lang = Properties.Settings.Default.Language;
            if (string.IsNullOrEmpty(lang))
            {
                lang = "ja"; // 初回起動時の既定値
            }

            var culture = new CultureInfo(lang);
            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;

            Application.Run(new Form1());
        }
    }
}
