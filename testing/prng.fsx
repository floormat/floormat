#I "bin/Debug/"
#I "C:/Users/Administrator/dev/floormat/floormat/bin/Debug/"
#r "floormat.exe"

let r = xorshift.prng_state.new_prng()

for i in 1 .. 10 do printfn "%A" <| r.next_double()
for i in 1 .. 10 do printfn "%f" <| r.next_normal(1.)
