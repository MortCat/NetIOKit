# NetIOKit High-Level API v1 (Client/Server Facade)

## 目標
讓使用者只處理「訊息內容」，不需要關心封包標頭、長度、拆包/黏包細節。

## 對外類型
- `NetIOClientFacade`
- `NetIOServerFacade`
- `NetIOMessageFormat`（格式說明常數）

## 預設格式（內部）
- 格式名稱：`LengthPrefixedUtf8V1`
- Header：4 bytes `Int32` little-endian payload length
- Payload：UTF-8 字串位元組

## 使用範例（概念）
1. Server 註冊 `Func<string, CancellationToken, ValueTask<string>>` 回覆邏輯。
2. Client 註冊 `Func<string, CancellationToken, ValueTask>` 接收邏輯。
3. Client `SendTextAsync("PING")`。
4. Server 收到後回覆 `ACK:PING`。

## 設計原則
- 封包格式由函式庫內部保證一致。
- 對外 API 保持簡潔，避免暴露 framing 細節。
- 若後續支援 SECS/GEM 等複雜協定，仍維持同樣高階抽象使用體驗。
