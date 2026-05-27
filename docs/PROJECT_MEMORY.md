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
- 建立初版「專案記憶檔」。
- 記錄使用者提出的核心需求、語言路線與協定方向。
- 建立架構藍圖文件並在本檔建立交叉引用。
- 建立第一版 C# 架構骨架（Abstractions/Core/Protocols/Strategies）。
- 新增 `LengthPrefixedPacketParser` 作為最小可行 framing codec。
- 新增 `ProtocolReadBuffer<TMessage>` 支援分段接收與累積解包。
- 新增共用錯誤碼與 `NetIOException`。
- 新增 `docs/PROTOCOLS.md`、`docs/ERROR_CODES.md`、`docs/TEST_MATRIX.md`。

## Decision Log
### 2026-05-27
- 決策：先產出架構藍圖，再進入程式實作。
- 原因：先確立邊界與契約，可降低後續重工，並有利多語言擴展。
- 決策：以「長度前綴（Int32 Little-Endian）」作為 v0 範例 framing。
- 原因：易於測試、可明確處理拆包/黏包，適合作為後續協定擴展基底。

## Open Questions
- SECS/GEM 將優先以哪個實際設備/場景做驗證？
- Ethernet I/O 的首批供應商/設備型號是哪些？
- 性能目標（吞吐量、延遲、連線數）是否已有具體 KPI？

## Next Actions
1. 加入最小 client/server demo 驗證 end-to-end 流程。
2. 將 `ProtocolReadBuffer<TMessage>` 連接到 session 收包流程。
3. 補單元測試專案（framing/partial frames/invalid length）。
4. 設計重連與退避策略介面（含 jitter）。
