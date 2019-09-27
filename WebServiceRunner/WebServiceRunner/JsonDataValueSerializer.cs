using System;
using System.Collections.Generic;
using System.Globalization;
using System.Json;
using System.Linq;
using NationalInstruments.Core;
using NationalInstruments.DataTypes;
using NationalInstruments.DataValues;

namespace NationalInstruments.WebServiceRunner
{
    /// <summary>
    /// Serializes values based on data types into the standardized JSON format.
    /// This serializer handles LabVIEW specific types that do not have a standard representation.
    /// </summary>
    public static class JsonDataValueSerializer
    {
        /// <summary>
        /// Serializes a value into the corresponding JavaScript model representation.
        /// </summary>
        /// <param name="value">The value to serialize, in the format of the C# model value or DataItem value</param>
        /// <returns>The corresponding JavaScript representation.</returns>
        public static JsonValue SerializeValue(object value)
        {
            if (value is Array array)
            {
                if (array.GetElementType() is ICluster)
                {
                    return SerializeArray1DCluster(value);
                }
                else
                {
                    return SerializeArray(array);
                }
            }
            if (value is bool b)
            {
                return SerializeBoolean(b);
            }
            if (IsJsonNumber(value))
            {
                return SerializeNumeric(value);
            }
            if (value is NIPath path)
            {
                return SerializePath(path);
            }
            if (value is string s)
            {
                return SerializeString(s);
            }
            if (value is PrecisionDateTime time)
            {
                return SerializePrecisionDateTime(time);
            }
            if (value is ICluster cluster)
            {
                return SerializeCluster(cluster);
            }
            if (value is TagRefnum refNum)
            {
                return SerializeString(refNum.Value);
            }
            if (value is ITimedWaveform waveform)
            {
                return SerializeWaveform(waveform);
            }
            return new JsonPrimitive("unknown");
        }

        /// <summary>
        /// Returns whether the input value can fit in a JSON number type
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>true if the number can be represented as a double without losing information</returns>
        public static bool IsJsonNumber(object value)
        {
            return (value is double) ||
                (value is float) ||
                (value is sbyte) ||
                (value is short) ||
                (value is int) ||
                (value is byte) ||
                (value is ushort) ||
                (value is uint);
        }

        /// <summary>
        /// Serializes arrays
        /// </summary>
        /// <param name="array">The array to serialize</param>
        /// <returns>A serialized array</returns>
        public static JsonValue SerializeArray(Array array)
        {
            if (array == null)
            {
                return new JsonArray();
            }
            var dimensions = array.Rank;
            int[] indices = new int[dimensions];
            for (int i = 0; i < dimensions; i++)
            {
                indices[i] = 0;
            }
            JsonValue[] jsonArray = new JsonValue[array.GetLength(0)];
            int dimension = 0;
            for (int i = 0; i < array.GetLength(0); i++)
            {
                indices[0] = i;
                jsonArray[i] = SerializeSubArray(array, indices, dimension + 1);
            }

            return new JsonArray(jsonArray);
        }

        /// <summary>
        /// Serializes the C# model representation of a 1D array of clusters.
        /// </summary>
        /// <param name="value">The C# representation of a 1D array of clusters (either a ClusterFieldCollection, or an ICluster[])</param>
        /// <returns>The corresponding JavaScript representation. Will be a <see cref="JsonValue"/> containing an array of objects.</returns>
        public static JsonValue SerializeArray1DCluster(object value)
        {
            Array arrayValue;
            var clusterFieldCollection = value as ClusterFieldCollection;
            if (clusterFieldCollection != null)
            {
                arrayValue = clusterFieldCollection.ToArray();
            }
            else
            {
                arrayValue = value as Array;
            }
            return SerializeArray(arrayValue);
        }

        /// <summary>
        /// Serializes a boolean into the corresponding JavaScript model representation.
        /// </summary>
        /// <param name="value">The C# representation of a boolean value</param>
        /// <returns>The corresponding JavaScript representation. Will be a <see cref="JsonValue"/> containing a boolean.</returns>
        public static JsonValue SerializeBoolean(bool value)
        {
            return new JsonPrimitive(value);
        }

        /// <summary>
        /// Serializes the C# model representation of a cluster
        /// </summary>
        /// <param name="cluster">The cluster value to serialize</param>
        /// <returns>The corresponding JavaScript representation. Will be a <see cref="JsonObject"/> with a field for each cluster field.</returns>
        public static JsonValue SerializeCluster(ICluster cluster)
        {
            var value = new JsonObject();
            for (int i = 0; i < cluster.Count; i++)
            {
                string fieldName = cluster.Names[i];
                value[fieldName] = SerializeValue(cluster[i]);
            }
            return value;
        }

        /// <summary>
        /// Serializes the C# model representation of color into the corresponding JavaScript model representation.
        /// </summary>
        /// <param name="color">The C# representation of a <see cref="SMColor"/> value</param>
        /// <returns>The corresponding JavaScript representation. Will be a <see cref="JsonValue"/> containing a string, which is formatted as a CSS color (#RRGGBB).</returns>
        public static JsonValue SerializeColor(SMColor color)
        {
            return SerializeString(color.ToCssColor());
        }

        /// <summary>
        /// Serializes the C# model representation of complex into the corresponding JavaScript model representation.
        /// </summary>
        /// <param name="value">The C# representation of a complex value</param>
        /// <returns>The corresponding JavaScript representation. Will be a <see cref="JsonValue"/> containing a string.</returns>
        public static JsonValue SerializeComplex(object value)
        {
            if (value == null)
            {
                return new JsonPrimitive("0+0i");
            }
            string realValue, imaginaryValue;
            if (value is ComplexSingle complexSingle)
            {
                realValue = ConvertToJSString(complexSingle.Real);
                imaginaryValue = ConvertToJSString(complexSingle.Imaginary);
            }
            else
            {
                var complexDouble = (ComplexDouble)value;
                realValue = ConvertToJSString(complexDouble.Real);
                imaginaryValue = ConvertToJSString(complexDouble.Imaginary);
            }
            string complexValue = realValue + ((imaginaryValue.StartsWith("-", StringComparison.Ordinal)) ? string.Empty : "+") + imaginaryValue + "i";
            return new JsonPrimitive(complexValue);
        }

        /// <summary>
        /// Serializes a double into the corresponding JavaScript model representation.
        /// </summary>
        /// <param name="value">The C# representation of a double value</param>
        /// <returns>The corresponding JavaScript representation. Will be a <see cref="JsonValue"/> containing a number.</returns>
        public static JsonValue SerializeDouble(double value)
        {
            string infNaNString = ConvertInfNaNToString(value);
            if (infNaNString != null)
            {
                return SerializeString(infNaNString);
            }

            return new JsonPrimitive(value);
        }

        /// <summary>
        /// Serializes the C# model representation of int into the corresponding JavaScript model representation.
        /// </summary>
        /// <param name="value">The C# representation of an int value</param>
        /// <returns>The corresponding JavaScript representation. Will be a <see cref="JsonValue"/> containing a number.</returns>
        public static JsonValue SerializeInt(int value)
        {
            return new JsonPrimitive(value);
        }

        /// <summary>
        /// Serializes a .NET numeric value into a <see cref="JsonValue"/>.
        /// </summary>
        /// <param name="value">The .NET numeric value to serialize</param>
        /// <returns>A <see cref="JsonValue"/> wrapping a numeric if the value is not an Int64 or UInt64 or Complex.</returns>
        public static JsonValue SerializeNumeric(object value)
        {
            if (value is long || value is ulong)
            {
                return SerializeString(value.ToString());
            }
            if (value is ComplexSingle || value is ComplexDouble)
            {
                return SerializeComplex(value);
            }
            if (IsJsonNumber(value))
            {
                var doubleValue = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                return SerializeDouble(doubleValue);
            }

            throw new ArgumentException($"The value parameter has an invalid type {value.GetType()}");
        }

        /// <summary>
        /// Serializes the C# model representation of date time into the corresponding JavaScript model representation.
        /// </summary>
        /// <param name="value">The C# representation of a <see cref="PrecisionDateTime"/> value</param>
        /// <returns>The corresponding JavaScript representation. Will be a <see cref="JsonValue"/> containing a string: "seconds since Jan 1 1904:fractional seconds" (both int64s).</returns>
        public static JsonValue SerializePrecisionDateTime(PrecisionDateTime value)
        {
            long valueSeconds;
            ulong fractionSeconds;

            value.ToLabViewTime(out valueSeconds, out fractionSeconds);
            return SerializeString(valueSeconds + ":" + fractionSeconds);
        }

        /// <summary>
        /// Serializes a string into the corresponding JavaScript model representation.
        /// </summary>
        /// <param name="value">The C# representation of a string value</param>
        /// <returns>The corresponding JavaScript representation. Will be a <see cref="JsonValue"/> containing a string.</returns>
        public static JsonValue SerializeString(string value)
        {
            if (value == null)
            {
                return new JsonPrimitive(string.Empty);
            }
            return new JsonPrimitive(value);
        }

        /// <summary>
        /// Creates a <see cref="JsonValue"/> representation of <paramref name="path"/>.
        /// Follows the format: {'components': [], 'type': 'absolute'}.
        /// </summary>
        /// <param name="path"><see cref="NIPath"/> to convert to JSON.</param>
        /// <returns>A valid <see cref="NIPath"/> JSON representation.</returns>
        public static JsonValue SerializePath(NIPath path)
        {
            if (path == null)
            {
                return new JsonPrimitive(string.Empty);
            }

            var pathObject = new JsonObject(
                new KeyValuePair<string, JsonValue>("components", new JsonArray()),
                new KeyValuePair<string, JsonValue>("type", new JsonPrimitive("notapath")));

            ((JsonArray)pathObject["components"]).AddRange(path.Components.Select(c => new JsonPrimitive(c)));
            if (path.PathType == NIPathType.Relative)
            {
                pathObject["type"] = "relative";
            }
            else if (path.PathType == NIPathType.Absolute)
            {
                pathObject["type"] = "absolute";
            }
            return pathObject;
        }

        /// <summary>
        /// Serializes the C# model representation of an AnalogWaveform
        /// </summary>
        /// <param name="waveform">The waveform value to serialize</param>
        /// <returns>The corresponding JavaScript representation. Will be a <see cref="JsonObject"/> with a field for each waveform field.</returns>
        public static JsonValue SerializeWaveform(ITimedWaveform waveform)
        {
            JsonValue result = null;
            if (waveform is AnalogWaveform<double>)
            {
                result = AnalogWaveformHelper<double>(waveform);
            }
            else if (waveform is AnalogWaveform<float>)
            {
                result = AnalogWaveformHelper<float>(waveform);
            }
            else if (waveform is AnalogWaveform<sbyte>)
            {
                result = AnalogWaveformHelper<sbyte>(waveform);
            }
            else if (waveform is AnalogWaveform<byte>)
            {
                result = AnalogWaveformHelper<byte>(waveform);
            }
            else if (waveform is AnalogWaveform<ushort>)
            {
                result = AnalogWaveformHelper<ushort>(waveform);
            }
            else if (waveform is AnalogWaveform<short>)
            {
                result = AnalogWaveformHelper<short>(waveform);
            }
            else if (waveform is AnalogWaveform<uint>)
            {
                result = AnalogWaveformHelper<uint>(waveform);
            }
            else if (waveform is AnalogWaveform<int>)
            {
                result = AnalogWaveformHelper<int>(waveform);
            }
            else if (waveform is ComplexWaveform<ComplexSingle>)
            {
                result = AnalogWaveformHelper<ComplexSingle>(waveform);
            }
            else if (waveform is ComplexWaveform<ComplexDouble>)
            {
                result = AnalogWaveformHelper<ComplexDouble>(waveform);
            }
            else
            {
                throw new ArgumentException($"Can't serialize this kind of analog waveform: {waveform.GetType()}", nameof(waveform));
            }

            return result;
        }

        private static JsonValue AnalogWaveformHelper<T>(ITimedWaveform waveform)
        {
            JsonObject value = new JsonObject();

            if (waveform is ComplexWaveform<T> complexData)
            {
                value[NITypeFactory.WaveformYFieldName] = SerializeArray(complexData.GetRawData());
            }
            else if (waveform is AnalogWaveform<T> analogData)
            {
                value[NITypeFactory.WaveformYFieldName] = SerializeArray(analogData.GetRawData());
            }
            if (waveform.PrecisionTiming != null)
            {
                value[NITypeFactory.WaveformT0FieldName] = SerializePrecisionDateTime(waveform.PrecisionTiming.StartTime);
                value[NITypeFactory.WaveformDTFieldName] = SerializeDouble(waveform.PrecisionTiming.SampleInterval.TotalSeconds);
            }

            if (!string.IsNullOrEmpty(waveform.ChannelName))
            {
                value["channelName"] = SerializeString(waveform.ChannelName);
            }

            return value;
        }

        private static string ConvertInfNaNToString(double value)
        {
            if (double.IsPositiveInfinity(value))
            {
                return "Infinity";
            }
            if (double.IsNegativeInfinity(value))
            {
                return "-Infinity";
            }
            if (double.IsNaN(value))
            {
                return "NaN";
            }

            return null;
        }

        private static string ConvertToJSString(double value)
        {
            string valueToSet = ConvertInfNaNToString(value);

            if (valueToSet == null)
            {
                // "R" means use compact representation if possible, but use up to 17 digits if necessary
                // https://msdn.microsoft.com/en-us/library/kfsatb94(v=vs.110).aspx
                valueToSet = value.ToString("R", CultureInfo.InvariantCulture);
            }

            return valueToSet;
        }

        private static JsonValue SerializeSubArray(Array array, int[] indices, int dimension)
        {
            if (dimension < indices.GetLength())
            {
                JsonValue[] jsonArray = new JsonValue[array.GetLength(dimension)];
                for (int i = 0; i < array.GetLength(dimension); i++)
                {
                    indices[dimension] = i;
                    jsonArray[i] = SerializeSubArray(array, indices, dimension + 1);
                }

                return new JsonArray(jsonArray);
            }
            else
            {
                return SerializeValue(array.GetValue(indices));
            }
        }
    }
}