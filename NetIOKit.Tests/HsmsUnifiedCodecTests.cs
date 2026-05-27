using NetIOKit.Protocols;

namespace NetIOKit.Tests;

public sealed class HsmsUnifiedCodecTests
{
    [Fact]
    public void EncodeDecode_ControlAndData_RoundTrip()
    {
        var codec = new HsmsUnifiedCodec();

        var control = HsmsUnifiedFrame.FromControl(new HsmsControlMessage(HsmsControlMessageType.SelectRequest, 1, 10));
        var data = HsmsUnifiedFrame.FromData(new HsmsDataMessage(1, 11, 2, 3, false, new byte[] { 0xAA }));

        var cBytes = codec.Encode(control);
        var dBytes = codec.Encode(data);

        Assert.True(codec.TryDecode(cBytes, out var cDecoded, out var cConsumed));
        Assert.True(codec.TryDecode(dBytes, out var dDecoded, out var dConsumed));

        Assert.Equal(cBytes.Length, cConsumed);
        Assert.Equal(dBytes.Length, dConsumed);
        Assert.Equal(HsmsFrameKind.Control, cDecoded.Kind);
        Assert.Equal(HsmsFrameKind.Data, dDecoded.Kind);
    }
}
