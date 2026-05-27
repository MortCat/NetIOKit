namespace NetIOKit.Core;

/// <summary>
/// NetIOKit v1 對外高階 API 預設訊息格式。
///
/// Frame 二進位格式（由內部處理，使用者無需自行組包）：
/// 1) Header 4 bytes: PayloadLength (Int32, Little-Endian)
/// 2) Payload N bytes: UTF-8 字串內容
///
/// 範例：字串 "PING"
/// - UTF-8 payload: 50 49 4E 47 (4 bytes)
/// - Header: 04 00 00 00
/// - 完整封包: 04 00 00 00 50 49 4E 47
/// </summary>
public static class NetIOMessageFormat
{
    public const string Name = "LengthPrefixedUtf8V1";
}
