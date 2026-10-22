using CSharpFunctionalExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Common.Serialisation
{
    public static class Binary
    {
        private static readonly object typeLock = new();
        private static readonly Dictionary<Type, PropertyInfo[]> typeProperties = [];

        // property
        // name¦Length¦Data
        // 0000¦0040¦00000000000000000000000000000000¦...

        // enumeration
        // Length¦Data
        // 0000¦00000000000000000000000000000000¦...

        public static byte[] Serialize<T>(T source) => Serialize(source.GetType(), source);
        public static byte[] Serialize(Type type, object source)
        {
            var bytes = Array.Empty<byte>();

            if (type.IsAssignableTo(typeof(IEnumerable)))
            {
                var enumerator = ((IEnumerable)source).GetEnumerator();

                while (enumerator.MoveNext())
                {
                    var buffer = Serialize(enumerator.Current);
                    var bufferLength = BitConverter.GetBytes(buffer.Length);

                    Array.Resize(ref bytes, bytes.Length + bufferLength.Length + buffer.Length);
                    Array.Copy(bufferLength, 0, bytes, bytes.Length - bufferLength.Length - buffer.Length, bufferLength.Length);
                    Array.Copy(buffer, 0, bytes, bytes.Length - buffer.Length, buffer.Length);
                }
            }
            else
            {
                var properties = GetReadableProperties(type);

                foreach (var property in properties)
                {
                    var buffer = PackProperty(property, source);

                    Array.Resize(ref bytes, bytes.Length + buffer.Length);
                    Array.Copy(buffer, 0, bytes, bytes.Length - buffer.Length, buffer.Length);
                }
            }

            return bytes;
        }

        public static T Deserialize<T>(byte[] source) => (T)Deserialize(typeof(T), source);
        public static object Deserialize(Type type, byte[] source)
        {
            if (source.IsNullOrEmpty()) return default;

            var instance = default(object);
            var span = source.AsSpan();
            var chunks = new List<byte[]>();
            int position = 0;

            if (type.IsAssignableTo(typeof(IEnumerable)))
            {
                var elementType = type;

                if (elementType.HasElementType)
                    elementType = elementType.GetElementType();
                else if (elementType.IsGenericType && elementType.GenericTypeArguments.Any())
                    elementType = elementType.GenericTypeArguments[0];

                var genericType = typeof(List<>).MakeGenericType(elementType);
                var elements = (IList)Activator.CreateInstance(genericType);

                while (position < source.Length)
                {
                    var length = BitConverter.ToInt32(span[position..(position + 4)]);
                    var chunk = source[(position + 4)..(position + 4 + length)];
                    var element = Deserialize(elementType, chunk);

                    elements.Add(element);

                    position = position + 4 + chunk.Length;
                }

                if (type.IsArray)
                {
                    instance = Array.CreateInstance(elementType, elements.Count);

                    elements.CopyTo((Array)instance, 0);
                }
                else if (type.IsAssignableTo(typeof(IList)))
                    instance = Convert.ChangeType(Activator.CreateInstance(elementType, elements), elementType);
                else if (type.IsAssignableTo(typeof(IEnumerable)))
                    instance = elements;
            }
            else
            {
                while (position < source.Length)
                {
                    var lengthStart = position + 4;
                    var lengthEnd = lengthStart + 4; // base 0
                    var length = BitConverter.ToInt32(span[lengthStart..lengthEnd]);

                    var chunk = source[position..(position + 4 + 4 + length)];

                    chunks.Add(chunk);

                    position = position + chunk.Length;
                }

                instance = Activator.CreateInstance(type);

                var properties = GetReadableProperties(type);

                foreach (var chunk in chunks)
                {
                    var hashCode = BitConverter.ToInt32(chunk[0..4]);
                    var property = properties.FirstOrDefault(p => p.Name.GetStableHashCode().Equals(hashCode));

                    if (property is null) continue;
                    if (!property.CanWrite) continue;

                    var length = BitConverter.ToInt32(chunk[4..8]);
                    var data = chunk[8..];

                    UnpackProperty(property, instance, ref data);
                }
            }

            return instance;
        }


        private static byte[] ReadSegment(ref byte[] source, ref int position, int length)
        {
            var buffer = new byte[length];

            Array.Copy(source, position, buffer, 0, length);

            position += length;

            return buffer;
        }

        private static byte[] PackProperty(PropertyInfo property, object source)
        {
            var key = BitConverter.GetBytes(property.Name.GetStableHashCode());
            var data = Array.Empty<byte>();
            var value = property.GetValue(source);

            if (value is not null)
            {
                if (property.PropertyType.IsValueType)
                {
                    Type underlyingEnumType = null;

                    if (property.PropertyType.IsEnum)
                        underlyingEnumType = Enum.GetUnderlyingType(property.PropertyType);

                    data = PackPropertyValue(value, underlyingEnumType);
                }
                else if (property.PropertyType.IsAssignableTo(typeof(string)))
                    data = System.Text.Encoding.UTF8.GetBytes((string)value);
                else if (property.PropertyType.IsAssignableTo(typeof(IEnumerable)))
                {
                    var elementType = property.PropertyType;

                    if (elementType.HasElementType)
                        elementType = elementType.GetElementType();
                    else if (elementType.IsGenericType && elementType.GenericTypeArguments.Any())
                        elementType = elementType.GenericTypeArguments[0];

                    var enumerator = ((IEnumerable)value).GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        var buffer = elementType.IsClass ? Serialize(elementType, enumerator.Current) : PackPropertyValue(enumerator.Current, null);
                        var bufferLength = BitConverter.GetBytes(buffer.Length);

                        Array.Resize(ref data, data.Length + bufferLength.Length + buffer.Length);
                        Array.Copy(bufferLength, 0, data, data.Length - bufferLength.Length - buffer.Length, bufferLength.Length);
                        Array.Copy(buffer, 0, data, data.Length - buffer.Length, buffer.Length);
                    }
                }
                else if (property.PropertyType.IsClass)
                    data = Serialize(property.PropertyType, value);
            }

            var dataLength = BitConverter.GetBytes(data.Length);

            Array.Resize(ref key, key.Length + dataLength.Length + data.Length);
            Array.Copy(dataLength, 0, key, key.Length - dataLength.Length - data.Length, dataLength.Length);
            Array.Copy(data, 0, key, key.Length - data.Length, data.Length);

            return key;
        }

        private static byte[] PackPropertyValue(object value, Type underlyingEnumType)
        {
            var data = Array.Empty<byte>();

            if (value is IEquatable<byte>)
                data = [(byte)value];
            else if (value is IEquatable<sbyte>)
                data = (byte[])(Array)new sbyte[] { (sbyte)value };
            else if (value is IEquatable<short>)
                data = BitConverter.GetBytes((short)value);
            else if (value is IEquatable<ushort>)
                data = BitConverter.GetBytes((ushort)value);
            else if (value is IEquatable<int>)
                data = BitConverter.GetBytes((int)value);
            else if (value is IEquatable<uint>)
                data = BitConverter.GetBytes((uint)value);
            else if (value is IEquatable<long>)
                data = BitConverter.GetBytes((long)value);
            else if (value is IEquatable<ulong>)
                data = BitConverter.GetBytes((ulong)value);
            else if (value is IEquatable<float>)
                data = BitConverter.GetBytes((float)value);
            else if (value is IEquatable<decimal>)
            {
                var bits = decimal.GetBits((decimal)value);
                var sign = (bits[3] & 0x80000000) != 0;
                var scale = (byte)(((bits[3] >> 16) & 0x7F) + (byte)(!sign ? 0x20 : 0x00));
                var bytes = new List<byte>();

                bytes.AddRange(BitConverter.GetBytes(bits[0]));
                bytes.AddRange(BitConverter.GetBytes(bits[1]));
                bytes.AddRange(BitConverter.GetBytes(bits[2]));
                bytes.Add(scale);

                data = bytes.ToArray();
            }
            else if (value is IEquatable<double>)
                data = BitConverter.GetBytes((double)value);
            else if (value is IEquatable<bool>)
                data = BitConverter.GetBytes((bool)value);
            else if (value is IEquatable<char>)
                data = BitConverter.GetBytes((char)value);
            else if (value is IEquatable<DateTime>)
                data = BitConverter.GetBytes(((DateTime)value).ToBinary());
            else if (value is IEquatable<TimeSpan>)
                data = BitConverter.GetBytes(((TimeSpan)value).Ticks);
            else if (value is IEquatable<Guid>)
                data = ((Guid)value).ToByteArray();
            else if (underlyingEnumType is not null)
            {
                if (underlyingEnumType.IsAssignableTo(typeof(IEquatable<byte>)))
                    data = [(byte)value];
                else if (underlyingEnumType.IsAssignableTo(typeof(IEquatable<sbyte>)))
                    data = [(byte)value];
                else if (underlyingEnumType.IsAssignableTo(typeof(IEquatable<short>)))
                    data = BitConverter.GetBytes((short)value);
                else if (underlyingEnumType.IsAssignableTo(typeof(IEquatable<ushort>)))
                    data = BitConverter.GetBytes((ushort)value);
                else if (underlyingEnumType.IsAssignableTo(typeof(IEquatable<int>)))
                    data = BitConverter.GetBytes((int)value);
                else if (underlyingEnumType.IsAssignableTo(typeof(IEquatable<uint>)))
                    data = BitConverter.GetBytes((uint)value);
                else if (underlyingEnumType.IsAssignableTo(typeof(IEquatable<long>)))
                    data = BitConverter.GetBytes((long)value);
                else if (underlyingEnumType.IsAssignableTo(typeof(IEquatable<ulong>)))
                    data = BitConverter.GetBytes((ulong)value);
            }

            return data;
        }

        private static void UnpackProperty(PropertyInfo property, object target, ref byte[] data)
        {
            object value = null;

            if (property.PropertyType.IsValueType)
            {
                Type underlyingEnumType = null;

                if (property.PropertyType.IsEnum)
                    underlyingEnumType = Enum.GetUnderlyingType(property.PropertyType);

                value = UnpackPropertyValue(property.PropertyType, ref data, underlyingEnumType);
            }
            else if (property.PropertyType.IsAssignableTo(typeof(string)))
                value = System.Text.Encoding.UTF8.GetString(data);
            else if (property.PropertyType.IsAssignableTo(typeof(IEnumerable)))
            {
                var cursor = 0;
                var elementType = property.PropertyType;

                if (elementType.HasElementType)
                    elementType = elementType.GetElementType();
                else if (elementType.IsGenericType && elementType.GenericTypeArguments.Any())
                    elementType = elementType.GenericTypeArguments[0];

                var genericType = typeof(List<>).MakeGenericType(elementType);
                var elements = (IList)Activator.CreateInstance(genericType);

                while (cursor < data.Length)
                {
                    var length = BitConverter.ToInt32(ReadSegment(ref data, ref cursor, 4));
                    var entry = ReadSegment(ref data, ref cursor, length);
                    var element = elementType.IsClass ? Deserialize(elementType, entry) : UnpackPropertyValue(elementType, ref entry, null);

                    elements.Add(element);
                }

                if (property.PropertyType.IsArray)
                {
                    value = Array.CreateInstance(elementType, elements.Count);

                    elements.CopyTo((Array)value, 0);
                }
                else if (property.PropertyType.IsAssignableTo(typeof(IList)))
                    value = Convert.ChangeType(Activator.CreateInstance(elementType, elements), elementType);
                else if (property.PropertyType.IsAssignableTo(typeof(IEnumerable)))
                    value = elements;
            }
            else if (property.PropertyType.IsClass)
                value = Deserialize(property.PropertyType, data);

            if (value is not null)
                property.SetValue(target, value);
        }

        private static object UnpackPropertyValue(Type type, ref byte[] data, Type underlyingEnumType)
        {
            object value = default;

            if (type.IsAssignableTo(typeof(IEquatable<byte>)))
                value = data[0];
            else if (type.IsAssignableTo(typeof(IEquatable<sbyte>)))
                value = (sbyte)data[0];
            else if (type.IsAssignableTo(typeof(IEquatable<short>)))
                value = BitConverter.ToInt16(data, 0);
            else if (type.IsAssignableTo(typeof(IEquatable<ushort>)))
                value = BitConverter.ToUInt16(data, 0);
            else if (type.IsAssignableTo(typeof(IEquatable<int>)))
                value = BitConverter.ToInt32(data, 0);
            else if (type.IsAssignableTo(typeof(IEquatable<uint>)))
                value = BitConverter.ToUInt32(data, 0);
            else if (type.IsAssignableTo(typeof(IEquatable<long>)))
                value = BitConverter.ToInt64(data, 0);
            else if (type.IsAssignableTo(typeof(IEquatable<ulong>)))
                value = BitConverter.ToUInt64(data, 0);
            else if (type.IsAssignableTo(typeof(IEquatable<float>)))
                value = BitConverter.ToSingle(data, 0);
            else if (type.IsAssignableTo(typeof(IEquatable<decimal>)))
            {
                // will be 13 as the conversion shorted the length from 16 by applied the sign flag;
                byte scale = data[12];
                bool sign = true;

                if (scale >= 0x20)
                {
                    sign = false;
                    scale -= 0x20;
                }

                value = new decimal(BitConverter.ToInt32(data, 0), BitConverter.ToInt32(data, 4), BitConverter.ToInt32(data, 8), sign, scale);
            }
            else if (type.IsAssignableTo(typeof(IEquatable<double>)))
                value = BitConverter.ToDouble(data, 0);
            else if (type.IsAssignableTo(typeof(IEquatable<bool>)))
                value = BitConverter.ToBoolean(data, 0);
            else if (type.IsAssignableTo(typeof(IEquatable<char>)))
                value = BitConverter.ToChar(data, 0);
            else if (type.IsAssignableTo(typeof(IEquatable<DateTime>)))
                value = DateTime.FromBinary(BitConverter.ToInt64(data, 0));
            else if (type.IsAssignableTo(typeof(IEquatable<TimeSpan>)))
                value = TimeSpan.FromTicks(BitConverter.ToInt64(data, 0));
            else if (type.IsAssignableTo(typeof(IEquatable<Guid>)))
                value = new Guid(data);
            else if (type.IsAssignableTo(typeof(IEnumerable)))
            {

            }
            else if (type.IsEnum)
            {
                if (underlyingEnumType.IsAssignableTo(typeof(IEquatable<byte>)))
                    value = data[0];
                else if (underlyingEnumType.IsAssignableTo(typeof(IEquatable<sbyte>)))
                    value = (sbyte)data[0];
                else if (underlyingEnumType.IsAssignableTo(typeof(IEquatable<short>)))
                    value = BitConverter.ToInt16(data, 0);
                else if (underlyingEnumType.IsAssignableTo(typeof(IEquatable<ushort>)))
                    value = BitConverter.ToUInt16(data, 0);
                else if (underlyingEnumType.IsAssignableTo(typeof(IEquatable<int>)))
                    value = BitConverter.ToInt32(data, 0);
                else if (underlyingEnumType.IsAssignableTo(typeof(IEquatable<uint>)))
                    value = BitConverter.ToUInt32(data, 0);
                else if (underlyingEnumType.IsAssignableTo(typeof(IEquatable<long>)))
                    value = BitConverter.ToInt64(data, 0);
                else if (underlyingEnumType.IsAssignableTo(typeof(IEquatable<ulong>)))
                    value = BitConverter.ToUInt64(data, 0);
            }

            return value;
        }

        private static PropertyInfo[] GetReadableProperties(Type type)
        {
            if (typeProperties.TryGetValue(type, out PropertyInfo[] value)) return value;

            lock (typeLock)
            {
                if (typeProperties.TryGetValue(type, out value)) return value;

                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                     .Where(x => x.CanRead)
                                     .ToArray();

                typeProperties.Add(type, properties);
            }

            return typeProperties[type];
        }
    }
}
