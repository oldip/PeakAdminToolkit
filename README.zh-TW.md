# PEAK Admin Toolkit 0.8.1

[English](README.md) · [翻譯貢獻](docs/CONTRIBUTING.zh-TW.md) · [測試紀錄](TESTING.md)

PEAK 的 BepInEx 管理工具預覽版。F8 開啟物品卡片、自身工具、玩家與世界操作。
介面支援英文、簡體與繁體中文，依解析度縮放。

## 安裝

關閉 PEAK，將建置出的 `out/PeakAdminToolkit.dll` 放入 `BepInEx/plugins`。
只保留一個工具版本。一般使用只需要這個 DLL，描述與介面翻譯已內建。
自身工具預設關閉；關閉視窗仍保持開啟中的工具，停用模組或切換場景會恢復狀態。

## 權限與限制

- Host、Client 都可提出物品生成、復活與傳送請求；遠端結果須看對方畫面。
- 無敵、體力、飛行、免摔傷及清除負面狀態作用於自己的角色。
- 掉落物找回、時刻與全隊跳區需要 Host；跳區不會點燃營火。
- 無懲罰復活略過新增詛咒與飢餓，不清除既有狀態或補滿體力。
- 飛行保留碰撞；飛行期間及結束後兩秒自動免摔傷。
- 成為房主是嘗試操作，已有人測試失敗。

完整功能與 API 限制見 [英文說明](README.md) 及 [相容性](docs/COMPATIBILITY.md)。

## 建置與測試

需要 Windows、.NET SDK、自己安裝的 PEAK 與 BepInEx。不得上傳遊戲 DLL。

```powershell
.\tests\run.ps1 -ManagedDirectory '你的 PEAK_Data\Managed' -BepInExCoreDirectory '你的 BepInEx\core'
.\build.cmd -ManagedDirectory '你的 PEAK_Data\Managed' -ReferenceDirectory '你的 BepInEx\core'
.\package.ps1
.\tests\check-release.ps1
```

翻譯檔位於 `locales/`，修改後執行 `tests/run.ps1 -TranslationsOnly -ManagedDirectory ...`。
透過 Pull Request 貢獻翻譯，不需改 C# 的既有語言文字。

## 授權與發布狀態

專案採 MIT；第三方描述保留原作者 MIT 聲明，見 [來源聲明](THIRD_PARTY_NOTICES.md)。
0.8.1 是預覽版，尚有遊戲內回歸測試，不能視為 1.0.0 全面驗證。
GitHub 上傳方式及剩餘清單見 [發布指南](docs/GITHUB_RELEASE.md)。
