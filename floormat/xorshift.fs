module xorshift

#nowarn "9"

module detail =
    [<Literal>]
    let eps = 1e-6
    let MAX_VALUE32 = uint64(System.UInt32.MaxValue)
    let pi = System.Math.PI

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

and prng(state__ : prng_state) =
        let mutable st = state__
        do if st.s0 = 0uL && st.s1 = 0uL then failwith "prng not seeded"
        member private this.next() =
            // algorithm described in paper:
            // Vigna, Sebastiano (April 2014). "Further scramblings of Marsaglia's xorshift generators". arXiv:1404.0390 [cs.DS]
            let x = st.s0
            let x = x ^^^ (x <<< 23)
            let y = st.s1
            let s0 = y
            let s1 = x ^^^ y ^^^ (x >>> 17) ^^^ (y >>> 26)
            st <- prng_state(s0, s1)
            s1 + y
        // the .NET System.Random generator doesn't allow without reseed heap-allocating
        // another Random instance. this implementation exists so that state
        // can be freely reseeded at runtime using stack-allocated structs.
        member this.state with get() = st and set x = st <- x
        member this.next_uint64() = this.next()
        member this.next_double() =
            System.BitConverter.Int64BitsToDouble(0x3FFUL <<< 52 ||| (this.next_uint64() >>> 12) |> int64) - 1.
        member this.next_float() = this.next_double() |> float32
        member this.next_normal(dev) =
            let mutable v1, v2 = 2. * this.next_double() - 1., 2. * this.next_double() - 1.
            let mutable s = v1 * v1 + v2 * v2
            while s >= 1. do
              v1 <- 2. * this.next_double() - 1.
              v2 <- 2. * this.next_double() - 1.
              s <- v1 * v1 + v2 * v2;
            let norm = sqrt(-2. * log(s) / s);
            v1 * norm * dev
        member this.next_int(max) = int(this.next_uint64() &&& detail.MAX_VALUE32) % max
