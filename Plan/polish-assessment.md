# 視覺與音效完成度評估

評估日期：2026-03-22

---

## 總覽

| 面向 | 完成度 | 備註 |
|------|--------|------|
| 視覺一致性與後處理 | 20% | URP 框架已有，缺後處理特效與中央管理 |
| 粒子系統 | 0% | 完全未實現 |
| UI/UX 動畫與 Tweening | 85% | DOTween 廣泛應用，運行良好 |
| Screen Shake / Hit Stop | 0% | 卡牌遊戲低優先，可暫跳過 |
| 音效與音樂 | 0% | 完全未實現，優先度高 |

---

## 1. 視覺一致性與後處理 — 20%

### 已有
- URP 已啟用（`UniversalRenderPipelineGlobalSettings.asset`、`Settings/Renderer2D.asset`）
- 自訂字體：`Assets/Fonts/Cubic_11.ttf` + SDF 版本，全 UI 使用 TextMeshPro
- 7 個 UI Prefab：Customer、OpenScroll、Relics、Rune、Scroll、Slot

### 缺少
- 無 Post-Processing Volume（無 Bloom、Color Grading、SSAO）
- 無後處理控制腳本
- 無統一的 UI 主題/色彩管理系統

---

## 2. 粒子系統 — 0%

### 已有
- 無

### 缺少
- 動作粒子（放入符文、卷軸展開、需求滿足等反饋特效）
- 無任何 ParticleSystem prefab 或 VFX 資產

---

## 3. UI/UX 動畫與 Tweening — 85%

### 已有（DOTween 廣泛使用）

| 腳本 | 動畫內容 |
|------|---------|
| `UI/OpenDeskUI.cs` | DOAnchorPosY（Desk 面板滑入/出） |
| `UI/FinalResult/FinalResult.cs` | DOScale、DOFade（結束畫面） |
| `UI/Enviroments/UpdateBrightness.cs` | DOColor、DOTween.To（數值遞增 + 閃爍） |
| `UI/Enviroments/UpdateTemperature.cs` | 同上 |
| `UI/Enviroments/UpdateMoisture.cs` | 同上 |
| `UI/Enviroments/UpdateRevenue.cs` | 同上 |
| `UI/RuneUI/RuneCardUI.cs` | DOScale（選中高亮） |
| `UI/ScrollsUI/OpenScrollUI.cs` | DOScale、DOKill（面板展開/收縮） |
| `UI/ScrollsUI/SlotUI.cs` | DOPunchScale、DOShakePosition（放入/退出） |
| `UI/ScrollsUI/ScrollCardUI.cs` | DOScale（卷軸選中高亮） |

均使用 Ease 函數（OutCubic、OutBack、OutQuad 等），有 `.SetLink(gameObject)` 防洩漏。

### 缺少
- 各腳本硬碼動畫時間/easing，無中央配置
- 無按鈕 hover 音效配合動畫

---

## 4. Screen Shake / Hit Stop / Input Buffer / Coyote Time — 0%

本專案為卡牌回合制，非動作遊戲。這些機制**低優先**，可暫不實現。
若未來有需要（例如遺物懲罰的衝擊演出），再評估加入 Screen Shake。

---

## 5. 音效與音樂 — 0%

### 已有
- 無

### 缺少
- 無 AudioManager
- 無任何 `.mp3` / `.wav` 音頻資產
- 無 `Assets/Audio/` 目錄
- 缺少項目：
  - 背景音樂（BGM）
  - UI 音效（按鈕點擊、確認、取消）
  - 動作音效（放入符文、送出卷軸、顧客滿意/離場）
  - 環境底噪

---

## 建議優先順序

1. **音效系統（高）** — 建立 AudioManager，加入 UI 音效與 BGM，對遊戲體驗影響最大
2. **後處理（中）** — 在 URP 框架上加 Volume，加 Bloom + Color Grading 即可有明顯提升
3. **粒子特效（中）** — 至少為「需求滿足」「符文放入」加入簡單粒子反饋
4. **動畫中央配置（低）** — DOTween 已運作良好，可之後重構
