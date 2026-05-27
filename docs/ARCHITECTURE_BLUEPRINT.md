# NetIOKit v0 架構藍圖

> 目標：在 C# 建立可擴充通訊核心，支援 Client/Server、高吞吐封包處理、策略化行為，並可演進為多語言一致實作。

## 1. 設計原則
1. **可靠優先**：先正確、再極致效能。
2. **分層解耦**：傳輸、會話、協定、策略、觀測各自獨立。
3. **可替換性**：協定與策略以插件方式掛載。
4. **跨語言一致性**：規格先行，實作後行。
5. **可觀測性內建**：每一層都可記錄與追蹤。

## 2. 邏輯分層

### 2.1 Transport Layer
責任：
- 管理 Socket/TCP(/TLS) 建連與收發。
- 提供 async I/O、buffer 管理、基本流控。

輸出：
- 位元組流（byte stream）事件給 Session Layer。

### 2.2 Session Layer
責任：
- 連線生命週期（Connect/Disconnect/Reconnect）。
- 心跳、超時、握手、會話狀態機。
- 端點角色統一（Client/Server 共用介面）。

輸出：
- 會話事件與 frame 交給 Protocol Layer。

### 2.3 Protocol Layer
責任：
- Frame 邊界處理（拆包/黏包/序列化/反序列化）。
- 協定狀態與語意驗證。
- 適配 SECS/GEM、Ethernet I/O、自訂協定。

輸出：
- 強型別訊息（domain message）給 Pipeline Layer。

### 2.4 Pipeline & Strategy Layer
責任：
- 訊息路由、佇列、背壓、優先級、重試策略。
- 請求/回應關聯（Correlation ID）、重送控制。

輸出：
- 應用層可消費事件/回調。

### 2.5 Observability Layer
責任：
- 結構化日誌、metrics、tracing、審計事件。
- 提供故障分析與性能瓶頸定位依據。

## 3. 核心抽象（語言無關概念）
- `ITransport`：open/send/receive/close。
- `ISession`：狀態管理、心跳與重連。
- `IProtocolCodec<TFrame, TMessage>`：編解碼與驗證。
- `IPipelineStrategy`：排程、重試、限流、背壓。
- `IEndpoint`：Client/Server 統一入口。

## 4. Client / Server 統一模型
- 同一套 session 與 protocol 契約，僅在角色行為上切分。
- 可透過設定選擇模式：
  - Active Dialer（Client）
  - Passive Listener（Server）
- 雙方共享：超時規則、錯誤碼、重連策略（Server 可關閉重連）。

## 5. 封包處理流程（高階）
1. Transport 收到 byte stream。
2. Session 檢查連線狀態與 timeout。
3. Protocol 完成 framing 與 decode。
4. Pipeline 套用策略（排程/背壓/重試）。
5. 交付應用層 handler。
6. 若需回應，反向 encode → send。

## 6. 錯誤模型與復原策略
- 錯誤分類：
  - TransportError（連線中斷、socket 異常）
  - SessionError（握手失敗、心跳逾時）
  - ProtocolError（格式錯誤、語意違規）
  - StrategyError（佇列滿載、重試耗盡）
- 復原：
  - 可重試錯誤：指數退避 + jitter
  - 不可重試錯誤：快速失敗 + 可觀測告警

## 7. 效能與容量設計要點
- 使用非阻塞 async I/O。
- 採用可重用 buffer pool，降低 GC 壓力。
- Pipeline 具背壓機制，防止記憶體暴衝。
- 可調整 worker 數量與 queue 深度，支援不同負載型態。

## 8. 多語言擴展策略
1. 先固化規格：
   - 協定描述（frame/message/schema）
   - 狀態機定義
   - 錯誤碼與恢復規則
2. 建立 conformance tests：
   - golden packets
   - state transition tests
   - fault injection tests
3. C# 為 reference implementation，其他語言對齊同一測試集。

## 9. 文件結構建議
- `docs/PROJECT_MEMORY.md`：專案記憶與決策歷程
- `docs/ARCHITECTURE_BLUEPRINT.md`：架構藍圖
- `docs/PROTOCOLS.md`：協定細節與狀態機
- `docs/ERROR_CODES.md`：錯誤碼字典
- `docs/TEST_MATRIX.md`：壓力/故障/相容性測試矩陣
- `docs/AI_AGENT_CONTEXT.md`：給 AI Agent 的快速上下文

## 10. 下一步（可直接落地）
1. 在 C# 建立 `Abstractions`、`Core`、`Protocols`、`Strategies` 模組骨架。
2. 先完成最小 TCP + framing codec + reconnect demo。
3. 加入基礎 metrics（吞吐、延遲、失敗率）與壓測腳本。
4. 補 `PROTOCOLS.md`（先以一種自訂 framing 協定示範）。
