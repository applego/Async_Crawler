using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace Async_Crawler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("HttpClientクラスでWebページを取得する");

            // 時間計測用のタイマー
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            // 取得したいWebページのURI
            Uri webUri = new Uri("http://dev.windows.com/ja-jp");
            //Uri webUri = new Uri("https://dev.windows.com/ja-jp"); // HTTPSでもOK!
            //Uri webUri = new Uri("https://dev.windows.com/"); // デフォルトではリダイレクト先を取得してくれる

            //Uri webUri = new Uri("https://appdev.microsoft.com/"); // 403エラー
            //Uri webUri = new Uri("https://appdev.microsoft.com/ja-JP/"); // 404エラー
            //Uri webUri = new Uri("http://notexist.example.com/"); // リモート名を解決できないエラー

            // GetWebPageAsyncメソッドを呼び出す
            Task<string> webTask = GetWebPageAsync(webUri);
            webTask.Wait(); // Mainメソッドではawaitできないので、処理が完了するまで待機する
            string result = webTask.Result;  // 結果を取得

            timer.Stop();
            Console.WriteLine("{0:0.000}秒", timer.Elapsed.TotalSeconds);
            Console.WriteLine();

            // 取得結果を使った処理
            if (result != null)
            {

                // サンプルとして、取得したHTMLデータの<h1>タグ以降を一定長だけ表示
                Console.WriteLine("========");
                int h1pos = result.IndexOf("<h1", StringComparison.OrdinalIgnoreCase);
                if (h1pos < 0) 
                    h1pos = 0;
                const int MaxLength = 720;
                int len = result.Length - h1pos;
                if (len > MaxLength)
                    len = MaxLength;
                Console.WriteLine(result.Substring(h1pos, len));
                Console.WriteLine("========");
                
            
                Console.ReadLine();
            
            }
        }
        static async Task<string> GetWebPageAsync(Uri uri)
        {
            using (HttpClient client = new HttpClient())
            {
                // ユーザーエージェント文字列をセット（オプション）
                client.DefaultRequestHeaders.Add(
                    "User-Agent",
                    "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");

                // 受け入れ言語をセット（オプション）
                client.DefaultRequestHeaders.Add("Accept-Language", "ja-JP");

                // タイムアウトをセット（オプション）
                client.Timeout = TimeSpan.FromSeconds(10.0);

                try
                {
                    // Webページを取得するのは、事実上この1行だけ
                    return await client.GetStringAsync(uri);
                }
                catch (HttpRequestException e)
                {
                    // 404エラーや、名前解決失敗など
                    Console.WriteLine("\n例外発生!");
                    // InnerExceptionも含めて、再帰的に例外メッセージを表示する
                    Exception ex = e;
                    while (ex != null)
                    {
                        Console.WriteLine("例外メッセージ: {0} ", ex.Message);
                        ex = ex.InnerException;
                    }
                }
                catch (TaskCanceledException e)
                {
                    // タスクがキャンセルされたとき（一般的にタイムアウト）
                    Console.WriteLine("\nタイムアウト!");
                    Console.WriteLine("例外メッセージ: {0} ", e.Message);
                }
                return null;
            }
        }
    }
}
