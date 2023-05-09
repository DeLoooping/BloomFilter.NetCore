using System;
using System.Buffers.Binary;

namespace BloomFilter.HashAlgorithms.Internal;

/// <summary>
///   Implements a modified FNV hash. Provides better distribution than FNV1 but it's only 32 bit long.
/// </summary>
internal class ModifiedFNV1 : NonCryptoHashAlgorithm
{
    private const int Size = sizeof(uint);

    private uint _current;

    private const uint Prime = 16777619;
    private const uint InitialState = 2166136261;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FNV1"/> class.
    /// </summary>
    public ModifiedFNV1()
        : base(Size)
    {
        Reset();
    }

    /// <summary>
    ///   Appends the contents of <paramref name="source"/> to the data already
    ///   processed for the current hash computation.
    /// </summary>
    /// <param name="source">The data to process.</param>
    public override void Append(ReadOnlySpan<byte> source)
    {
        _current = Update(_current, source);
    }

    /// <summary>
    ///   Resets the hash computation to the initial state.
    /// </summary>
    public override void Reset()
    {
        _current = InitialState;
    }

    /// <summary>
    ///   Writes the computed hash value to <paramref name="destination"/>
    ///   without modifying accumulated state.
    /// </summary>
    /// <param name="destination">The buffer that receives the computed hash value.</param>
    protected override void GetCurrentHashCore(Span<byte> destination)
    {
        _current = GetCurrentHashAsUInt32();
        BinaryPrimitives.WriteUInt32LittleEndian(destination, _current);
    }

    /// <summary>
    ///   Writes the computed hash value to <paramref name="destination"/>
    ///   then clears the accumulated state.
    /// </summary>
    protected override void GetHashAndResetCore(Span<byte> destination)
    {
        _current = GetCurrentHashAsUInt32();
        BinaryPrimitives.WriteUInt32LittleEndian(destination, _current);
        _current = InitialState;
    }

    /// <summary>Gets the current computed hash value without modifying accumulated state.</summary>
    /// <returns>The hash value for the data already provided.</returns>
    public uint GetCurrentHashAsUInt32()
    {
        return Finalization(_current);
    }

    /// <summary>Computes the FNV1 hash of the provided data.</summary>
    /// <param name="source">The data to hash.</param>
    /// <returns>The computed FNV1 hash.</returns>
    public static uint HashToUInt32(ReadOnlySpan<byte> source) =>
        Finalization(Update(InitialState, source));

    private static uint Update(uint current, ReadOnlySpan<byte> source)
    {
        for (int i = 0; i < source.Length; i++)
        {
            current ^= source[i];
            current *= Prime;
        }

        return current;
    }

    private static uint Finalization(uint current)
    {
        current += current << 13;
        current ^= current >> 7;
        current += current << 3;
        current ^= current >> 17;
        current += current << 5;

        return current;
    }
}