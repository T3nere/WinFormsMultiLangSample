# Windows Forms 多言語対応（英語・日本語）実装手順書
## Resources.resx方式 / Excel→resx変換方式 / Excel直接読込方式

このドキュメントは、Windows Formsアプリケーションを英語・日本語の2言語に対応させる方法のうち、以下の3パターンをまとめたものです。

- **方式1: `Resources.resx`（独立した文字列リソースファイル）を使う方法**
- **方式2（パターンA）: Excelで文言を管理し、`.resx`に変換して使う方法**
- **方式3（パターンB）: Excelファイルを実行時に直接読み込む方法**

いずれも以下を前提とします。

- 言語は英語(en) / 日本語(ja) の2つ
- 言語切替は「設定メニューのボタン」から行う
- 実際に文言が切り替わるのは**アプリケーションの再起動後**
- ボタン押下時に「再起動後に切り替わる」旨のメッセージを表示する
- 言語設定の保存先は `Properties.Settings.Default.Language`（string, User スコープ）

---

## 共通部分：起動時の言語読み込み（`Program.cs`）

3方式すべてに共通する、起動時の設定読み込みコードです。

```csharp
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        // 保存済みの言語設定を読み込む
        string lang = Properties.Settings.Default.Language;
        if (string.IsNullOrEmpty(lang))
        {
            lang = "en"; // 初回起動時の既定値
        }

        var culture = new CultureInfo(lang);
        Thread.CurrentThread.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;

        // ApplicationConfiguration.Initialize(); // .NET 6+ の場合のみ必要（.NET Frameworkでは不要・削除する）
        Application.Run(new Form1());
    }
}
```

> **注意**: `CurrentUICulture` は、フォームが生成される（`InitializeComponent`が呼ばれる）**前**に設定する必要があります。これが「再起動しないと切り替わらない」仕様の技術的な理由です。

---

## 方式1: `Resources.resx` を使う方法

### 特徴

- フォームに紐づかない、**独立した文字列リソースファイル**
- 複数のフォームから共通で参照できる
- コード側で `Strings.BtnJapanese` のように**明示的にプロパティを呼び出して**Textに割り当てる必要がある（Form1.resx方式のようなデザイナー自動連携はない）

### 手順

#### 1. リソースファイルを作成する

1. ソリューションエクスプローラーで任意のフォルダ（例: プロジェクト直下、または新規作成した`Resources`フォルダ）を右クリック
2. 「追加」→「新しい項目」
3. 検索ボックスに `resource` と入力
4. 「**リソースファイル**」(Resources File) を選択
5. ファイル名を `Strings.resx` にして追加

#### 2. 既定言語（英語）の文字列を追加する

`Strings.resx` をダブルクリックして開き、表形式のエディタに入力する。

| 名前(Name)         | 値(Value)                                                              |
|---------------------|-------------------------------------------------------------------------|
| BtnJapanese          | Switch to Japanese                                                       |
| BtnEnglish           | Switch to English                                                        |
| MsgRestartRequired   | Language changes will take effect after restarting the application.     |

#### 3. 日本語版のリソースファイルを追加する

1. ソリューションエクスプローラーで `Strings.resx` を**コピー＆貼り付け**
2. ファイル名を `Strings.ja.resx` にリネーム（**`ファイル名.カルチャコード.resx` という命名規則が重要**）
3. `Strings.ja.resx` を開き、**Name列はそのまま**、**Value列だけ日本語に書き換える**

| 名前(Name)         | 値(Value)                                             |
|---------------------|----------------------------------------------------------|
| BtnJapanese          | 日本語に切り替え                                          |
| BtnEnglish           | 英語に切り替え                                            |
| MsgRestartRequired   | 言語設定はアプリケーションの再起動後に反映されます。       |

> Name列（キー）は英語版・日本語版で**完全に一致させる**こと。これが対応関係の紐付けになります。

#### 4. コード側で参照する

```csharp
private void Form1_Load(object sender, EventArgs e)
{
    // CurrentUICultureに応じて自動的にen/jaが切り替わる
    btnJapanese.Text = Strings.BtnJapanese;
    btnEnglish.Text = Strings.BtnEnglish;
}

private void btnJapanese_Click(object sender, EventArgs e)
{
    Properties.Settings.Default.Language = "ja";
    Properties.Settings.Default.Save();

    MessageBox.Show(Strings.MsgRestartRequired);
}

private void btnEnglish_Click(object sender, EventArgs e)
{
    Properties.Settings.Default.Language = "en";
    Properties.Settings.Default.Save();

    MessageBox.Show(Strings.MsgRestartRequired);
}
```

`Strings.BtnJapanese` のようにプロパティとして呼び出すだけで、その時点の `Thread.CurrentThread.CurrentUICulture` に応じて `Strings.resx`（英語）か `Strings.ja.resx`（日本語）かが自動選択されます（内部的に`ResourceManager`がCultureを見て処理している）。

---

## 方式2（パターンA）: Excelで管理し、`.resx`に変換して使う方法

### 特徴

- **翻訳者・担当者はExcelで文言を管理**（キー・英語・日本語の3列など）
- 開発者がその内容を `Strings.resx` / `Strings.ja.resx` に変換して取り込む
- 実行時の仕組みは方式1と全く同じ（`.resx`ベース）。Excelはあくまで「管理用マスターファイル」という位置づけ
- アプリの実行速度・安定性に影響を与えない

### 前提とするExcelの形式

| キー               | 英語                                                                   | 日本語                                                    |
|---------------------|-------------------------------------------------------------------------|--------------------------------------------------------------|
| BtnJapanese          | Switch to Japanese                                                       | 日本語に切り替え                                              |
| BtnEnglish           | Switch to English                                                        | 英語に切り替え                                                |
| MsgRestartRequired   | Language changes will take effect after restarting the application.     | 言語設定はアプリケーションの再起動後に反映されます。          |

### 手順

#### 1. Excelから `.resx` へ変換する

**手動でコピペする場合（行数が少ない場合はこれで十分）**

1. Excelの「キー」列と「英語」列をコピー
2. `Strings.resx` のエディタ（Name列・Value列）に貼り付け
3. Excelの「キー」列と「日本語」列をコピー
4. `Strings.ja.resx` のエディタに貼り付け

**自動変換する場合（行数が多い、更新頻度が高い場合におすすめ）**

Excelを一度CSVとして保存し、以下のようなC#コンソールアプリ、またはスクリプトで `.resx` のXML形式に変換します。

```csharp
// 簡易変換ツールの例（コンソールアプリとして別途作成）
using System.Xml.Linq;

void ConvertCsvToResx(string csvPath, string resxPath, int valueColumnIndex)
{
    var doc = new XDocument(
        new XElement("root",
            File.ReadAllLines(csvPath)
                .Skip(1) // ヘッダー行をスキップ
                .Select(line => line.Split(','))
                .Select(cols => new XElement("data",
                    new XAttribute("name", cols[0]),
                    new XElement("value", cols[valueColumnIndex])
                ))
        )
    );

    doc.Save(resxPath);
}

// 使用例
ConvertCsvToResx("Strings.csv", "Strings.resx", 1);    // 英語列
ConvertCsvToResx("Strings.csv", "Strings.ja.resx", 2); // 日本語列
```

> `.resx`は本来もう少し詳細なXMLヘッダー（スキーマ定義など）を含みますが、Visual Studioが読み込む際に最低限`<root><data name="キー"><value>値</value></data></root>`の形式があれば動作します。慣れないうちは、まず手動コピペで運用し、必要になったら変換ツールを整備することを推奨します。

#### 2. コードでの参照方法

方式1と全く同じです。

```csharp
btnJapanese.Text = Strings.BtnJapanese;
btnEnglish.Text = Strings.BtnEnglish;
MessageBox.Show(Strings.MsgRestartRequired);
```

### このパターンのメリット・デメリット

| メリット | デメリット |
|---|---|
| 実行時の速度・安定性は`.resx`本来のメリットをそのまま享受 | Excel→resx変換の一手間が発生する（自動化すれば軽減可能） |
| Excelは翻訳者が編集しやすいマスターデータとして使える | アプリを再ビルドしないと文言修正が反映されない |
| 追加ライブラリのインストールが不要 | |

---

## 方式3（パターンB）: Excelファイルを実行時に直接読み込む方法

### 特徴

- Excelファイル自体をアプリと一緒に配布し、**起動時にアプリがExcelを直接読み込んで**文言を取得する
- 再ビルドせずにExcelファイルだけ差し替えれば文言修正・追加言語対応ができる
- その代わり、Excel読み込み用ライブラリの追加や、ファイル欠損時のエラーハンドリングが必要になる

### 手順

#### 1. NuGetパッケージを追加する

Excelファイル（`.xlsx`）を読み込むためのライブラリを追加します。以下のいずれかを使います。

- `EPPlus`
- `ClosedXML`

Visual Studioの「NuGetパッケージの管理」からインストールしてください。

```powershell
# パッケージマネージャーコンソールでの例（EPPlusの場合）
Install-Package EPPlus
```

#### 2. Excelファイルをプロジェクトに含める

1. `Strings.xlsx` をプロジェクトフォルダ内（例: `Resources/Strings.xlsx`）に配置
2. ソリューションエクスプローラーで該当ファイルを選択し、プロパティで「**出力ディレクトリにコピー**」を「**新しい場合はコピーする**」に設定（ビルド時に実行フォルダへ自動コピーされるようにするため）

#### 3. 読み込み用クラスを作成する

```csharp
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;

public static class Localization
{
    private static Dictionary<string, string> _strings = new();

    public static void Load(string cultureCode)
    {
        _strings.Clear();

        string path = Path.Combine(AppContext.BaseDirectory, "Resources", "Strings.xlsx");

        using var package = new ExcelPackage(new FileInfo(path));
        var sheet = package.Workbook.Worksheets[0];

        // 列構成: A列=キー, B列=英語, C列=日本語 という前提
        int col = cultureCode == "ja" ? 3 : 2;
        int row = 2; // 1行目はヘッダーなのでスキップ

        while (!string.IsNullOrEmpty(sheet.Cells[row, 1].Text))
        {
            string key = sheet.Cells[row, 1].Text;
            string value = sheet.Cells[row, col].Text;
            _strings[key] = value;
            row++;
        }
    }

    public static string Get(string key) =>
        _strings.TryGetValue(key, out var v) ? v : key;
}
```

#### 4. `Program.cs` で読み込みを呼び出す

```csharp
static void Main()
{
    string lang = Properties.Settings.Default.Language;
    if (string.IsNullOrEmpty(lang)) lang = "en";

    var culture = new CultureInfo(lang);
    Thread.CurrentThread.CurrentUICulture = culture;

    Localization.Load(lang); // Excelから文言を読み込む

    Application.Run(new Form1());
}
```

#### 5. フォーム側での使用

```csharp
private void Form1_Load(object sender, EventArgs e)
{
    btnJapanese.Text = Localization.Get("BtnJapanese");
    btnEnglish.Text = Localization.Get("BtnEnglish");
}

private void btnJapanese_Click(object sender, EventArgs e)
{
    Properties.Settings.Default.Language = "ja";
    Properties.Settings.Default.Save();

    MessageBox.Show(Localization.Get("MsgRestartRequired"));
}
```

### このパターンのメリット・デメリット

| メリット | デメリット |
|---|---|
| 再ビルドせずにExcelを差し替えるだけで文言更新・追加言語対応ができる | Excel読み込み用ライブラリ（EPPlus等）の追加インストールが必要 |
| 翻訳担当者がExcelを直接編集すればそのまま反映される | Excelファイルが誤って削除・移動されると起動時にエラーになるリスクがある |
| | `.resx`方式に比べ起動時の処理が若干重くなる（Excel読み込みのオーバーヘッド） |

---

## 3方式の比較まとめ

| 項目 | 方式1: Resources.resx | 方式2: Excel→resx変換 | 方式3: Excel直接読込 |
|---|---|---|---|
| 文言の管理場所 | Visual Studioの`.resx`エディタ | Excel（マスター）→`.resx`（実行用） | Excelファイルそのもの |
| 再ビルドの要否（文言修正時） | 必要 | 必要 | 不要（Excel差し替えのみ） |
| 追加ライブラリ | 不要 | 不要 | 必要（EPPlus等） |
| 実行時の安定性・速度 | 高い | 高い（方式1と同等） | Excel読み込み分のオーバーヘッドあり、ファイル欠損リスクあり |
| 向いているケース | 文言数が少〜中規模、開発者が直接管理 | 翻訳者がExcelで管理し、開発側で反映 | 頻繁な文言更新を非開発者が行いたい、再ビルド不可な運用 |

---

## 補足：`.resx`の命名規則

`.resx`ファイルは「ベース名 + カルチャコード + `.resx`」という命名規則でカルチャを識別します。

```
Strings.resx      → 既定（Neutral）。設定したカルチャに該当ファイルがない場合のフォールバック先
Strings.ja.resx   → 日本語 (ja)
Strings.en.resx   → 英語 (en)。通常は作らず、Strings.resx を英語の既定として使うことが多い
```

今回の実装では `Strings.resx` に英語を書き、`Strings.ja.resx` に日本語を書く、という運用にしています。
