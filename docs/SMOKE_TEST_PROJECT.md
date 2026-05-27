# NetIOKit Smoke Test Project

這是一個**最簡單、最直觀**的通訊對接測試專案：`NetIOKit.SmokeTest`。

## 它做什麼
1. 啟動本機 `NetIOServerFacade`
2. 啟動 `NetIOClientFacade`
3. Client 傳送 `PING`
4. Server 回覆 `ACK:PING`
5. Console 印出 `PASS/FAIL`

## 執行
```bash
dotnet run --project NetIOKit.SmokeTest
```

## 為什麼需要它
- 給整合方快速驗證「函式庫是否可直接對接」
- 不需要理解封包頭、長度、拆包等細節
- 可作為 CI 前置 smoke check
