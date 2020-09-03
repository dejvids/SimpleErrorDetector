using System.Collections;

namespace SimpleDetector.Models
{
    public interface ICodesIdentifier
    {
        BitArray Original { get; set; }
        BitArray Encoded { get; set; }
        BitArray Disturbed { get; set; }
        BitArray Decoded { get; set; }
    }
    public enum ErroCode
    {
        Ok,
        ErrorDetected,
        ErrorNotDetected,
        ErrorFixed,
        ControlBitFixed,
        ControlBit,
        ControllBitError,
        Null,
        Blank
    }
}
