module xorshift

module detail =
    let MAX_VALUE = System.UInt64.MaxValue
    let DIV64 = 2./float(MAX_VALUE)
    let DIV32 = 2.f/float32(MAX_VALUE)

// implements the algorithm from the paper:
// Vigna, Sebastiano (April 2014). "Further scramblings of Marsaglia's xorshift generators". arXiv:1404.0390 [cs.DS]

type prng_state(s0_ : uint64, s1_ : uint64) =
    struct
        static let seeder = new System.Threading.ThreadLocal<_>(fun () -> System.Random(System.Guid.NewGuid().GetHashCode()))
        member this.s0 = s0_
        member this.s1 = s1_
        static member new_prng() =
            let rnd = seeder.Value
            let s0 = uint64(rnd.Next()) <<< 32 ||| uint64(rnd.Next())
            let s1 = uint64(rnd.Next()) <<< 32 ||| uint64(rnd.Next())
            prng(prng_state(s0, s1))
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
#if false
        interface System.Collections.Generic.IEnumerable<uint64> with
            member this.GetEnumerator() =
                new prng_enumerator(this) :> System.Collections.Generic.IEnumerator<uint64>
        interface System.Collections.IEnumerable with
            member this.GetEnumerator() =
                new prng_enumerator(this) :> System.Collections.IEnumerator
#endif
    end

#if false
and [<NoComparison; NoEquality>] prng_enumerator =
    struct
        val first : prng
        val mutable cur : prng
        val mutable first_iter : bool
        new(first) = { first = first; cur = first; first_iter = true }
        interface System.IDisposable with
            member this.Dispose() = ()
        interface System.Collections.Generic.IEnumerator<uint64> with
            member this.Current =
                this.cur.value
        interface System.Collections.IEnumerator with
            member this.MoveNext() = 
                if this.first_iter then
                    this.first_iter <- false
                else
                    this.cur <- this.cur.next()
                true
            member this.Reset() =
                this.cur <- this.first
                this.first_iter <- true
            member this.Current =
                this.cur.value :> obj
    end

    module detail =
        // #time shows GC gen0: 27, gen1: 3, gen2: 0
        let test () =
            for i in 0 .. 1000000 do
                let enum = prng(prng_state.new_state()) :> System.Collections.Generic.IEnumerable<uint64>
                let enum = enum.GetEnumerator()
                let mutable idx = 0
                while idx < 1000 do
                    idx <- idx + 1
                    enum.MoveNext() |> ignore
                    enum.Current |> ignore
            ()
#endif
