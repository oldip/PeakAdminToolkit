# 貢獻指南

專案尚未自動發布至 GitHub。英文指南在根目錄 CONTRIBUTING.md。

## 修改介面翻譯

每個語言有獨立 UTF-8 JSON 原始碼：

- locales/en.json：英文，也是訊息鍵名的基準。
- locales/zh-CN.json：簡體中文。
- locales/zh-TW.json：繁體中文。

只修改文字值，保留鍵名、{0}／{1} 參數與必要換行。不要加入重複鍵名或
空白翻譯。改善現有三種語言不需要修改 C#。更新版本時一併更新 Foundation。

只改翻譯時執行 `tests/run.ps1 -TranslationsOnly`；程式或版本修改則跑完整
`tests/run.ps1`。可檢查鍵名、重複內容、空白值、格式參數及 DLL
資源實際讀出的文字。另有缺漏、損壞 JSON 和錯誤格式的英文回退測試。

目前新增「第四種語言」還需要登錄語言代碼：Localization.Resolve、
Config 的可選值、MainWindow 的選單及顯示名稱、build.ps1 與
tests/run.ps1 的資源清單，並補上對應測試；不是只放檔案就自動啟用。

## 描述資料與打包

data/descriptions/ 保存使用者提供、來自另一模組的原始描述。
繁體物品描述由 Windows 將簡體轉換；不用加入字表。
若要更正描述，請附來源並另提修改，保留封存資料的原始雜湊。

翻譯和描述在建置時包入 DLL。玩家不需要額外放 JSON，也不能以 config
覆寫；原本 BepInEx 的快捷鍵、啟用及介面語言設定仍保留。

## 功能維護

功能 API 各自分檔，先測試 API 缺失或變更，再修改實作。生成 API
失效不能拖累目錄；單一物品失效不能讓其他物品消失。共用的游標與輸入
API 則是安全開啟介面的必要條件。

本機版本封存目錄不要修改已完成版本；GitHub 的獨立專案請用分支修改，
用 tag 保留發布版本。依序跑 tests/run.ps1、build.cmd、package.ps1；
source ZIP 不能包含遊戲、Unity、BepInEx 等 DLL 或測試輸出。
保留三份原始 TSV 雜湊，不複製舊 1.8.2 方法內容。

GitHub 開放後，可用 fork／分支提交 Pull Request，註明語言或功能範圍與檢查結果。
專案與介面翻譯採 MIT，貢獻也使用此授權。第三方描述已比對固定上游版本，
解析後內容相同，保留上游 MIT 聲明；見 THIRD_PARTY_NOTICES.md。
獨立原始碼測試需指定 `-ManagedDirectory` 與 `-BepInExCoreDirectory`；
只改翻譯可加 `-TranslationsOnly`。不得把本機參考 DLL 提交到 GitHub。
術語與解析度檢查方式見 locales/README.md；發布待辦見 docs/RELEASE_READINESS.md。
