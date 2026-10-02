# PEAK Admin Toolkit 0.8.1

[English](README.md) · [下載](https://github.com/oldip/PeakAdminToolkit/releases) · [翻譯貢獻](docs/CONTRIBUTING.zh-TW.md)

PEAK 的 BepInEx 管理工具。按 **F8** 開啟介面，視窗依解析度縮放。
支援英文、簡體與繁體中文。

## 功能

| 頁面 | 功能 |
|---|---|
| 概覽 | 目前語言與 Host／Client 狀態 |
| 物品 | 物品卡片、遊戲圖示、分類、多語言搜尋與生成 |
| 自身工具 | 無敵、無限體力、飛行、免摔傷、清除負面狀態 |
| 玩家 | 正常／無懲罰復活、雙向傳送、勾選掉落物找回 |
| 世界 | 當天時刻調整、全隊傳送至下一區營火旁或山頂 |

Host、Client 都可提出生成、復活與傳送請求；自身工具只作用於自己。
掉落物找回與世界操作需要 Host。

無懲罰復活略過新增詛咒與飢餓，不保證補滿體力或移除既有狀態。
飛行保留碰撞，飛行中及結束後兩秒免摔傷。全隊跳區不會點燃營火。
特殊變體、未使用物品與大廳玩具可在「其他」篩選中開啟。

## 安裝

1. 安裝 PEAK 的 BepInEx。
2. 從 Releases 下載 `PeakAdminToolkit-0.8.1.zip` 並解壓。
3. 關閉 PEAK，將 `PeakAdminToolkit.dll` 放入 `BepInEx/plugins`。
4. 啟動遊戲，按 F8。只保留一個工具版本。

描述與介面翻譯已內建，執行時只需要工具 DLL。
自身工具預設關閉；關閉視窗仍保持開啟中的工具，停用模組或切換場景會重設。

## 設定

| 設定 | 選項 |
|---|---|
| General.Enabled | 啟用／停用工具 |
| Interface.ToggleKey | 預設 F8；None 停用快捷鍵 |
| Interface.Language | Auto、en、zh-CN、zh-TW；Auto 跟隨 PEAK |

## 建置

需要 Windows、.NET SDK，以及本機 PEAK／BepInEx 參考檔。

```powershell
.\tests\run.ps1 -ManagedDirectory '<PEAK>\PEAK_Data\Managed' -BepInExCoreDirectory '<PEAK>\BepInEx\core'
.\build.cmd -ManagedDirectory '<PEAK>\PEAK_Data\Managed' -ReferenceDirectory '<PEAK>\BepInEx\core'
.\package.ps1
.\tests\check-release.ps1
```

建置結果位於 `out/`，翻譯原始碼位於 `locales/`。
測試結果見 [TESTING.md](TESTING.md)，模組與 API 說明見 [ARCHITECTURE.md](ARCHITECTURE.md)。

## 授權

[MIT](LICENSE)。物品描述致謝與授權見 [來源聲明](THIRD_PARTY_NOTICES.md)。
