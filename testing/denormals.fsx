#r "bin/Debug/floormat.exe"

open flush_denormals

let test () =
    let eps = 2.22507385850e-308
    let mutable ret = 0.
    let mutable i = 0u
    while i < (1u <<< 26) do
        i <- i + 1u
        ret <- eps * (1. - 1e-24)
        ret <- ret * (1. - 1e-24)
        ret <- ret * (1. - 1e-24)
    ret

#time "on"

// sample results in debug mode on core i7
// denormals enabled: CPU: 00:00:09.468
// denormals disabled: CPU: 00:00:00.218
