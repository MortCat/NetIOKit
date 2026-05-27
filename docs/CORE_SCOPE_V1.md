# NetIOKit Core Scope v1

## 目標
定義 NetIOKit 的「核心範圍」與「非核心範圍」，確保函式庫可同時支援 Client/Server 端，並維持穩定度、效率與簡潔度。

## 核心範圍（Core Library Mainline）
以下能力屬於核心主線，必須長期維持 API 穩定與品質：

1. Transport 抽象與實作
   - `ITransport`
   - `TcpTransport`
   - `ITcpEndpoint` / `TcpEndpoint`

2. Session 生命週期與執行控制
   - `ISession`
   - `SessionBase`
   - `SessionRunner<TMessage>`
   - 取消、重連、例外映射（`NetIOException` + `ErrorCodes`）

3. Protocol 編解碼與緩衝
   - `IProtocolCodec<TMessage>`
   - `LengthPrefixedPacketParser`（作為 reference codec）
   - `ProtocolReadBuffer<TMessage>`（拆包/黏包）

4. 策略擴充點（最小）
   - `IPipelineStrategy<TMessage>`
   - `DefaultPipelineStrategy<TMessage>`
   - `IReconnectPolicy` / `ExponentialBackoffReconnectPolicy`

5. 最小可觀測性 Hook
   - `ISessionRunnerMetrics`
   - `NullSessionRunnerMetrics`
   - `InMemorySessionRunnerMetrics`

## 非核心範圍（Optional Tools / Sidecar）
以下屬於工具層，不應耦合到 Core API 設計：

1. 壓測/長跑工具
   - `NetIOKit.Benchmarks`
   - loopback throughput/latency harness

2. 報表輸出
   - JSON/CSV 匯出
   - 統計報告腳本

3. CI 環境特定流程
   - 壓測 pipeline orchestration
   - 外部觀測平台整合腳本

## 設計原則（避免複雜度外溢）
1. 使用者只需面對高階 API，不需理解封包標頭/長度/序號細節。
2. 協定複雜度（framing/重連/狀態控制）應封裝在函式庫內部。
3. 新功能若非核心必需，優先放到 sidecar 工具專案。
4. 每次新增功能需註明：
   - 是否影響 Core API
   - 是否增加使用者認知負擔
   - 是否有對應測試

## 近期執行順序（依優先）
1. 強化 Core 穩定性（錯誤分類、重連邊界、壓力下行為一致性）。
2. 完成最小 client/server demo（僅依賴 Core API）。
3. 逐步引入協定 adapter（如 SECS/GEM 變體）但維持對外 API 簡潔。
4. 工具層（benchmark/report）獨立演進，不反向污染 Core。
