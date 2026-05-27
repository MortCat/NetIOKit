# NetIOKit 專案記憶檔 (Project Memory)

> 目的：作為跨對話、跨功能迭代的長期記錄。每次新增功能、修正、決策、風險與摘要都應更新本檔，讓人類與 AI Agent 都能快速延續脈絡。

## 1) 專案定位
- 專案名稱：**NetIOKit**
- 目標：打造通訊函式庫，提供**穩定、嚴謹、高吞吐、高速資料處理**能力。
- 使用場景：同時支援 **Client / Server** 角色。
- 首發語言：**C#**。
- 長期目標：擴展到多語言版本（Python、C/C++、Java、Rust...）。

## 2) 範圍與願景
- 支援多種通訊/協定情境：
  - TCP/IP（含複雜封包格式與狀態控制）
  - Ethernet I/O（常見工控場域）
  - SECS/GEM（Client/Server）
- 支援策略化通訊能力：重試、重連、心跳、超時、背壓、流控、優先級等。

## 3) 架構方向（摘要）
- Core Engine + Protocol Adapters 模式。
- 分層建議：
  1. Transport Layer
  2. Session Layer
  3. Protocol Layer
  4. Pipeline & Strategy Layer
  5. Observability Layer
- 詳細版本請參考：`docs/ARCHITECTURE_BLUEPRINT.md`。

## 4) 多語言策略（摘要）
- 先穩定 C# 版作為 reference implementation。
- 以「語言無關規格 + conformance tests」保證跨語言一致。
- 避免各語言獨立演化造成行為偏移。

## 5) 維運與更新規範（給人類與 AI）
每次變更請至少更新以下欄位：
1. **Change Log**：這次做了什麼。
2. **Decision Log**：關鍵設計決策與原因。
3. **Open Questions**：待確認議題。
4. **Next Actions**：下一步執行項。

---

## Change Log
### 2026-05-27
- 新增 `docs/GO_NO_GO_CHECKLIST.md`（試點上線決策清單）。
- 針對 unified HSMS codec/session 做可讀性重構與註解補強（不變更外部契約）。
- 新增 HSMS T6/T7 timers 最小骨架與 timeout 契約測試。
- 新增 HSMS control/data unified multiplexing skeleton 與精簡整合測試。
- 新增 HSMS data message 最小通道與 control/data 共存整合測試。
- 新增 HSMS state machine 接到 session 管線（`NetIOHsmsSession`）與精簡整合測試。
- 新增 HSMS 最小 state machine（Select/Linktest lifecycle）與精簡測試。
- 新增 HSMS control message 最小子集（Select/Linktest）骨架與最小整合測試。
- 新增 HSMS/SECS-II item model 最小子集（ASCII/U4）與編解碼契約測試。
- 新增 SECS/GEM-style 協定 adapter 最小骨架（`SecsGemStyleCodec`）與契約測試。
- 新增 `NetIOKit.SmokeTest` 簡易對接測試專案（PING/ACK 一鍵驗證）。
- 新增高階 API v1 版本化相容策略與契約測試（防止未來破壞性擴充）。
- 新增高階 API v1（Client/Server Facade）與對應 integration tests。
- 補強 SessionRunner 邊界一致性 integration tests（斷線、取消、重連競態）。
- 新增最小 `NetIOKit.Demo` client/server round-trip，並補 `MinimalClientServerDemoTests` integration tests。
- 新增 `docs/CORE_SCOPE_V1.md`，正式定義 Core 主線與 Optional Tools 邊界。
- 進入 Step C.6：新增 `NetIOKit.Benchmarks` 第一版 harness，提供吞吐與 P50/P95/P99 輸出。
- 進入 Step C.5：補強 InMemory metrics（snapshot/reset/messages-per-second）與對應測試。
- 進入 Step C：新增 SessionRunner metrics 介面與 InMemory 指標實作，補 metrics 測試。
- 進入 Step B：新增重連策略（Exponential Backoff + jitter）與 SessionRunner 重連控制。
- 進入 Step A：新增 `TcpTransport`、`SessionRunner`、loopback/integration 測試。
- 新增 FakeTransport + Integration Tests（fragmentation/sticky/斷線重連模擬）。
- 新增 `SessionBase` 並行/釋放行為測試（connect/disconnect/dispose）。
- 建立初版「專案記憶檔」。
- 記錄使用者提出的核心需求、語言路線與協定方向。
- 建立架構藍圖文件並在本檔建立交叉引用。
- 建立第一版 C# 架構骨架（Abstractions/Core/Protocols/Strategies）。
- 新增 `LengthPrefixedPacketParser` 作為最小可行 framing codec。
- 新增 `ProtocolReadBuffer<TMessage>` 支援分段接收與累積解包。
- 新增共用錯誤碼與 `NetIOException`。
- 新增 `docs/PROTOCOLS.md`、`docs/ERROR_CODES.md`、`docs/TEST_MATRIX.md`。
- 新增 `NetIOKit.Tests` 測試專案（xUnit），覆蓋 framing round-trip、partial header、invalid length、multi-packet buffer。

## Decision Log
### 2026-05-27
- 新增 `docs/GO_NO_GO_CHECKLIST.md`（試點上線決策清單）。
- 針對 unified HSMS codec/session 做可讀性重構與註解補強（不變更外部契約）。
- 新增 HSMS T6/T7 timers 最小骨架與 timeout 契約測試。
- 新增 HSMS control/data unified multiplexing skeleton 與精簡整合測試。
- 新增 HSMS data message 最小通道與 control/data 共存整合測試。
- 新增 HSMS state machine 接到 session 管線（`NetIOHsmsSession`）與精簡整合測試。
- 新增 HSMS 最小 state machine（Select/Linktest lifecycle）與精簡測試。
- 新增 HSMS control message 最小子集（Select/Linktest）骨架與最小整合測試。
- 新增 HSMS/SECS-II item model 最小子集（ASCII/U4）與編解碼契約測試。
- 新增 SECS/GEM-style 協定 adapter 最小骨架（`SecsGemStyleCodec`）與契約測試。
- 新增 `NetIOKit.SmokeTest` 簡易對接測試專案（PING/ACK 一鍵驗證）。
- 新增高階 API v1 版本化相容策略與契約測試（防止未來破壞性擴充）。
- 新增高階 API v1（Client/Server Facade）與對應 integration tests。
- 補強 SessionRunner 邊界一致性 integration tests（斷線、取消、重連競態）。
- 新增最小 `NetIOKit.Demo` client/server round-trip，並補 `MinimalClientServerDemoTests` integration tests。
- 新增 `docs/CORE_SCOPE_V1.md`，正式定義 Core 主線與 Optional Tools 邊界。
- 進入 Step C.5：補強 InMemory metrics（snapshot/reset/messages-per-second）與對應測試。
- 進入 Step C：新增 SessionRunner metrics 介面與 InMemory 指標實作，補 metrics 測試。
- 進入 Step B：新增重連策略（Exponential Backoff + jitter）與 SessionRunner 重連控制。
- 進入 Step A：新增 `TcpTransport`、`SessionRunner`、loopback/integration 測試。
- 決策：先產出架構藍圖，再進入程式實作。
- 原因：先確立邊界與契約，可降低後續重工，並有利多語言擴展。
- 決策：以「長度前綴（Int32 Little-Endian）」作為 v0 範例 framing。
- 原因：易於測試、可明確處理拆包/黏包，適合作為後續協定擴展基底。

## Open Questions
- SECS/GEM 將優先以哪個實際設備/場景做驗證？
- Ethernet I/O 的首批供應商/設備型號是哪些？
- 性能目標（吞吐量、延遲、連線數）是否已有具體 KPI？

## Next Actions
1. 依 `GO_NO_GO_CHECKLIST.md` 完成試點前全項打勾與缺口修補。
2. 將 SECS-II item model 擴充到清單/巢狀結構（List + 基本型別族）。
3. 擴充 HSMS：將 T6/T7 與 unified session/reconnect 策略整合。
4. 擴充 soak 模式：加入 reconnect churn/失敗注入。
