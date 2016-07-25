using System.ComponentModel;
using System.ComponentModel.Composition;
using NationalInstruments.RuntimeSerialization;
using FanControl;
using NationalInstruments.Core;

namespace FanDemo
{
    // This is necessary for the parser to understand the 'FanSpeed' enum datatype, which is persisted.
    // The EnumSerializer generic type is a useful type to derive a converter from for custom enum types.
    // If you do this, then the following is all that is needed for the parser to recognize your enum type.
    [ExportTypeConverter(typeof(FanSpeed), "FanSpeed")]
    public class FanSpeedConverter : EnumSerializer<FanSpeed>
    {
    }
}
