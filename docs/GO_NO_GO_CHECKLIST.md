# NetIOKit Go/No-Go 上線清單（試點版）

> 目的：在「小規模試點上線」前，快速判斷是否具備最低可用與可維運條件。

## A. API / 契約穩定性（必過）
- [ ] High-Level API v1 契約測試通過（`NetIOClientFacade` / `NetIOServerFacade`）。
- [ ] `NetIOMessageFormat.Name` 仍為 `LengthPrefixedUtf8V1`（未破壞既有整合）。
- [ ] 本次版本沒有刪除或改壞 v1 已公開方法簽名。

**Go 條件**：以上 3 項全部通過。任一失敗 => No-Go。

---

## B. 核心通訊可用性（必過）
- [ ] `NetIOKit.SmokeTest` 可穩定得到 `PING -> ACK:PING`。
- [ ] `MinimalClientServerDemo` 可在連續執行下穩定完成 round-trip。
- [ ] `SessionRunner` 在斷線/取消/重連邊界測試仍符合預期。

**Go 條件**：三項通過且無偶發非預期例外。否則 No-Go。

---

## C. HSMS/SECS 協定骨架完整性（必過）
- [ ] HSMS control message（Select/Linktest）編解碼測試通過。
- [ ] HSMS unified control/data multiplexing 測試通過。
- [ ] HSMS state machine + T6/T7 timeout 契約測試通過。
- [ ] SECS-II 最小 item（ASCII/U4）契約測試通過。

**Go 條件**：皆通過，可進入「試點」；若任一失敗則 No-Go。

---

## D. 觀測與故障可診斷性（試點建議必過）
- [ ] 至少啟用 InMemory metrics 並能讀取 bytes/messages/reconnect/failure。
- [ ] 發生超時（T6/T7）可辨識且錯誤訊息可追溯。
- [ ] 錯誤碼（`ErrorCodes`）與 `NetIOException` 映射未退化。

**Go 條件**：建議 3/3 通過；最低 2/3 + 有補救計畫。

---

## E. 執行與部署前檢查（必過）
- [ ] `dotnet build NetIOKit.sln` 通過。
- [ ] `dotnet test NetIOKit.Tests/NetIOKit.Tests.csproj` 通過。
- [ ] README/文件版本與實作一致（至少 `docs/PROTOCOLS.md`、`docs/HIGH_LEVEL_API_VERSIONING.md`）。

**Go 條件**：建置與測試必須全綠，否則 No-Go。

---

## F. 實設備試點（階段門檻）
- [ ] 至少 1 台設備完成基本握手與資料交握。
- [ ] 連續運行 30~60 分鐘無致命斷線循環。
- [ ] 出現錯誤時可由 log/metrics 快速定位。

**Go 條件**：達成後可進入擴大試點；未達成僅限內部驗證。

---

## 建議決策規則
- **Go**：A/B/C/E 全過，且 D、F 達最低門檻。
- **Conditional Go**：A/B/C/E 全過，但 D 或 F 有 1 項缺口且已建立修補排程。
- **No-Go**：A/B/C/E 任一未過。

---

## 本階段結論模板（可直接填）
- 決策日期：`YYYY-MM-DD`
- 決策：`Go / Conditional Go / No-Go`
- 未過項目：
  1. 
  2. 
- 補救計畫（Owner / ETA）：
  1. 
  2. 
