module xorshift

#nowarn "9"

module detail =
    let MAX_VALUE = System.UInt64.MaxValue
    let MAX_VALUE32 = uint64(System.UInt32.MaxValue)
    let DIV64 = 1./float(MAX_VALUE)
    let DIV32 = 1./float(MAX_VALUE32)
    let eps = 1e-6
    let pi = System.Math.PI

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

and prng(state__ : prng_state) =
        let mutable st = state__
        do if st.s0 = 0uL && st.s1 = 0uL then failwith "prng not seeded"
        member private this.next() =
            let x = st.s0
            let x = x ^^^ (x <<< 23)
            let y = st.s1
            let s0 = y
            let s1 = x ^^^ y ^^^ (x >>> 17) ^^^ (y >>> 26)
            st <- prng_state(s0, s1)
            s1 + y
        // the .NET System.Random generator doesn't allow without heap-allocating
        // another Random instance. this implementation exists so that state
        // can be freely reseeded at runtime using stack-allocated structs.
        member this.state with get() = st and set x = st <- x
        member this.next_uint64() = this.next()
        member this.next_double() = (this.next_uint64() |> float) * detail.DIV64
            
        member this.next_float() = this.next_double() |> float32
        member this.next_normal(dev) =
            let mutable u, v = this.next_double(), this.next_double()
            while u < detail.eps do
                u <- this.next_double()
                v <- this.next_double()
            // throw away the second solution
            sqrt(-2. * log(u)) * sin(2. * detail.pi * v) * dev
        member this.next_int(max) = int(this.next_uint64() &&& detail.MAX_VALUE32) % max
