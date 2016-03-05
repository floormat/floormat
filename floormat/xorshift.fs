module xorshift

module detail =
    let MAX_VALUE = System.UInt64.MaxValue
    let DIV64 = 2./float(MAX_VALUE)
    let DIV32 = 2.f/float32(MAX_VALUE)

// implements the algorithm from the paper:
// Vigna, Sebastiano (April 2014). "Further scramblings of Marsaglia's xorshift generators". arXiv:1404.0390 [cs.DS]

type prng_state(s0_ : uint64, s1_ : uint64) =
    struct
        static let seeder = System.Random(System.Guid.NewGuid().GetHashCode())
        member this.s0 = s0_
        member this.s1 = s1_
        static member new_prng() =
            (fun () -> let s0 = uint64(seeder.Next()) <<< 32 ||| uint64(seeder.Next())
                       let s1 = uint64(seeder.Next()) <<< 32 ||| uint64(seeder.Next())
                       prng(prng_state(max 1UL s0, s1)))
            |> lock seeder
    end

and prng private(s0 : uint64, s1 : uint64, value : uint64) =
    struct
        new(state : prng_state) =
            let next : prng = prng.next(state.s0, state.s1)
            prng(state.s0, state.s1, next.to_uint64)
        member this.next() =
            if s0 = 0uL && s1 = 0uL then
                failwith "prng not seeded"
            prng.next(s0, s1)
        static member private next(s0, s1) =
            let x = s0
            let x = x ^^^ (x <<< 23)
            let y = s1
            let s0 = y
            let s1 = x ^^^ y ^^^ (x >>> 17) ^^^ (y >>> 26)
            prng(s0, s1, s1 + y)
        member this.state = prng_state(s0, s1)
        member this.to_uint64 = value
        member this.to_double = (this.to_uint64 |> float) * detail.DIV64 - 1.
        member this.to_float = (this.to_uint64 |> float32) * detail.DIV32 - 1.f
        member this.to_int(max) = int(this.to_uint64 &&& uint64(System.Int32.MaxValue)) % max
        // TODO gaussian
