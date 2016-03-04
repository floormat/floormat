#I "bin/Debug/"
#I "C:/Users/Administrator/dev/floormat/floormat/bin/Debug/"
#r "floormat.exe"

let r = xorshift.prng_state.new_prng()
let mutable r = xorshift.prng_state.new_prng() in for i in 1 .. 10 do printfn "%A" <| r.to_int(100); r <- r.next();;
