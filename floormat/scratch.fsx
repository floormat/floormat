#if INTERACTIVE
#I @"bin/Debug/"
#r @"floormat"
#endif

// ---
open xorshift
let r = prng_state.new_prng()
let mutable r = xorshift.prng_state.new_prng() in for i in 1 .. 10 do printfn "%A" <| r.to_int(100); r <- r.next();;
// ---

// ---
