# 貢獻指南

[English](../CONTRIBUTING.md)

## 翻譯

介面文字位於 `locales/en.json`、`locales/zh-CN.json`、`locales/zh-TW.json`。
修改文字值，保留鍵名、格式參數與換行。

執行 `tests/run.ps1 -TranslationsOnly -ManagedDirectory <PEAK_Data/Managed>`。
術語與新增語言方式見 [locales/README.md](../locales/README.md)。

物品描述位於 `data/descriptions/`，繁體描述使用 Windows 簡繁轉換。
作者致謝與授權見 [THIRD_PARTY_NOTICES.md](../THIRD_PARTY_NOTICES.md)。

## 程式碼

功能 API 維持獨立模組，修改行為時加入對應測試。
指定本機 Managed／BepInEx 路徑，執行 tests/run.ps1 與 build.cmd。
透過 Pull Request 提交修改，說明改動與檢查結果。

貢獻使用專案的 MIT 授權。
